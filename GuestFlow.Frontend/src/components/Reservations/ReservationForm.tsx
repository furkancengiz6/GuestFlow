import { useEffect } from 'react'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  Box,
} from '@mui/material'
import { Reservation, CreateReservationRequest, UpdateReservationRequest } from '../../services/reservationService'
import { dropdownService } from '../../services/dropdownService'
import { useQuery } from '@tanstack/react-query'
import { format } from 'date-fns'

const reservationSchema = z.object({
  reservationDate: z.string().min(1, 'Tarih seçmelisiniz'),
  guestId: z.number().min(1, 'Misafir seçmelisiniz'),
  personnelId: z.number().min(1, 'Personel seçmelisiniz'),
  status: z.string().min(1, 'Durum seçmelisiniz'),
  note: z.string().max(500, 'Not en fazla 500 karakter olabilir').optional().or(z.literal('')),
})

type ReservationFormData = z.infer<typeof reservationSchema>

interface ReservationFormProps {
  open: boolean
  onClose: () => void
  onSubmit: (data: CreateReservationRequest | UpdateReservationRequest) => Promise<void>
  reservation?: Reservation | null
  isLoading?: boolean
}

const ReservationForm = ({ open, onClose, onSubmit, reservation, isLoading = false }: ReservationFormProps) => {
  const { data: guests } = useQuery({
    queryKey: ['guests-dropdown'],
    queryFn: () => dropdownService.getGuests(),
  })

  const { data: personnel } = useQuery({
    queryKey: ['personnel-dropdown'],
    queryFn: () => dropdownService.getPersonnel(),
  })

  const {
    register,
    handleSubmit,
    control,
    formState: { errors, isSubmitting },
    reset,
    setValue,
  } = useForm<ReservationFormData>({
    resolver: zodResolver(reservationSchema),
    defaultValues: {
      reservationDate: format(new Date(), 'yyyy-MM-dd'),
      guestId: 0,
      personnelId: 0,
      status: 'Pending',
      note: '',
    },
  })

  useEffect(() => {
    if (reservation) {
      setValue('reservationDate', reservation.reservationDate.split('T')[0])
      setValue('guestId', reservation.guestId)
      setValue('personnelId', reservation.personnelId)
      setValue('status', reservation.status)
      setValue('note', reservation.note || '')
    } else {
      reset({
        reservationDate: format(new Date(), 'yyyy-MM-dd'),
        guestId: 0,
        personnelId: 0,
        status: 'Pending',
        note: '',
      })
    }
  }, [reservation, setValue, reset])

  const handleFormSubmit = async (data: ReservationFormData) => {
    try {
      await onSubmit(data)
    } catch (error) {
      console.error('Form submission error:', error)
    }
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <form onSubmit={handleSubmit(handleFormSubmit)}>
        <DialogTitle>{reservation ? 'Rezervasyon Düzenle' : 'Yeni Rezervasyon'}</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
            <TextField
              {...register('reservationDate')}
              label="Rezervasyon Tarihi"
              type="date"
              fullWidth
              InputLabelProps={{ shrink: true }}
              error={!!errors.reservationDate}
              helperText={errors.reservationDate?.message}
            />
            <Controller
              name="guestId"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  select
                  label="Misafir"
                  fullWidth
                  error={!!errors.guestId}
                  helperText={errors.guestId?.message}
                  SelectProps={{
                    native: true,
                  }}
                >
                  <option value={0}>Misafir Seçiniz</option>
                  {guests?.map((guest) => (
                    <option key={guest.id} value={guest.id}>
                      {guest.fullName} ({guest.guestCode})
                    </option>
                  ))}
                </TextField>
              )}
            />
            <Controller
              name="personnelId"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  select
                  label="Personel"
                  fullWidth
                  error={!!errors.personnelId}
                  helperText={errors.personnelId?.message}
                  SelectProps={{
                    native: true,
                  }}
                >
                  <option value={0}>Personel Seçiniz</option>
                  {personnel?.map((p) => (
                    <option key={p.id} value={p.id}>
                      {p.fullName}
                    </option>
                  ))}
                </TextField>
              )}
            />
            <Controller
              name="status"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  select
                  label="Durum"
                  fullWidth
                  error={!!errors.status}
                  helperText={errors.status?.message}
                  SelectProps={{
                    native: true,
                  }}
                >
                  <option value="Pending">Beklemede</option>
                  <option value="Confirmed">Onaylandı</option>
                  <option value="Cancelled">İptal Edildi</option>
                  <option value="Completed">Tamamlandı</option>
                </TextField>
              )}
            />
            <TextField
              {...register('note')}
              label="Not (Opsiyonel)"
              multiline
              rows={3}
              fullWidth
              error={!!errors.note}
              helperText={errors.note?.message}
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose} disabled={isSubmitting || isLoading}>
            İptal
          </Button>
          <Button type="submit" variant="contained" disabled={isSubmitting || isLoading}>
            {isSubmitting || isLoading ? 'Kaydediliyor...' : reservation ? 'Güncelle' : 'Ekle'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  )
}

export default ReservationForm


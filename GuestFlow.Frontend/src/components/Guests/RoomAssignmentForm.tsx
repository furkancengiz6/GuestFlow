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
  Grid,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Typography,
  Alert,
} from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns'
import { useQuery } from '@tanstack/react-query'
import { dropdownService } from '../../services/dropdownService'
import { RoomAssignment } from '../../services/roomService'

// Zod schema for room assignment
const roomAssignmentSchema = z.object({
  guestId: z.number().min(1, 'Misafir seçilmelidir'),
  hotelId: z.number().optional(),
  roomNumber: z.string().min(1, 'Oda numarası gereklidir').max(20, 'Oda numarası en fazla 20 karakter olabilir'),
  startDate: z.date({ required_error: 'Başlangıç tarihi gereklidir' }),
  endDate: z.date().optional(),
  notes: z.string().max(500, 'Notlar en fazla 500 karakter olabilir').optional(),
})

type RoomAssignmentFormData = z.infer<typeof roomAssignmentSchema>

interface RoomAssignmentFormProps {
  open: boolean
  onClose: () => void
  onSubmit: (data: RoomAssignmentFormData) => Promise<void>
  roomAssignment?: RoomAssignment | null
  guestId?: number // Pre-select guest if provided
  isLoading?: boolean
}

const RoomAssignmentForm = ({
  open,
  onClose,
  onSubmit,
  roomAssignment,
  guestId,
  isLoading = false
}: RoomAssignmentFormProps) => {
  const {
    register,
    handleSubmit,
    control,
    formState: { errors, isSubmitting },
    reset,
    setValue,
  } = useForm<RoomAssignmentFormData>({
    resolver: zodResolver(roomAssignmentSchema),
    defaultValues: {
      guestId: guestId || 0,
      hotelId: undefined,
      roomNumber: '',
      startDate: new Date(),
      endDate: undefined,
      notes: '',
    },
  })

  // Fetch dropdown data
  const { data: guests } = useQuery({
    queryKey: ['guests-dropdown'],
    queryFn: () => dropdownService.getGuests(),
    enabled: open && !guestId, // Only fetch if guest not pre-selected
  })

  const { data: hotels } = useQuery({
    queryKey: ['hotels-dropdown'],
    queryFn: () => dropdownService.getHotels(),
    enabled: open,
  })

  // Form'u room assignment verisiyle doldur (düzenleme modu)
  useEffect(() => {
    if (roomAssignment) {
      setValue('guestId', roomAssignment.guestId)
      setValue('hotelId', roomAssignment.hotelId || undefined)
      setValue('roomNumber', roomAssignment.roomNumber)
      setValue('startDate', new Date(roomAssignment.startDate))
      setValue('endDate', roomAssignment.endDate ? new Date(roomAssignment.endDate) : undefined)
      setValue('notes', roomAssignment.notes || '')
    } else if (guestId) {
      setValue('guestId', guestId)
    } else {
      reset()
    }
  }, [roomAssignment, guestId, setValue, reset])

  const handleFormSubmit = async (data: RoomAssignmentFormData) => {
    try {
      const submitData = {
        ...data,
        startDate: data.startDate.toISOString().split('T')[0], // Date only
        endDate: data.endDate ? data.endDate.toISOString().split('T')[0] : undefined,
        notes: data.notes || undefined,
      }

      await onSubmit(submitData)
      reset()
      onClose()
    } catch (error) {
      console.error('Room assignment submission error:', error)
    }
  }

  const handleClose = () => {
    reset()
    onClose()
  }

  return (
    <LocalizationProvider dateAdapter={AdapterDateFns}>
      <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
        <form onSubmit={handleSubmit(handleFormSubmit)}>
          <DialogTitle>
            {roomAssignment ? 'Oda Atamasını Düzenle' : 'Yeni Oda Ataması Ekle'}
          </DialogTitle>
          <DialogContent>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
              <Alert severity="info">
                Oda numarası, misafirin kaldığı odayı belirli bir tarih aralığında gösterir.
                Aynı misafir farklı tarihlerde farklı odalarda kalabilir.
              </Alert>

              <Grid container spacing={2}>
                {!guestId && (
                  <Grid item xs={12}>
                    <FormControl fullWidth required error={!!errors.guestId} disabled={isSubmitting || isLoading}>
                      <InputLabel>Misafir</InputLabel>
                      <Controller
                        name="guestId"
                        control={control}
                        render={({ field }) => (
                          <Select {...field} value={field.value || ''}>
                            <MenuItem value="">Seçiniz</MenuItem>
                            {guests?.map((guest) => (
                              <MenuItem key={guest.id} value={guest.id}>
                                {guest.fullName} ({guest.guestCode})
                              </MenuItem>
                            ))}
                          </Select>
                        )}
                      />
                    </FormControl>
                    {errors.guestId && (
                      <Typography variant="caption" color="error" sx={{ mt: 0.5 }}>
                        {errors.guestId.message}
                      </Typography>
                    )}
                  </Grid>
                )}

                <Grid item xs={12}>
                  <FormControl fullWidth disabled={isSubmitting || isLoading}>
                    <InputLabel>Otel (Opsiyonel)</InputLabel>
                    <Controller
                      name="hotelId"
                      control={control}
                      render={({ field }) => (
                        <Select {...field} value={field.value || ''}>
                          <MenuItem value="">Seçiniz</MenuItem>
                          {hotels?.map((hotel) => (
                            <MenuItem key={hotel.id} value={hotel.id}>
                              {hotel.hotelName}
                            </MenuItem>
                          ))}
                        </Select>
                      )}
                    />
                  </FormControl>
                </Grid>

                <Grid item xs={12} md={6}>
                  <TextField
                    label="Oda Numarası"
                    fullWidth
                    required
                    {...register('roomNumber')}
                    error={!!errors.roomNumber}
                    helperText={errors.roomNumber?.message}
                    disabled={isSubmitting || isLoading}
                    placeholder="101, 205, VIP-1, vb."
                  />
                </Grid>

                <Grid item xs={12} md={6}>
                  <Controller
                    name="startDate"
                    control={control}
                    render={({ field }) => (
                      <DatePicker
                        label="Başlangıç Tarihi"
                        value={field.value}
                        onChange={field.onChange}
                        slotProps={{
                          textField: {
                            fullWidth: true,
                            error: !!errors.startDate,
                            helperText: errors.startDate?.message,
                            disabled: isSubmitting || isLoading,
                          },
                        }}
                      />
                    )}
                  />
                </Grid>

                <Grid item xs={12} md={6}>
                  <Controller
                    name="endDate"
                    control={control}
                    render={({ field }) => (
                      <DatePicker
                        label="Bitiş Tarihi (Opsiyonel)"
                        value={field.value}
                        onChange={field.onChange}
                        slotProps={{
                          textField: {
                            fullWidth: true,
                            error: !!errors.endDate,
                            helperText: errors.endDate?.message,
                            disabled: isSubmitting || isLoading,
                          },
                        }}
                      />
                    )}
                  />
                  <Typography variant="caption" color="text.secondary">
                    Boş bırakılırsa, oda ataması devam ediyor olarak işaretlenir
                  </Typography>
                </Grid>

                <Grid item xs={12}>
                  <TextField
                    label="Notlar"
                    fullWidth
                    multiline
                    rows={3}
                    {...register('notes')}
                    error={!!errors.notes}
                    helperText={errors.notes?.message}
                    disabled={isSubmitting || isLoading}
                    placeholder="Oda değişikliği nedeni, özel talepler, vb."
                  />
                </Grid>
              </Grid>
            </Box>
          </DialogContent>
          <DialogActions sx={{ px: 3, pb: 2 }}>
            <Button onClick={handleClose} disabled={isSubmitting || isLoading}>
              İptal
            </Button>
            <Button type="submit" variant="contained" disabled={isSubmitting || isLoading}>
              {isSubmitting || isLoading
                ? 'Kaydediliyor...'
                : roomAssignment
                  ? 'Güncelle'
                  : 'Oda Ataması Ekle'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </LocalizationProvider>
  )
}

export default RoomAssignmentForm

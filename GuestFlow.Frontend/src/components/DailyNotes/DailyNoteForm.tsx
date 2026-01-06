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
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns'
import { tr } from 'date-fns/locale'
import { DailyNote, CreateDailyNoteRequest, UpdateDailyNoteRequest } from '../../services/dailyNoteService'
import { dropdownService } from '../../services/dropdownService'
import { useQuery } from '@tanstack/react-query'
import { format } from 'date-fns'

const dailyNoteSchema = z.object({
  noteDate: z.string().min(1, 'Tarih seçmelisiniz'),
  note: z.string().min(1, 'Not en az 1 karakter olmalıdır').max(1000, 'Not en fazla 1000 karakter olabilir'),
  personnelId: z.number().min(1, 'Personel seçmelisiniz'),
})

type DailyNoteFormData = z.infer<typeof dailyNoteSchema>

interface DailyNoteFormProps {
  open: boolean
  onClose: () => void
  onSubmit: (data: CreateDailyNoteRequest | UpdateDailyNoteRequest) => Promise<void>
  dailyNote?: DailyNote | null
  isLoading?: boolean
}

const DailyNoteForm = ({ open, onClose, onSubmit, dailyNote, isLoading = false }: DailyNoteFormProps) => {
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
  } = useForm<DailyNoteFormData>({
    resolver: zodResolver(dailyNoteSchema),
    defaultValues: {
      noteDate: format(new Date(), 'yyyy-MM-dd'),
      note: '',
      personnelId: 0,
    },
  })

  useEffect(() => {
    if (dailyNote) {
      setValue('noteDate', dailyNote.noteDate.split('T')[0])
      setValue('note', dailyNote.note)
      setValue('personnelId', dailyNote.personnelId)
    } else {
      reset({
        noteDate: format(new Date(), 'yyyy-MM-dd'),
        note: '',
        personnelId: 0,
      })
    }
  }, [dailyNote, setValue, reset])

  const handleFormSubmit = async (data: DailyNoteFormData) => {
    try {
      await onSubmit(data)
    } catch (error) {
      console.error('Form submission error:', error)
    }
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <form onSubmit={handleSubmit(handleFormSubmit)}>
        <DialogTitle>{dailyNote ? 'Günlük Not Düzenle' : 'Yeni Günlük Not'}</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
            <TextField
              {...register('noteDate')}
              label="Tarih"
              type="date"
              fullWidth
              InputLabelProps={{ shrink: true }}
              error={!!errors.noteDate}
              helperText={errors.noteDate?.message}
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
            <TextField
              {...register('note')}
              label="Not"
              multiline
              rows={6}
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
            {isSubmitting || isLoading ? 'Kaydediliyor...' : dailyNote ? 'Güncelle' : 'Ekle'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  )
}

export default DailyNoteForm


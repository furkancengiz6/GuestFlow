import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
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
import { DailyRevenue, CreateDailyRevenueRequest, UpdateDailyRevenueRequest } from '../../services/dailyRevenueService'
import { format } from 'date-fns'

const dailyRevenueSchema = z.object({
  revenueDate: z.string().min(1, 'Tarih seçmelisiniz'),
  revenueAmount: z.number().min(0, 'Gelir miktarı 0 veya daha büyük olmalıdır'),
  currency: z.string().min(3, 'Para birimi kodu 3 karakter olmalıdır').max(3, 'Para birimi kodu 3 karakter olmalıdır'),
  note: z.string().max(500, 'Not en fazla 500 karakter olabilir').optional().or(z.literal('')),
})

type DailyRevenueFormData = z.infer<typeof dailyRevenueSchema>

interface DailyRevenueFormProps {
  open: boolean
  onClose: () => void
  onSubmit: (data: CreateDailyRevenueRequest | UpdateDailyRevenueRequest) => Promise<void>
  dailyRevenue?: DailyRevenue | null
  isLoading?: boolean
}

const DailyRevenueForm = ({ open, onClose, onSubmit, dailyRevenue, isLoading = false }: DailyRevenueFormProps) => {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    reset,
    setValue,
  } = useForm<DailyRevenueFormData>({
    resolver: zodResolver(dailyRevenueSchema),
    defaultValues: {
      revenueDate: format(new Date(), 'yyyy-MM-dd'),
      revenueAmount: 0,
      currency: 'USD',
      note: '',
    },
  })

  useEffect(() => {
    if (dailyRevenue) {
      setValue('revenueDate', dailyRevenue.revenueDate.split('T')[0])
      setValue('revenueAmount', dailyRevenue.revenueAmount)
      setValue('currency', dailyRevenue.currency)
      setValue('note', dailyRevenue.note || '')
    } else {
      reset({
        revenueDate: format(new Date(), 'yyyy-MM-dd'),
        revenueAmount: 0,
        currency: 'USD',
        note: '',
      })
    }
  }, [dailyRevenue, setValue, reset])

  const handleFormSubmit = async (data: DailyRevenueFormData) => {
    try {
      await onSubmit(data)
    } catch (error) {
      console.error('Form submission error:', error)
    }
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <form onSubmit={handleSubmit(handleFormSubmit)}>
        <DialogTitle>{dailyRevenue ? 'Günlük Gelir Düzenle' : 'Yeni Günlük Gelir'}</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
            <TextField
              {...register('revenueDate')}
              label="Tarih"
              type="date"
              fullWidth
              InputLabelProps={{ shrink: true }}
              error={!!errors.revenueDate}
              helperText={errors.revenueDate?.message}
            />
            <TextField
              {...register('revenueAmount', { valueAsNumber: true })}
              label="Gelir Miktarı"
              type="number"
              fullWidth
              error={!!errors.revenueAmount}
              helperText={errors.revenueAmount?.message}
            />
            <TextField
              {...register('currency')}
              label="Para Birimi (USD, EUR, TRY)"
              fullWidth
              error={!!errors.currency}
              helperText={errors.currency?.message}
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
            {isSubmitting || isLoading ? 'Kaydediliyor...' : dailyRevenue ? 'Güncelle' : 'Ekle'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  )
}

export default DailyRevenueForm


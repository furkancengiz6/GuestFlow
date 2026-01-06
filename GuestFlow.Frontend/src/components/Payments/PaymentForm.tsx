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
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns'
import { tr } from 'date-fns/locale'
import { useQuery } from '@tanstack/react-query'
import { dropdownService } from '../../services/dropdownService'
import { PaymentMethod } from '../../types/enums'

// Zod schema for payment creation
const paymentSchema = z.object({
  amount: z.number().min(0.01, 'Ödeme tutarı 0.01\'den büyük olmalıdır'),
  currency: z.string().min(1, 'Para birimi gereklidir'),
  paymentMethod: z.nativeEnum(PaymentMethod, { required_error: 'Ödeme yöntemi gereklidir' }),
  paymentDate: z.date({ required_error: 'Ödeme tarihi gereklidir' }),
  guestId: z.number().min(1, 'Misafir seçilmelidir'),
  invoiceId: z.number().optional(),
  transferId: z.number().optional(),
  cityTourId: z.number().optional(),
  yachtTourId: z.number().optional(),
  notes: z.string().max(1000, 'Notlar en fazla 1000 karakter olabilir').optional(),
})

type PaymentFormData = z.infer<typeof paymentSchema>

interface PaymentFormProps {
  open: boolean
  onClose: () => void
  onSubmit: (data: PaymentFormData) => Promise<void>
  preselectedService?: {
    serviceType: 'transfer' | 'citytour' | 'yachttour'
    serviceId: number
    guestId: number
    amount: number
    currency: string
  }
  isLoading?: boolean
}

const PaymentForm = ({ open, onClose, onSubmit, preselectedService, isLoading = false }: PaymentFormProps) => {
  const {
    register,
    handleSubmit,
    control,
    formState: { errors, isSubmitting },
    reset,
    setValue,
    watch,
  } = useForm<PaymentFormData>({
    resolver: zodResolver(paymentSchema),
    defaultValues: {
      amount: 0,
      currency: 'TRY',
      paymentMethod: PaymentMethod.Cash,
      paymentDate: new Date(),
      notes: '',
    },
  })

  // Fetch dropdown data
  const { data: guests } = useQuery({
    queryKey: ['guests-dropdown'],
    queryFn: () => dropdownService.getGuests(),
    enabled: open,
  })

  const { data: invoices } = useQuery({
    queryKey: ['invoices-dropdown'],
    queryFn: () => dropdownService.getInvoices(),
    enabled: open,
  })

  const { data: transfers } = useQuery({
    queryKey: ['transfers-dropdown'],
    queryFn: () => dropdownService.getTransfers(),
    enabled: open,
  })

  const { data: cityTours } = useQuery({
    queryKey: ['citytours-dropdown'],
    queryFn: () => dropdownService.getCityTours(),
    enabled: open,
  })

  const { data: yachtTours } = useQuery({
    queryKey: ['yachttours-dropdown'],
    queryFn: () => dropdownService.getYachtTours(),
    enabled: open,
  })

  // Pre-fill form with preselected service data
  useEffect(() => {
    if (preselectedService) {
      setValue('guestId', preselectedService.guestId)
      setValue('amount', preselectedService.amount)
      setValue('currency', preselectedService.currency)

      // Set the appropriate service ID based on type
      switch (preselectedService.serviceType) {
        case 'transfer':
          setValue('transferId', preselectedService.serviceId)
          break
        case 'citytour':
          setValue('cityTourId', preselectedService.serviceId)
          break
        case 'yachttour':
          setValue('yachtTourId', preselectedService.serviceId)
          break
      }
    }
  }, [preselectedService, setValue])

  const handleFormSubmit = async (data: PaymentFormData) => {
    try {
      const submitData = {
        ...data,
        paymentDate: data.paymentDate.toISOString(),
        // Convert optional IDs to undefined if 0
        invoiceId: data.invoiceId && data.invoiceId > 0 ? data.invoiceId : undefined,
        transferId: data.transferId && data.transferId > 0 ? data.transferId : undefined,
        cityTourId: data.cityTourId && data.cityTourId > 0 ? data.cityTourId : undefined,
        yachtTourId: data.yachtTourId && data.yachtTourId > 0 ? data.yachtTourId : undefined,
        notes: data.notes || undefined,
      }

      await onSubmit(submitData)
      reset()
      onClose()
    } catch (error) {
      console.error('Payment submission error:', error)
    }
  }

  const handleClose = () => {
    reset()
    onClose()
  }

  return (
    <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={tr}>
      <Dialog open={open} onClose={handleClose} maxWidth="md" fullWidth>
        <form onSubmit={handleSubmit(handleFormSubmit)}>
          <DialogTitle>Ödeme Ekle</DialogTitle>
          <DialogContent>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
              {preselectedService && (
                <Alert severity="info">
                  Bu ödeme, seçili hizmet için oluşturulacaktır. Tutar ve para birimi önceden doldurulmuştur.
                </Alert>
              )}

              <Grid container spacing={2}>
                <Grid item xs={12} md={6}>
                  <TextField
                    label="Ödeme Tutarı"
                    type="number"
                    fullWidth
                    required
                    {...register('amount', { valueAsNumber: true })}
                    error={!!errors.amount}
                    helperText={errors.amount?.message}
                    disabled={isSubmitting || isLoading}
                    inputProps={{ step: '0.01', min: '0.01' }}
                  />
                </Grid>

                <Grid item xs={12} md={6}>
                  <FormControl fullWidth required disabled={isSubmitting || isLoading}>
                    <InputLabel>Para Birimi</InputLabel>
                    <Controller
                      name="currency"
                      control={control}
                      render={({ field }) => (
                        <Select {...field} value={field.value || 'TRY'}>
                          <MenuItem value="TRY">TRY - Türk Lirası</MenuItem>
                          <MenuItem value="USD">USD - US Dollar</MenuItem>
                          <MenuItem value="EUR">EUR - Euro</MenuItem>
                          <MenuItem value="GBP">GBP - British Pound</MenuItem>
                          <MenuItem value="RUB">RUB - Russian Ruble</MenuItem>
                        </Select>
                      )}
                    />
                  </FormControl>
                  {errors.currency && (
                    <Typography variant="caption" color="error" sx={{ mt: 0.5 }}>
                      {errors.currency.message}
                    </Typography>
                  )}
                </Grid>

                <Grid item xs={12} md={6}>
                  <FormControl fullWidth required error={!!errors.paymentMethod} disabled={isSubmitting || isLoading}>
                    <InputLabel>Ödeme Yöntemi</InputLabel>
                    <Controller
                      name="paymentMethod"
                      control={control}
                      render={({ field }) => (
                        <Select {...field} value={field.value || PaymentMethod.Cash}>
                          <MenuItem value={PaymentMethod.Cash}>Nakit</MenuItem>
                          <MenuItem value={PaymentMethod.CreditCard}>Kredi Kartı</MenuItem>
                          <MenuItem value={PaymentMethod.RoomCharge}>Odaya Charge</MenuItem>
                          <MenuItem value={PaymentMethod.BankTransfer}>Banka Havalesi</MenuItem>
                          <MenuItem value={PaymentMethod.Other}>Diğer</MenuItem>
                        </Select>
                      )}
                    />
                  </FormControl>
                  {errors.paymentMethod && (
                    <Typography variant="caption" color="error" sx={{ mt: 0.5 }}>
                      {errors.paymentMethod.message}
                    </Typography>
                  )}
                </Grid>

                <Grid item xs={12} md={6}>
                  <Controller
                    name="paymentDate"
                    control={control}
                    render={({ field }) => (
                      <DateTimePicker
                        label="Ödeme Tarihi"
                        value={field.value}
                        onChange={field.onChange}
                        slotProps={{
                          textField: {
                            fullWidth: true,
                            error: !!errors.paymentDate,
                            helperText: errors.paymentDate?.message,
                            disabled: isSubmitting || isLoading,
                          },
                        }}
                      />
                    )}
                  />
                </Grid>

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

                <Grid item xs={12} md={6}>
                  <FormControl fullWidth disabled={isSubmitting || isLoading}>
                    <InputLabel>Fatura (Opsiyonel)</InputLabel>
                    <Controller
                      name="invoiceId"
                      control={control}
                      render={({ field }) => (
                        <Select {...field} value={field.value || ''}>
                          <MenuItem value="">Seçiniz</MenuItem>
                          {invoices?.map((invoice) => (
                            <MenuItem key={invoice.id} value={invoice.id}>
                              #{invoice.invoiceNumber} - {invoice.totalAmount} {invoice.currency}
                            </MenuItem>
                          ))}
                        </Select>
                      )}
                    />
                  </FormControl>
                </Grid>

                <Grid item xs={12} md={6}>
                  <FormControl fullWidth disabled={isSubmitting || isLoading}>
                    <InputLabel>Transfer (Opsiyonel)</InputLabel>
                    <Controller
                      name="transferId"
                      control={control}
                      render={({ field }) => (
                        <Select {...field} value={field.value || ''}>
                          <MenuItem value="">Seçiniz</MenuItem>
                          {transfers?.map((transfer) => (
                            <MenuItem key={transfer.id} value={transfer.id}>
                              {transfer.pickupAddress} → {transfer.dropoffAddress}
                            </MenuItem>
                          ))}
                        </Select>
                      )}
                    />
                  </FormControl>
                </Grid>

                <Grid item xs={12} md={6}>
                  <FormControl fullWidth disabled={isSubmitting || isLoading}>
                    <InputLabel>Şehir Turu (Opsiyonel)</InputLabel>
                    <Controller
                      name="cityTourId"
                      control={control}
                      render={({ field }) => (
                        <Select {...field} value={field.value || ''}>
                          <MenuItem value="">Seçiniz</MenuItem>
                          {cityTours?.map((tour) => (
                            <MenuItem key={tour.id} value={tour.id}>
                              {tour.tourName} - {new Date(tour.tourDate).toLocaleDateString()}
                            </MenuItem>
                          ))}
                        </Select>
                      )}
                    />
                  </FormControl>
                </Grid>

                <Grid item xs={12} md={6}>
                  <FormControl fullWidth disabled={isSubmitting || isLoading}>
                    <InputLabel>Yat Turu (Opsiyonel)</InputLabel>
                    <Controller
                      name="yachtTourId"
                      control={control}
                      render={({ field }) => (
                        <Select {...field} value={field.value || ''}>
                          <MenuItem value="">Seçiniz</MenuItem>
                          {yachtTours?.map((tour) => (
                            <MenuItem key={tour.id} value={tour.id}>
                              {tour.yachtName || 'Yat'} - {new Date(tour.tourDate).toLocaleDateString()}
                            </MenuItem>
                          ))}
                        </Select>
                      )}
                    />
                  </FormControl>
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
              {isSubmitting || isLoading ? 'Kaydediliyor...' : 'Ödeme Ekle'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </LocalizationProvider>
  )
}

export default PaymentForm

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
  FormControlLabel,
  Switch,
  Typography,
} from '@mui/material'
import { DateTimePicker } from '@mui/x-date-pickers/DateTimePicker'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns'
import { tr } from 'date-fns/locale'
import { useQuery } from '@tanstack/react-query'
import { dropdownService } from '../../services/dropdownService'
import { Transfer, CreateTransferRequest, UpdateTransferRequest } from '../../services/transferService'

// Zod schema
const transferSchema = z.object({
  transferDate: z.date({ required_error: 'Transfer tarihi gereklidir' }),
  pickupAddress: z.string().min(1, 'Alış adresi gereklidir').max(100, 'Alış adresi en fazla 100 karakter olabilir'),
  dropoffAddress: z.string().min(1, 'Bırakış adresi gereklidir').max(100, 'Bırakış adresi en fazla 100 karakter olabilir'),
  price: z.number().min(0.01, 'Fiyat 0.01\'den büyük olmalıdır'),
  guestId: z.number().min(1, 'Misafir seçilmelidir'),
  personnelId: z.number().min(1, 'Personel seçilmelidir'),
  airportId: z.number().min(1, 'Havaalanı seçilmelidir'),
  vehicleId: z.number().min(1, 'Araç seçilmelidir'),
  note: z.string().optional(),
  status: z.string().min(1, 'Durum seçilmelidir'),
  isFromAirport: z.boolean(),
  pickupCityId: z.number().min(1, 'Alış şehri seçilmelidir'),
  dropoffCityId: z.number().min(1, 'Bırakış şehri seçilmelidir'),
  createInvoice: z.boolean().optional(),
  discountPercentage: z.number().min(0).max(100).optional(),
  invoiceDescription: z.string().optional(),
  currency: z.string().optional(),
})

type TransferFormData = z.infer<typeof transferSchema>

interface TransferFormProps {
  open: boolean
  onClose: () => void
  onSubmit: (data: CreateTransferRequest | UpdateTransferRequest) => Promise<void>
  transfer?: Transfer | null
  isLoading?: boolean
}

const TransferForm = ({ open, onClose, onSubmit, transfer, isLoading = false }: TransferFormProps) => {
  const {
    register,
    handleSubmit,
    control,
    formState: { errors, isSubmitting },
    reset,
    setValue,
    watch,
  } = useForm<TransferFormData>({
    resolver: zodResolver(transferSchema),
    defaultValues: {
      transferDate: new Date(),
      pickupAddress: '',
      dropoffAddress: '',
      price: 0,
      guestId: 0,
      personnelId: 0,
      airportId: 0,
      vehicleId: 0,
      note: '',
      status: 'Pending',
      isFromAirport: false,
      pickupCityId: 0,
      dropoffCityId: 0,
      createInvoice: false,
      discountPercentage: 0,
      invoiceDescription: '',
      currency: 'TRY',
    },
  })

  // Fetch dropdown data
  const { data: guests } = useQuery({
    queryKey: ['guests-dropdown'],
    queryFn: () => dropdownService.getGuests(),
    enabled: open,
  })

  const { data: personnel } = useQuery({
    queryKey: ['personnel-dropdown'],
    queryFn: () => dropdownService.getPersonnel(),
    enabled: open,
  })

  const { data: airports } = useQuery({
    queryKey: ['airports-dropdown'],
    queryFn: () => dropdownService.getAirports(),
    enabled: open,
  })

  const { data: vehicles } = useQuery({
    queryKey: ['vehicles-dropdown'],
    queryFn: () => dropdownService.getVehicles(),
    enabled: open,
  })

  const { data: cities } = useQuery({
    queryKey: ['cities-dropdown'],
    queryFn: () => dropdownService.getCities(),
    enabled: open,
  })

  const createInvoice = watch('createInvoice')

  // Form'u transfer verisiyle doldur (düzenleme modu)
  useEffect(() => {
    if (transfer) {
      setValue('transferDate', new Date(transfer.transferDate))
      setValue('pickupAddress', transfer.pickupAddress)
      setValue('dropoffAddress', transfer.dropoffAddress)
      setValue('price', transfer.price)
      setValue('guestId', transfer.guestId)
      setValue('personnelId', transfer.personnelId)
      setValue('airportId', transfer.airportId)
      setValue('vehicleId', transfer.vehicleId)
      setValue('note', transfer.note || '')
      setValue('status', transfer.status)
      setValue('isFromAirport', transfer.isFromAirport)
      setValue('pickupCityId', transfer.pickupCityId)
      setValue('dropoffCityId', transfer.dropoffCityId)
    } else {
      reset()
    }
  }, [transfer, setValue, reset])

  const handleFormSubmit = async (data: TransferFormData) => {
    try {
      const submitData: any = {
        transferDate: data.transferDate.toISOString(),
        pickupAddress: data.pickupAddress,
        dropoffAddress: data.dropoffAddress,
        price: data.price,
        guestId: data.guestId,
        personnelId: data.personnelId,
        airportId: data.airportId,
        vehicleId: data.vehicleId,
        note: data.note || undefined,
        status: data.status,
        isFromAirport: data.isFromAirport,
        pickupCityId: data.pickupCityId,
        dropoffCityId: data.dropoffCityId,
      }

      // Sadece create modunda invoice bilgileri ekle
      if (!transfer && createInvoice) {
        submitData.createInvoice = true
        if (data.discountPercentage) submitData.discountPercentage = data.discountPercentage
        if (data.invoiceDescription) submitData.invoiceDescription = data.invoiceDescription
        if (data.currency) submitData.currency = data.currency
      }

      await onSubmit(submitData)
      reset()
      onClose()
    } catch (error) {
      console.error('Form submission error:', error)
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
          <DialogTitle>{transfer ? 'Transfer Düzenle' : 'Yeni Transfer Ekle'}</DialogTitle>
          <DialogContent>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
              <Grid container spacing={2}>
                <Grid item xs={12} md={6}>
                  <Controller
                    name="transferDate"
                    control={control}
                    render={({ field }) => (
                      <DateTimePicker
                        label="Transfer Tarihi"
                        value={field.value}
                        onChange={field.onChange}
                        slotProps={{
                          textField: {
                            fullWidth: true,
                            error: !!errors.transferDate,
                            helperText: errors.transferDate?.message,
                            disabled: isSubmitting || isLoading,
                          },
                        }}
                      />
                    )}
                  />
                </Grid>

                <Grid item xs={12} md={6}>
                  <TextField
                    label="Fiyat"
                    type="number"
                    fullWidth
                    required
                    {...register('price', { valueAsNumber: true })}
                    error={!!errors.price}
                    helperText={errors.price?.message}
                    disabled={isSubmitting || isLoading}
                    inputProps={{ step: '0.01', min: '0.01' }}
                  />
                </Grid>

                <Grid item xs={12} md={6}>
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
                  <FormControl fullWidth required error={!!errors.personnelId} disabled={isSubmitting || isLoading}>
                    <InputLabel>Personel</InputLabel>
                    <Controller
                      name="personnelId"
                      control={control}
                      render={({ field }) => (
                        <Select {...field} value={field.value || ''}>
                          <MenuItem value="">Seçiniz</MenuItem>
                          {personnel?.map((p) => (
                            <MenuItem key={p.id} value={p.id}>
                              {p.fullName}
                            </MenuItem>
                          ))}
                        </Select>
                      )}
                    />
                  </FormControl>
                  {errors.personnelId && (
                    <Typography variant="caption" color="error" sx={{ mt: 0.5 }}>
                      {errors.personnelId.message}
                    </Typography>
                  )}
                </Grid>

                <Grid item xs={12} md={6}>
                  <FormControl fullWidth required error={!!errors.airportId} disabled={isSubmitting || isLoading}>
                    <InputLabel>Havaalanı</InputLabel>
                    <Controller
                      name="airportId"
                      control={control}
                      render={({ field }) => (
                        <Select {...field} value={field.value || ''}>
                          <MenuItem value="">Seçiniz</MenuItem>
                          {airports?.map((airport) => (
                            <MenuItem key={airport.id} value={airport.id}>
                              {airport.airportName} {airport.cityName && `- ${airport.cityName}`}
                            </MenuItem>
                          ))}
                        </Select>
                      )}
                    />
                  </FormControl>
                  {errors.airportId && (
                    <Typography variant="caption" color="error" sx={{ mt: 0.5 }}>
                      {errors.airportId.message}
                    </Typography>
                  )}
                </Grid>

                <Grid item xs={12} md={6}>
                  <FormControl fullWidth required error={!!errors.vehicleId} disabled={isSubmitting || isLoading}>
                    <InputLabel>Araç</InputLabel>
                    <Controller
                      name="vehicleId"
                      control={control}
                      render={({ field }) => (
                        <Select {...field} value={field.value || ''}>
                          <MenuItem value="">Seçiniz</MenuItem>
                          {vehicles?.map((vehicle) => (
                            <MenuItem key={vehicle.id} value={vehicle.id}>
                              {vehicle.plateNumber} ({vehicle.vehicleType}
                              {vehicle.capacity && ` - ${vehicle.capacity} kişi`})
                            </MenuItem>
                          ))}
                        </Select>
                      )}
                    />
                  </FormControl>
                  {errors.vehicleId && (
                    <Typography variant="caption" color="error" sx={{ mt: 0.5 }}>
                      {errors.vehicleId.message}
                    </Typography>
                  )}
                </Grid>

                <Grid item xs={12} md={6}>
                  <FormControl fullWidth required error={!!errors.pickupCityId} disabled={isSubmitting || isLoading}>
                    <InputLabel>Alış Şehri</InputLabel>
                    <Controller
                      name="pickupCityId"
                      control={control}
                      render={({ field }) => (
                        <Select {...field} value={field.value || ''}>
                          <MenuItem value="">Seçiniz</MenuItem>
                          {cities?.map((city) => (
                            <MenuItem key={city.id} value={city.id}>
                              {city.cityName}
                            </MenuItem>
                          ))}
                        </Select>
                      )}
                    />
                  </FormControl>
                  {errors.pickupCityId && (
                    <Typography variant="caption" color="error" sx={{ mt: 0.5 }}>
                      {errors.pickupCityId.message}
                    </Typography>
                  )}
                </Grid>

                <Grid item xs={12} md={6}>
                  <FormControl fullWidth required error={!!errors.dropoffCityId} disabled={isSubmitting || isLoading}>
                    <InputLabel>Bırakış Şehri</InputLabel>
                    <Controller
                      name="dropoffCityId"
                      control={control}
                      render={({ field }) => (
                        <Select {...field} value={field.value || ''}>
                          <MenuItem value="">Seçiniz</MenuItem>
                          {cities?.map((city) => (
                            <MenuItem key={city.id} value={city.id}>
                              {city.cityName}
                            </MenuItem>
                          ))}
                        </Select>
                      )}
                    />
                  </FormControl>
                  {errors.dropoffCityId && (
                    <Typography variant="caption" color="error" sx={{ mt: 0.5 }}>
                      {errors.dropoffCityId.message}
                    </Typography>
                  )}
                </Grid>

                <Grid item xs={12}>
                  <TextField
                    label="Alış Adresi"
                    fullWidth
                    required
                    {...register('pickupAddress')}
                    error={!!errors.pickupAddress}
                    helperText={errors.pickupAddress?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12}>
                  <TextField
                    label="Bırakış Adresi"
                    fullWidth
                    required
                    {...register('dropoffAddress')}
                    error={!!errors.dropoffAddress}
                    helperText={errors.dropoffAddress?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12} md={6}>
                  <FormControl fullWidth required error={!!errors.status} disabled={isSubmitting || isLoading}>
                    <InputLabel>Durum</InputLabel>
                    <Controller
                      name="status"
                      control={control}
                      render={({ field }) => (
                        <Select {...field}>
                          <MenuItem value="Pending">Beklemede</MenuItem>
                          <MenuItem value="InProgress">Devam Ediyor</MenuItem>
                          <MenuItem value="Completed">Tamamlandı</MenuItem>
                          <MenuItem value="Cancelled">İptal Edildi</MenuItem>
                        </Select>
                      )}
                    />
                  </FormControl>
                  {errors.status && (
                    <Typography variant="caption" color="error" sx={{ mt: 0.5 }}>
                      {errors.status.message}
                    </Typography>
                  )}
                </Grid>

                <Grid item xs={12} md={6}>
                  <Controller
                    name="isFromAirport"
                    control={control}
                    render={({ field }) => (
                      <FormControlLabel
                        control={<Switch checked={field.value} onChange={field.onChange} disabled={isSubmitting || isLoading} />}
                        label="Havaalanından mı?"
                      />
                    )}
                  />
                </Grid>

                <Grid item xs={12}>
                  <TextField
                    label="Not"
                    fullWidth
                    multiline
                    rows={3}
                    {...register('note')}
                    error={!!errors.note}
                    helperText={errors.note?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                {!transfer && (
                  <>
                    <Grid item xs={12}>
                      <Controller
                        name="createInvoice"
                        control={control}
                        render={({ field }) => (
                          <FormControlLabel
                            control={<Switch checked={field.value} onChange={field.onChange} disabled={isSubmitting || isLoading} />}
                            label="Fatura Oluştur"
                          />
                        )}
                      />
                    </Grid>

                    {createInvoice && (
                      <>
                        <Grid item xs={12} md={6}>
                          <TextField
                            label="İndirim Yüzdesi"
                            type="number"
                            fullWidth
                            {...register('discountPercentage', { valueAsNumber: true })}
                            error={!!errors.discountPercentage}
                            helperText={errors.discountPercentage?.message}
                            disabled={isSubmitting || isLoading}
                            inputProps={{ step: '0.01', min: '0', max: '100' }}
                          />
                        </Grid>

                        <Grid item xs={12} md={6}>
                          <FormControl fullWidth disabled={isSubmitting || isLoading}>
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
                        </Grid>

                        <Grid item xs={12}>
                          <TextField
                            label="Fatura Açıklaması"
                            fullWidth
                            multiline
                            rows={2}
                            {...register('invoiceDescription')}
                            error={!!errors.invoiceDescription}
                            helperText={errors.invoiceDescription?.message}
                            disabled={isSubmitting || isLoading}
                          />
                        </Grid>
                      </>
                    )}
                  </>
                )}
              </Grid>
            </Box>
          </DialogContent>
          <DialogActions sx={{ px: 3, pb: 2 }}>
            <Button onClick={handleClose} disabled={isSubmitting || isLoading}>
              İptal
            </Button>
            <Button type="submit" variant="contained" disabled={isSubmitting || isLoading}>
              {isSubmitting || isLoading ? 'Kaydediliyor...' : transfer ? 'Güncelle' : 'Ekle'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </LocalizationProvider>
  )
}

export default TransferForm


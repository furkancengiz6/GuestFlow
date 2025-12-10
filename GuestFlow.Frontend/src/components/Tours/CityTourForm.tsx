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
import { CityTour, CreateCityTourRequest, UpdateCityTourRequest } from '../../services/tourService'

// Zod schema
const cityTourSchema = z.object({
  tourDate: z.date({ required_error: 'Tur tarihi gereklidir' }),
  language: z.string().min(1, 'Dil gereklidir').max(50, 'Dil en fazla 50 karakter olabilir'),
  durationHours: z.number().min(1, 'Süre en az 1 saat olmalıdır').max(24, 'Süre en fazla 24 saat olabilir'),
  price: z.number().min(0.01, 'Fiyat 0.01\'den büyük olmalıdır'),
  ownerGuestId: z.number().min(1, 'Misafir seçilmelidir'),
  personnelId: z.number().min(1, 'Personel seçilmelidir'),
  cityId: z.number().min(1, 'Şehir seçilmelidir'),
  createInvoice: z.boolean().optional(),
  discountPercentage: z.number().min(0).max(100).optional(),
  invoiceDescription: z.string().optional(),
  currency: z.string().optional(),
})

type CityTourFormData = z.infer<typeof cityTourSchema>

interface CityTourFormProps {
  open: boolean
  onClose: () => void
  onSubmit: (data: CreateCityTourRequest | UpdateCityTourRequest) => Promise<void>
  cityTour?: CityTour | null
  isLoading?: boolean
}

const CityTourForm = ({ open, onClose, onSubmit, cityTour, isLoading = false }: CityTourFormProps) => {
  const {
    register,
    handleSubmit,
    control,
    formState: { errors, isSubmitting },
    reset,
    setValue,
    watch,
  } = useForm<CityTourFormData>({
    resolver: zodResolver(cityTourSchema),
    defaultValues: {
      tourDate: new Date(),
      language: 'Türkçe',
      durationHours: 2,
      price: 0,
      ownerGuestId: 0,
      personnelId: 0,
      cityId: 0,
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

  const { data: cities } = useQuery({
    queryKey: ['cities-dropdown'],
    queryFn: () => dropdownService.getCities(),
    enabled: open,
  })

  const createInvoice = watch('createInvoice')

  // Form'u cityTour verisiyle doldur (düzenleme modu)
  useEffect(() => {
    if (cityTour) {
      setValue('tourDate', new Date(cityTour.tourDate))
      setValue('language', cityTour.language)
      setValue('durationHours', cityTour.durationHours)
      setValue('price', cityTour.price)
      setValue('ownerGuestId', cityTour.ownerGuestId)
      setValue('personnelId', cityTour.personnelId)
      setValue('cityId', cityTour.cityId)
    } else {
      reset()
    }
  }, [cityTour, setValue, reset])

  const handleFormSubmit = async (data: CityTourFormData) => {
    try {
      const submitData: any = {
        tourDate: data.tourDate.toISOString(),
        language: data.language,
        durationHours: data.durationHours,
        price: data.price,
        ownerGuestId: data.ownerGuestId,
        personnelId: data.personnelId,
        cityId: data.cityId,
      }

      // Sadece create modunda invoice bilgileri ekle
      if (!cityTour && createInvoice) {
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

  const languages = ['Türkçe', 'English', 'Deutsch', 'Français', 'Русский', 'العربية', '中文']

  return (
    <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={tr}>
      <Dialog open={open} onClose={handleClose} maxWidth="md" fullWidth>
        <form onSubmit={handleSubmit(handleFormSubmit)}>
          <DialogTitle>{cityTour ? 'Şehir Turu Düzenle' : 'Yeni Şehir Turu Ekle'}</DialogTitle>
          <DialogContent>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
              <Grid container spacing={2}>
                <Grid item xs={12} md={6}>
                  <Controller
                    name="tourDate"
                    control={control}
                    render={({ field }) => (
                      <DateTimePicker
                        label="Tur Tarihi"
                        value={field.value}
                        onChange={field.onChange}
                        slotProps={{
                          textField: {
                            fullWidth: true,
                            error: !!errors.tourDate,
                            helperText: errors.tourDate?.message,
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
                  <FormControl fullWidth required error={!!errors.ownerGuestId} disabled={isSubmitting || isLoading}>
                    <InputLabel>Misafir</InputLabel>
                    <Controller
                      name="ownerGuestId"
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
                  {errors.ownerGuestId && (
                    <Typography variant="caption" color="error" sx={{ mt: 0.5 }}>
                      {errors.ownerGuestId.message}
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
                  <FormControl fullWidth required error={!!errors.cityId} disabled={isSubmitting || isLoading}>
                    <InputLabel>Şehir</InputLabel>
                    <Controller
                      name="cityId"
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
                  {errors.cityId && (
                    <Typography variant="caption" color="error" sx={{ mt: 0.5 }}>
                      {errors.cityId.message}
                    </Typography>
                  )}
                </Grid>

                <Grid item xs={12} md={6}>
                  <FormControl fullWidth required error={!!errors.language} disabled={isSubmitting || isLoading}>
                    <InputLabel>Dil</InputLabel>
                    <Controller
                      name="language"
                      control={control}
                      render={({ field }) => (
                        <Select {...field}>
                          {languages.map((lang) => (
                            <MenuItem key={lang} value={lang}>
                              {lang}
                            </MenuItem>
                          ))}
                        </Select>
                      )}
                    />
                  </FormControl>
                  {errors.language && (
                    <Typography variant="caption" color="error" sx={{ mt: 0.5 }}>
                      {errors.language.message}
                    </Typography>
                  )}
                </Grid>

                <Grid item xs={12} md={6}>
                  <TextField
                    label="Süre (Saat)"
                    type="number"
                    fullWidth
                    required
                    {...register('durationHours', { valueAsNumber: true })}
                    error={!!errors.durationHours}
                    helperText={errors.durationHours?.message}
                    disabled={isSubmitting || isLoading}
                    inputProps={{ step: '1', min: '1', max: '24' }}
                  />
                </Grid>

                {!cityTour && (
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
              {isSubmitting || isLoading ? 'Kaydediliyor...' : cityTour ? 'Güncelle' : 'Ekle'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </LocalizationProvider>
  )
}

export default CityTourForm


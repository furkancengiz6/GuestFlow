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

const optionalId = z.preprocess(
  (val) => (val === '' || val === null || val === undefined ? undefined : Number(val)),
  z.number().optional()
)

// Zod schema
const cityTourSchema = z.object({
  tourDate: z.date({ required_error: 'Tur tarihi gereklidir' } as any),
  language: z.string().min(1, 'Dil gereklidir').max(50, 'Dil en fazla 50 karakter olabilir'),
  durationHours: z.number().min(1, 'Süre en az 1 saat olmalıdır').max(24, 'Süre en fazla 24 saat olabilir'),
  price: z.number().min(0.01, 'Fiyat 0.01\'den büyük olmalıdır'),

  // Group composition fields
  adultCount: z.preprocess(
    (val) => (val === '' || val === null || val === undefined ? undefined : Number(val)),
    z.number().min(0, 'Yetişkin sayısı 0 veya daha büyük olmalıdır').optional()
  ),
  childCount: z.preprocess(
    (val) => (val === '' || val === null || val === undefined ? undefined : Number(val)),
    z.number().min(0, 'Çocuk sayısı 0 veya daha büyük olmalıdır').optional()
  ),
  infantCount: z.preprocess(
    (val) => (val === '' || val === null || val === undefined ? undefined : Number(val)),
    z.number().min(0, 'Bebek sayısı 0 veya daha büyük olmalıdır').optional()
  ),
  ownerGuestId: z.number().min(1, 'Misafir seçilmelidir'),
  personnelId: optionalId,
  cityId: z.number().min(1, 'Şehir seçilmelidir'),
  tourId: z.number().min(1, 'Tur seçilmelidir'),
  createInvoice: z.boolean().optional(),
  discountPercentage: z.preprocess(
    (val) => (val === '' || val === null || val === undefined ? undefined : Number(val)),
    z.number().min(0).max(100).optional()
  ),
  invoiceDescription: z.string().optional(),
  currency: z.string().optional(),
  vehicleId: optionalId,
  driverName: z.string().optional(),
  guideName: z.string().optional(),
  guidePhone: z.string().optional(),
  externalVehiclePlate: z.string().optional(),
  externalDriverName: z.string().optional(),
  externalDriverPhone: z.string().optional(),
  startTime: z.string().optional(),
  endTime: z.string().optional(),

  // Safety & emergency fields
  groupLeaderName: z.string().max(100, 'Grup lideri adı en fazla 100 karakter olabilir').optional(),
  groupLeaderPhone: z.string().max(20, 'Grup lideri telefonu en fazla 20 karakter olabilir').optional(),
  emergencyContactName: z.string().max(100, 'Acil iletişim adı en fazla 100 karakter olabilir').optional(),
  emergencyContactPhone: z.string().max(20, 'Acil iletişim telefonu en fazla 20 karakter olabilir').optional(),

  // Coordination fields
  meetingPersonName: z.string().max(100, 'Buluşma kişisi adı en fazla 100 karakter olabilir').optional(),
  meetingPointDetails: z.string().max(500, 'Buluşma noktası detayları en fazla 500 karakter olabilir').optional(),

  // Guide fields
  guideLanguages: z.string().max(200, 'Rehber dilleri en fazla 200 karakter olabilir').optional(),
  backupGuideName: z.string().max(100, 'Yedek rehber adı en fazla 100 karakter olabilir').optional(),
  backupGuidePhone: z.string().max(20, 'Yedek rehber telefonu en fazla 20 karakter olabilir').optional(),

  // Operational details
  tourDifficultyLevel: z.string().max(50, 'Tur zorluk seviyesi en fazla 50 karakter olabilir').optional(),
  weatherDependent: z.boolean().optional(),
  minimumParticipantCount: z.preprocess(
    (val) => (val === '' || val === null || val === undefined ? undefined : Number(val)),
    z.number().min(1, 'Minimum katılımcı sayısı en az 1 olmalıdır').optional()
  ),
  maximumParticipantCount: z.preprocess(
    (val) => (val === '' || val === null || val === undefined ? undefined : Number(val)),
    z.number().min(1, 'Maksimum katılımcı sayısı en az 1 olmalıdır').optional()
  ),

  // Guest experience fields
  dietaryRequirements: z.string().max(500, 'Beslenme ihtiyaçları en fazla 500 karakter olabilir').optional(),
  accessibilityNeeds: z.string().max(500, 'Erişilebilirlik ihtiyaçları en fazla 500 karakter olabilir').optional(),
  photographyAllowed: z.boolean().optional(),

  captainPhone: z.string().optional(),

  // Internal coordination fields
  conciergeInternalNotes: z.string().max(1000, 'İç notlar en fazla 1000 karakter olabilir').optional(),
  // isPaymentReceived removed - payment status is calculated from PaymentEntity
  paymentNote: z.string().optional(),
  supplierName: z.string().optional(),
  supplierCost: z.preprocess(
    (val) => (val === '' || val === null || val === undefined ? undefined : Number(val)),
    z.number().optional()
  ),
  supplierCurrency: z.string().optional(),
  supplierPaymentStatus: z.string().optional(),
  supplierPaymentDate: z.string().optional(),
  supplierInvoiceNumber: z.string().optional(),
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
    resolver: zodResolver(cityTourSchema) as any,
    defaultValues: {
      tourDate: new Date(),
      language: 'Türkçe',
      durationHours: 2,
      price: 0,

      // Group composition fields
      adultCount: undefined,
      childCount: undefined,
      infantCount: undefined,

      ownerGuestId: 0,
      personnelId: undefined,
      cityId: 0,
      tourId: 0,
      createInvoice: false,
      discountPercentage: 0,
      invoiceDescription: '',
      currency: 'TRY',
      vehicleId: undefined,
      driverName: '',
      guideName: '',
      guidePhone: '',
      // Guide fields
      guideLanguages: '',
      backupGuideName: '',
      backupGuidePhone: '',
      externalVehiclePlate: '',
      externalDriverName: '',
      externalDriverPhone: '',
      startTime: '',
      endTime: '',

      // Safety & emergency fields
      groupLeaderName: '',
      groupLeaderPhone: '',
      emergencyContactName: '',
      emergencyContactPhone: '',

      // Coordination fields
      meetingPersonName: '',
      meetingPointDetails: '',

      // Operational details
      tourDifficultyLevel: '',
      weatherDependent: false,
      minimumParticipantCount: undefined,
      maximumParticipantCount: undefined,

      // Guest experience fields
      dietaryRequirements: '',
      accessibilityNeeds: '',
      photographyAllowed: false,

      captainPhone: '',
      // isPaymentReceived removed - payment status is calculated from PaymentEntity
      paymentNote: '',
      supplierName: '',
      supplierCost: undefined,
      supplierCurrency: '',
      supplierPaymentStatus: '',
      supplierPaymentDate: '',
      supplierInvoiceNumber: '',

      // Internal coordination fields
      conciergeInternalNotes: '',
    },
  })

  // Fetch dropdown data
  const { data: guests } = useQuery({
    queryKey: ['guests-dropdown'],
    queryFn: () => dropdownService.getGuests(),
    enabled: open,
  })

  // Personnel is automatically assigned from logged-in user, no need for dropdown

  const { data: cities } = useQuery({
    queryKey: ['cities-dropdown'],
    queryFn: () => dropdownService.getCities(),
    enabled: open,
  })

  const { data: vehicles } = useQuery({
    queryKey: ['vehicles-dropdown'],
    queryFn: () => dropdownService.getVehicles(),
    enabled: open,
  })

  const selectedCityId = watch('cityId')
  const selectedTourId = watch('tourId')

  const { data: tours } = useQuery({
    queryKey: ['tours-dropdown', selectedCityId],
    queryFn: () => dropdownService.getTours(selectedCityId > 0 ? selectedCityId : undefined),
    enabled: open && !!selectedCityId && selectedCityId > 0,
  })

  // Şehir değişince gelen tur listesinden ilkini otomatik seç
  useEffect(() => {
    if (tours && tours.length > 0) {
      // Eğer mevcut seçim yoksa veya liste bu şehre ait değilse ilk turu seç
      const hasCurrent = tours.some((t) => t.id === selectedTourId)
      if (!hasCurrent) {
        setValue('tourId', tours[0].id)
      }
    } else if (selectedTourId) {
      // Seçili tur, şehir değiştiği için geçersiz olabilir
      setValue('tourId', 0)
    }
  }, [tours, selectedTourId, setValue])

  const createInvoice = watch('createInvoice')

  // Form'u cityTour verisiyle doldur (düzenleme modu)
  useEffect(() => {
    if (cityTour) {
      setValue('tourDate', new Date(cityTour.tourDate))
      setValue('language', cityTour.language)
      setValue('durationHours', cityTour.durationHours)
      setValue('price', cityTour.price)
      setValue('ownerGuestId', cityTour.ownerGuestId)
      setValue('personnelId', cityTour.personnelId || undefined)
      setValue('cityId', cityTour.cityId)
      setValue('tourId', cityTour.tourId || 0)
      setValue('vehicleId', cityTour.vehicleId || undefined)
      setValue('driverName', cityTour.driverName || '')
      setValue('guideName', cityTour.guideName || '')
      setValue('guidePhone', cityTour.guidePhone || '')
      setValue('externalVehiclePlate', cityTour.externalVehiclePlate || '')
      setValue('externalDriverName', cityTour.externalDriverName || '')
      setValue('externalDriverPhone', cityTour.externalDriverPhone || '')
      setValue('startTime', cityTour.startTime || '')
      setValue('endTime', cityTour.endTime || '')
      setValue('captainPhone', (cityTour as any).captainPhone || '')
      // isPaymentReceived removed - payment status is calculated from PaymentEntity
      setValue('paymentNote', (cityTour as any).paymentNote || '')
      setValue('supplierName', (cityTour as any).supplierName || '')
      setValue('supplierCost', (cityTour as any).supplierCost || undefined)
      setValue('supplierCurrency', (cityTour as any).supplierCurrency || '')
      setValue('supplierPaymentStatus', (cityTour as any).supplierPaymentStatus || '')
      setValue('supplierPaymentDate', (cityTour as any).supplierPaymentDate || '')
      setValue('supplierInvoiceNumber', (cityTour as any).supplierInvoiceNumber || '')

      // Group composition fields
      setValue('adultCount', (cityTour as any).adultCount || null)
      setValue('childCount', (cityTour as any).childCount || null)
      setValue('infantCount', (cityTour as any).infantCount || null)

      // Safety & emergency fields
      setValue('groupLeaderName', (cityTour as any).groupLeaderName || '')
      setValue('groupLeaderPhone', (cityTour as any).groupLeaderPhone || '')
      setValue('emergencyContactName', (cityTour as any).emergencyContactName || '')
      setValue('emergencyContactPhone', (cityTour as any).emergencyContactPhone || '')

      // Coordination fields
      setValue('meetingPersonName', (cityTour as any).meetingPersonName || '')
      setValue('meetingPointDetails', (cityTour as any).meetingPointDetails || '')

      // Guide fields
      setValue('guideLanguages', (cityTour as any).guideLanguages || '')
      setValue('backupGuideName', (cityTour as any).backupGuideName || '')
      setValue('backupGuidePhone', (cityTour as any).backupGuidePhone || '')

      // Operational details
      setValue('tourDifficultyLevel', (cityTour as any).tourDifficultyLevel || '')
      setValue('weatherDependent', (cityTour as any).weatherDependent || false)
      setValue('minimumParticipantCount', (cityTour as any).minimumParticipantCount || null)
      setValue('maximumParticipantCount', (cityTour as any).maximumParticipantCount || null)

      // Guest experience fields
      setValue('dietaryRequirements', (cityTour as any).dietaryRequirements || '')
      setValue('accessibilityNeeds', (cityTour as any).accessibilityNeeds || '')
      setValue('photographyAllowed', (cityTour as any).photographyAllowed || false)

      // Internal coordination fields
      setValue('conciergeInternalNotes', (cityTour as any).conciergeInternalNotes || '')
    } else {
      reset()
    }
  }, [cityTour, setValue, reset])

  const handleFormSubmit = async (data: any) => {
    try {
      const submitData: any = {
        tourDate: data.tourDate.toISOString(),
        language: data.language,
        durationHours: data.durationHours,
        price: data.price,
        ownerGuestId: data.ownerGuestId,
      personnelId: data.personnelId,
        cityId: data.cityId,
      tourId: data.tourId,
      vehicleId: data.vehicleId,
      driverName: data.driverName || undefined,
      guideName: data.guideName || undefined,
      guidePhone: data.guidePhone || undefined,
      externalVehiclePlate: data.externalVehiclePlate || undefined,
      externalDriverName: data.externalDriverName || undefined,
      externalDriverPhone: data.externalDriverPhone || undefined,
      startTime: data.startTime || undefined,
      endTime: data.endTime || undefined,
      captainPhone: data.captainPhone || undefined,
      // isPaymentReceived removed - payment status is calculated from PaymentEntity
      paymentNote: data.paymentNote || undefined,
      supplierName: data.supplierName || undefined,
      supplierCost: data.supplierCost ?? undefined,
      supplierCurrency: data.supplierCurrency || undefined,
      supplierPaymentStatus: data.supplierPaymentStatus || undefined,
      supplierPaymentDate: data.supplierPaymentDate || undefined,
      supplierInvoiceNumber: data.supplierInvoiceNumber || undefined,

      // Group composition fields
      adultCount: data.adultCount && data.adultCount > 0 ? data.adultCount : undefined,
      childCount: data.childCount && data.childCount > 0 ? data.childCount : undefined,
      infantCount: data.infantCount && data.infantCount > 0 ? data.infantCount : undefined,

      // Safety & emergency fields
      groupLeaderName: data.groupLeaderName || undefined,
      groupLeaderPhone: data.groupLeaderPhone || undefined,
      emergencyContactName: data.emergencyContactName || undefined,
      emergencyContactPhone: data.emergencyContactPhone || undefined,

      // Coordination fields
      meetingPersonName: data.meetingPersonName || undefined,
      meetingPointDetails: data.meetingPointDetails || undefined,

      // Guide fields
      guideLanguages: data.guideLanguages || undefined,
      backupGuideName: data.backupGuideName || undefined,
      backupGuidePhone: data.backupGuidePhone || undefined,

      // Operational details
      tourDifficultyLevel: data.tourDifficultyLevel || undefined,
      weatherDependent: data.weatherDependent || undefined,
      minimumParticipantCount: data.minimumParticipantCount && data.minimumParticipantCount > 0 ? data.minimumParticipantCount : undefined,
      maximumParticipantCount: data.maximumParticipantCount && data.maximumParticipantCount > 0 ? data.maximumParticipantCount : undefined,

      // Guest experience fields
      dietaryRequirements: data.dietaryRequirements || undefined,
      accessibilityNeeds: data.accessibilityNeeds || undefined,
      photographyAllowed: data.photographyAllowed || undefined,

      // Internal coordination fields
      conciergeInternalNotes: data.conciergeInternalNotes || undefined,
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

                {/* Personnel is automatically assigned from logged-in user, no dropdown needed */}

                <Grid item xs={12} md={6}>
                  <FormControl fullWidth required error={!!errors.tourId} disabled={isSubmitting || isLoading || !tours}>
                    <InputLabel>Tur</InputLabel>
                    <Controller
                      name="tourId"
                      control={control}
                      render={({ field }) => (
                        <Select {...field} value={field.value || ''}>
                          <MenuItem value="">Seçiniz</MenuItem>
                          {tours?.map((tour) => (
                            <MenuItem key={tour.id} value={tour.id}>
                              {tour.name}
                            </MenuItem>
                          ))}
                        </Select>
                      )}
                    />
                  </FormControl>
                  {errors.tourId && (
                    <Typography variant="caption" color="error" sx={{ mt: 0.5 }}>
                      {errors.tourId.message}
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

                <Grid item xs={12} md={6}>
                  <FormControl fullWidth error={!!errors.vehicleId} disabled={isSubmitting || isLoading}>
                    <InputLabel>Araç (Opsiyonel)</InputLabel>
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
                  <TextField
                    label="Şoför Adı (Opsiyonel)"
                    fullWidth
                    value={watch('driverName') || ''} onChange={(e) => setValue('driverName', e.target.value)}
                    error={!!errors.driverName}
                    helperText={errors.driverName?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12} md={6}>
                  <TextField
                    label="Rehber Adı (Opsiyonel)"
                    fullWidth
                    value={watch('guideName') || ''} onChange={(e) => setValue('guideName', e.target.value)}
                    error={!!errors.guideName}
                    helperText={errors.guideName?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12} md={6}>
                  <TextField
                    label="Rehber Telefonu (Opsiyonel)"
                    fullWidth
                    value={watch('guidePhone') || ''} onChange={(e) => setValue('guidePhone', e.target.value)}
                    error={!!errors.guidePhone}
                    helperText={errors.guidePhone?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12} md={6}>
                  <TextField
                    label="Kaptan Telefonu (Opsiyonel)"
                    fullWidth
                    value={watch('captainPhone') || ''} onChange={(e) => setValue('captainPhone', e.target.value)}
                    error={!!errors.captainPhone}
                    helperText={errors.captainPhone?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12} md={6}>
                  <TextField
                    label="Dış Araç Plakası (Opsiyonel)"
                    fullWidth
                    value={watch('externalVehiclePlate') || ''} onChange={(e) => setValue('externalVehiclePlate', e.target.value)}
                    error={!!errors.externalVehiclePlate}
                    helperText={errors.externalVehiclePlate?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12} md={6}>
                  <TextField
                    label="Dış Şoför Adı (Opsiyonel)"
                    fullWidth
                    value={watch('externalDriverName') || ''} onChange={(e) => setValue('externalDriverName', e.target.value)}
                    error={!!errors.externalDriverName}
                    helperText={errors.externalDriverName?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12} md={6}>
                  <TextField
                    label="Dış Şoför Tel (Opsiyonel)"
                    fullWidth
                    value={watch('externalDriverPhone') || ''} onChange={(e) => setValue('externalDriverPhone', e.target.value)}
                    error={!!errors.externalDriverPhone}
                    helperText={errors.externalDriverPhone?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12} md={6}>
                  <TextField
                    label="Başlangıç Saati (Opsiyonel, HH:mm)"
                    fullWidth
                    value={watch('startTime') || ''} onChange={(e) => setValue('startTime', e.target.value)}
                    error={!!errors.startTime}
                    helperText={errors.startTime?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12} md={6}>
                  <TextField
                    label="Bitiş Saati (Opsiyonel, HH:mm)"
                    fullWidth
                    value={watch('endTime') || ''} onChange={(e) => setValue('endTime', e.target.value)}
                    error={!!errors.endTime}
                    helperText={errors.endTime?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                {/* Tedarikçi Bilgileri */}
                <Grid item xs={12}>
                  <Typography variant="subtitle1" sx={{ mt: 2 }}>
                    Tedarikçi Bilgileri (Opsiyonel)
                  </Typography>
                </Grid>

                <Grid item xs={12} md={6}>
                  <TextField
                    label="Tedarikçi Adı"
                    fullWidth
                    value={watch('supplierName') || ''} onChange={(e) => setValue('supplierName', e.target.value)}
                    error={!!errors.supplierName}
                    helperText={errors.supplierName?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12} md={3}>
                  <TextField
                    label="Tedarikçi Maliyeti"
                    type="number"
                    fullWidth
                    {...register('supplierCost', { valueAsNumber: true })}
                    error={!!errors.supplierCost}
                    helperText={errors.supplierCost?.message}
                    disabled={isSubmitting || isLoading}
                    inputProps={{ step: '0.01', min: '0' }}
                  />
                </Grid>

                <Grid item xs={12} md={3}>
                  <TextField
                    label="Tedarikçi Para Birimi"
                    fullWidth
                    value={watch('supplierCurrency') || ''} onChange={(e) => setValue('supplierCurrency', e.target.value)}
                    error={!!errors.supplierCurrency}
                    helperText={errors.supplierCurrency?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12} md={4}>
                  <TextField
                    label="Tedarikçi Ödeme Durumu"
                    fullWidth
                    value={watch('supplierPaymentStatus') || ''} onChange={(e) => setValue('supplierPaymentStatus', e.target.value)}
                    error={!!errors.supplierPaymentStatus}
                    helperText={errors.supplierPaymentStatus?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12} md={4}>
                  <TextField
                    label="Tedarikçi Ödeme Tarihi"
                    type="date"
                    fullWidth
                    InputLabelProps={{ shrink: true }}
                    value={watch('supplierPaymentDate') || ''} onChange={(e) => setValue('supplierPaymentDate', e.target.value)}
                    error={!!errors.supplierPaymentDate}
                    helperText={errors.supplierPaymentDate?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12} md={4}>
                  <TextField
                    label="Tedarikçi Fatura No"
                    fullWidth
                    value={watch('supplierInvoiceNumber') || ''} onChange={(e) => setValue('supplierInvoiceNumber', e.target.value)}
                    error={!!errors.supplierInvoiceNumber}
                    helperText={errors.supplierInvoiceNumber?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                {/* isPaymentReceived toggle removed - payment status is calculated from PaymentEntity */}

                <Grid item xs={12}>
                  <TextField
                    label="Ödeme Notu (Opsiyonel)"
                    fullWidth
                    multiline
                    minRows={2}
                    value={watch('paymentNote') || ''} onChange={(e) => setValue('paymentNote', e.target.value)}
                    error={!!errors.paymentNote}
                    helperText={errors.paymentNote?.message}
                    disabled={isSubmitting || isLoading}
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
                            value={watch('invoiceDescription') || ''} onChange={(e) => setValue('invoiceDescription', e.target.value)}
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



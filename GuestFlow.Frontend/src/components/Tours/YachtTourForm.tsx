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
import { YachtTour, CreateYachtTourRequest, UpdateYachtTourRequest } from '../../services/tourService'
import { TourCategory } from '../../types/enums'

const optionalId = z.preprocess(
  (val) => (val === '' || val === null || val === undefined ? undefined : Number(val)),
  z.number().optional()
)

// Zod schema
const yachtTourSchema = z.object({
  tourDate: z.date({ required_error: 'Tur tarihi gereklidir' } as any),
  numberOfPeople: z.number().min(1, 'Kişi sayısı en az 1 olmalıdır'),

  // Group composition fields
  childCount: z.preprocess(
    (val) => (val === '' || val === null || val === undefined ? undefined : Number(val)),
    z.number().min(0, 'Çocuk sayısı 0 veya daha büyük olmalıdır').optional()
  ),
  infantCount: z.preprocess(
    (val) => (val === '' || val === null || val === undefined ? undefined : Number(val)),
    z.number().min(0, 'Bebek sayısı 0 veya daha büyük olmalıdır').optional()
  ),

  price: z.number().min(0.01, 'Fiyat 0.01\'den büyük olmalıdır'),
  specialRequest: z.string().optional(),
  yachtName: z.string().min(1, 'Yat adı gereklidir').max(100, 'Yat adı en fazla 100 karakter olabilir'),

  // Group coordination fields
  groupLeaderName: z.string().max(100, 'Grup lideri adı en fazla 100 karakter olabilir').optional(),
  groupLeaderPhone: z.string().max(20, 'Grup lideri telefonu en fazla 20 karakter olabilir').optional(),

  ownerGuestId: z.number().min(1, 'Misafir seçilmelidir'),
  personnelId: optionalId,
  cityId: z.number().min(1, 'Şehir seçilmelidir'),
  createInvoice: z.boolean().optional(),
  discountPercentage: z.preprocess(
    (val) => (val === '' || val === null || val === undefined ? undefined : Number(val)),
    z.number().min(0).max(100).optional()
  ),
  invoiceDescription: z.string().optional(),
  currency: z.string().optional(),
  pickupPier: z.string().optional(),
  dropoffPier: z.string().optional(),
  pierAddress: z.string().optional(),
  startTime: z.string().optional(),
  endTime: z.string().optional(),
  tourCategory: z.nativeEnum(TourCategory).optional(),

  // Safety & regulatory fields
  lifeJacketsProvided: z.boolean().optional(),
  lifeJacketCount: z.preprocess(
    (val) => (val === '' || val === null || val === undefined ? undefined : Number(val)),
    z.number().min(1, 'Can yeleği sayısı en az 1 olmalıdır').optional()
  ),
  safetyEquipmentCheck: z.boolean().optional(),
  emergencyEquipment: z.string().max(500, 'Acil durum malzemeleri en fazla 500 karakter olabilir').optional(),

  // Capacity & compliance fields
  yachtCapacity: z.preprocess(
    (val) => (val === '' || val === null || val === undefined ? undefined : Number(val)),
    z.number().min(1, 'Yat kapasitesi en az 1 olmalıdır').optional()
  ),
  yachtType: z.string().max(50, 'Yat tipi en fazla 50 karakter olabilir').optional(),
  yachtLicenceRequired: z.boolean().optional(),
  coastGuardApproved: z.boolean().optional(),

  // Operational details
  crewSize: z.preprocess(
    (val) => (val === '' || val === null || val === undefined ? undefined : Number(val)),
    z.number().min(1, 'Mürettebat sayısı en az 1 olmalıdır').optional()
  ),
  captainExperience: z.string().max(100, 'Kaptan deneyimi en fazla 100 karakter olabilir').optional(),
  fuelRange: z.preprocess(
    (val) => (val === '' || val === null || val === undefined ? undefined : Number(val)),
    z.number().min(1, 'Yakıt menzili en az 1 olmalıdır').optional()
  ),
  weatherBackupPlan: z.string().max(500, 'Hava durumu yedek planı en fazla 500 karakter olabilir').optional(),

  captainPhone: z.string().optional(),
  // isPaymentReceived removed - payment status is calculated from PaymentEntity
  paymentNote: z.string().optional(),

  // Guest safety fields
  swimmingProficiency: z.string().max(100, 'Yüzme yeterliliği en fazla 100 karakter olabilir').optional(),
  medicalConditions: z.string().max(500, 'Tıbbi durumlar en fazla 500 karakter olabilir').optional(),
  alcoholPolicy: z.string().max(200, 'Alkol politikası en fazla 200 karakter olabilir').optional(),

  // Amenities & experience fields
  foodBeverageIncluded: z.boolean().optional(),
  beverageType: z.string().max(200, 'İçecek tipi en fazla 200 karakter olabilir').optional(),
  musicSystem: z.boolean().optional(),
  waterSportsEquipment: z.string().max(300, 'Su sporu malzemeleri en fazla 300 karakter olabilir').optional(),

  // Coordination fields
  marinaContactName: z.string().max(100, 'Marina iletişim adı en fazla 100 karakter olabilir').optional(),
  marinaContactPhone: z.string().max(20, 'Marina iletişim telefonu en fazla 20 karakter olabilir').optional(),

  // Internal coordination fields
  conciergeInternalNotes: z.string().max(1000, 'İç notlar en fazla 1000 karakter olabilir').optional(),

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

type YachtTourFormData = z.infer<typeof yachtTourSchema>

interface YachtTourFormProps {
  open: boolean
  onClose: () => void
  onSubmit: (data: CreateYachtTourRequest | UpdateYachtTourRequest) => Promise<void>
  yachtTour?: YachtTour | null
  isLoading?: boolean
}

const YachtTourForm = ({ open, onClose, onSubmit, yachtTour, isLoading = false }: YachtTourFormProps) => {
  const {
    register,
    handleSubmit,
    control,
    formState: { errors, isSubmitting },
    reset,
    setValue,
    watch,
  } = useForm<YachtTourFormData>({
    resolver: zodResolver(yachtTourSchema) as any,
    defaultValues: {
      tourDate: new Date(),
      numberOfPeople: 1,

      // Group composition fields
      childCount: undefined,
      infantCount: undefined,

      price: 0,
      specialRequest: '',
      yachtName: '',

      // Group coordination fields
      groupLeaderName: '',
      groupLeaderPhone: '',

      ownerGuestId: 0,
      personnelId: undefined,
      cityId: 0,
      createInvoice: false,
      discountPercentage: 0,
      invoiceDescription: '',
      currency: 'TRY',
      pickupPier: '',
      dropoffPier: '',
      pierAddress: '',
      startTime: '',
      endTime: '',
      tourCategory: TourCategory.Daily,

      // Safety & regulatory fields
      lifeJacketsProvided: false,
      lifeJacketCount: undefined,
      safetyEquipmentCheck: false,
      emergencyEquipment: '',

      // Capacity & compliance fields
      yachtCapacity: undefined,
      yachtType: '',
      yachtLicenceRequired: false,
      coastGuardApproved: false,

      // Operational details
      crewSize: undefined,
      captainExperience: '',
      fuelRange: undefined,
      weatherBackupPlan: '',

      captainPhone: '',
      // isPaymentReceived removed - payment status is calculated from PaymentEntity
      paymentNote: '',

      // Guest safety fields
      swimmingProficiency: '',
      medicalConditions: '',
      alcoholPolicy: '',

      // Amenities & experience fields
      foodBeverageIncluded: false,
      beverageType: '',
      musicSystem: false,
      waterSportsEquipment: '',

      // Coordination fields
      marinaContactName: '',
      marinaContactPhone: '',

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

  const createInvoice = watch('createInvoice')

  // Form'u yachtTour verisiyle doldur (düzenleme modu)
  useEffect(() => {
    if (yachtTour) {
      setValue('tourDate', new Date(yachtTour.tourDate))
      setValue('numberOfPeople', yachtTour.numberOfPeople)
      setValue('price', yachtTour.price)
      setValue('specialRequest', yachtTour.specialRequest || '')
      setValue('yachtName', (yachtTour as any).yachtName || '')
      setValue('ownerGuestId', yachtTour.ownerGuestId)
      setValue('personnelId', yachtTour.personnelId || undefined)
      setValue('cityId', yachtTour.cityId)
      setValue('pickupPier', yachtTour.pickupPier || '')
      setValue('dropoffPier', yachtTour.dropoffPier || '')
      setValue('pierAddress', (yachtTour as any).pierAddress || '')
      setValue('startTime', yachtTour.startTime || '')
      setValue('endTime', yachtTour.endTime || '')
      setValue('tourCategory', (yachtTour as any).tourCategory || TourCategory.Daily)
      setValue('captainPhone', (yachtTour as any).captainPhone || '')
      // isPaymentReceived removed - payment status is calculated from PaymentEntity
      setValue('paymentNote', (yachtTour as any).paymentNote || '')
      setValue('supplierName', (yachtTour as any).supplierName || '')
      setValue('supplierCost', (yachtTour as any).supplierCost || undefined)
      setValue('supplierCurrency', (yachtTour as any).supplierCurrency || '')
      setValue('supplierPaymentStatus', (yachtTour as any).supplierPaymentStatus || '')
      setValue('supplierPaymentDate', (yachtTour as any).supplierPaymentDate || '')
      setValue('supplierInvoiceNumber', (yachtTour as any).supplierInvoiceNumber || '')

      // Group composition fields
      setValue('childCount', (yachtTour as any).childCount || null)
      setValue('infantCount', (yachtTour as any).infantCount || null)

      // Group coordination fields
      setValue('groupLeaderName', (yachtTour as any).groupLeaderName || '')
      setValue('groupLeaderPhone', (yachtTour as any).groupLeaderPhone || '')

      // Safety & regulatory fields
      setValue('lifeJacketsProvided', (yachtTour as any).lifeJacketsProvided || false)
      setValue('lifeJacketCount', (yachtTour as any).lifeJacketCount || null)
      setValue('safetyEquipmentCheck', (yachtTour as any).safetyEquipmentCheck || false)
      setValue('emergencyEquipment', (yachtTour as any).emergencyEquipment || '')

      // Capacity & compliance fields
      setValue('yachtCapacity', (yachtTour as any).yachtCapacity || null)
      setValue('yachtType', (yachtTour as any).yachtType || '')
      setValue('yachtLicenceRequired', (yachtTour as any).yachtLicenceRequired || false)
      setValue('coastGuardApproved', (yachtTour as any).coastGuardApproved || false)

      // Operational details
      setValue('crewSize', (yachtTour as any).crewSize || null)
      setValue('captainExperience', (yachtTour as any).captainExperience || '')
      setValue('fuelRange', (yachtTour as any).fuelRange || null)
      setValue('weatherBackupPlan', (yachtTour as any).weatherBackupPlan || '')

      // Guest safety fields
      setValue('swimmingProficiency', (yachtTour as any).swimmingProficiency || '')
      setValue('medicalConditions', (yachtTour as any).medicalConditions || '')
      setValue('alcoholPolicy', (yachtTour as any).alcoholPolicy || '')

      // Amenities & experience fields
      setValue('foodBeverageIncluded', (yachtTour as any).foodBeverageIncluded || false)
      setValue('beverageType', (yachtTour as any).beverageType || '')
      setValue('musicSystem', (yachtTour as any).musicSystem || false)
      setValue('waterSportsEquipment', (yachtTour as any).waterSportsEquipment || '')

      // Coordination fields
      setValue('marinaContactName', (yachtTour as any).marinaContactName || '')
      setValue('marinaContactPhone', (yachtTour as any).marinaContactPhone || '')

      // Internal coordination fields
      setValue('conciergeInternalNotes', (yachtTour as any).conciergeInternalNotes || '')
    } else {
      reset()
    }
  }, [yachtTour, setValue, reset])

  const handleFormSubmit = async (data: any) => {
    try {
      const submitData: any = {
        tourDate: data.tourDate.toISOString(),
        numberOfPeople: data.numberOfPeople,
        price: data.price,
        yachtName: data.yachtName,
        ownerGuestId: data.ownerGuestId,
        personnelId: data.personnelId,
        cityId: data.cityId,
        pickupPier: data.pickupPier || undefined,
        dropoffPier: data.dropoffPier || undefined,
        pierAddress: data.pierAddress || undefined,
        startTime: data.startTime || undefined,
        endTime: data.endTime || undefined,
        tourCategory: data.tourCategory,
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
        childCount: data.childCount && data.childCount > 0 ? data.childCount : undefined,
        infantCount: data.infantCount && data.infantCount > 0 ? data.infantCount : undefined,

        // Group coordination fields
        groupLeaderName: data.groupLeaderName || undefined,
        groupLeaderPhone: data.groupLeaderPhone || undefined,

        // Safety & regulatory fields
        lifeJacketsProvided: data.lifeJacketsProvided || undefined,
        lifeJacketCount: data.lifeJacketCount && data.lifeJacketCount > 0 ? data.lifeJacketCount : undefined,
        safetyEquipmentCheck: data.safetyEquipmentCheck || undefined,
        emergencyEquipment: data.emergencyEquipment || undefined,

        // Capacity & compliance fields
        yachtCapacity: data.yachtCapacity && data.yachtCapacity > 0 ? data.yachtCapacity : undefined,
        yachtType: data.yachtType || undefined,
        yachtLicenceRequired: data.yachtLicenceRequired || undefined,
        coastGuardApproved: data.coastGuardApproved || undefined,

        // Operational details
        crewSize: data.crewSize && data.crewSize > 0 ? data.crewSize : undefined,
        captainExperience: data.captainExperience || undefined,
        fuelRange: data.fuelRange && data.fuelRange > 0 ? data.fuelRange : undefined,
        weatherBackupPlan: data.weatherBackupPlan || undefined,

        // Guest safety fields
        swimmingProficiency: data.swimmingProficiency || undefined,
        medicalConditions: data.medicalConditions || undefined,
        alcoholPolicy: data.alcoholPolicy || undefined,

        // Amenities & experience fields
        foodBeverageIncluded: data.foodBeverageIncluded || undefined,
        beverageType: data.beverageType || undefined,
        musicSystem: data.musicSystem || undefined,
        waterSportsEquipment: data.waterSportsEquipment || undefined,

        // Coordination fields
        marinaContactName: data.marinaContactName || undefined,
        marinaContactPhone: data.marinaContactPhone || undefined,

        // Internal coordination fields
        conciergeInternalNotes: data.conciergeInternalNotes || undefined,
      }

      if (data.specialRequest) {
        submitData.specialRequest = data.specialRequest
      }

      // Sadece create modunda invoice bilgileri ekle
      if (!yachtTour && createInvoice) {
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
          <DialogTitle>{yachtTour ? 'Yat Turu Düzenle' : 'Yeni Yat Turu Ekle'}</DialogTitle>
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
                  <TextField
                    label="Yat Adı"
                    fullWidth
                    required
                    value={watch('yachtName') || ''} onChange={(e) => setValue('yachtName', e.target.value)}
                    error={!!errors.yachtName}
                    helperText={errors.yachtName?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12} md={6}>
                  <TextField
                    label="Kişi Sayısı"
                    type="number"
                    fullWidth
                    required
                    {...register('numberOfPeople', { valueAsNumber: true })}
                    error={!!errors.numberOfPeople}
                    helperText={errors.numberOfPeople?.message}
                    disabled={isSubmitting || isLoading}
                    inputProps={{ step: '1', min: '1' }}
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
                  <FormControl fullWidth error={!!errors.tourCategory} disabled={isSubmitting || isLoading}>
                    <InputLabel>Tur Kategorisi (Opsiyonel)</InputLabel>
                    <Controller
                      name="tourCategory"
                      control={control}
                      render={({ field }) => (
                        <Select {...field} value={field.value || ''}>
                          <MenuItem value="">Seçiniz</MenuItem>
                          {Object.values(TourCategory).map((cat) => (
                            <MenuItem key={cat} value={cat}>
                              {cat}
                            </MenuItem>
                          ))}
                        </Select>
                      )}
                    />
                  </FormControl>
                  {errors.tourCategory && (
                    <Typography variant="caption" color="error" sx={{ mt: 0.5 }}>
                      {errors.tourCategory.message}
                    </Typography>
                  )}
                </Grid>

                <Grid item xs={12} md={6}>
                  <TextField
                    label="Alış İskelesi (Opsiyonel)"
                    fullWidth
                    value={watch('pickupPier') || ''} onChange={(e) => setValue('pickupPier', e.target.value)}
                    error={!!errors.pickupPier}
                    helperText={errors.pickupPier?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12} md={6}>
                  <TextField
                    label="Bırakış İskelesi (Opsiyonel)"
                    fullWidth
                    value={watch('dropoffPier') || ''} onChange={(e) => setValue('dropoffPier', e.target.value)}
                    error={!!errors.dropoffPier}
                    helperText={errors.dropoffPier?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12}>
                  <TextField
                    label="İskele Adresi (Opsiyonel)"
                    fullWidth
                    value={watch('pierAddress') || ''} onChange={(e) => setValue('pierAddress', e.target.value)}
                    error={!!errors.pierAddress}
                    helperText={errors.pierAddress?.message}
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

                <Grid item xs={12}>
                  <TextField
                    label="Özel İstek"
                    fullWidth
                    multiline
                    rows={3}
                    value={watch('specialRequest') || ''} onChange={(e) => setValue('specialRequest', e.target.value)}
                    error={!!errors.specialRequest}
                    helperText={errors.specialRequest?.message}
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

                {!yachtTour && (
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
              {isSubmitting || isLoading ? 'Kaydediliyor...' : yachtTour ? 'Güncelle' : 'Ekle'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </LocalizationProvider>
  )
}

export default YachtTourForm



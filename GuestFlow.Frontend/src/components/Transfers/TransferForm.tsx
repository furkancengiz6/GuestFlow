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
import { PaymentMethod, TransferType } from '../../types/enums'

const optionalId = z.preprocess(
  (val) => (val === '' || val === null || val === undefined ? undefined : Number(val)),
  z.number().optional()
)

const optionalPaymentMethod = z.preprocess(
  (val) => (val === '' || val === null ? undefined : val),
  z.nativeEnum(PaymentMethod).optional()
)

// Zod schema
const transferSchema = z.object({
  transferDate: z.date().min(new Date('1900-01-01'), 'Transfer tarihi gereklidir'),
  pickupTime: z.string().optional(),
  serviceStartTime: z.string().optional(),
  pickupAddress: z.string().min(1, 'Alış adresi gereklidir').max(500, 'Alış adresi en fazla 500 karakter olabilir'),
  dropoffAddress: z.string().min(1, 'Bırakış adresi gereklidir').max(500, 'Bırakış adresi en fazla 500 karakter olabilir'),
  price: z.number().min(0.01, 'Fiyat 0.01\'den büyük olmalıdır'),
  guestId: z.number().min(1, 'Misafir seçilmelidir'),
  personnelId: optionalId,
  airportId: optionalId,
  vehicleId: optionalId,
  note: z.string().optional(),
  status: z.string().optional(),
  pickupCityId: optionalId,
  dropoffCityId: optionalId,
  createInvoice: z.boolean().optional(),
  discountPercentage: z.preprocess(
    (val) => (val === '' || val === null || val === undefined ? undefined : Number(val)),
    z.number().min(0).max(100).optional()
  ),
  invoiceDescription: z.string().optional(),
  currency: z.string().optional(),
  transferType: z.nativeEnum(TransferType).optional(),
  hotelId: optionalId,
  restaurantId: optionalId,

  // Guest coordination fields
  contactPersonName: z.string().max(100, 'İletişim kişisi adı en fazla 100 karakter olabilir').optional(),
  meetingPointDetails: z.string().max(500, 'Buluşma noktası detayları en fazla 500 karakter olabilir').optional(),

  // Group management fields
  groupSize: z.preprocess(
    (val) => (val === '' || val === null || val === undefined ? undefined : Number(val)),
    z.number().min(1, 'Grup boyutu en az 1 olmalıdır').optional()
  ),
  childCount: z.preprocess(
    (val) => (val === '' || val === null || val === undefined ? undefined : Number(val)),
    z.number().min(0, 'Çocuk sayısı 0 veya daha büyük olmalıdır').optional()
  ),
  infantCount: z.preprocess(
    (val) => (val === '' || val === null || val === undefined ? undefined : Number(val)),
    z.number().min(0, 'Bebek sayısı 0 veya daha büyük olmalıdır').optional()
  ),

  // Communication fields
  guestLanguage: z.string().max(50, 'Konuşulan dil en fazla 50 karakter olabilir').optional(),
  emergencyContactPhone: z.string().max(20, 'Acil iletişim telefonu en fazla 20 karakter olabilir').optional(),

  // Service quality fields
  accessibilityRequirements: z.string().max(500, 'Erişilebilirlik ihtiyaçları en fazla 500 karakter olabilir').optional(),
  specialHandlingNotes: z.string().max(1000, 'Özel işlem notları en fazla 1000 karakter olabilir').optional(),

  // Internal coordination fields
  conciergeInternalNotes: z.string().max(1000, 'İç notlar en fazla 1000 karakter olabilir').optional(),
  guestVisibleNotes: z.string().max(500, 'Misafire gösterilecek notlar en fazla 500 karakter olabilir').optional(),

  // Supplier contact fields
  supplierContactPhone: z.string().max(20, 'Tedarikçi iletişim telefonu en fazla 20 karakter olabilir').optional(),
  supplierEmergencyContact: z.string().max(20, 'Tedarikçi acil telefonu en fazla 20 karakter olabilir').optional(),
  paymentMethod: optionalPaymentMethod,
  // isPaymentReceived removed - payment status is calculated from PaymentEntity
  paymentNote: z.string().max(1000, 'Ödeme notu en fazla 1000 karakter olabilir').optional(),
  externalVehiclePlate: z.string().max(50, 'Plaka en fazla 50 karakter olabilir').optional(),
  externalDriverName: z.string().max(200, 'Şoför adı en fazla 200 karakter olabilir').optional(),
  externalDriverPhone: z.string().max(20, 'Telefon en fazla 20 karakter olabilir').optional(),
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
    handleSubmit,
    control,
    formState: { errors, isSubmitting },
    reset,
    setValue,
    watch,
  } = useForm<TransferFormData>({
    resolver: zodResolver(transferSchema) as any,
    defaultValues: {
      transferDate: new Date(),
      pickupTime: '',
      serviceStartTime: '',
      pickupAddress: '',
      dropoffAddress: '',
      price: 0,
      guestId: 0,
      personnelId: undefined,
      airportId: undefined,
      vehicleId: undefined,
      note: '',
      status: 'Pending',
      pickupCityId: undefined,
      dropoffCityId: undefined,
      // INVOICE REALITY: Invoices are NOT created automatically - default is false
      createInvoice: false,
      discountPercentage: 0,
      invoiceDescription: '',
      currency: 'TRY',
      transferType: TransferType.Custom,
      hotelId: undefined,
      restaurantId: undefined,
      // Guest coordination fields
      contactPersonName: '',
      meetingPointDetails: '',
      // Group management fields
      groupSize: undefined,
      childCount: undefined,
      infantCount: undefined,
      // Communication fields
      guestLanguage: '',
      emergencyContactPhone: '',
      // Service quality fields
      accessibilityRequirements: '',
      specialHandlingNotes: '',
      // Internal coordination fields
      conciergeInternalNotes: '',
      guestVisibleNotes: '',
      // Supplier contact fields
      supplierContactPhone: '',
      supplierEmergencyContact: '',
      paymentMethod: undefined,
      // isPaymentReceived removed - payment status is calculated from PaymentEntity
      paymentNote: '',
      externalVehiclePlate: '',
      externalDriverName: '',
      externalDriverPhone: '',
    },
  })

  // Fetch dropdown data
  const { data: guests } = useQuery({
    queryKey: ['guests-dropdown'],
    queryFn: () => dropdownService.getGuests(),
    enabled: open,
  })

  // Personnel is automatically assigned from logged-in user, no need for dropdown

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
      setValue('personnelId', transfer.personnelId || undefined)
      setValue('airportId', transfer.airportId || undefined)
      setValue('vehicleId', transfer.vehicleId || undefined)
      setValue('note', transfer.note || '')
      setValue('status', transfer.status || 'Pending')
      setValue('pickupCityId', transfer.pickupCityId || undefined)
      setValue('dropoffCityId', transfer.dropoffCityId || undefined)
      setValue('transferType', (transfer as any).transferType || TransferType.Custom)
      setValue('hotelId', (transfer as any).hotelId || null)
      setValue('restaurantId', (transfer as any).restaurantId || null)

      // Guest coordination fields
      setValue('contactPersonName', (transfer as any).contactPersonName || '')
      setValue('meetingPointDetails', (transfer as any).meetingPointDetails || '')

      // Group management fields
      setValue('groupSize', (transfer as any).groupSize || null)
      setValue('childCount', (transfer as any).childCount || null)
      setValue('infantCount', (transfer as any).infantCount || null)

      // Communication fields
      setValue('guestLanguage', (transfer as any).guestLanguage || '')
      setValue('emergencyContactPhone', (transfer as any).emergencyContactPhone || '')

      // Service quality fields
      setValue('accessibilityRequirements', (transfer as any).accessibilityRequirements || '')
      setValue('specialHandlingNotes', (transfer as any).specialHandlingNotes || '')

      // Internal coordination fields
      setValue('conciergeInternalNotes', (transfer as any).conciergeInternalNotes || '')
      setValue('guestVisibleNotes', (transfer as any).guestVisibleNotes || '')

      // Supplier contact fields
      setValue('supplierContactPhone', (transfer as any).supplierContactPhone || '')
      setValue('supplierEmergencyContact', (transfer as any).supplierEmergencyContact || '')

      // isPaymentReceived removed - payment status is calculated from PaymentEntity
      setValue('externalVehiclePlate', transfer.externalVehiclePlate || '')
      setValue('externalDriverName', transfer.externalDriverName || '')
      setValue('externalDriverPhone', transfer.externalDriverPhone || '')
    } else {
      reset()
    }
  }, [transfer, setValue, reset])

  const handleFormSubmit = async (data: any) => {
    try {
      const submitData: any = {
        transferDate: data.transferDate.toISOString(),
        pickupAddress: data.pickupAddress,
        dropoffAddress: data.dropoffAddress,
        price: data.price,
        guestId: data.guestId,
        personnelId: data.personnelId && data.personnelId > 0 ? data.personnelId : undefined,
        airportId: data.airportId && data.airportId > 0 ? data.airportId : undefined,
        vehicleId: data.vehicleId && data.vehicleId > 0 ? data.vehicleId : undefined,
        note: data.note || undefined,
        status: data.status || 'Pending',
        transferType: data.transferType || TransferType.Custom,
        pickupCityId: data.pickupCityId && data.pickupCityId > 0 ? data.pickupCityId : undefined,
        hotelId: data.hotelId && data.hotelId > 0 ? data.hotelId : undefined,
        restaurantId: data.restaurantId && data.restaurantId > 0 ? data.restaurantId : undefined,
        dropoffCityId: data.dropoffCityId && data.dropoffCityId > 0 ? data.dropoffCityId : undefined,

        // Guest coordination fields
        contactPersonName: data.contactPersonName || undefined,
        meetingPointDetails: data.meetingPointDetails || undefined,

        // Group management fields
        groupSize: data.groupSize && data.groupSize > 0 ? data.groupSize : undefined,
        childCount: data.childCount && data.childCount > 0 ? data.childCount : undefined,
        infantCount: data.infantCount && data.infantCount > 0 ? data.infantCount : undefined,

        // Communication fields
        guestLanguage: data.guestLanguage || undefined,
        emergencyContactPhone: data.emergencyContactPhone || undefined,

        // Service quality fields
        accessibilityRequirements: data.accessibilityRequirements || undefined,
        specialHandlingNotes: data.specialHandlingNotes || undefined,

        // Internal coordination fields
        conciergeInternalNotes: data.conciergeInternalNotes || undefined,
        guestVisibleNotes: data.guestVisibleNotes || undefined,

        // Supplier contact fields
        supplierContactPhone: data.supplierContactPhone || undefined,
        supplierEmergencyContact: data.supplierEmergencyContact || undefined,
        paymentMethod: data.paymentMethod || undefined,
        // isPaymentReceived removed - payment status is calculated from PaymentEntity
        paymentNote: data.paymentNote || undefined,
        externalVehiclePlate: data.externalVehiclePlate || undefined,
        externalDriverName: data.externalDriverName || undefined,
        externalDriverPhone: data.externalDriverPhone || undefined,
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
                    value={watch('price') || 0}
                    onChange={(e) => setValue('price', e.target.value ? Number(e.target.value) : 0)}
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
                  <FormControl fullWidth error={!!errors.airportId} disabled={isSubmitting || isLoading}>
                    <InputLabel>Havaalanı (Opsiyonel)</InputLabel>
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
                  <FormControl fullWidth error={!!errors.pickupCityId} disabled={isSubmitting || isLoading}>
                    <InputLabel>Alış Şehri (Opsiyonel)</InputLabel>
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
                  <FormControl fullWidth error={!!errors.dropoffCityId} disabled={isSubmitting || isLoading}>
                    <InputLabel>Bırakış Şehri (Opsiyonel)</InputLabel>
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
                    value={watch('pickupAddress') || ''}
                    onChange={(e) => setValue('pickupAddress', e.target.value)}
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
                    value={watch('dropoffAddress') || ''}
                    onChange={(e) => setValue('dropoffAddress', e.target.value)}
                    error={!!errors.dropoffAddress}
                    helperText={errors.dropoffAddress?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                {/* Guest Coordination Section */}
                <Grid item xs={12}>
                  <Typography variant="h6" sx={{ mt: 2, mb: 1 }}>Misafir Koordinasyonu</Typography>
                </Grid>

                <Grid item xs={12} md={6}>
                  <TextField
                    label="İletişim Kişisi Adı"
                    fullWidth
                    value={watch('contactPersonName') || ''}
                    onChange={(e) => setValue('contactPersonName', e.target.value)}
                    error={!!errors.contactPersonName}
                    helperText={errors.contactPersonName?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12} md={6}>
                  <TextField
                    label="Acil İletişim Telefonu"
                    fullWidth
                    value={watch('emergencyContactPhone') || ''}
                    onChange={(e) => setValue('emergencyContactPhone', e.target.value)}
                    error={!!errors.emergencyContactPhone}
                    helperText={errors.emergencyContactPhone?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12} md={6}>
                  <TextField
                    label="Grup Boyutu"
                    type="number"
                    fullWidth
                    value={watch('groupSize') || ''}
                    onChange={(e) => setValue('groupSize', e.target.value ? Number(e.target.value) || undefined : undefined)}
                    error={!!errors.groupSize}
                    helperText={errors.groupSize?.message}
                    disabled={isSubmitting || isLoading}
                    inputProps={{ min: 1 }}
                  />
                </Grid>

                <Grid item xs={12} md={6}>
                  <TextField
                    label="Konuşulan Dil"
                    fullWidth
                    value={watch('guestLanguage') || ''}
                    onChange={(e) => setValue('guestLanguage', e.target.value)}
                    error={!!errors.guestLanguage}
                    helperText={errors.guestLanguage?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12} md={6}>
                  <TextField
                    label="Çocuk Sayısı"
                    type="number"
                    fullWidth
                    value={watch('childCount') || ''}
                    onChange={(e) => setValue('childCount', e.target.value ? Number(e.target.value) || undefined : undefined)}
                    error={!!errors.childCount}
                    helperText={errors.childCount?.message}
                    disabled={isSubmitting || isLoading}
                    inputProps={{ min: 0 }}
                  />
                </Grid>

                <Grid item xs={12} md={6}>
                  <TextField
                    label="Bebek Sayısı"
                    type="number"
                    fullWidth
                    value={watch('infantCount') || ''}
                    onChange={(e) => setValue('infantCount', e.target.value ? Number(e.target.value) || undefined : undefined)}
                    error={!!errors.infantCount}
                    helperText={errors.infantCount?.message}
                    disabled={isSubmitting || isLoading}
                    inputProps={{ min: 0 }}
                  />
                </Grid>

                <Grid item xs={12}>
                  <TextField
                    label="Buluşma Noktası Detayları"
                    fullWidth
                    multiline
                    rows={2}
                    value={watch('meetingPointDetails') || ''}
                    onChange={(e) => setValue('meetingPointDetails', e.target.value)}
                    error={!!errors.meetingPointDetails}
                    helperText={errors.meetingPointDetails?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                {/* Service Quality Section */}
                <Grid item xs={12}>
                  <Typography variant="h6" sx={{ mt: 2, mb: 1 }}>Hizmet Kalitesi</Typography>
                </Grid>

                <Grid item xs={12}>
                  <TextField
                    label="Erişilebilirlik İhtiyaçları"
                    fullWidth
                    multiline
                    rows={2}
                    value={watch('accessibilityRequirements') || ''}
                    onChange={(e) => setValue('accessibilityRequirements', e.target.value)}
                    error={!!errors.accessibilityRequirements}
                    helperText={errors.accessibilityRequirements?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12}>
                  <TextField
                    label="Özel İşlem Notları"
                    fullWidth
                    multiline
                    rows={3}
                    value={watch('specialHandlingNotes') || ''}
                    onChange={(e) => setValue('specialHandlingNotes', e.target.value)}
                    error={!!errors.specialHandlingNotes}
                    helperText={errors.specialHandlingNotes?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                {/* Internal Coordination Section */}
                <Grid item xs={12}>
                  <Typography variant="h6" sx={{ mt: 2, mb: 1 }}>İç Koordinasyon</Typography>
                </Grid>

                <Grid item xs={12}>
                  <TextField
                    label="Concierge İç Notları"
                    fullWidth
                    multiline
                    rows={3}
                    value={watch('conciergeInternalNotes') || ''}
                    onChange={(e) => setValue('conciergeInternalNotes', e.target.value)}
                    error={!!errors.conciergeInternalNotes}
                    helperText={errors.conciergeInternalNotes?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12}>
                  <TextField
                    label="Misafire Gösterilecek Notlar"
                    fullWidth
                    multiline
                    rows={2}
                    value={watch('guestVisibleNotes') || ''}
                    onChange={(e) => setValue('guestVisibleNotes', e.target.value)}
                    error={!!errors.guestVisibleNotes}
                    helperText={errors.guestVisibleNotes?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                {/* Supplier Contact Section */}
                <Grid item xs={12}>
                  <Typography variant="h6" sx={{ mt: 2, mb: 1 }}>Tedarikçi İletişim</Typography>
                </Grid>

                <Grid item xs={12} md={6}>
                  <TextField
                    label="Tedarikçi İletişim Telefonu"
                    fullWidth
                    value={watch('supplierContactPhone') || ''}
                    onChange={(e) => setValue('supplierContactPhone', e.target.value)}
                    error={!!errors.supplierContactPhone}
                    helperText={errors.supplierContactPhone?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12} md={6}>
                  <TextField
                    label="Tedarikçi Acil Telefon"
                    fullWidth
                    value={watch('supplierEmergencyContact') || ''}
                    onChange={(e) => setValue('supplierEmergencyContact', e.target.value)}
                    error={!!errors.supplierEmergencyContact}
                    helperText={errors.supplierEmergencyContact?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12} md={6}>
                  <FormControl fullWidth error={!!errors.status} disabled={isSubmitting || isLoading}>
                    <InputLabel>Durum (Opsiyonel)</InputLabel>
                    <Controller
                      name="status"
                      control={control}
                      render={({ field }) => (
                        <Select {...field} value={field.value || 'Pending'}>
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
                  <FormControl fullWidth error={!!errors.paymentMethod} disabled={isSubmitting || isLoading}>
                    <InputLabel>Ödeme Yöntemi</InputLabel>
                    <Controller
                      name="paymentMethod"
                      control={control}
                      render={({ field }) => (
                        <Select {...field} value={field.value ?? ''}>
                          <MenuItem value="">Seçiniz</MenuItem>
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

                {/* isPaymentReceived toggle removed - payment status is calculated from PaymentEntity */}

                <Grid item xs={12}>
                  <TextField
                    label="Ödeme Notu"
                    fullWidth
                    multiline
                    rows={2}
                    value={watch('paymentNote') || ''}
                    onChange={(e) => setValue('paymentNote', e.target.value)}
                    error={!!errors.paymentNote}
                    helperText={errors.paymentNote?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12} md={4}>
                  <TextField
                    label="Dışarıdan Araç Plakası"
                    fullWidth
                    value={watch('externalVehiclePlate') || ''}
                    onChange={(e) => setValue('externalVehiclePlate', e.target.value)}
                    error={!!errors.externalVehiclePlate}
                    helperText={errors.externalVehiclePlate?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12} md={4}>
                  <TextField
                    label="Dışarıdan Şoför Adı"
                    fullWidth
                    value={watch('externalDriverName') || ''}
                    onChange={(e) => setValue('externalDriverName', e.target.value)}
                    error={!!errors.externalDriverName}
                    helperText={errors.externalDriverName?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12} md={4}>
                  <TextField
                    label="Dışarıdan Şoför Telefonu"
                    fullWidth
                    value={watch('externalDriverPhone') || ''}
                    onChange={(e) => setValue('externalDriverPhone', e.target.value)}
                    error={!!errors.externalDriverPhone}
                    helperText={errors.externalDriverPhone?.message}
                    disabled={isSubmitting || isLoading}
                  />
                </Grid>

                <Grid item xs={12}>
                  <TextField
                    label="Not"
                    fullWidth
                    multiline
                    rows={3}
                    value={watch('note') || ''}
                    onChange={(e) => setValue('note', e.target.value)}
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
                            value={watch('discountPercentage') || ''}
                            onChange={(e) => setValue('discountPercentage', e.target.value ? Number(e.target.value) || undefined : undefined)}
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
                            value={watch('invoiceDescription') || ''}
                            onChange={(e) => setValue('invoiceDescription', e.target.value)}
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


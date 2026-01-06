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
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Grid,
  Switch,
  FormControlLabel,
} from '@mui/material'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns'
import { tr } from 'date-fns/locale'
import { ServicePackage, CreateServicePackageRequest, UpdateServicePackageRequest } from '../../services/servicePackageService'
import { PackageType, PackageTypeLabels } from '../../types/enums'
import { useFormErrorHandler } from '../../hooks/useFormErrorHandler'

// Zod schema
const servicePackageSchema = z.object({
  packageName: z.string().min(2, 'Paket adı en az 2 karakter olmalıdır').max(200, 'Paket adı en fazla 200 karakter olabilir'),
  description: z.string().max(1000, 'Açıklama en fazla 1000 karakter olabilir').optional().or(z.literal('')),
  packageType: z.nativeEnum(PackageType),
  startDate: z.date().optional().nullable(),
  endDate: z.date().optional().nullable(),
  discountPercentage: z.number().min(0).max(100).optional().nullable(),
  currency: z.string().max(3, 'Para birimi kodu en fazla 3 karakter olabilir').optional().or(z.literal('')),
  isActive: z.boolean(),
  packageContent: z.string().max(2000, 'Paket içeriği en fazla 2000 karakter olabilir').optional().or(z.literal('')),
  notes: z.string().max(1000, 'Notlar en fazla 1000 karakter olabilir').optional().or(z.literal('')),
})

type ServicePackageFormData = z.infer<typeof servicePackageSchema>

interface ServicePackageFormProps {
  open: boolean
  onClose: () => void
  onSubmit: (data: CreateServicePackageRequest | UpdateServicePackageRequest) => Promise<void>
  servicePackage?: ServicePackage | null
  isLoading?: boolean
}

const ServicePackageForm = ({ open, onClose, onSubmit, servicePackage, isLoading = false }: ServicePackageFormProps) => {
  const {
    register,
    handleSubmit,
    control,
    formState: { errors, isSubmitting },
    reset,
    setValue,
    setError,
    watch,
  } = useForm<ServicePackageFormData>({
    resolver: zodResolver(servicePackageSchema),
    defaultValues: {
      packageName: '',
      description: '',
      packageType: PackageType.Standard,
      startDate: null,
      endDate: null,
      discountPercentage: null,
      currency: 'TRY',
      isActive: true,
      packageContent: '',
      notes: '',
    },
  })

  const { handleFormError } = useFormErrorHandler(setError)

  // Form'u servicePackage verisiyle doldur (düzenleme modu)
  useEffect(() => {
    if (servicePackage) {
      setValue('packageName', servicePackage.packageName)
      setValue('description', servicePackage.description || '')
      setValue('packageType', servicePackage.packageType as PackageType)
      setValue('startDate', servicePackage.startDate ? new Date(servicePackage.startDate) : null)
      setValue('endDate', servicePackage.endDate ? new Date(servicePackage.endDate) : null)
      setValue('discountPercentage', servicePackage.discountPercentage || null)
      setValue('currency', servicePackage.currency || 'TRY')
      setValue('isActive', servicePackage.isActive)
      setValue('packageContent', servicePackage.packageContent || '')
      setValue('notes', servicePackage.notes || '')
    } else {
      reset()
    }
  }, [servicePackage, setValue, reset])

  const onSubmitForm = async (data: ServicePackageFormData) => {
    try {
      const submitData: any = {
        packageName: data.packageName,
        description: data.description || undefined,
        packageType: data.packageType,
        startDate: data.startDate ? data.startDate.toISOString() : undefined,
        endDate: data.endDate ? data.endDate.toISOString() : undefined,
        discountPercentage: data.discountPercentage || undefined,
        currency: data.currency || 'TRY',
        isActive: data.isActive,
        packageContent: data.packageContent || undefined,
        notes: data.notes || undefined,
      }

      if (servicePackage) {
        submitData.id = servicePackage.id
      }

      await onSubmit(submitData)
      if (!servicePackage) {
        reset()
      }
    } catch (error) {
      handleFormError(error, false)
    }
  }

  return (
    <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={tr}>
      <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
        <form onSubmit={handleSubmit(onSubmitForm)}>
          <DialogTitle>{servicePackage ? 'Servis Paketi Düzenle' : 'Yeni Servis Paketi'}</DialogTitle>
          <DialogContent>
            <Box sx={{ pt: 2 }}>
              <Grid container spacing={2}>
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    label="Paket Adı"
                    {...register('packageName')}
                    error={!!errors.packageName}
                    helperText={errors.packageName?.message}
                    required
                  />
                </Grid>
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    label="Açıklama"
                    {...register('description')}
                    error={!!errors.description}
                    helperText={errors.description?.message}
                    multiline
                    rows={3}
                  />
                </Grid>
                <Grid item xs={12} md={6}>
                  <FormControl fullWidth error={!!errors.packageType}>
                    <InputLabel>Paket Tipi *</InputLabel>
                    <Controller
                      name="packageType"
                      control={control}
                      render={({ field }) => (
                        <Select {...field} label="Paket Tipi *">
                          {Object.entries(PackageTypeLabels).map(([key, label]) => (
                            <MenuItem key={key} value={Number(key)}>
                              {label}
                            </MenuItem>
                          ))}
                        </Select>
                      )}
                    />
                  </FormControl>
                </Grid>
                <Grid item xs={12} md={6}>
                  <FormControlLabel
                    control={
                      <Controller
                        name="isActive"
                        control={control}
                        render={({ field }) => (
                          <Switch {...field} checked={field.value} />
                        )}
                      />
                    }
                    label="Aktif"
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
                        onChange={(date) => field.onChange(date)}
                        slotProps={{
                          textField: {
                            fullWidth: true,
                            error: !!errors.startDate,
                            helperText: errors.startDate?.message,
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
                        label="Bitiş Tarihi"
                        value={field.value}
                        onChange={(date) => field.onChange(date)}
                        slotProps={{
                          textField: {
                            fullWidth: true,
                            error: !!errors.endDate,
                            helperText: errors.endDate?.message,
                          },
                        }}
                      />
                    )}
                  />
                </Grid>
                <Grid item xs={12} md={6}>
                  <TextField
                    fullWidth
                    label="İndirim Yüzdesi"
                    type="number"
                    {...register('discountPercentage', { valueAsNumber: true })}
                    error={!!errors.discountPercentage}
                    helperText={errors.discountPercentage?.message}
                    inputProps={{ min: 0, max: 100, step: 0.01 }}
                  />
                </Grid>
                <Grid item xs={12} md={6}>
                  <TextField
                    fullWidth
                    label="Para Birimi"
                    {...register('currency')}
                    error={!!errors.currency}
                    helperText={errors.currency?.message}
                    placeholder="TRY"
                  />
                </Grid>
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    label="Paket İçeriği"
                    {...register('packageContent')}
                    error={!!errors.packageContent}
                    helperText={errors.packageContent?.message}
                    multiline
                    rows={4}
                  />
                </Grid>
                <Grid item xs={12}>
                  <TextField
                    fullWidth
                    label="Notlar"
                    {...register('notes')}
                    error={!!errors.notes}
                    helperText={errors.notes?.message}
                    multiline
                    rows={3}
                  />
                </Grid>
              </Grid>
            </Box>
          </DialogContent>
          <DialogActions>
            <Button onClick={onClose} disabled={isSubmitting || isLoading}>
              İptal
            </Button>
            <Button type="submit" variant="contained" disabled={isSubmitting || isLoading}>
              {isSubmitting || isLoading ? 'Kaydediliyor...' : servicePackage ? 'Güncelle' : 'Oluştur'}
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </LocalizationProvider>
  )
}

export default ServicePackageForm


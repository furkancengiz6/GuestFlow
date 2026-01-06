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
  FormControlLabel,
  Switch,
} from '@mui/material'
import { useQuery } from '@tanstack/react-query'
import { Restaurant, CreateRestaurantRequest, UpdateRestaurantRequest } from '../../services/restaurantService'
import { dropdownService } from '../../services/dropdownService'

// Zod schema
const restaurantSchema = z.object({
  restaurantName: z.string().min(2, 'Restoran adı en az 2 karakter olmalıdır').max(200, 'Restoran adı en fazla 200 karakter olabilir'),
  address: z.string().min(5, 'Adres en az 5 karakter olmalıdır').max(500, 'Adres en fazla 500 karakter olabilir'),
  cityId: z.number().min(1, 'Şehir seçiniz'),
  phone: z.string().optional().or(z.literal('')),
  email: z.string().email('Geçerli bir e-posta adresi giriniz').optional().or(z.literal('')),
  cuisineType: z.string().optional().or(z.literal('')),
  capacity: z.number().min(1, 'Kapasite en az 1 olmalıdır'),
  operatingHours: z.string().optional().or(z.literal('')),
  reservationRequired: z.boolean(),
})

type RestaurantFormData = z.infer<typeof restaurantSchema>

interface RestaurantFormProps {
  open: boolean
  onClose: () => void
  onSubmit: (data: CreateRestaurantRequest | UpdateRestaurantRequest) => Promise<void>
  restaurant?: Restaurant | null
  isLoading?: boolean
}

const RestaurantForm = ({ open, onClose, onSubmit, restaurant, isLoading = false }: RestaurantFormProps) => {
  const {
    register,
    handleSubmit,
    control,
    formState: { errors, isSubmitting },
    reset,
    setValue,
  } = useForm<RestaurantFormData>({
    resolver: zodResolver(restaurantSchema),
    defaultValues: {
      restaurantName: '',
      address: '',
      cityId: 0,
      phone: '',
      email: '',
      cuisineType: '',
      capacity: 10,
      operatingHours: '',
      reservationRequired: false,
    },
  })

  const { data: cities } = useQuery({
    queryKey: ['cities-dropdown'],
    queryFn: () => dropdownService.getCities(),
  })

  // Form'u restaurant verisiyle doldur (düzenleme modu)
  useEffect(() => {
    if (restaurant) {
      setValue('restaurantName', restaurant.restaurantName)
      setValue('address', restaurant.address)
      setValue('cityId', restaurant.cityId)
      setValue('phone', restaurant.phone || '')
      setValue('email', restaurant.email || '')
      setValue('cuisineType', restaurant.cuisineType || '')
      setValue('capacity', restaurant.capacity)
      setValue('operatingHours', restaurant.operatingHours || '')
      setValue('reservationRequired', restaurant.reservationRequired)
    } else {
      reset()
    }
  }, [restaurant, setValue, reset])

  const onSubmitForm = async (data: RestaurantFormData) => {
    await onSubmit(data)
    if (!restaurant) {
      reset()
    }
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <form onSubmit={handleSubmit(onSubmitForm)}>
        <DialogTitle>{restaurant ? 'Restoran Düzenle' : 'Yeni Restoran'}</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Restoran Adı"
                  {...register('restaurantName')}
                  error={!!errors.restaurantName}
                  helperText={errors.restaurantName?.message}
                  required
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Adres"
                  {...register('address')}
                  error={!!errors.address}
                  helperText={errors.address?.message}
                  required
                  multiline
                  rows={2}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <FormControl fullWidth error={!!errors.cityId}>
                  <InputLabel>Şehir *</InputLabel>
                  <Controller
                    name="cityId"
                    control={control}
                    render={({ field }) => (
                      <Select {...field} label="Şehir *">
                        <MenuItem value={0}>Şehir Seçiniz</MenuItem>
                        {cities?.map((city) => (
                          <MenuItem key={city.id} value={city.id}>
                            {city.cityName}
                          </MenuItem>
                        ))}
                      </Select>
                    )}
                  />
                  {errors.cityId && (
                    <Box component="span" sx={{ color: 'error.main', fontSize: '0.75rem', mt: 0.5, ml: 1.75 }}>
                      {errors.cityId.message}
                    </Box>
                  )}
                </FormControl>
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Kapasite"
                  type="number"
                  {...register('capacity', { valueAsNumber: true })}
                  error={!!errors.capacity}
                  helperText={errors.capacity?.message}
                  required
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Telefon"
                  {...register('phone')}
                  error={!!errors.phone}
                  helperText={errors.phone?.message}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="E-posta"
                  type="email"
                  {...register('email')}
                  error={!!errors.email}
                  helperText={errors.email?.message}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Mutfak Tipi"
                  {...register('cuisineType')}
                  helperText="Örn: Türk, İtalyan, Fransız"
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Çalışma Saatleri"
                  {...register('operatingHours')}
                  helperText="Örn: 09:00-23:00"
                />
              </Grid>
              <Grid item xs={12}>
                <FormControlLabel
                  control={
                    <Controller
                      name="reservationRequired"
                      control={control}
                      render={({ field }) => (
                        <Switch {...field} checked={field.value} />
                      )}
                    />
                  }
                  label="Rezervasyon Gerekli"
                />
              </Grid>
            </Grid>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose}>İptal</Button>
          <Button type="submit" variant="contained" disabled={isSubmitting || isLoading}>
            {isSubmitting || isLoading ? 'Kaydediliyor...' : restaurant ? 'Güncelle' : 'Ekle'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  )
}

export default RestaurantForm


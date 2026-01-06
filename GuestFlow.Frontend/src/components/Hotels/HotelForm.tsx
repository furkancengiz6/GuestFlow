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
} from '@mui/material'
import { useQuery } from '@tanstack/react-query'
import { Hotel, CreateHotelRequest, UpdateHotelRequest } from '../../services/hotelService'
import { dropdownService } from '../../services/dropdownService'
import { useFormErrorHandler } from '../../hooks/useFormErrorHandler'

// Zod schema
const hotelSchema = z.object({
  hotelName: z.string().min(2, 'Otel adı en az 2 karakter olmalıdır').max(200, 'Otel adı en fazla 200 karakter olabilir'),
  address: z.string().min(5, 'Adres en az 5 karakter olmalıdır').max(500, 'Adres en fazla 500 karakter olabilir'),
  cityId: z.number().min(1, 'Şehir seçiniz'),
  phone: z.string().optional().or(z.literal('')),
  email: z.string().email('Geçerli bir e-posta adresi giriniz').optional().or(z.literal('')),
  starRating: z.number().min(1).max(5),
  checkInTime: z.string().optional().or(z.literal('')),
  checkOutTime: z.string().optional().or(z.literal('')),
  roomTypes: z.string().optional().or(z.literal('')),
  amenities: z.string().optional().or(z.literal('')),
})

type HotelFormData = z.infer<typeof hotelSchema>

interface HotelFormProps {
  open: boolean
  onClose: () => void
  onSubmit: (data: CreateHotelRequest | UpdateHotelRequest) => Promise<void>
  hotel?: Hotel | null
  isLoading?: boolean
}

const HotelForm = ({ open, onClose, onSubmit, hotel, isLoading = false }: HotelFormProps) => {
  const {
    register,
    handleSubmit,
    control,
    formState: { errors, isSubmitting },
    reset,
    setValue,
    setError,
  } = useForm<HotelFormData>({
    resolver: zodResolver(hotelSchema),
    defaultValues: {
      hotelName: '',
      address: '',
      cityId: 0,
      phone: '',
      email: '',
      starRating: 3,
      checkInTime: '',
      checkOutTime: '',
      roomTypes: '',
      amenities: '',
    },
  })

  const { data: cities } = useQuery({
    queryKey: ['cities-dropdown'],
    queryFn: () => dropdownService.getCities(),
  })

  const { handleFormError } = useFormErrorHandler(setError)

  // Form'u hotel verisiyle doldur (düzenleme modu)
  useEffect(() => {
    if (hotel) {
      setValue('hotelName', hotel.hotelName)
      setValue('address', hotel.address)
      setValue('cityId', hotel.cityId)
      setValue('phone', hotel.phone || '')
      setValue('email', hotel.email || '')
      setValue('starRating', hotel.starRating)
      setValue('checkInTime', hotel.checkInTime || '')
      setValue('checkOutTime', hotel.checkOutTime || '')
      setValue('roomTypes', hotel.roomTypes || '')
      setValue('amenities', hotel.amenities || '')
    } else {
      reset()
    }
  }, [hotel, setValue, reset])

  const onSubmitForm = async (data: HotelFormData) => {
    try {
      await onSubmit(data)
      if (!hotel) {
        reset()
      }
    } catch (error) {
      handleFormError(error, false) // Don't show notification as parent handles it
    }
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <form onSubmit={handleSubmit(onSubmitForm)}>
        <DialogTitle>{hotel ? 'Otel Düzenle' : 'Yeni Otel'}</DialogTitle>
        <DialogContent>
          <Box sx={{ pt: 2 }}>
            <Grid container spacing={2}>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Otel Adı"
                  {...register('hotelName')}
                  error={!!errors.hotelName}
                  helperText={errors.hotelName?.message}
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
                <FormControl fullWidth error={!!errors.starRating}>
                  <InputLabel>Yıldız Sayısı *</InputLabel>
                  <Controller
                    name="starRating"
                    control={control}
                    render={({ field }) => (
                      <Select {...field} label="Yıldız Sayısı *">
                        {[1, 2, 3, 4, 5].map((rating) => (
                          <MenuItem key={rating} value={rating}>
                            {rating} Yıldız
                          </MenuItem>
                        ))}
                      </Select>
                    )}
                  />
                </FormControl>
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
                  label="Check-in Saati"
                  type="time"
                  {...register('checkInTime')}
                  InputLabelProps={{ shrink: true }}
                />
              </Grid>
              <Grid item xs={12} md={6}>
                <TextField
                  fullWidth
                  label="Check-out Saati"
                  type="time"
                  {...register('checkOutTime')}
                  InputLabelProps={{ shrink: true }}
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Oda Tipleri"
                  {...register('roomTypes')}
                  helperText="Virgülle ayırarak birden fazla oda tipi ekleyebilirsiniz"
                  multiline
                  rows={2}
                />
              </Grid>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  label="Olanaklar"
                  {...register('amenities')}
                  helperText="Virgülle ayırarak birden fazla olanak ekleyebilirsiniz"
                  multiline
                  rows={2}
                />
              </Grid>
            </Grid>
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose}>İptal</Button>
          <Button type="submit" variant="contained" disabled={isSubmitting || isLoading}>
            {isSubmitting || isLoading ? 'Kaydediliyor...' : hotel ? 'Güncelle' : 'Ekle'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  )
}

export default HotelForm


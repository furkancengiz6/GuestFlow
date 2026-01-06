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
  FormControlLabel,
  Switch,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Typography,
} from '@mui/material'
import { Guest, CreateGuestRequest, UpdateGuestRequest } from '../../services/guestService'
import { COUNTRIES } from '../../utils/countries'

// Zod schema
const guestSchema = z.object({
  fullName: z
    .string()
    .min(5, 'Ad soyad en az 5 karakter olmalıdır')
    .max(100, 'Ad soyad en fazla 100 karakter olabilir'),
  email: z
    .string()
    .email('Geçerli bir e-posta adresi giriniz')
    .optional()
    .or(z.literal('')),
  phoneNumber: z.string().optional().or(z.literal('')),
  nationality: z
    .string()
    .min(2, 'Uyruk en az 2 karakter olmalıdır')
    .max(100, 'Uyruk en fazla 100 karakter olabilir'),
  isSpecialGuest: z.boolean(),
})

type GuestFormData = z.infer<typeof guestSchema>

interface GuestFormProps {
  open: boolean
  onClose: () => void
  onSubmit: (data: CreateGuestRequest | UpdateGuestRequest) => Promise<void>
  guest?: Guest | null
  isLoading?: boolean
}

const GuestForm = ({ open, onClose, onSubmit, guest, isLoading = false }: GuestFormProps) => {
  const {
    register,
    handleSubmit,
    control,
    formState: { errors, isSubmitting },
    reset,
    setValue,
  } = useForm<GuestFormData>({
    resolver: zodResolver(guestSchema),
    defaultValues: {
      fullName: '',
      email: '',
      phoneNumber: '',
      nationality: '',
      isSpecialGuest: false,
    },
  })

  // Form'u guest verisiyle doldur (düzenleme modu)
  useEffect(() => {
    if (guest) {
      setValue('fullName', guest.fullName)
      setValue('email', guest.email || '')
      setValue('phoneNumber', guest.phoneNumber || '')
      setValue('nationality', guest.nationality)
      setValue('isSpecialGuest', guest.isSpecialGuest)
    } else {
      reset()
    }
  }, [guest, setValue, reset])

  const handleFormSubmit = async (data: GuestFormData) => {
    try {
      // Boş string'leri undefined'a çevir
      const submitData = {
        fullName: data.fullName,
        email: data.email || undefined,
        phoneNumber: data.phoneNumber || undefined,
        nationality: data.nationality,
        isSpecialGuest: data.isSpecialGuest,
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
    <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth>
      <form onSubmit={handleSubmit(handleFormSubmit)}>
        <DialogTitle>{guest ? 'Misafir Düzenle' : 'Yeni Misafir Ekle'}</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
            <TextField
              label="Ad Soyad"
              fullWidth
              required
              {...register('fullName')}
              error={!!errors.fullName}
              helperText={errors.fullName?.message}
              disabled={isSubmitting || isLoading}
            />

            <TextField
              label="E-posta"
              type="email"
              fullWidth
              {...register('email')}
              error={!!errors.email}
              helperText={errors.email?.message}
              disabled={isSubmitting || isLoading}
            />

            <TextField
              label="Telefon"
              fullWidth
              {...register('phoneNumber')}
              error={!!errors.phoneNumber}
              helperText={errors.phoneNumber?.message}
              disabled={isSubmitting || isLoading}
            />

            <FormControl fullWidth required error={!!errors.nationality} disabled={isSubmitting || isLoading}>
              <InputLabel>Uyruk</InputLabel>
              <Controller
                name="nationality"
                control={control}
                render={({ field }) => (
                  <Select {...field} value={field.value || ''} label="Uyruk">
                    <MenuItem value="">Seçiniz</MenuItem>
                    {COUNTRIES.map((country) => (
                      <MenuItem key={country.code} value={country.name}>
                        {country.name}
                      </MenuItem>
                    ))}
                  </Select>
                )}
              />
              {errors.nationality && (
                <Typography variant="caption" color="error" sx={{ mt: 0.5, ml: 1.75 }}>
                  {errors.nationality.message}
                </Typography>
              )}
            </FormControl>

            <Controller
              name="isSpecialGuest"
              control={control}
              render={({ field }) => (
                <FormControlLabel
                  control={
                    <Switch
                      checked={field.value}
                      onChange={field.onChange}
                      disabled={isSubmitting || isLoading}
                    />
                  }
                  label="Özel Misafir"
                />
              )}
            />
          </Box>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={handleClose} disabled={isSubmitting || isLoading}>
            İptal
          </Button>
          <Button
            type="submit"
            variant="contained"
            disabled={isSubmitting || isLoading}
          >
            {isSubmitting || isLoading
              ? 'Kaydediliyor...'
              : guest
                ? 'Güncelle'
                : 'Ekle'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  )
}

export default GuestForm


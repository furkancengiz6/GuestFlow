import { useEffect } from 'react'
import { useForm } from 'react-hook-form'
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
} from '@mui/material'
import { City, CreateCityRequest, UpdateCityRequest } from '../../services/cityService'

const citySchema = z.object({
  cityName: z.string().min(2, 'Şehir adı en az 2 karakter olmalıdır').max(100, 'Şehir adı en fazla 100 karakter olabilir'),
})

type CityFormData = z.infer<typeof citySchema>

interface CityFormProps {
  open: boolean
  onClose: () => void
  onSubmit: (data: CreateCityRequest | UpdateCityRequest) => Promise<void>
  city?: City | null
  isLoading?: boolean
}

const CityForm = ({ open, onClose, onSubmit, city, isLoading = false }: CityFormProps) => {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    reset,
    setValue,
  } = useForm<CityFormData>({
    resolver: zodResolver(citySchema),
    defaultValues: {
      cityName: '',
    },
  })

  useEffect(() => {
    if (city) {
      setValue('cityName', city.cityName)
    } else {
      reset()
    }
  }, [city, setValue, reset])

  const handleFormSubmit = async (data: CityFormData) => {
    try {
      await onSubmit(data)
    } catch (error) {
      console.error('Form submission error:', error)
    }
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <form onSubmit={handleSubmit(handleFormSubmit)}>
        <DialogTitle>{city ? 'Şehir Düzenle' : 'Yeni Şehir'}</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
            <TextField
              {...register('cityName')}
              label="Şehir Adı"
              fullWidth
              error={!!errors.cityName}
              helperText={errors.cityName?.message}
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose} disabled={isSubmitting || isLoading}>
            İptal
          </Button>
          <Button type="submit" variant="contained" disabled={isSubmitting || isLoading}>
            {isSubmitting || isLoading ? 'Kaydediliyor...' : city ? 'Güncelle' : 'Ekle'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  )
}

export default CityForm


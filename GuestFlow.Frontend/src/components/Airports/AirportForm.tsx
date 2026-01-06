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
} from '@mui/material'
import { Airport, CreateAirportRequest, UpdateAirportRequest } from '../../services/airportService'
import { dropdownService } from '../../services/dropdownService'
import { useQuery } from '@tanstack/react-query'

const airportSchema = z.object({
  airportName: z.string().min(2, 'Havalimanı adı en az 2 karakter olmalıdır').max(100, 'Havalimanı adı en fazla 100 karakter olabilir'),
  cityId: z.number().min(1, 'Şehir seçmelisiniz'),
})

type AirportFormData = z.infer<typeof airportSchema>

interface AirportFormProps {
  open: boolean
  onClose: () => void
  onSubmit: (data: CreateAirportRequest | UpdateAirportRequest) => Promise<void>
  airport?: Airport | null
  isLoading?: boolean
}

const AirportForm = ({ open, onClose, onSubmit, airport, isLoading = false }: AirportFormProps) => {
  const { data: cities } = useQuery({
    queryKey: ['cities-dropdown'],
    queryFn: () => dropdownService.getCities(),
  })

  const {
    register,
    handleSubmit,
    control,
    formState: { errors, isSubmitting },
    reset,
    setValue,
  } = useForm<AirportFormData>({
    resolver: zodResolver(airportSchema),
    defaultValues: {
      airportName: '',
      cityId: 0,
    },
  })

  useEffect(() => {
    if (airport) {
      setValue('airportName', airport.airportName)
      setValue('cityId', airport.cityId)
    } else {
      reset()
    }
  }, [airport, setValue, reset])

  const handleFormSubmit = async (data: AirportFormData) => {
    try {
      await onSubmit(data)
    } catch (error) {
      console.error('Form submission error:', error)
    }
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <form onSubmit={handleSubmit(handleFormSubmit)}>
        <DialogTitle>{airport ? 'Havalimanı Düzenle' : 'Yeni Havalimanı'}</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
            <TextField
              {...register('airportName')}
              label="Havalimanı Adı"
              fullWidth
              error={!!errors.airportName}
              helperText={errors.airportName?.message}
            />
            <Controller
              name="cityId"
              control={control}
              render={({ field }) => (
                <TextField
                  {...field}
                  select
                  label="Şehir"
                  fullWidth
                  error={!!errors.cityId}
                  helperText={errors.cityId?.message}
                  SelectProps={{
                    native: true,
                  }}
                >
                  <option value={0}>Şehir Seçiniz</option>
                  {cities?.map((city) => (
                    <option key={city.id} value={city.id}>
                      {city.cityName}
                    </option>
                  ))}
                </TextField>
              )}
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose} disabled={isSubmitting || isLoading}>
            İptal
          </Button>
          <Button type="submit" variant="contained" disabled={isSubmitting || isLoading}>
            {isSubmitting || isLoading ? 'Kaydediliyor...' : airport ? 'Güncelle' : 'Ekle'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  )
}

export default AirportForm


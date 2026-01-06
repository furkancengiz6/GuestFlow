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
import { Vehicle, CreateVehicleRequest, UpdateVehicleRequest } from '../../services/vehicleService'

const vehicleSchema = z.object({
  plateNumber: z.string().min(2, 'Plaka numarası en az 2 karakter olmalıdır').max(20, 'Plaka numarası en fazla 20 karakter olabilir'),
  vehicleType: z.string().min(2, 'Araç tipi en az 2 karakter olmalıdır').max(50, 'Araç tipi en fazla 50 karakter olabilir'),
  capacity: z.number().min(1, 'Kapasite en az 1 olmalıdır').max(100, 'Kapasite en fazla 100 olabilir'),
})

type VehicleFormData = z.infer<typeof vehicleSchema>

interface VehicleFormProps {
  open: boolean
  onClose: () => void
  onSubmit: (data: CreateVehicleRequest | UpdateVehicleRequest) => Promise<void>
  vehicle?: Vehicle | null
  isLoading?: boolean
}

const VehicleForm = ({ open, onClose, onSubmit, vehicle, isLoading = false }: VehicleFormProps) => {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    reset,
    setValue,
  } = useForm<VehicleFormData>({
    resolver: zodResolver(vehicleSchema),
    defaultValues: {
      plateNumber: '',
      vehicleType: '',
      capacity: 1,
    },
  })

  useEffect(() => {
    if (vehicle) {
      setValue('plateNumber', vehicle.plateNumber)
      setValue('vehicleType', vehicle.vehicleType)
      setValue('capacity', vehicle.capacity)
    } else {
      reset()
    }
  }, [vehicle, setValue, reset])

  const handleFormSubmit = async (data: VehicleFormData) => {
    try {
      await onSubmit(data)
    } catch (error) {
      console.error('Form submission error:', error)
    }
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <form onSubmit={handleSubmit(handleFormSubmit)}>
        <DialogTitle>{vehicle ? 'Araç Düzenle' : 'Yeni Araç'}</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
            <TextField
              {...register('plateNumber')}
              label="Plaka Numarası"
              fullWidth
              error={!!errors.plateNumber}
              helperText={errors.plateNumber?.message}
            />
            <TextField
              {...register('vehicleType')}
              label="Araç Tipi"
              fullWidth
              error={!!errors.vehicleType}
              helperText={errors.vehicleType?.message}
            />
            <TextField
              {...register('capacity', { valueAsNumber: true })}
              label="Kapasite"
              type="number"
              fullWidth
              error={!!errors.capacity}
              helperText={errors.capacity?.message}
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose} disabled={isSubmitting || isLoading}>
            İptal
          </Button>
          <Button type="submit" variant="contained" disabled={isSubmitting || isLoading}>
            {isSubmitting || isLoading ? 'Kaydediliyor...' : vehicle ? 'Güncelle' : 'Ekle'}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  )
}

export default VehicleForm


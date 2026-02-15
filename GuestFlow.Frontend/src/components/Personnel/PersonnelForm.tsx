import { useEffect } from 'react'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  FormHelperText,
  Grid,
} from '@mui/material'
import { LoadingButton } from '@mui/lab'
import { Personnel, CreatePersonnelRequest, UpdatePersonnelRequest } from '../../services/personnelService'

const schema = z.object({
  fullName: z.string().min(2, 'Ad Soyad en az 2 karakter olmalıdır'),
  email: z.string().email('Geçerli bir e-posta adresi giriniz'),
  userType: z.string().min(1, 'Kullanıcı tipi seçiniz'),
  password: z.string().optional().refine((_val) => {
    // If we are creating (no id), password is required (e.g. min 6 chars)
    // If updating, it's optional
    return true
  }),
})
// We'll refine the password requirement in the component based on mode

type FormValues = z.infer<typeof schema>

interface PersonnelFormProps {
  open: boolean
  onClose: () => void
  onSubmit: (data: CreatePersonnelRequest | UpdatePersonnelRequest) => void
  initialData?: Personnel | null
  loading?: boolean
}

export const PersonnelForm = ({
  open,
  onClose,
  onSubmit,
  initialData,
  loading = false,
}: PersonnelFormProps) => {
  const isEditMode = !!initialData

  // Dynamic schema based on edit mode
  const formSchema = schema.refine(
    (data) => {
      if (!isEditMode && (!data.password || data.password.length < 6)) {
        return false
      }
      return true
    },
    {
      message: 'Şifre en az 6 karakter olmalıdır',
      path: ['password'],
    }
  )

  const {
    control,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      fullName: '',
      email: '',
      userType: 'Staff',
      password: '',
    },
  })

  useEffect(() => {
    if (open) {
      if (initialData) {
        reset({
          fullName: initialData.fullName,
          email: initialData.email,
          userType: initialData.userType,
          password: '',
        })
      } else {
        reset({
          fullName: '',
          email: '',
          userType: 'Staff',
          password: '',
        })
      }
    }
  }, [open, initialData, reset])

  const handleFormSubmit = (data: FormValues) => {
    const payload = {
      ...data,
      // Only include password if it's provided
      password: data.password || undefined,
    }
    onSubmit(payload as any)
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>
        {isEditMode ? 'Personel Düzenle' : 'Yeni Personel Ekle'}
      </DialogTitle>
      <form onSubmit={handleSubmit(handleFormSubmit)}>
        <DialogContent dividers>
          <Grid container spacing={2}>
            <Grid item xs={12}>
              <Controller
                name="fullName"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label="Ad Soyad"
                    fullWidth
                    error={!!errors.fullName}
                    helperText={errors.fullName?.message}
                  />
                )}
              />
            </Grid>
            <Grid item xs={12}>
              <Controller
                name="email"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label="E-posta"
                    fullWidth
                    error={!!errors.email}
                    helperText={errors.email?.message}
                  />
                )}
              />
            </Grid>
            <Grid item xs={12}>
              <Controller
                name="userType"
                control={control}
                render={({ field }) => (
                  <FormControl fullWidth error={!!errors.userType}>
                    <InputLabel>Kullanıcı Tipi</InputLabel>
                    <Select {...field} label="Kullanıcı Tipi">
                      <MenuItem value="Staff">Personel (Staff)</MenuItem>
                      <MenuItem value="Admin">Yönetici (Admin)</MenuItem>
                    </Select>
                    <FormHelperText>{errors.userType?.message}</FormHelperText>
                  </FormControl>
                )}
              />
            </Grid>
            <Grid item xs={12}>
              <Controller
                name="password"
                control={control}
                render={({ field }) => (
                  <TextField
                    {...field}
                    label={isEditMode ? 'Yeni Şifre (Değiştirmek isterseniz)' : 'Şifre'}
                    type="password"
                    fullWidth
                    error={!!errors.password}
                    helperText={errors.password?.message}
                  />
                )}
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose} disabled={loading}>
            İptal
          </Button>
          <LoadingButton
            type="submit"
            variant="contained"
            loading={loading}
          >
            {isEditMode ? 'Güncelle' : 'Kaydet'}
          </LoadingButton>
        </DialogActions>
      </form>
    </Dialog>
  )
}

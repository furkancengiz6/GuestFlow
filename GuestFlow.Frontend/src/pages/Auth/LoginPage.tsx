import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  Container,
  Box,
  Card,
  CardContent,
  TextField,
  Button,
  Typography,
  Alert,
} from '@mui/material'
import { useForm } from 'react-hook-form'
import { useAuthStore } from '../../stores/authStore'
import apiClient from '../../services/api'
import { LoginRequest } from '../../types'

const LoginPage = () => {
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const { login } = useAuthStore()
  const navigate = useNavigate()

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginRequest>()

  const onSubmit = async (data: LoginRequest) => {
    setError(null)
    setLoading(true)

    try {
      // 1. Login isteği - Backend direkt LoginResponse döndürüyor (data wrapper yok)
      const response = await apiClient.post('/auth/login', data)
      const loginResponse = response.data
      const { accessToken, refreshToken } = loginResponse

      if (!accessToken || !refreshToken) {
        throw new Error('Token bilgileri alınamadı')
      }

      // 2. Token'ları önce authStore'a kaydet (interceptor için)
      login(accessToken, refreshToken, null)

      // 3. User bilgisini almak için /auth/me endpoint'ini çağır
      try {
        const userResponse = await apiClient.get('/auth/me')
        const userData = userResponse.data.data || userResponse.data
        
        // Backend UserInfoResponse döndürüyor, userType'ı role olarak map et
        const user = {
          id: userData.id,
          email: userData.email,
          fullName: userData.fullName,
          role: userData.userType || userData.role, // Backend'de userType, frontend'de role
          userType: userData.userType,
          createdDate: userData.createdDate,
        }

        // 4. Auth store'a user bilgisini güncelle
        const { login: updateLogin } = useAuthStore.getState()
        updateLogin(accessToken, refreshToken, user)
        navigate('/dashboard')
      } catch (userError: any) {
        // User bilgisi alınamazsa bile login yap, user null olabilir
        // Dashboard'da tekrar deneyebiliriz
        console.warn('User bilgisi alınamadı:', userError)
        // Token'lar zaten kaydedildi, sadece navigate et
        navigate('/dashboard')
      }
    } catch (err: any) {
      console.error('Login error:', err)
      const errorMessage =
        err.response?.data?.message ||
        err.message ||
        'Giriş yapılırken bir hata oluştu. Lütfen tekrar deneyin.'
      setError(errorMessage)
    } finally {
      setLoading(false)
    }
  }

  return (
    <Container maxWidth="sm">
      <Box
        sx={{
          minHeight: '100vh',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
        }}
      >
        <Card sx={{ width: '100%', maxWidth: 400 }}>
          <CardContent sx={{ p: 4 }}>
            <Typography variant="h4" component="h1" gutterBottom align="center" sx={{ mb: 3 }}>
              GuestFlow
            </Typography>
            <Typography variant="body2" color="text.secondary" align="center" sx={{ mb: 4 }}>
              Misafir Yönetim Sistemi
            </Typography>

            {error && (
              <Alert severity="error" sx={{ mb: 2 }}>
                {error}
              </Alert>
            )}

            <form onSubmit={handleSubmit(onSubmit)}>
              <TextField
                fullWidth
                label="E-posta"
                type="email"
                margin="normal"
                {...register('email', {
                  required: 'E-posta adresi gereklidir',
                  pattern: {
                    value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                    message: 'Geçerli bir e-posta adresi giriniz',
                  },
                })}
                error={!!errors.email}
                helperText={errors.email?.message}
                autoComplete="email"
              />

              <TextField
                fullWidth
                label="Şifre"
                type="password"
                margin="normal"
                {...register('password', {
                  required: 'Şifre gereklidir',
                  minLength: {
                    value: 6,
                    message: 'Şifre en az 6 karakter olmalıdır',
                  },
                })}
                error={!!errors.password}
                helperText={errors.password?.message}
                autoComplete="current-password"
              />

              <Button
                type="submit"
                fullWidth
                variant="contained"
                sx={{ mt: 3, mb: 2 }}
                disabled={loading}
              >
                {loading ? 'Giriş yapılıyor...' : 'Giriş Yap'}
              </Button>
            </form>
          </CardContent>
        </Card>
      </Box>
    </Container>
  )
}

export default LoginPage


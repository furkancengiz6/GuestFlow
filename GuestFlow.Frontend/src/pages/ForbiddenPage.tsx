import { Box, Typography, Button } from '@mui/material'
import { useNavigate, useLocation } from 'react-router-dom'

const ForbiddenPage = () => {
  const navigate = useNavigate()
  const location = useLocation()
  const from = (location.state as any)?.from?.pathname || '/dashboard'

  return (
    <Box
      sx={{
        minHeight: '60vh',
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        gap: 2,
        textAlign: 'center',
      }}
    >
      <Typography variant="h4">403 - Erişim Yok</Typography>
      <Typography color="text.secondary">
        Bu sayfaya erişim yetkiniz bulunmuyor. Yetkili bir hesapla giriş yapın veya başka bir sayfaya gidin.
      </Typography>
      <Box sx={{ display: 'flex', gap: 2, mt: 2 }}>
        <Button variant="contained" onClick={() => navigate(from)}>
          Geri dön
        </Button>
        <Button variant="outlined" onClick={() => navigate('/dashboard')}>
          Dashboard
        </Button>
      </Box>
    </Box>
  )
}

export default ForbiddenPage


import { Box, Button, Paper, Stack, Typography } from '@mui/material'

type Props = {
  error: Error
  onRetry: () => void
}

const QueryErrorFallback = ({ error, onRetry }: Props) => {
  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        p: 2,
      }}
    >
      <Paper elevation={3} sx={{ maxWidth: 520, width: '100%', p: 4 }}>
        <Stack spacing={2}>
          <Typography variant="h5" component="h2">
            Beklenmeyen bir hata oluştu
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {error.message || 'Lütfen tekrar deneyin.'}
          </Typography>
          <Stack direction="row" spacing={2} justifyContent="flex-end">
            <Button variant="outlined" onClick={() => window.location.reload()}>
              Sayfayı yenile
            </Button>
            <Button variant="contained" onClick={onRetry}>
              Tekrar dene
            </Button>
          </Stack>
        </Stack>
      </Paper>
    </Box>
  )
}

export default QueryErrorFallback


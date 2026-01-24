import React, { Component, ReactNode } from 'react'
import { Box, Typography, Button, Paper, Alert } from '@mui/material'
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline'
import RefreshIcon from '@mui/icons-material/Refresh'

interface Props {
  children: ReactNode
  onReset?: () => void
  fallback?: (error: Error, reset: () => void) => ReactNode
  showDetails?: boolean
}

interface State {
  error: Error | null
  errorInfo: React.ErrorInfo | null
}

class EnhancedErrorBoundary extends Component<Props, State> {
  state: State = {
    error: null,
    errorInfo: null,
  }

  static getDerivedStateFromError(error: Error): Partial<State> {
    return { error }
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    // Log the error for diagnostics
    console.error('Error caught by EnhancedErrorBoundary:', error, errorInfo)
    
    // You can send error to logging service here
    // Example: logErrorToService(error, errorInfo)
    
    this.setState({ errorInfo })
    try {
      // Persist last error details to localStorage for E2E diagnostics (development only)
      if (typeof window !== 'undefined' && window.localStorage) {
        const payload = {
          message: error?.message,
          stack: errorInfo?.componentStack,
          time: new Date().toISOString(),
        }
        window.localStorage.setItem('E2E_LAST_ERROR', JSON.stringify(payload))
      }
    } catch {
      // ignore storage errors
    }
  }

  reset = () => {
    this.props.onReset?.()
    try {
      if (typeof window !== 'undefined' && window.localStorage) {
        window.localStorage.removeItem('E2E_LAST_ERROR')
      }
    } catch {
      // ignore storage errors
    }
    this.setState({ error: null, errorInfo: null })
  }

  render() {
    const { error, errorInfo } = this.state
    const { children, fallback } = this.props
    // In dev/test environments, show error details by default unless explicitly overridden
    const showDetails = (this.props.showDetails ?? (process.env.DEV ?? false)) as boolean

    if (error) {
      if (fallback) {
        return fallback(error, this.reset)
      }

      return (
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            minHeight: '100vh',
            p: 3,
            bgcolor: 'background.default',
          }}
        >
          <Paper
            elevation={3}
            sx={{
              p: 4,
              maxWidth: 600,
              width: '100%',
              textAlign: 'center',
            }}
          >
            <ErrorOutlineIcon sx={{ fontSize: 64, color: 'error.main', mb: 2 }} />
            <Typography variant="h4" gutterBottom color="error">
              Bir Hata Oluştu
            </Typography>
            <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
              Üzgünüz, beklenmeyen bir hata oluştu. Lütfen sayfayı yenileyin veya tekrar deneyin.
            </Typography>

            {showDetails && errorInfo && (
              <Alert severity="info" sx={{ mb: 2, textAlign: 'left' }}>
                <Typography variant="caption" component="pre" sx={{ whiteSpace: 'pre-wrap', fontSize: '0.75rem' }}>
                  {error.toString()}
                  {errorInfo.componentStack}
                </Typography>
              </Alert>
            )}

            <Box sx={{ display: 'flex', gap: 2, justifyContent: 'center' }}>
              <Button variant="contained" startIcon={<RefreshIcon />} onClick={this.reset}>
                Sayfayı Yenile
              </Button>
              <Button variant="outlined" onClick={() => (window.location.href = '/dashboard')}>
                Ana Sayfaya Dön
              </Button>
            </Box>
          </Paper>
        </Box>
      )
    }

    return children
  }
}

export default EnhancedErrorBoundary


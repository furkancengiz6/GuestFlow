import { Box, Button, Typography, Stack } from '@mui/material'
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline'
import InboxIcon from '@mui/icons-material/Inbox'
import RefreshIcon from '@mui/icons-material/Refresh'
import AddIcon from '@mui/icons-material/Add'
import { SkeletonLoader } from './SkeletonLoader'

interface EnhancedContentStateProps {
  state: 'loading' | 'empty' | 'error'
  title?: string
  description?: string
  actionLabel?: string
  actionIcon?: React.ReactNode
  onAction?: () => void
  loadingVariant?: 'table' | 'card' | 'list' | 'form'
  loadingRows?: number
  emptyIcon?: React.ReactNode
  errorIcon?: React.ReactNode
}

/**
 * Enhanced content state component with better UX
 */
export const EnhancedContentState = ({
  state,
  title,
  description,
  actionLabel,
  actionIcon,
  onAction,
  loadingVariant = 'list',
  loadingRows = 3,
  emptyIcon,
  errorIcon,
}: EnhancedContentStateProps) => {
  if (state === 'loading') {
    return <SkeletonLoader variant={loadingVariant} rows={loadingRows} />
  }

  const isError = state === 'error'
  const defaultIcon = isError
    ? errorIcon || <ErrorOutlineIcon sx={{ fontSize: 64, color: 'error.main' }} />
    : emptyIcon || <InboxIcon sx={{ fontSize: 64, color: 'text.disabled' }} />

  const defaultTitle = isError
    ? title || 'Bir hata oluştu'
    : title || 'Henüz veri yok'

  const defaultDescription = isError
    ? description || 'Lütfen tekrar deneyin veya sayfayı yenileyin.'
    : description || 'Bu bölümde henüz veri bulunmuyor.'

  const defaultActionLabel = isError
    ? actionLabel || 'Tekrar Dene'
    : actionLabel || 'Yeni Ekle'

  const defaultActionIcon = isError ? <RefreshIcon /> : <AddIcon />

  return (
    <Box
      sx={{
        py: 8,
        px: 2,
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        gap: 2,
        textAlign: 'center',
        minHeight: 400,
      }}
    >
      <Box sx={{ color: isError ? 'error.main' : 'text.disabled' }}>{defaultIcon}</Box>

      <Stack spacing={1} alignItems="center">
        <Typography variant="h5" color="text.primary" fontWeight={600}>
          {defaultTitle}
        </Typography>
        <Typography variant="body1" color="text.secondary" maxWidth={400}>
          {defaultDescription}
        </Typography>
      </Stack>

      {onAction && (
        <Button
          variant={isError ? 'outlined' : 'contained'}
          onClick={onAction}
          startIcon={actionIcon || defaultActionIcon}
          sx={{ mt: 2 }}
        >
          {defaultActionLabel}
        </Button>
      )}
    </Box>
  )
}

export default EnhancedContentState


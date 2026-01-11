import { Box, Button, Typography } from '@mui/material'
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline'
import InboxIcon from '@mui/icons-material/Inbox'
import { SkeletonLoader } from './SkeletonLoader'

type Props = {
  state: 'loading' | 'empty' | 'error'
  title?: string
  description?: string
  actionLabel?: string
  onAction?: () => void
  skeletonLines?: number
}

const ContentState = ({
  state,
  title,
  description,
  actionLabel,
  onAction,
  skeletonLines = 3,
}: Props) => {
  if (state === 'loading') {
    return <SkeletonLoader variant="list" rows={skeletonLines} />
  }

  const isError = state === 'error'
  const icon = isError ? <ErrorOutlineIcon color="error" fontSize="large" /> : <InboxIcon color="disabled" fontSize="large" />

  return (
    <Box
      sx={{
        py: 6,
        px: 2,
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        gap: 1.5,
        textAlign: 'center',
        color: 'text.secondary',
      }}
    >
      {icon}
      {title && (
        <Typography variant="h6" color="text.primary">
          {title}
        </Typography>
      )}
      {description && <Typography>{description}</Typography>}
      {actionLabel && onAction && (
        <Button variant={isError ? 'outlined' : 'contained'} onClick={onAction} sx={{ mt: 1 }}>
          {actionLabel}
        </Button>
      )}
    </Box>
  )
}

export default ContentState


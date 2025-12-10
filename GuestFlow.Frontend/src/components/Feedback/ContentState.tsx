import { Box, Button, Skeleton, Stack, Typography } from '@mui/material'
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline'
import InboxIcon from '@mui/icons-material/Inbox'

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
    return (
      <Stack spacing={1}>
        {Array.from({ length: skeletonLines }).map((_, idx) => (
          <Skeleton key={idx} variant="rectangular" height={28} />
        ))}
      </Stack>
    )
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


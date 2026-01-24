import { Box, Skeleton, Stack, Card, CardContent, Grid } from '@mui/material'

interface SkeletonLoaderProps {
  variant?: 'table' | 'card' | 'list' | 'form' | 'custom'
  rows?: number
  columns?: number
  showAvatar?: boolean
  showActions?: boolean
}

/**
 * Enhanced skeleton loader component with multiple variants
 */
export const SkeletonLoader = ({
  variant = 'list',
  rows = 3,
  columns = 1,
  showAvatar = false,
  showActions = false,
}: SkeletonLoaderProps) => {
  if (variant === 'table') {
    return (
      <Box>
        {/* Table header */}
        <Stack direction="row" spacing={2} sx={{ mb: 2 }}>
          {Array.from({ length: columns }).map((_, idx) => (
            <Skeleton key={idx} variant="rectangular" width="100%" height={40} />
          ))}
        </Stack>
        {/* Table rows */}
        {Array.from({ length: rows }).map((_, rowIdx) => (
          <Stack key={rowIdx} direction="row" spacing={2} sx={{ mb: 1 }}>
            {Array.from({ length: columns }).map((_, colIdx) => (
              <Skeleton key={colIdx} variant="rectangular" width="100%" height={56} />
            ))}
          </Stack>
        ))}
      </Box>
    )
  }

  if (variant === 'card') {
    return (
      <Grid container spacing={2}>
        {Array.from({ length: rows }).map((_, idx) => (
          <Grid item xs={12} sm={6} md={4} key={idx}>
            <Card>
              <CardContent>
                <Stack spacing={2}>
                  {showAvatar && <Skeleton variant="circular" width={40} height={40} />}
                  <Skeleton variant="text" width="60%" height={24} />
                  <Skeleton variant="text" width="100%" height={20} />
                  <Skeleton variant="text" width="80%" height={20} />
                  {showActions && (
                    <Stack direction="row" spacing={1}>
                      <Skeleton variant="rectangular" width={80} height={36} />
                      <Skeleton variant="rectangular" width={80} height={36} />
                    </Stack>
                  )}
                </Stack>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>
    )
  }

  if (variant === 'form') {
    return (
      <Stack spacing={3}>
        {Array.from({ length: rows }).map((_, idx) => (
          <Box key={idx}>
            <Skeleton variant="text" width="30%" height={20} sx={{ mb: 1 }} />
            <Skeleton variant="rectangular" width="100%" height={56} />
          </Box>
        ))}
        {showActions && (
          <Stack direction="row" spacing={2} justifyContent="flex-end">
            <Skeleton variant="rectangular" width={100} height={36} />
            <Skeleton variant="rectangular" width={100} height={36} />
          </Stack>
        )}
      </Stack>
    )
  }

  // Default: list variant
  return (
    <Stack spacing={2} data-testid="skeleton-loader">
      {Array.from({ length: rows }).map((_, idx) => (
        <Box key={idx}>
          <Stack direction="row" spacing={2} alignItems="center">
            {showAvatar && <Skeleton variant="circular" width={40} height={40} />}
            <Box sx={{ flex: 1 }}>
              <Skeleton variant="text" width="60%" height={24} />
              <Skeleton variant="text" width="40%" height={20} />
            </Box>
            {showActions && <Skeleton variant="rectangular" width={80} height={36} />}
          </Stack>
        </Box>
      ))}
    </Stack>
  )
}

export default SkeletonLoader


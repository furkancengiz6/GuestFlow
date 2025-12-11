import { LinearProgress } from '@mui/material'
import { useIsFetching, useIsMutating } from '@tanstack/react-query'

const GlobalLoadingIndicator = () => {
  const fetchingCount = useIsFetching()
  const mutatingCount = useIsMutating()
  const visible = fetchingCount > 0 || mutatingCount > 0

  if (!visible) return null

  return (
    <LinearProgress
      color="secondary"
      sx={{ position: 'fixed', top: 0, left: 0, width: '100%', zIndex: 2000 }}
    />
  )
}

export default GlobalLoadingIndicator


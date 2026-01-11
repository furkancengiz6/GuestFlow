import { Chip, Tooltip } from '@mui/material'
import { Wifi, WifiOff } from '@mui/icons-material'
import { useSignalR } from '../../hooks/useSignalR'

/**
 * Connection status indicator component
 */
export const ConnectionStatus = () => {
  const { isConnected, connectionState } = useSignalR({ autoConnect: false })

  if (!isConnected) {
    return (
      <Tooltip title={`Bağlantı durumu: ${connectionState}`}>
        <Chip
          icon={<WifiOff />}
          label="Bağlantı Yok"
          color="error"
          size="small"
          sx={{ cursor: 'help' }}
        />
      </Tooltip>
    )
  }

  return (
    <Tooltip title={`Bağlantı durumu: ${connectionState}`}>
      <Chip
        icon={<Wifi />}
        label="Canlı"
        color="success"
        size="small"
        sx={{ cursor: 'help' }}
      />
    </Tooltip>
  )
}

export default ConnectionStatus


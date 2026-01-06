import { Snackbar, Alert, AlertColor, IconButton } from '@mui/material'
import CloseIcon from '@mui/icons-material/Close'
import { useEffect, useState } from 'react'

export interface ToastNotificationProps {
  open: boolean
  message: string
  severity?: AlertColor
  duration?: number
  onClose: () => void
  action?: React.ReactNode
}

/**
 * Toast notification component
 */
export const ToastNotification = ({
  open,
  message,
  severity = 'info',
  duration = 6000,
  onClose,
  action,
}: ToastNotificationProps) => {
  const [isOpen, setIsOpen] = useState(open)

  useEffect(() => {
    setIsOpen(open)
  }, [open])

  const handleClose = (_event?: React.SyntheticEvent | Event, reason?: string) => {
    if (reason === 'clickaway') {
      return
    }
    setIsOpen(false)
    onClose()
  }

  return (
    <Snackbar
      open={isOpen}
      autoHideDuration={duration}
      onClose={handleClose}
      anchorOrigin={{ vertical: 'top', horizontal: 'right' }}
      sx={{ mt: 8 }}
    >
      <Alert
        onClose={handleClose}
        severity={severity}
        variant="filled"
        action={
          action || (
            <IconButton size="small" aria-label="close" color="inherit" onClick={handleClose}>
              <CloseIcon fontSize="small" />
            </IconButton>
          )
        }
        sx={{ width: '100%' }}
      >
        {message}
      </Alert>
    </Snackbar>
  )
}

export default ToastNotification


import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  Button,
  Box,
  Typography,
} from '@mui/material'
import WarningIcon from '@mui/icons-material/Warning'
import ErrorOutlineIcon from '@mui/icons-material/ErrorOutline'
import InfoIcon from '@mui/icons-material/Info'
import CheckCircleIcon from '@mui/icons-material/CheckCircle'

export type ConfirmationType = 'warning' | 'error' | 'info' | 'success'

interface ConfirmationDialogProps {
  open: boolean
  title: string
  message: string
  type?: ConfirmationType
  confirmText?: string
  cancelText?: string
  onConfirm: () => void
  onCancel: () => void
  loading?: boolean
  destructive?: boolean // For delete operations
}

/**
 * Reusable confirmation dialog component
 */
export const ConfirmationDialog = ({
  open,
  title,
  message,
  type = 'warning',
  confirmText = 'Onayla',
  cancelText = 'İptal',
  onConfirm,
  onCancel,
  loading = false,
  destructive = false,
}: ConfirmationDialogProps) => {
  const getIcon = () => {
    switch (type) {
      case 'error':
        return <ErrorOutlineIcon color="error" sx={{ fontSize: 48 }} />
      case 'info':
        return <InfoIcon color="info" sx={{ fontSize: 48 }} />
      case 'success':
        return <CheckCircleIcon color="success" sx={{ fontSize: 48 }} />
      default:
        return <WarningIcon color="warning" sx={{ fontSize: 48 }} />
    }
  }

  const getConfirmColor = () => {
    if (destructive) return 'error'
    if (type === 'error') return 'error'
    if (type === 'success') return 'success'
    return 'primary'
  }

  return (
    <Dialog
      open={open}
      onClose={onCancel}
      maxWidth="sm"
      fullWidth
      aria-labelledby="confirmation-dialog-title"
      aria-describedby="confirmation-dialog-description"
    >
      <DialogTitle id="confirmation-dialog-title">
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          {getIcon()}
          <Typography variant="h6" component="span">
            {title}
          </Typography>
        </Box>
      </DialogTitle>
      <DialogContent>
        <DialogContentText id="confirmation-dialog-description" sx={{ mt: 1 }}>
          {message}
        </DialogContentText>
      </DialogContent>
      <DialogActions sx={{ px: 3, pb: 2 }}>
        <Button onClick={onCancel} disabled={loading} color="inherit">
          {cancelText}
        </Button>
        <Button
          onClick={onConfirm}
          disabled={loading}
          variant="contained"
          color={getConfirmColor()}
          autoFocus
        >
          {loading ? 'İşleniyor...' : confirmText}
        </Button>
      </DialogActions>
    </Dialog>
  )
}

export default ConfirmationDialog


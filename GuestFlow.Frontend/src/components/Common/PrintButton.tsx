import { Button, IconButton, Tooltip } from '@mui/material'
import PrintIcon from '@mui/icons-material/Print'

interface PrintButtonProps {
  variant?: 'button' | 'icon'
  onClick?: () => void
  label?: string
}

const PrintButton = ({ variant = 'button', onClick, label = 'Yazdır' }: PrintButtonProps) => {
  const handlePrint = () => {
    if (onClick) {
      onClick()
    } else {
      window.print()
    }
  }

  if (variant === 'icon') {
    return (
      <Tooltip title={label}>
        <IconButton onClick={handlePrint} size="small">
          <PrintIcon />
        </IconButton>
      </Tooltip>
    )
  }

  return (
    <Button variant="outlined" startIcon={<PrintIcon />} onClick={handlePrint}>
      {label}
    </Button>
  )
}

export default PrintButton


import { Box, Typography, Button } from '@mui/material'
import PrintIcon from '@mui/icons-material/Print'
import { ReactNode, useEffect } from 'react'

interface PrintViewProps {
  children: ReactNode
  title?: string
  showPrintButton?: boolean
  printButtonLabel?: string
}

/**
 * Print-friendly view component
 */
export const PrintView = ({
  children,
  title,
  showPrintButton = true,
  printButtonLabel = 'Yazdır',
}: PrintViewProps) => {
  const handlePrint = () => {
    window.print()
  }

  useEffect(() => {
    // Add print styles
    const style = document.createElement('style')
    style.textContent = `
      @media print {
        @page {
          margin: 1cm;
        }
        body * {
          visibility: hidden;
        }
        .print-view, .print-view * {
          visibility: visible;
        }
        .print-view {
          position: absolute;
          left: 0;
          top: 0;
          width: 100%;
        }
        .no-print {
          display: none !important;
        }
      }
    `
    document.head.appendChild(style)

    return () => {
      document.head.removeChild(style)
    }
  }, [])

  return (
    <Box className="print-view">
      {showPrintButton && (
        <Box className="no-print" sx={{ mb: 2, textAlign: 'right' }}>
          <Button
            variant="contained"
            startIcon={<PrintIcon />}
            onClick={handlePrint}
          >
            {printButtonLabel}
          </Button>
        </Box>
      )}
      {title && (
        <Typography variant="h4" component="h1" gutterBottom sx={{ mb: 3 }}>
          {title}
        </Typography>
      )}
      {children}
    </Box>
  )
}

export default PrintView


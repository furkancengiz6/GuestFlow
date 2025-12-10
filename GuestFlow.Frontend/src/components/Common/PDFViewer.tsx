import { Box, CircularProgress, Typography, Paper, IconButton, Tooltip } from '@mui/material'
import { useState } from 'react'
import DownloadIcon from '@mui/icons-material/Download'
import OpenInNewIcon from '@mui/icons-material/OpenInNew'
import CloseIcon from '@mui/icons-material/Close'
import Dialog from '@mui/material/Dialog'
import DialogContent from '@mui/material/DialogContent'

interface PDFViewerProps {
  url: string
  fileName?: string
  onClose?: () => void
  showDownload?: boolean
  showOpenInNew?: boolean
  fullScreen?: boolean
}

const PDFViewer = ({
  url,
  fileName,
  onClose,
  showDownload = true,
  showOpenInNew = true,
  fullScreen = false,
}: PDFViewerProps) => {
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(false)

  const handleDownload = () => {
    const link = document.createElement('a')
    link.href = url
    link.download = fileName || 'document.pdf'
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
  }

  const handleOpenInNew = () => {
    window.open(url, '_blank')
  }

  const content = (
    <Box sx={{ position: 'relative', width: '100%', height: '100%', minHeight: '600px' }}>
      {loading && (
        <Box
          sx={{
            position: 'absolute',
            top: '50%',
            left: '50%',
            transform: 'translate(-50%, -50%)',
            display: 'flex',
            flexDirection: 'column',
            alignItems: 'center',
            gap: 2,
          }}
        >
          <CircularProgress />
          <Typography>PDF yükleniyor...</Typography>
        </Box>
      )}
      {error && (
        <Box
          sx={{
            position: 'absolute',
            top: '50%',
            left: '50%',
            transform: 'translate(-50%, -50%)',
            textAlign: 'center',
          }}
        >
          <Typography color="error">PDF yüklenemedi</Typography>
        </Box>
      )}
      <iframe
        src={url}
        style={{
          width: '100%',
          height: '100%',
          border: 'none',
          display: error ? 'none' : 'block',
        }}
        onLoad={() => setLoading(false)}
        onError={() => {
          setLoading(false)
          setError(true)
        }}
        title={fileName || 'PDF Viewer'}
      />
      {!fullScreen && (showDownload || showOpenInNew || onClose) && (
        <Box
          sx={{
            position: 'absolute',
            top: 8,
            right: 8,
            display: 'flex',
            gap: 1,
            bgcolor: 'rgba(255, 255, 255, 0.9)',
            borderRadius: 1,
            p: 0.5,
          }}
        >
          {showDownload && (
            <Tooltip title="İndir">
              <IconButton size="small" onClick={handleDownload}>
                <DownloadIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
          {showOpenInNew && (
            <Tooltip title="Yeni sekmede aç">
              <IconButton size="small" onClick={handleOpenInNew}>
                <OpenInNewIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
          {onClose && (
            <Tooltip title="Kapat">
              <IconButton size="small" onClick={onClose}>
                <CloseIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
        </Box>
      )}
    </Box>
  )

  if (fullScreen) {
    return (
      <Dialog open={true} onClose={onClose} maxWidth="lg" fullWidth>
        <DialogContent sx={{ p: 0, height: '80vh' }}>
          {content}
        </DialogContent>
      </Dialog>
    )
  }

  return (
    <Paper sx={{ p: 2, height: '600px' }}>
      {content}
    </Paper>
  )
}

export default PDFViewer


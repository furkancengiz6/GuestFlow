import { useState } from 'react'
import {
  Dialog,
  DialogContent,
  DialogTitle,
  IconButton,
  Box,
  Typography,
  CircularProgress,
  Tooltip,
} from '@mui/material'
import {
  Close as CloseIcon,
  Download as DownloadIcon,
  OpenInNew as OpenInNewIcon,
  ZoomIn as ZoomInIcon,
  ZoomOut as ZoomOutIcon,
  Fullscreen as FullscreenIcon,
} from '@mui/icons-material'
import PDFViewer from './PDFViewer'

interface FilePreviewProps {
  open: boolean
  onClose: () => void
  fileUrl: string
  fileName?: string
  fileType?: string
  showDownload?: boolean
}

/**
 * Universal file preview component supporting PDF, images, and other file types
 */
export const FilePreview = ({
  open,
  onClose,
  fileUrl,
  fileName,
  fileType,
  showDownload = true,
}: FilePreviewProps) => {
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(false)
  const [zoom, setZoom] = useState(1)
  const [isFullscreen, setIsFullscreen] = useState(false)

  const detectedType = fileType || getFileType(fileUrl, fileName)

  const handleDownload = () => {
    const link = document.createElement('a')
    link.href = fileUrl
    link.download = fileName || 'file'
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
  }

  const handleOpenInNew = () => {
    window.open(fileUrl, '_blank')
  }

  const handleZoomIn = () => {
    setZoom((prev) => Math.min(prev + 0.25, 3))
  }

  const handleZoomOut = () => {
    setZoom((prev) => Math.max(prev - 0.25, 0.5))
  }

  const handleFullscreen = () => {
    setIsFullscreen((prev) => !prev)
  }

  const renderContent = () => {
    if (detectedType === 'pdf') {
      return (
        <PDFViewer
          url={fileUrl}
          fileName={fileName}
          onClose={onClose}
          showDownload={showDownload}
          showOpenInNew
          fullScreen={isFullscreen}
        />
      )
    }

    if (detectedType === 'image') {
      return (
        <Box
          sx={{
            position: 'relative',
            width: '100%',
            height: '100%',
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            overflow: 'auto',
            bgcolor: 'grey.100',
            p: 2,
          }}
        >
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
              <Typography>Resim yükleniyor...</Typography>
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
              <Typography color="error">Resim yüklenemedi</Typography>
            </Box>
          )}
          <img
            src={fileUrl}
            alt={fileName || 'Preview'}
            style={{
              maxWidth: `${100 * zoom}%`,
              maxHeight: `${100 * zoom}%`,
              objectFit: 'contain',
              display: error ? 'none' : 'block',
            }}
            onLoad={() => setLoading(false)}
            onError={() => {
              setLoading(false)
              setError(true)
            }}
          />
          {!loading && !error && (
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
              <Tooltip title="Yakınlaştır">
                <IconButton size="small" onClick={handleZoomIn} disabled={zoom >= 3}>
                  <ZoomInIcon fontSize="small" />
                </IconButton>
              </Tooltip>
              <Tooltip title="Uzaklaştır">
                <IconButton size="small" onClick={handleZoomOut} disabled={zoom <= 0.5}>
                  <ZoomOutIcon fontSize="small" />
                </IconButton>
              </Tooltip>
              <Tooltip title="Tam Ekran">
                <IconButton size="small" onClick={handleFullscreen}>
                  <FullscreenIcon fontSize="small" />
                </IconButton>
              </Tooltip>
              {showDownload && (
                <Tooltip title="İndir">
                  <IconButton size="small" onClick={handleDownload}>
                    <DownloadIcon fontSize="small" />
                  </IconButton>
                </Tooltip>
              )}
              <Tooltip title="Yeni sekmede aç">
                <IconButton size="small" onClick={handleOpenInNew}>
                  <OpenInNewIcon fontSize="small" />
                </IconButton>
              </Tooltip>
              <Tooltip title="Kapat">
                <IconButton size="small" onClick={onClose}>
                  <CloseIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            </Box>
          )}
        </Box>
      )
    }

    // Unsupported file type
    return (
      <Box
        sx={{
          p: 4,
          textAlign: 'center',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          gap: 2,
        }}
      >
        <Typography variant="h6" color="text.secondary">
          Bu dosya türü önizlenemiyor
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {fileName || 'Dosya'}
        </Typography>
        <Box sx={{ display: 'flex', gap: 1 }}>
          {showDownload && (
            <IconButton onClick={handleDownload} color="primary">
              <DownloadIcon />
            </IconButton>
          )}
          <IconButton onClick={handleOpenInNew} color="primary">
            <OpenInNewIcon />
          </IconButton>
        </Box>
      </Box>
    )
  }

  return (
    <Dialog
      open={open}
      onClose={onClose}
      maxWidth="lg"
      fullWidth
      fullScreen={isFullscreen}
      PaperProps={{
        sx: {
          height: isFullscreen ? '100vh' : '80vh',
        },
      }}
    >
      <DialogTitle sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Typography variant="h6">{fileName || 'Dosya Önizleme'}</Typography>
        <IconButton onClick={onClose} size="small">
          <CloseIcon />
        </IconButton>
      </DialogTitle>
      <DialogContent sx={{ p: 0, height: '100%', position: 'relative' }}>
        {renderContent()}
      </DialogContent>
    </Dialog>
  )
}

/**
 * Detect file type from URL or filename
 */
const getFileType = (url: string, fileName?: string): 'pdf' | 'image' | 'other' => {
  const name = fileName || url.toLowerCase()
  
  if (name.endsWith('.pdf')) {
    return 'pdf'
  }
  
  const imageExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp', '.svg']
  if (imageExtensions.some((ext) => name.endsWith(ext))) {
    return 'image'
  }
  
  return 'other'
}

export default FilePreview


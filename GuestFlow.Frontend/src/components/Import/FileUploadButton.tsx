import { Button, Box, Typography } from '@mui/material'
import { Upload as UploadIcon, CloudUpload as CloudUploadIcon } from '@mui/icons-material'
import { useRef, useState } from 'react'
import { useNotification } from '../../hooks/useNotification'

interface FileUploadButtonProps {
  onFileSelect: (file: File) => void
  accept?: string
  maxSize?: number // in MB
  label?: string
  variant?: 'text' | 'outlined' | 'contained'
  size?: 'small' | 'medium' | 'large'
  multiple?: boolean
  disabled?: boolean
}

/**
 * File upload button component
 */
export const FileUploadButton = ({
  onFileSelect,
  accept = '.xlsx,.xls,.csv',
  maxSize = 10, // 10 MB default
  label = 'Dosya Seç',
  variant = 'outlined',
  size = 'medium',
  multiple = false,
  disabled = false,
}: FileUploadButtonProps) => {
  const fileInputRef = useRef<HTMLInputElement>(null)
  const [dragActive, setDragActive] = useState(false)
  const notification = useNotification()

  const handleFileSelect = (files: FileList | null) => {
    if (!files || files.length === 0) return

    const file = files[0]
    const fileSizeMB = file.size / (1024 * 1024)

    if (fileSizeMB > maxSize) {
      notification.showError(`Dosya boyutu ${maxSize} MB'dan büyük olamaz.`)
      return
    }

    // Check file extension
    const fileName = file.name.toLowerCase()
    const acceptedExtensions = accept.split(',').map((ext) => ext.trim().replace('.', ''))
    const fileExtension = fileName.split('.').pop()

    if (!fileExtension || !acceptedExtensions.includes(fileExtension)) {
      notification.showError(
        `Geçersiz dosya formatı. İzin verilen formatlar: ${accept}`
      )
      return
    }

    onFileSelect(file)
  }

  const handleClick = () => {
    fileInputRef.current?.click()
  }

  const handleChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    handleFileSelect(event.target.files)
    // Reset input to allow selecting the same file again
    if (fileInputRef.current) {
      fileInputRef.current.value = ''
    }
  }

  const handleDrag = (e: React.DragEvent) => {
    e.preventDefault()
    e.stopPropagation()
    if (e.type === 'dragenter' || e.type === 'dragover') {
      setDragActive(true)
    } else if (e.type === 'dragleave') {
      setDragActive(false)
    }
  }

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault()
    e.stopPropagation()
    setDragActive(false)

    if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {
      handleFileSelect(e.dataTransfer.files)
    }
  }

  return (
    <Box>
      <input
        ref={fileInputRef}
        type="file"
        accept={accept}
        multiple={multiple}
        onChange={handleChange}
        style={{ display: 'none' }}
        disabled={disabled}
      />
      <Box
        onDragEnter={handleDrag}
        onDragLeave={handleDrag}
        onDragOver={handleDrag}
        onDrop={handleDrop}
        sx={{
          border: dragActive ? '2px dashed' : '2px dashed transparent',
          borderColor: dragActive ? 'primary.main' : 'transparent',
          borderRadius: 1,
          p: 2,
          transition: 'all 0.2s',
          backgroundColor: dragActive ? 'action.hover' : 'transparent',
        }}
      >
        <Button
          variant={variant}
          size={size}
          startIcon={<UploadIcon />}
          onClick={handleClick}
          disabled={disabled}
          fullWidth
        >
          {label}
        </Button>
        <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: 'block', textAlign: 'center' }}>
          veya dosyayı buraya sürükleyin
        </Typography>
        <Typography variant="caption" color="text.secondary" sx={{ mt: 0.5, display: 'block', textAlign: 'center' }}>
          Maksimum dosya boyutu: {maxSize} MB
        </Typography>
      </Box>
    </Box>
  )
}

export default FileUploadButton


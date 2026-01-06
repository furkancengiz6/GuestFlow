import { useState, useRef } from 'react'
import {
  Box,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  Typography,
  Button,
  IconButton,
  Tooltip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  InputAdornment,
  Chip,
  Grid,
  Card,
  CardContent,
} from '@mui/material'
import {
  Upload as UploadIcon,
  Download as DownloadIcon,
  Delete as DeleteIcon,
  Search as SearchIcon,
  Clear as ClearIcon,
  Folder as FolderIcon,
  InsertDriveFile as FileIcon,
} from '@mui/icons-material'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { fileService, FileFilters, FileInfo } from '../../services/fileService'
import { formatDate } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'
import { useNotification } from '../../hooks/useNotification'

const FilesPage = () => {
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
  const [uploadDialogOpen, setUploadDialogOpen] = useState(false)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [fileToDelete, setFileToDelete] = useState<FileInfo | null>(null)
  const [searchTerm, setSearchTerm] = useState('')
  const [selectedCategory, setSelectedCategory] = useState<string>('')
  const [uploadCategory, setUploadCategory] = useState<string>('')
  const fileInputRef = useRef<HTMLInputElement>(null)

  const queryClient = useQueryClient()
  const notification = useNotification()

  const filters: FileFilters = {
    ...(searchTerm && { searchTerm }),
    ...(selectedCategory && { category: selectedCategory }),
  }

  const { data, isLoading, error } = useQuery({
    queryKey: ['files', page + 1, rowsPerPage, filters],
    queryFn: () => fileService.getFiles(page + 1, rowsPerPage, filters),
  })

  const { data: categories } = useQuery({
    queryKey: ['file-categories'],
    queryFn: () => fileService.getCategories(),
  })

  const { data: statistics } = useQuery({
    queryKey: ['file-statistics'],
    queryFn: () => fileService.getStatistics(),
  })

  const uploadMutation = useMutation({
    mutationFn: (file: File) => fileService.uploadFile(file, uploadCategory || undefined),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['files'] })
      queryClient.invalidateQueries({ queryKey: ['file-statistics'] })
      setUploadDialogOpen(false)
      setUploadCategory('')
      if (fileInputRef.current) {
        fileInputRef.current.value = ''
      }
      notification.showSuccess('Dosya başarıyla yüklendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Dosya yüklenirken bir hata oluştu.')
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (fileName: string) => fileService.deleteFile(fileName),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['files'] })
      queryClient.invalidateQueries({ queryKey: ['file-statistics'] })
      setDeleteDialogOpen(false)
      setFileToDelete(null)
      notification.showSuccess('Dosya başarıyla silindi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Dosya silinirken bir hata oluştu.')
    },
  })

  const downloadMutation = useMutation({
    mutationFn: async (fileName: string) => {
      const blob = await fileService.downloadFile(fileName)
      const url = window.URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = fileName
      document.body.appendChild(a)
      a.click()
      window.URL.revokeObjectURL(url)
      document.body.removeChild(a)
    },
    onSuccess: () => {
      notification.showSuccess('Dosya indiriliyor...')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Dosya indirilirken bir hata oluştu.')
    },
  })

  const handleChangePage = (_event: unknown, newPage: number) => {
    setPage(newPage)
  }

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10))
    setPage(0)
  }

  const handleFileUpload = () => {
    const fileInput = fileInputRef.current
    if (fileInput && fileInput.files && fileInput.files.length > 0) {
      uploadMutation.mutate(fileInput.files[0])
    } else {
      notification.showError('Lütfen bir dosya seçin.')
    }
  }

  const handleDeleteClick = (file: FileInfo) => {
    setFileToDelete(file)
    setDeleteDialogOpen(true)
  }

  const handleDeleteConfirm = () => {
    if (fileToDelete) {
      deleteMutation.mutate(fileToDelete.fileName)
    }
  }

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 Bytes'
    const k = 1024
    const sizes = ['Bytes', 'KB', 'MB', 'GB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i]
  }

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={6} />
  }

  if (error) {
    return (
      <ContentState
        state="error"
        title="Dosyalar yüklenemedi"
        description="Lütfen daha sonra tekrar deneyin."
        actionLabel="Tekrar dene"
        onAction={() => queryClient.refetchQueries({ queryKey: ['files'] })}
      />
    )
  }

  const hasData = data && data.data && data.data.length > 0

  return (
    <Box p={3}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" sx={{ fontWeight: 600 }}>
          Dosya Yönetimi
        </Typography>
        <Button
          variant="contained"
          startIcon={<UploadIcon />}
          onClick={() => setUploadDialogOpen(true)}
        >
          Dosya Yükle
        </Button>
      </Box>

      {statistics && (
        <Grid container spacing={2} sx={{ mb: 3 }}>
          <Grid item xs={12} sm={6} md={4}>
            <Card>
              <CardContent>
                <Typography variant="body2" color="text.secondary">
                  Toplam Dosya
                </Typography>
                <Typography variant="h5" sx={{ fontWeight: 600 }}>
                  {statistics.totalFiles}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={4}>
            <Card>
              <CardContent>
                <Typography variant="body2" color="text.secondary">
                  Toplam Boyut
                </Typography>
                <Typography variant="h5" sx={{ fontWeight: 600 }}>
                  {formatFileSize(statistics.totalSize)}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      <Paper sx={{ p: 2, mb: 3 }}>
        <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
          <TextField
            placeholder="Ara (dosya adı)..."
            value={searchTerm}
            onChange={(e) => {
              setSearchTerm(e.target.value)
              setPage(0)
            }}
            size="small"
            sx={{ flex: 1 }}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon />
                </InputAdornment>
              ),
              endAdornment: searchTerm && (
                <InputAdornment position="end">
                  <IconButton
                    size="small"
                    onClick={() => {
                      setSearchTerm('')
                      setPage(0)
                    }}
                  >
                    <ClearIcon fontSize="small" />
                  </IconButton>
                </InputAdornment>
              ),
            }}
          />
          <TextField
            select
            label="Kategori"
            size="small"
            value={selectedCategory}
            onChange={(e) => {
              setSelectedCategory(e.target.value)
              setPage(0)
            }}
            SelectProps={{
              native: true,
            }}
            sx={{ minWidth: 150 }}
          >
            <option value="">Tüm Kategoriler</option>
            {categories?.map((category) => (
              <option key={category} value={category}>
                {category}
              </option>
            ))}
          </TextField>
        </Box>
      </Paper>

      {!hasData ? (
        <ContentState
          state="empty"
          title="Dosya bulunamadı"
          description="Henüz yüklenmiş dosya bulunmamaktadır."
        />
      ) : (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell><strong>Dosya Adı</strong></TableCell>
                <TableCell><strong>Kategori</strong></TableCell>
                <TableCell><strong>Boyut</strong></TableCell>
                <TableCell><strong>Tip</strong></TableCell>
                <TableCell><strong>Yükleme Tarihi</strong></TableCell>
                <TableCell><strong>İşlemler</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {data?.data.map((file) => (
                <TableRow key={file.fileName} hover>
                  <TableCell>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <FileIcon fontSize="small" />
                      {file.fileName}
                    </Box>
                  </TableCell>
                  <TableCell>
                    <Chip label={file.category} size="small" icon={<FolderIcon />} />
                  </TableCell>
                  <TableCell>{formatFileSize(file.fileSize)}</TableCell>
                  <TableCell>{file.contentType}</TableCell>
                  <TableCell>{formatDate(file.uploadedDate)}</TableCell>
                  <TableCell>
                    <Box sx={{ display: 'flex', gap: 1 }}>
                      <Tooltip title="İndir">
                        <IconButton
                          size="small"
                          color="primary"
                          onClick={() => downloadMutation.mutate(file.fileName)}
                        >
                          <DownloadIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Sil">
                        <IconButton
                          size="small"
                          color="error"
                          onClick={() => handleDeleteClick(file)}
                        >
                          <DeleteIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </Box>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
          <TablePagination
            component="div"
            count={data?.totalCount || 0}
            page={page}
            onPageChange={handleChangePage}
            rowsPerPage={rowsPerPage}
            onRowsPerPageChange={handleChangeRowsPerPage}
            rowsPerPageOptions={[5, 10, 25, 50]}
            labelRowsPerPage="Sayfa başına:"
            labelDisplayedRows={({ from, to, count }) => `${from}-${to} / ${count}`}
          />
        </TableContainer>
      )}

      <Dialog open={uploadDialogOpen} onClose={() => setUploadDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Dosya Yükle</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
            <TextField
              select
              label="Kategori (Opsiyonel)"
              fullWidth
              value={uploadCategory}
              onChange={(e) => setUploadCategory(e.target.value)}
              SelectProps={{
                native: true,
              }}
            >
              <option value="">Kategori Seçiniz (Opsiyonel)</option>
              {categories?.map((category) => (
                <option key={category} value={category}>
                  {category}
                </option>
              ))}
            </TextField>
            <input
              ref={fileInputRef}
              type="file"
              style={{ display: 'none' }}
              onChange={(e) => {
                if (e.target.files && e.target.files.length > 0) {
                  // File selected, ready to upload
                }
              }}
            />
            <Button
              variant="outlined"
              component="label"
              startIcon={<UploadIcon />}
              fullWidth
            >
              Dosya Seç
              <input
                type="file"
                hidden
                ref={fileInputRef}
                onChange={(e) => {
                  if (e.target.files && e.target.files.length > 0) {
                    // File selected
                  }
                }}
              />
            </Button>
            {fileInputRef.current?.files && fileInputRef.current.files.length > 0 && (
              <Typography variant="body2" color="text.secondary">
                Seçilen dosya: {fileInputRef.current.files[0].name}
              </Typography>
            )}
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setUploadDialogOpen(false)} disabled={uploadMutation.isPending}>
            İptal
          </Button>
          <Button
            onClick={handleFileUpload}
            variant="contained"
            disabled={uploadMutation.isPending}
          >
            {uploadMutation.isPending ? 'Yükleniyor...' : 'Yükle'}
          </Button>
        </DialogActions>
      </Dialog>

      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>Dosya Sil</DialogTitle>
        <DialogContent>
          <Typography>
            {fileToDelete && (
              <>
                <strong>{fileToDelete.fileName}</strong> dosyasını silmek istediğinize emin
                misiniz? Bu işlem geri alınamaz.
              </>
            )}
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)} disabled={deleteMutation.isPending}>
            İptal
          </Button>
          <Button
            onClick={handleDeleteConfirm}
            color="error"
            variant="contained"
            disabled={deleteMutation.isPending}
          >
            {deleteMutation.isPending ? 'Siliniyor...' : 'Sil'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

export default FilesPage


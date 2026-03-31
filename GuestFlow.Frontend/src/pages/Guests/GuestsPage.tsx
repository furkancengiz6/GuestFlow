import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useLiveUpdates } from '../../hooks/useLiveUpdates'
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
  Chip,
  Button,
  IconButton,
  Tooltip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  TextField,
  InputAdornment,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Grid,
  Collapse,
  Card,
  CardContent,
} from '@mui/material'
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Visibility as VisibilityIcon,
  Search as SearchIcon,
  FilterList as FilterListIcon,
  Clear as ClearIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon,
} from '@mui/icons-material'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { guestService, CreateGuestRequest, UpdateGuestRequest, GuestFilters } from '../../services/guestService'
import { formatDate } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'
import GuestForm from '../../components/Guests/GuestForm'
import { Guest } from '../../types/guest'
import { useNotification } from '../../hooks/useNotification'
import { exportService, GuestExportFilters } from '../../services/exportService'
import FileDownloadIcon from '@mui/icons-material/FileDownload'

const GuestsPage = () => {
  const navigate = useNavigate()

  // Enable real-time updates for guest changes
  useLiveUpdates(['guest'])

  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
  const [formOpen, setFormOpen] = useState(false)
  const [editingGuest, setEditingGuest] = useState<Guest | null>(null)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [guestToDelete, setGuestToDelete] = useState<Guest | null>(null)
  const [filtersOpen, setFiltersOpen] = useState(false)

  // Filter states
  const [searchTerm, setSearchTerm] = useState('')
  const [nationality, setNationality] = useState('')
  const [isSpecialGuest, setIsSpecialGuest] = useState<boolean | undefined>(undefined)
  const [sortBy, setSortBy] = useState('')
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc')

  const queryClient = useQueryClient()
  const notification = useNotification()

  // Build filters object
  const filters: GuestFilters = {
    ...(searchTerm && { searchTerm }),
    ...(nationality && { nationality }),
    ...(isSpecialGuest !== undefined && { isSpecialGuest }),
    ...(sortBy && { sortBy }),
    ...(sortOrder && { sortOrder }),
  }

  const { data, isLoading, error } = useQuery({
    queryKey: ['guests', page + 1, rowsPerPage, filters],
    queryFn: () => guestService.getGuests(page + 1, rowsPerPage, filters),
  })

  const createMutation = useMutation({
    mutationFn: (data: CreateGuestRequest) => guestService.createGuest(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['guests'] })
      setFormOpen(false)
      notification.showSuccess('Misafir başarıyla eklendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Misafir eklenirken bir hata oluştu.')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateGuestRequest }) =>
      guestService.updateGuest(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['guests'] })
      setFormOpen(false)
      setEditingGuest(null)
      notification.showSuccess('Misafir başarıyla güncellendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Misafir güncellenirken bir hata oluştu.')
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (id: number) => guestService.deleteGuest(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['guests'] })
      setDeleteDialogOpen(false)
      setGuestToDelete(null)
      notification.showSuccess('Misafir başarıyla silindi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Misafir silinirken bir hata oluştu.')
    },
  })

  const handleChangePage = (_event: unknown, newPage: number) => {
    setPage(newPage)
  }

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10))
    setPage(0)
  }

  const handleOpenForm = (guest?: Guest) => {
    if (guest) {
      setEditingGuest(guest)
    } else {
      setEditingGuest(null)
    }
    setFormOpen(true)
  }

  const handleCloseForm = () => {
    setFormOpen(false)
    setEditingGuest(null)
  }

  const handleFormSubmit = async (data: CreateGuestRequest | UpdateGuestRequest) => {
    if (editingGuest) {
      await updateMutation.mutateAsync({ id: editingGuest.id, data })
    } else {
      await createMutation.mutateAsync(data)
    }
  }

  const handleDeleteClick = (guest: Guest) => {
    setGuestToDelete(guest)
    setDeleteDialogOpen(true)
  }

  const handleDeleteConfirm = () => {
    if (guestToDelete) {
      deleteMutation.mutate(guestToDelete.id)
    }
  }

  const handleClearFilters = () => {
    setSearchTerm('')
    setNationality('')
    setIsSpecialGuest(undefined)
    setSortBy('')
    setSortOrder('asc')
    setPage(0)
  }

  const hasActiveFilters = searchTerm || nationality || isSpecialGuest !== undefined || sortBy

  // Get unique nationalities from data for filter dropdown
  const nationalities = Array.from(
    new Set(data?.data.map((g) => g.nationality).filter(Boolean) || [])
  ).sort()

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={6} />
  }

  if (error) {
    return (
      <ContentState
        state="error"
        title="Misafirler yüklenemedi"
        description="Lütfen daha sonra tekrar deneyin."
        actionLabel="Tekrar dene"
        onAction={() => queryClient.refetchQueries({ queryKey: ['guests'] })}
      />
    )
  }

  const hasData = data && data.data && data.data.length > 0
  return (
    <Box className="fade-in" p={3}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" component="h1" className="premium-gradient-text" sx={{ fontWeight: 800 }}>
          Misafir Yönetimi
        </Typography>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button
            variant="outlined"
            startIcon={<FileDownloadIcon />}
            onClick={async () => {
              try {
                const exportFilters: GuestExportFilters = {
                  ...(searchTerm && { searchTerm }),
                  ...(nationality && { nationality }),
                  ...(isSpecialGuest !== undefined && { isSpecialGuest }),
                }
                await exportService.exportGuestsToExcel(exportFilters)
                notification.showSuccess('Excel dosyası indiriliyor...')
              } catch (error: any) {
                notification.showError(error?.response?.data?.message || 'Dışa aktarma başarısız oldu.')
              }
            }}
          >
            Excel
          </Button>
          <Button
            variant="outlined"
            startIcon={<FileDownloadIcon />}
            onClick={async () => {
              try {
                const exportFilters: GuestExportFilters = {
                  ...(searchTerm && { searchTerm }),
                  ...(nationality && { nationality }),
                  ...(isSpecialGuest !== undefined && { isSpecialGuest }),
                }
                await exportService.exportGuestsToCsv(exportFilters)
                notification.showSuccess('CSV dosyası indiriliyor...')
              } catch (error: any) {
                notification.showError(error?.response?.data?.message || 'Dışa aktarma başarısız oldu.')
              }
            }}
          >
            CSV
          </Button>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => handleOpenForm()}
          >
            Yeni Misafir
          </Button>
        </Box>
      </Box>

      {/* Search and Filter Section */}
      <Card className="glass-panel" sx={{ p: 2, mb: 3 }}>
        <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', mb: 2 }}>
          <TextField
            placeholder="Ara (isim, email, telefon, misafir kodu)..."
            value={searchTerm}
            onChange={(e) => {
              setSearchTerm(e.target.value)
              setPage(0)
            }}
            size="small"
            fullWidth
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
          <Button
            variant={filtersOpen ? 'contained' : 'outlined'}
            startIcon={<FilterListIcon />}
            endIcon={filtersOpen ? <ExpandLessIcon /> : <ExpandMoreIcon />}
            onClick={() => setFiltersOpen(!filtersOpen)}
          >
            Filtreler
          </Button>
          {hasActiveFilters && (
            <Button
              variant="outlined"
              color="error"
              startIcon={<ClearIcon />}
              onClick={handleClearFilters}
            >
              Temizle
            </Button>
          )}
        </Box>

        <Collapse in={filtersOpen}>
          <Grid container spacing={2}>
            <Grid item xs={12} sm={6} md={3}>
              <FormControl fullWidth size="small">
                <InputLabel>Uyruk</InputLabel>
                <Select
                  value={nationality}
                  label="Uyruk"
                  onChange={(e) => {
                    setNationality(e.target.value)
                    setPage(0)
                  }}
                >
                  <MenuItem value="">Tümü</MenuItem>
                  {nationalities.map((nat) => (
                    <MenuItem key={nat} value={nat}>
                      {nat}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <FormControl fullWidth size="small">
                <InputLabel>Özel Misafir</InputLabel>
                <Select
                  value={isSpecialGuest === undefined ? '' : isSpecialGuest ? 'true' : 'false'}
                  label="Özel Misafir"
                  onChange={(e) => {
                    const value = e.target.value
                    setIsSpecialGuest(
                      value === '' ? undefined : value === 'true'
                    )
                    setPage(0)
                  }}
                >
                  <MenuItem value="">Tümü</MenuItem>
                  <MenuItem value="true">Evet</MenuItem>
                  <MenuItem value="false">Hayır</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <FormControl fullWidth size="small">
                <InputLabel>Sırala</InputLabel>
                <Select
                  value={sortBy}
                  label="Sırala"
                  onChange={(e) => {
                    setSortBy(e.target.value)
                    setPage(0)
                  }}
                >
                  <MenuItem value="">Varsayılan</MenuItem>
                  <MenuItem value="FullName">Ad Soyad</MenuItem>
                  <MenuItem value="Email">E-posta</MenuItem>
                  <MenuItem value="Nationality">Uyruk</MenuItem>
                  <MenuItem value="CreatedDate">Kayıt Tarihi</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            {sortBy && (
              <Grid item xs={12} sm={6} md={3}>
                <FormControl fullWidth size="small">
                  <InputLabel>Sıralama Yönü</InputLabel>
                  <Select
                    value={sortOrder}
                    label="Sıralama Yönü"
                    onChange={(e) => {
                      setSortOrder(e.target.value as 'asc' | 'desc')
                      setPage(0)
                    }}
                  >
                    <MenuItem value="asc">Artan (A-Z)</MenuItem>
                    <MenuItem value="desc">Azalan (Z-A)</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
            )}
          </Grid>
        </Collapse>
      </Card>

      {!hasData ? (
        <ContentState
          state="empty"
          title="Misafir bulunamadı"
          description="Henüz kayıtlı misafir bulunmamaktadır."
        />
      ) : (
        <Card className="glass-panel">
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell><strong>Misafir Kodu</strong></TableCell>
                  <TableCell><strong>Ad Soyad</strong></TableCell>
                  <TableCell><strong>Email</strong></TableCell>
                  <TableCell><strong>Telefon</strong></TableCell>
                  <TableCell><strong>Uyruk</strong></TableCell>
                  <TableCell><strong>Özel Misafir</strong></TableCell>
                  <TableCell><strong>Kayıt Tarihi</strong></TableCell>
                  <TableCell><strong>İşlemler</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data?.data.map((guest) => (
                  <TableRow key={guest.id} hover>
                    <TableCell>{guest.guestCode}</TableCell>
                    <TableCell>
                      <Button
                        variant="text"
                        onClick={() => navigate(`/guests/${guest.id}`)}
                        sx={{ textTransform: 'none', fontWeight: 500 }}
                      >
                        {guest.fullName}
                      </Button>
                    </TableCell>
                    <TableCell>{guest.email || '-'}</TableCell>
                    <TableCell>{guest.phoneNumber || '-'}</TableCell>
                    <TableCell>{guest.nationality}</TableCell>
                    <TableCell>
                      {guest.isSpecialGuest ? (
                        <Chip label="Evet" color="primary" size="small" />
                      ) : (
                        <Chip label="Hayır" size="small" />
                      )}
                    </TableCell>
                    <TableCell>{formatDate(guest.createdDate)}</TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', gap: 1 }}>
                        <Tooltip title="Detay">
                          <IconButton
                            size="small"
                            color="primary"
                            onClick={() => navigate(`/guests/${guest.id}`)}
                          >
                            <VisibilityIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Düzenle">
                          <IconButton
                            size="small"
                            color="primary"
                            onClick={() => handleOpenForm(guest)}
                          >
                            <EditIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Sil">
                          <IconButton
                            size="small"
                            color="error"
                            onClick={() => handleDeleteClick(guest)}
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
        </Card>
      )}

      <GuestForm
        open={formOpen}
        onClose={handleCloseForm}
        onSubmit={handleFormSubmit}
        guest={editingGuest}
        isLoading={createMutation.isPending || updateMutation.isPending}
      />

      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>Misafir Sil</DialogTitle>
        <DialogContent>
          <DialogContentText>
            {guestToDelete && (
              <>
                <strong>{guestToDelete.fullName}</strong> adlı misafiri silmek istediğinize emin
                misiniz? Bu işlem geri alınamaz.
              </>
            )}
          </DialogContentText>
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
    </Box >
  )
}

export default GuestsPage


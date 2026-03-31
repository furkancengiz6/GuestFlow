import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
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
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns'
import { tr } from 'date-fns/locale'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { personnelService, Personnel, PersonnelFilters } from '../../services/personnelService'
import { formatDate } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'
import { PersonnelForm } from '../../components/Personnel/PersonnelForm'
import { useNotification } from '../../hooks/useNotification'

const PersonnelPage = () => {
  const navigate = useNavigate()
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
  const [filtersOpen, setFiltersOpen] = useState(false)

  // Form states
  const [formOpen, setFormOpen] = useState(false)
  const [editingPersonnel, setEditingPersonnel] = useState<Personnel | null>(null)

  // Filter states
  const [searchTerm, setSearchTerm] = useState('')
  const [userType, setUserType] = useState('')
  const [startDate, setStartDate] = useState<Date | null>(null)
  const [endDate, setEndDate] = useState<Date | null>(null)
  const [sortBy, setSortBy] = useState('')
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc')

  const queryClient = useQueryClient()
  const notification = useNotification()

  // Build filters object
  const filters: PersonnelFilters = {
    ...(searchTerm && { searchTerm }),
    ...(userType && { userType }),
    ...(startDate && { startDate: startDate.toISOString().split('T')[0] }),
    ...(endDate && { endDate: endDate.toISOString().split('T')[0] }),
    ...(sortBy && { sortBy }),
    ...(sortOrder && { sortOrder }),
  }

  const { data, isLoading, error } = useQuery({
    queryKey: ['personnel', page + 1, rowsPerPage, filters],
    queryFn: () => personnelService.getPersonnel(page + 1, rowsPerPage, filters),
  })

  const deleteMutation = useMutation({
    mutationFn: (id: number) => personnelService.deletePersonnel(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['personnel'] })
      setDeleteDialogOpen(false)
      setPersonnelToDelete(null)
      notification.showSuccess('Personel başarıyla silindi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Personel silinirken bir hata oluştu.')
    },
  })

  const createMutation = useMutation({
    mutationFn: (data: any) => personnelService.createPersonnel(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['personnel'] })
      setFormOpen(false)
      notification.showSuccess('Personel başarıyla oluşturuldu.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Personel oluşturulurken bir hata oluştu.')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: any }) => personnelService.updatePersonnel(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['personnel'] })
      setFormOpen(false)
      setEditingPersonnel(null)
      notification.showSuccess('Personel başarıyla güncellendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Personel güncellenirken bir hata oluştu.')
    },
  })

  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [personnelToDelete, setPersonnelToDelete] = useState<Personnel | null>(null)

  const handleChangePage = (_event: unknown, newPage: number) => {
    setPage(newPage)
  }

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10))
    setPage(0)
  }

  const handleDeleteClick = (personnel: Personnel) => {
    setPersonnelToDelete(personnel)
    setDeleteDialogOpen(true)
  }

  const handleDeleteConfirm = () => {
    if (personnelToDelete) {
      deleteMutation.mutate(personnelToDelete.id)
    }
  }

  const handleClearFilters = () => {
    setSearchTerm('')
    setUserType('')
    setStartDate(null)
    setEndDate(null)
    setSortBy('')
    setSortOrder('asc')
    setPage(0)
  }

  const hasActiveFilters = searchTerm || userType || startDate || endDate || sortBy

  const getUserTypeLabel = (type: string | number) => {
    if (type === undefined || type === null) return 'Bilinmiyor'
    const typeStr = String(type).toLowerCase()
    switch (typeStr) {
      case '0':
      case 'owner':
        return 'Mülk Sahibi'
      case '1':
      case 'manager':
        return 'Yönetici'
      case '2':
      case 'admin':
        return 'Admin'
      case '3':
      case 'concierge':
        return 'Concierge'
      case '4':
      case 'reception':
        return 'Resepsiyon'
      case '5':
      case 'staff':
        return 'Personel'
      default:
        return typeStr
    }
  }

  const getUserTypeColor = (type: string | number) => {
    if (type === undefined || type === null) return 'default'
    const typeStr = String(type).toLowerCase()
    switch (typeStr) {
      case '0':
      case 'owner':
        return 'secondary'
      case '1':
      case 'manager':
        return 'warning'
      case '2':
      case 'admin':
        return 'error'
      case '3':
      case 'concierge':
        return 'info'
      case '4':
      case 'reception':
        return 'success'
      case '5':
      case 'staff':
        return 'primary'
      default:
        return 'default'
    }
  }

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={6} />
  }

  if (error) {
    return (
      <ContentState
        state="error"
        title="Personel yüklenemedi"
        description="Lütfen daha sonra tekrar deneyin."
        actionLabel="Tekrar dene"
        onAction={() => queryClient.refetchQueries({ queryKey: ['personnel'] })}
      />
    )
  }

  const hasData = data && data.data && data.data.length > 0

  return (
    <Box p={3}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" sx={{ fontWeight: 600 }}>
          Personel
        </Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => {
            setEditingPersonnel(null)
            setFormOpen(true)
          }}
        >
          Yeni Personel
        </Button>
      </Box>

      <PersonnelForm
        open={formOpen}
        onClose={() => {
          setFormOpen(false)
          setEditingPersonnel(null)
        }}
        onSubmit={(data) => {
          if (editingPersonnel) {
            updateMutation.mutate({ id: editingPersonnel.id, data })
          } else {
            createMutation.mutate(data as any)
          }
        }}
        initialData={editingPersonnel}
        loading={createMutation.isPending || updateMutation.isPending}
      />

      {/* Search and Filter Section */}
      <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={tr}>
        <Paper sx={{ p: 2, mb: 3 }}>
          <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', mb: 2 }}>
            <TextField
              placeholder="Ara (isim, email)..."
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
                <FormControl size="small" sx={{ minWidth: 150 }}>
                  <InputLabel id="role-filter-label">Rol Filtresi</InputLabel>
                  <Select
                    labelId="role-filter-label"
                    value={userType || ''}
                    label="Rol Filtresi"
                    onChange={(e) => setUserType(e.target.value)}
                  >
                    <MenuItem value="">Hepsi</MenuItem>
                    <MenuItem value="Owner">Mülk Sahibi</MenuItem>
                    <MenuItem value="Manager">Yönetici</MenuItem>
                    <MenuItem value="Admin">Admin</MenuItem>
                    <MenuItem value="Concierge">Concierge</MenuItem>
                    <MenuItem value="Reception">Resepsiyon</MenuItem>
                    <MenuItem value="Staff">Personel</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <DatePicker
                  label="Başlangıç Tarihi"
                  value={startDate}
                  onChange={(newValue) => {
                    setStartDate(newValue)
                    setPage(0)
                  }}
                  slotProps={{
                    textField: { size: 'small', fullWidth: true },
                  }}
                />
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <DatePicker
                  label="Bitiş Tarihi"
                  value={endDate}
                  onChange={(newValue) => {
                    setEndDate(newValue)
                    setPage(0)
                  }}
                  slotProps={{
                    textField: { size: 'small', fullWidth: true },
                  }}
                />
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
                    <MenuItem value="UserType">Kullanıcı Tipi</MenuItem>
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
        </Paper>
      </LocalizationProvider>

      {!hasData ? (
        <ContentState
          state="empty"
          title="Personel bulunamadı"
          description="Henüz kayıtlı personel bulunmamaktadır."
        />
      ) : (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell><strong>Ad Soyad</strong></TableCell>
                <TableCell><strong>Email</strong></TableCell>
                <TableCell><strong>Kullanıcı Tipi</strong></TableCell>
                <TableCell><strong>Kayıt Tarihi</strong></TableCell>
                <TableCell><strong>İşlemler</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {data?.data.map((personnel) => (
                <TableRow key={personnel.id} hover>
                  <TableCell>
                    <Button
                      variant="text"
                      onClick={() => navigate(`/personnel/${personnel.id}`)}
                      sx={{ textTransform: 'none', fontWeight: 500 }}
                    >
                      {personnel.fullName}
                    </Button>
                  </TableCell>
                  <TableCell>{personnel.email}</TableCell>
                  <TableCell>
                    <Chip
                      label={getUserTypeLabel(personnel.userType)}
                      color={getUserTypeColor(personnel.userType) as any}
                      size="small"
                    />
                  </TableCell>
                  <TableCell>{formatDate(personnel.createdDate)}</TableCell>
                  <TableCell>
                    <Box sx={{ display: 'flex', gap: 1 }}>
                      <Tooltip title="Detay">
                        <IconButton
                          size="small"
                          color="primary"
                          onClick={() => navigate(`/personnel/${personnel.id}`)}
                        >
                          <VisibilityIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Düzenle">
                        <IconButton
                          size="small"
                          color="primary"
                          onClick={() => {
                            setEditingPersonnel(personnel)
                            setFormOpen(true)
                          }}
                        >
                          <EditIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Sil">
                        <IconButton
                          size="small"
                          color="error"
                          onClick={() => handleDeleteClick(personnel)}
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

      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>Personel Sil</DialogTitle>
        <DialogContent>
          <DialogContentText>
            {personnelToDelete && (
              <>
                <strong>{personnelToDelete.fullName}</strong> adlı personeli silmek istediğinizden emin misiniz?
                <br />
                Bu işlem geri alınamaz.
              </>
            )}
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>İptal</Button>
          <Button onClick={handleDeleteConfirm} color="error" variant="contained" disabled={deleteMutation.isPending}>
            {deleteMutation.isPending ? 'Siliniyor...' : 'Sil'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

export default PersonnelPage


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
  Chip,
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
  Search as SearchIcon,
  Clear as ClearIcon,
  FilterList as FilterListIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon,
  Timeline as TimelineIcon,
} from '@mui/icons-material'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { itineraryService, CreateItineraryRequest, UpdateItineraryRequest, ItineraryFilters, Itinerary } from '../../services/itineraryService'
import { formatDate, formatCurrency } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'
import ItineraryForm from '../../components/Itineraries/ItineraryForm'
import { useNotification } from '../../hooks/useNotification'

const ItinerariesPage = () => {
  const navigate = useNavigate()
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
  const [formOpen, setFormOpen] = useState(false)
  const [editingItinerary, setEditingItinerary] = useState<Itinerary | null>(null)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [itineraryToDelete, setItineraryToDelete] = useState<Itinerary | null>(null)
  const [filtersOpen, setFiltersOpen] = useState(false)
  
  // Filter states
  const [searchTerm, setSearchTerm] = useState('')
  const [status, setStatus] = useState('')
  const [sortBy, setSortBy] = useState('')
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('desc')

  const queryClient = useQueryClient()
  const notification = useNotification()

  // Build filters object
  const filters: ItineraryFilters = {
    ...(searchTerm && { searchTerm }),
    ...(status && { status }),
    ...(sortBy && { sortBy }),
    ...(sortOrder && { sortOrder }),
  }

  const { data, isLoading, error } = useQuery({
    queryKey: ['itineraries', page + 1, rowsPerPage, filters],
    queryFn: () => itineraryService.getItineraries(page + 1, rowsPerPage, filters),
  })

  const createMutation = useMutation({
    mutationFn: (data: CreateItineraryRequest) => itineraryService.createItinerary(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['itineraries'] })
      setFormOpen(false)
      notification.showSuccess('İtinerary başarıyla oluşturuldu.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'İtinerary oluşturulurken bir hata oluştu.')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateItineraryRequest }) =>
      itineraryService.updateItinerary(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['itineraries'] })
      setFormOpen(false)
      setEditingItinerary(null)
      notification.showSuccess('İtinerary başarıyla güncellendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'İtinerary güncellenirken bir hata oluştu.')
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (id: number) => itineraryService.deleteItinerary(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['itineraries'] })
      setDeleteDialogOpen(false)
      setItineraryToDelete(null)
      notification.showSuccess('İtinerary başarıyla silindi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'İtinerary silinirken bir hata oluştu.')
    },
  })

  const handleChangePage = (_event: unknown, newPage: number) => {
    setPage(newPage)
  }

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10))
    setPage(0)
  }

  const handleOpenForm = (itinerary?: Itinerary) => {
    if (itinerary) {
      setEditingItinerary(itinerary)
    } else {
      setEditingItinerary(null)
    }
    setFormOpen(true)
  }

  const handleCloseForm = () => {
    setFormOpen(false)
    setEditingItinerary(null)
  }

  const handleFormSubmit = async (data: CreateItineraryRequest | UpdateItineraryRequest) => {
    if (editingItinerary) {
      await updateMutation.mutateAsync({ id: editingItinerary.id, data: data as UpdateItineraryRequest })
    } else {
      await createMutation.mutateAsync(data as CreateItineraryRequest)
    }
  }

  const handleDeleteClick = (itinerary: Itinerary) => {
    setItineraryToDelete(itinerary)
    setDeleteDialogOpen(true)
  }

  const handleDeleteConfirm = () => {
    if (itineraryToDelete) {
      deleteMutation.mutate(itineraryToDelete.id)
    }
  }

  const handleViewTimeline = (id: number) => {
    navigate(`/itineraries/${id}/timeline`)
  }

  const getStatusColor = (status: string | number): 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning' => {
    // Handle enum number values
    if (typeof status === 'number') {
      switch (status) {
        case 1: // Draft
          return 'warning'
        case 2: // Confirmed
          return 'success'
        case 3: // InProgress
          return 'info'
        case 4: // Completed
          return 'default'
        case 5: // Cancelled
          return 'error'
        default:
          return 'default'
      }
    }
    
    // Handle string values
    const statusStr = String(status).toLowerCase()
    switch (statusStr) {
      case 'draft':
      case '1':
        return 'warning'
      case 'confirmed':
      case '2':
        return 'success'
      case 'inprogress':
      case 'in_progress':
      case '3':
        return 'info'
      case 'completed':
      case '4':
        return 'default'
      case 'cancelled':
      case '5':
        return 'error'
      default:
        return 'default'
    }
  }

  const getStatusLabel = (status: string | number) => {
    // Handle enum number values (1=Draft, 2=Confirmed, 3=InProgress, 4=Completed, 5=Cancelled)
    if (typeof status === 'number') {
      switch (status) {
        case 1:
          return 'Taslak'
        case 2:
          return 'Onaylandı'
        case 3:
          return 'Devam Ediyor'
        case 4:
          return 'Tamamlandı'
        case 5:
          return 'İptal Edildi'
        default:
          return 'Bilinmeyen'
      }
    }
    
    // Handle string values
    const statusStr = String(status).toLowerCase()
    switch (statusStr) {
      case 'draft':
      case '1':
        return 'Taslak'
      case 'confirmed':
      case '2':
        return 'Onaylandı'
      case 'inprogress':
      case 'in_progress':
      case '3':
        return 'Devam Ediyor'
      case 'completed':
      case '4':
        return 'Tamamlandı'
      case 'cancelled':
      case '5':
        return 'İptal Edildi'
      default:
        return String(status)
    }
  }

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={6} />
  }

  if (error) {
    return (
      <ContentState
        state="error"
        title="Hata"
        description="İtineraryler yüklenirken bir hata oluştu."
      />
    )
  }

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">İtineraryler</Typography>
        <Box>
          <Button
            variant="outlined"
            startIcon={<FilterListIcon />}
            onClick={() => setFiltersOpen(!filtersOpen)}
            sx={{ mr: 1 }}
          >
            Filtreler
            {filtersOpen ? <ExpandLessIcon /> : <ExpandMoreIcon />}
          </Button>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => handleOpenForm()}
          >
            Yeni İtinerary
          </Button>
        </Box>
      </Box>

      {/* Filters */}
      <Collapse in={filtersOpen}>
        <Paper sx={{ p: 2, mb: 2 }}>
          <Grid container spacing={2} alignItems="center">
            <Grid item xs={12} md={4}>
              <TextField
                fullWidth
                size="small"
                label="Ara"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <SearchIcon />
                    </InputAdornment>
                  ),
                  endAdornment: searchTerm && (
                    <InputAdornment position="end">
                      <IconButton size="small" onClick={() => setSearchTerm('')}>
                        <ClearIcon />
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
            <Grid item xs={12} md={3}>
              <FormControl fullWidth size="small">
                <InputLabel>Durum</InputLabel>
                <Select
                  value={status}
                  label="Durum"
                  onChange={(e) => setStatus(e.target.value)}
                >
                  <MenuItem value="">Tümü</MenuItem>
                  <MenuItem value="Draft">Taslak</MenuItem>
                  <MenuItem value="Confirmed">Onaylandı</MenuItem>
                  <MenuItem value="InProgress">Devam Ediyor</MenuItem>
                  <MenuItem value="Completed">Tamamlandı</MenuItem>
                  <MenuItem value="Cancelled">İptal Edildi</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={5}>
              <Button
                variant="outlined"
                size="small"
                onClick={() => {
                  setSearchTerm('')
                  setStatus('')
                  setSortBy('')
                  setSortOrder('desc')
                }}
                startIcon={<ClearIcon />}
              >
                Temizle
              </Button>
            </Grid>
          </Grid>
        </Paper>
      </Collapse>

      {/* Table */}
      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>İtinerary No</TableCell>
              <TableCell>Misafir</TableCell>
              <TableCell>Personel</TableCell>
              <TableCell>Başlangıç</TableCell>
              <TableCell>Bitiş</TableCell>
              <TableCell>Durum</TableCell>
              <TableCell>Toplam Tutar</TableCell>
              <TableCell>Öğe Sayısı</TableCell>
              <TableCell align="right">İşlemler</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {data?.data.length === 0 ? (
              <TableRow>
                <TableCell colSpan={9} align="center">
                  <Typography variant="body2" color="text.secondary" sx={{ py: 3 }}>
                    İtinerary bulunamadı
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              data?.data.map((itinerary) => (
                <TableRow key={itinerary.id} hover>
                  <TableCell>{itinerary.itineraryNumber}</TableCell>
                  <TableCell>{itinerary.guestName}</TableCell>
                  <TableCell>{itinerary.personnelName}</TableCell>
                  <TableCell>{formatDate(itinerary.startDate)}</TableCell>
                  <TableCell>{formatDate(itinerary.endDate)}</TableCell>
                  <TableCell>
                    <Chip
                      label={getStatusLabel(itinerary.status)}
                      size="small"
                      color={getStatusColor(itinerary.status)}
                    />
                  </TableCell>
                  <TableCell>
                    {formatCurrency(itinerary.totalCost, itinerary.currency)}
                  </TableCell>
                  <TableCell>{itinerary.items?.length || 0}</TableCell>
                  <TableCell align="right">
                    <Tooltip title="Timeline Görüntüle">
                      <IconButton
                        size="small"
                        onClick={() => handleViewTimeline(itinerary.id)}
                        color="primary"
                      >
                        <TimelineIcon />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Düzenle">
                      <IconButton
                        size="small"
                        onClick={() => handleOpenForm(itinerary)}
                        color="primary"
                      >
                        <EditIcon />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Sil">
                      <IconButton
                        size="small"
                        onClick={() => handleDeleteClick(itinerary)}
                        color="error"
                      >
                        <DeleteIcon />
                      </IconButton>
                    </Tooltip>
                  </TableCell>
                </TableRow>
              ))
            )}
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
        />
      </TableContainer>

      {/* Form Dialog */}
      <ItineraryForm
        open={formOpen}
        onClose={handleCloseForm}
        onSubmit={handleFormSubmit}
        itinerary={editingItinerary}
        isLoading={createMutation.isPending || updateMutation.isPending}
      />

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>İtinerary Sil</DialogTitle>
        <DialogContent>
          <DialogContentText>
            "{itineraryToDelete?.itineraryNumber}" numaralı itinerary'yi silmek istediğinizden emin misiniz? Bu işlem geri alınamaz.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>İptal</Button>
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

export default ItinerariesPage


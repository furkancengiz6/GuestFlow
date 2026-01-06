import { useState } from 'react'
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
} from '@mui/icons-material'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { hotelService, CreateHotelRequest, UpdateHotelRequest, HotelFilters, Hotel } from '../../services/hotelService'
import { dropdownService } from '../../services/dropdownService'
import { formatDate } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'
import HotelForm from '../../components/Hotels/HotelForm'
import { useNotification } from '../../hooks/useNotification'

const HotelsPage = () => {
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
  const [formOpen, setFormOpen] = useState(false)
  const [editingHotel, setEditingHotel] = useState<Hotel | null>(null)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [hotelToDelete, setHotelToDelete] = useState<Hotel | null>(null)
  const [filtersOpen, setFiltersOpen] = useState(false)
  
  // Filter states
  const [searchTerm, setSearchTerm] = useState('')
  const [cityId, setCityId] = useState<number | ''>('')
  const [starRating, setStarRating] = useState<number | ''>('')
  const [sortBy, setSortBy] = useState('')
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc')

  const queryClient = useQueryClient()
  const notification = useNotification()

  // Cities dropdown
  const { data: cities } = useQuery({
    queryKey: ['cities-dropdown'],
    queryFn: () => dropdownService.getCities(),
  })

  // Build filters object
  const filters: HotelFilters = {
    ...(searchTerm && { searchTerm }),
    ...(cityId && { cityId: Number(cityId) }),
    ...(starRating && { starRating: Number(starRating) }),
    ...(sortBy && { sortBy }),
    ...(sortOrder && { sortOrder }),
  }

  const { data, isLoading, error } = useQuery({
    queryKey: ['hotels', page + 1, rowsPerPage, filters],
    queryFn: () => hotelService.getHotels(page + 1, rowsPerPage, filters),
  })

  const createMutation = useMutation({
    mutationFn: (data: CreateHotelRequest) => hotelService.createHotel(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['hotels'] })
      setFormOpen(false)
      notification.showSuccess('Otel başarıyla eklendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Otel eklenirken bir hata oluştu.')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateHotelRequest }) =>
      hotelService.updateHotel(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['hotels'] })
      setFormOpen(false)
      setEditingHotel(null)
      notification.showSuccess('Otel başarıyla güncellendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Otel güncellenirken bir hata oluştu.')
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (id: number) => hotelService.deleteHotel(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['hotels'] })
      setDeleteDialogOpen(false)
      setHotelToDelete(null)
      notification.showSuccess('Otel başarıyla silindi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Otel silinirken bir hata oluştu.')
    },
  })

  const handleChangePage = (_event: unknown, newPage: number) => {
    setPage(newPage)
  }

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10))
    setPage(0)
  }

  const handleOpenForm = (hotel?: Hotel) => {
    if (hotel) {
      setEditingHotel(hotel)
    } else {
      setEditingHotel(null)
    }
    setFormOpen(true)
  }

  const handleCloseForm = () => {
    setFormOpen(false)
    setEditingHotel(null)
  }

  const handleFormSubmit = async (data: CreateHotelRequest | UpdateHotelRequest) => {
    if (editingHotel) {
      await updateMutation.mutateAsync({ id: editingHotel.id, data })
    } else {
      await createMutation.mutateAsync(data)
    }
  }

  const handleDeleteClick = (hotel: Hotel) => {
    setHotelToDelete(hotel)
    setDeleteDialogOpen(true)
  }

  const handleDeleteConfirm = () => {
    if (hotelToDelete) {
      deleteMutation.mutate(hotelToDelete.id)
    }
  }

  const handleClearFilters = () => {
    setSearchTerm('')
    setCityId('')
    setStarRating('')
    setSortBy('')
    setSortOrder('asc')
  }

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={6} />
  }

  if (error) {
    return (
      <ContentState
        state="error"
        title="Hata"
        description="Oteller yüklenirken bir hata oluştu."
      />
    )
  }

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">Oteller</Typography>
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
            Yeni Otel
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
                <InputLabel>Şehir</InputLabel>
                <Select
                  value={cityId}
                  label="Şehir"
                  onChange={(e) => setCityId(e.target.value as number | '')}
                >
                  <MenuItem value="">Tümü</MenuItem>
                  {cities?.map((city) => (
                    <MenuItem key={city.id} value={city.id}>
                      {city.cityName}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={2}>
              <FormControl fullWidth size="small">
                <InputLabel>Yıldız</InputLabel>
                <Select
                  value={starRating}
                  label="Yıldız"
                  onChange={(e) => setStarRating(e.target.value as number | '')}
                >
                  <MenuItem value="">Tümü</MenuItem>
                  {[1, 2, 3, 4, 5].map((rating) => (
                    <MenuItem key={rating} value={rating}>
                      {rating} Yıldız
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={3}>
              <Box display="flex" gap={1}>
                <Button
                  variant="outlined"
                  size="small"
                  onClick={handleClearFilters}
                  startIcon={<ClearIcon />}
                >
                  Temizle
                </Button>
              </Box>
            </Grid>
          </Grid>
        </Paper>
      </Collapse>

      {/* Table */}
      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Otel Adı</TableCell>
              <TableCell>Şehir</TableCell>
              <TableCell>Adres</TableCell>
              <TableCell>Yıldız</TableCell>
              <TableCell>Telefon</TableCell>
              <TableCell>E-posta</TableCell>
              <TableCell>Oluşturulma</TableCell>
              <TableCell align="right">İşlemler</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {data?.data.length === 0 ? (
              <TableRow>
                <TableCell colSpan={8} align="center">
                  <Typography variant="body2" color="text.secondary" sx={{ py: 3 }}>
                    Otel bulunamadı
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              data?.data.map((hotel) => (
                <TableRow key={hotel.id} hover>
                  <TableCell>{hotel.hotelName}</TableCell>
                  <TableCell>{hotel.cityName || '-'}</TableCell>
                  <TableCell>{hotel.address}</TableCell>
                  <TableCell>
                    <Chip
                      label={`${hotel.starRating} Yıldız`}
                      size="small"
                      color="primary"
                    />
                  </TableCell>
                  <TableCell>{hotel.phone || '-'}</TableCell>
                  <TableCell>{hotel.email || '-'}</TableCell>
                  <TableCell>{formatDate(hotel.createdDate)}</TableCell>
                  <TableCell align="right">
                    <Tooltip title="Düzenle">
                      <IconButton
                        size="small"
                        onClick={() => handleOpenForm(hotel)}
                        color="primary"
                      >
                        <EditIcon />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Sil">
                      <IconButton
                        size="small"
                        onClick={() => handleDeleteClick(hotel)}
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
      <HotelForm
        open={formOpen}
        onClose={handleCloseForm}
        onSubmit={handleFormSubmit}
        hotel={editingHotel}
        isLoading={createMutation.isPending || updateMutation.isPending}
      />

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>Otel Sil</DialogTitle>
        <DialogContent>
          <DialogContentText>
            "{hotelToDelete?.hotelName}" adlı oteli silmek istediğinizden emin misiniz? Bu işlem geri alınamaz.
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

export default HotelsPage


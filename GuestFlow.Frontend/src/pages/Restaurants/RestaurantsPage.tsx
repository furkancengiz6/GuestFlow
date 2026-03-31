import { useState } from 'react'
import {
  Box,
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
  FormControlLabel,
  Switch,
  Card,
  CardContent,
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
import { restaurantService, CreateRestaurantRequest, UpdateRestaurantRequest, RestaurantFilters, Restaurant } from '../../services/restaurantService'
import { dropdownService } from '../../services/dropdownService'
import { formatDate } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'
import RestaurantForm from '../../components/Restaurants/RestaurantForm'
import { useNotification } from '../../hooks/useNotification'

const RestaurantsPage = () => {
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
  const [formOpen, setFormOpen] = useState(false)
  const [editingRestaurant, setEditingRestaurant] = useState<Restaurant | null>(null)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [restaurantToDelete, setRestaurantToDelete] = useState<Restaurant | null>(null)
  const [filtersOpen, setFiltersOpen] = useState(false)

  // Filter states
  const [searchTerm, setSearchTerm] = useState('')
  const [cityId, setCityId] = useState<number | ''>('')
  const [cuisineType, setCuisineType] = useState('')
  const [reservationRequired, setReservationRequired] = useState<boolean | undefined>(undefined)
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
  const filters: RestaurantFilters = {
    ...(searchTerm && { searchTerm }),
    ...(cityId && { cityId: Number(cityId) }),
    ...(cuisineType && { cuisineType }),
    ...(reservationRequired !== undefined && { reservationRequired }),
    ...(sortBy && { sortBy }),
    ...(sortOrder && { sortOrder }),
  }

  const { data, isLoading, error } = useQuery({
    queryKey: ['restaurants', page + 1, rowsPerPage, filters],
    queryFn: () => restaurantService.getRestaurants(page + 1, rowsPerPage, filters),
  })

  const createMutation = useMutation({
    mutationFn: (data: CreateRestaurantRequest) => restaurantService.createRestaurant(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['restaurants'] })
      setFormOpen(false)
      notification.showSuccess('Restoran başarıyla eklendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Restoran eklenirken bir hata oluştu.')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateRestaurantRequest }) =>
      restaurantService.updateRestaurant(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['restaurants'] })
      setFormOpen(false)
      setEditingRestaurant(null)
      notification.showSuccess('Restoran başarıyla güncellendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Restoran güncellenirken bir hata oluştu.')
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (id: number) => restaurantService.deleteRestaurant(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['restaurants'] })
      setDeleteDialogOpen(false)
      setRestaurantToDelete(null)
      notification.showSuccess('Restoran başarıyla silindi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Restoran silinirken bir hata oluştu.')
    },
  })

  const handleChangePage = (_event: unknown, newPage: number) => {
    setPage(newPage)
  }

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10))
    setPage(0)
  }

  const handleOpenForm = (restaurant?: Restaurant) => {
    if (restaurant) {
      setEditingRestaurant(restaurant)
    } else {
      setEditingRestaurant(null)
    }
    setFormOpen(true)
  }

  const handleCloseForm = () => {
    setFormOpen(false)
    setEditingRestaurant(null)
  }

  const handleFormSubmit = async (data: CreateRestaurantRequest | UpdateRestaurantRequest) => {
    if (editingRestaurant) {
      await updateMutation.mutateAsync({ id: editingRestaurant.id, data })
    } else {
      await createMutation.mutateAsync(data)
    }
  }

  const handleDeleteClick = (restaurant: Restaurant) => {
    setRestaurantToDelete(restaurant)
    setDeleteDialogOpen(true)
  }

  const handleDeleteConfirm = () => {
    if (restaurantToDelete) {
      deleteMutation.mutate(restaurantToDelete.id)
    }
  }

  const handleClearFilters = () => {
    setSearchTerm('')
    setCityId('')
    setCuisineType('')
    setReservationRequired(undefined)
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
        description="Restoranlar yüklenirken bir hata oluştu."
      />
    )
  }

  return (
    <Box className="fade-in" p={3}>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4" component="h1" className="premium-gradient-text" sx={{ fontWeight: 800 }}>Restoran Listesi</Typography>
        <Box>
          <Button
            variant="outlined"
            startIcon={<FilterListIcon />}
            onClick={() => setFiltersOpen(!filtersOpen)}
            sx={{ mr: 1, borderRadius: 2 }}
          >
            Filtreler
            {filtersOpen ? <ExpandLessIcon /> : <ExpandMoreIcon />}
          </Button>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => handleOpenForm()}
            className="premium-gradient"
            sx={{ borderRadius: 2, boxShadow: '0 4px 14px 0 rgba(0,118,255,0.39)' }}
          >
            Yeni Restoran
          </Button>
        </Box>
      </Box>

      {/* Filters */}
      <Collapse in={filtersOpen}>
        <Card className="glass-panel" sx={{ p: 2, mb: 2 }}>
          <Grid container spacing={2} alignItems="center">
            <Grid item xs={12} md={3}>
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
            <Grid item xs={12} md={3}>
              <TextField
                fullWidth
                size="small"
                label="Mutfak Tipi"
                value={cuisineType}
                onChange={(e) => setCuisineType(e.target.value)}
              />
            </Grid>
            <Grid item xs={12} md={3}>
              <FormControlLabel
                control={
                  <Switch
                    checked={reservationRequired === true}
                    onChange={(e) =>
                      setReservationRequired(e.target.checked ? true : undefined)
                    }
                  />
                }
                label="Rezervasyon Gerekli"
              />
            </Grid>
            <Grid item xs={12}>
              <Button
                variant="outlined"
                size="small"
                onClick={handleClearFilters}
                startIcon={<ClearIcon />}
              >
                Temizle
              </Button>
            </Grid>
          </Grid>
        </Card>
      </Collapse>

      {/* Table */}
      <Card className="glass-panel">
        <TableContainer>
          <Table>
            <TableHead sx={{ bgcolor: 'rgba(0,0,0,0.02)' }}>
              <TableRow>
                <TableCell sx={{ fontWeight: 700 }}>Restoran Adı</TableCell>
                <TableCell sx={{ fontWeight: 700 }}>Şehir</TableCell>
                <TableCell sx={{ fontWeight: 700 }}>Adres</TableCell>
                <TableCell sx={{ fontWeight: 700 }}>Mutfak Tipi</TableCell>
                <TableCell sx={{ fontWeight: 700 }}>Kapasite</TableCell>
                <TableCell sx={{ fontWeight: 700 }}>Rezervasyon</TableCell>
                <TableCell sx={{ fontWeight: 700 }}>Telefon</TableCell>
                <TableCell sx={{ fontWeight: 700 }}>Oluşturulma</TableCell>
                <TableCell align="right" sx={{ fontWeight: 700 }}>İşlemler</TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {data?.data.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={9} align="center">
                    <Typography variant="body2" color="text.secondary" sx={{ py: 3 }}>
                      Restoran bulunamadı
                    </Typography>
                  </TableCell>
                </TableRow>
              ) : (
                data?.data.map((restaurant) => (
                  <TableRow key={restaurant.id} hover>
                    <TableCell>{restaurant.restaurantName}</TableCell>
                    <TableCell>{restaurant.cityName || '-'}</TableCell>
                    <TableCell>{restaurant.address}</TableCell>
                    <TableCell>{restaurant.cuisineType || '-'}</TableCell>
                    <TableCell>{restaurant.capacity}</TableCell>
                    <TableCell>
                      <Chip
                        label={restaurant.reservationRequired ? 'Gerekli' : 'Gerekli Değil'}
                        size="small"
                        color={restaurant.reservationRequired ? 'primary' : 'default'}
                      />
                    </TableCell>
                    <TableCell>{restaurant.phone || '-'}</TableCell>
                    <TableCell>{formatDate(restaurant.createdDate)}</TableCell>
                    <TableCell align="right">
                      <Tooltip title="Düzenle">
                        <IconButton
                          size="small"
                          onClick={() => handleOpenForm(restaurant)}
                          color="primary"
                        >
                          <EditIcon />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Sil">
                        <IconButton
                          size="small"
                          onClick={() => handleDeleteClick(restaurant)}
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
      </Card>

      {/* Form Dialog */}
      <RestaurantForm
        open={formOpen}
        onClose={handleCloseForm}
        onSubmit={handleFormSubmit}
        restaurant={editingRestaurant}
        isLoading={createMutation.isPending || updateMutation.isPending}
      />

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>Restoran Sil</DialogTitle>
        <DialogContent>
          <DialogContentText>
            "{restaurantToDelete?.restaurantName}" adlı restoranı silmek istediğinizden emin misiniz? Bu işlem geri alınamaz.
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

export default RestaurantsPage


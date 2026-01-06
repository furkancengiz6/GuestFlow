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
} from '@mui/material'
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Search as SearchIcon,
  Clear as ClearIcon,
} from '@mui/icons-material'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { cityService, CreateCityRequest, UpdateCityRequest, CityFilters, City } from '../../services/cityService'
import { formatDate } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'
import CityForm from '../../components/Cities/CityForm'
import { useNotification } from '../../hooks/useNotification'

const CitiesPage = () => {
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
  const [formOpen, setFormOpen] = useState(false)
  const [editingCity, setEditingCity] = useState<City | null>(null)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [cityToDelete, setCityToDelete] = useState<City | null>(null)
  const [searchTerm, setSearchTerm] = useState('')

  const queryClient = useQueryClient()
  const notification = useNotification()

  const filters: CityFilters = {
    ...(searchTerm && { searchTerm }),
  }

  const { data, isLoading, error } = useQuery({
    queryKey: ['cities', page + 1, rowsPerPage, filters],
    queryFn: () => cityService.getCities(page + 1, rowsPerPage, filters),
  })

  const createMutation = useMutation({
    mutationFn: (data: CreateCityRequest) => cityService.createCity(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cities'] })
      queryClient.invalidateQueries({ queryKey: ['cities-dropdown'] })
      setFormOpen(false)
      notification.showSuccess('Şehir başarıyla eklendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Şehir eklenirken bir hata oluştu.')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateCityRequest }) =>
      cityService.updateCity(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cities'] })
      queryClient.invalidateQueries({ queryKey: ['cities-dropdown'] })
      setFormOpen(false)
      setEditingCity(null)
      notification.showSuccess('Şehir başarıyla güncellendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Şehir güncellenirken bir hata oluştu.')
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (id: number) => cityService.deleteCity(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cities'] })
      queryClient.invalidateQueries({ queryKey: ['cities-dropdown'] })
      setDeleteDialogOpen(false)
      setCityToDelete(null)
      notification.showSuccess('Şehir başarıyla silindi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Şehir silinirken bir hata oluştu.')
    },
  })

  const handleChangePage = (_event: unknown, newPage: number) => {
    setPage(newPage)
  }

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10))
    setPage(0)
  }

  const handleOpenForm = (city?: City) => {
    if (city) {
      setEditingCity(city)
    } else {
      setEditingCity(null)
    }
    setFormOpen(true)
  }

  const handleCloseForm = () => {
    setFormOpen(false)
    setEditingCity(null)
  }

  const handleFormSubmit = async (data: CreateCityRequest | UpdateCityRequest) => {
    if (editingCity) {
      await updateMutation.mutateAsync({ id: editingCity.id, data })
    } else {
      await createMutation.mutateAsync(data)
    }
  }

  const handleDeleteClick = (city: City) => {
    setCityToDelete(city)
    setDeleteDialogOpen(true)
  }

  const handleDeleteConfirm = () => {
    if (cityToDelete) {
      deleteMutation.mutate(cityToDelete.id)
    }
  }

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={6} />
  }

  if (error) {
    return (
      <ContentState
        state="error"
        title="Şehirler yüklenemedi"
        description="Lütfen daha sonra tekrar deneyin."
        actionLabel="Tekrar dene"
        onAction={() => queryClient.refetchQueries({ queryKey: ['cities'] })}
      />
    )
  }

  const hasData = data && data.data && data.data.length > 0

  return (
    <Box p={3}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" sx={{ fontWeight: 600 }}>
          Şehirler
        </Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => handleOpenForm()}
        >
          Yeni Şehir
        </Button>
      </Box>

      <Paper sx={{ p: 2, mb: 3 }}>
        <TextField
          placeholder="Ara (şehir adı)..."
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
      </Paper>

      {!hasData ? (
        <ContentState
          state="empty"
          title="Şehir bulunamadı"
          description="Henüz kayıtlı şehir bulunmamaktadır."
        />
      ) : (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell><strong>Şehir Adı</strong></TableCell>
                <TableCell><strong>Kayıt Tarihi</strong></TableCell>
                <TableCell><strong>İşlemler</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {data?.data.map((city) => (
                <TableRow key={city.id} hover>
                  <TableCell>{city.cityName}</TableCell>
                  <TableCell>{formatDate(city.createdDate)}</TableCell>
                  <TableCell>
                    <Box sx={{ display: 'flex', gap: 1 }}>
                      <Tooltip title="Düzenle">
                        <IconButton
                          size="small"
                          color="primary"
                          onClick={() => handleOpenForm(city)}
                        >
                          <EditIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Sil">
                        <IconButton
                          size="small"
                          color="error"
                          onClick={() => handleDeleteClick(city)}
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

      <CityForm
        open={formOpen}
        onClose={handleCloseForm}
        onSubmit={handleFormSubmit}
        city={editingCity}
        isLoading={createMutation.isPending || updateMutation.isPending}
      />

      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>Şehir Sil</DialogTitle>
        <DialogContent>
          <DialogContentText>
            {cityToDelete && (
              <>
                <strong>{cityToDelete.cityName}</strong> adlı şehri silmek istediğinize emin
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
    </Box>
  )
}

export default CitiesPage


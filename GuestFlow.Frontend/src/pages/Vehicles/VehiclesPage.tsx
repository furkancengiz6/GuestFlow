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
import { vehicleService, CreateVehicleRequest, UpdateVehicleRequest, VehicleFilters, Vehicle } from '../../services/vehicleService'
import { formatDate } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'
import VehicleForm from '../../components/Vehicles/VehicleForm'
import { useNotification } from '../../hooks/useNotification'

const VehiclesPage = () => {
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
  const [formOpen, setFormOpen] = useState(false)
  const [editingVehicle, setEditingVehicle] = useState<Vehicle | null>(null)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [vehicleToDelete, setVehicleToDelete] = useState<Vehicle | null>(null)
  const [searchTerm, setSearchTerm] = useState('')

  const queryClient = useQueryClient()
  const notification = useNotification()

  const filters: VehicleFilters = {
    ...(searchTerm && { searchTerm }),
  }

  const { data, isLoading, error } = useQuery({
    queryKey: ['vehicles', page + 1, rowsPerPage, filters],
    queryFn: () => vehicleService.getVehicles(page + 1, rowsPerPage, filters),
  })

  const createMutation = useMutation({
    mutationFn: (data: CreateVehicleRequest) => vehicleService.createVehicle(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vehicles'] })
      setFormOpen(false)
      notification.showSuccess('Araç başarıyla eklendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Araç eklenirken bir hata oluştu.')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateVehicleRequest }) =>
      vehicleService.updateVehicle(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vehicles'] })
      setFormOpen(false)
      setEditingVehicle(null)
      notification.showSuccess('Araç başarıyla güncellendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Araç güncellenirken bir hata oluştu.')
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (id: number) => vehicleService.deleteVehicle(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['vehicles'] })
      setDeleteDialogOpen(false)
      setVehicleToDelete(null)
      notification.showSuccess('Araç başarıyla silindi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Araç silinirken bir hata oluştu.')
    },
  })

  const handleChangePage = (_event: unknown, newPage: number) => {
    setPage(newPage)
  }

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10))
    setPage(0)
  }

  const handleOpenForm = (vehicle?: Vehicle) => {
    if (vehicle) {
      setEditingVehicle(vehicle)
    } else {
      setEditingVehicle(null)
    }
    setFormOpen(true)
  }

  const handleCloseForm = () => {
    setFormOpen(false)
    setEditingVehicle(null)
  }

  const handleFormSubmit = async (data: CreateVehicleRequest | UpdateVehicleRequest) => {
    if (editingVehicle) {
      await updateMutation.mutateAsync({ id: editingVehicle.id, data })
    } else {
      await createMutation.mutateAsync(data)
    }
  }

  const handleDeleteClick = (vehicle: Vehicle) => {
    setVehicleToDelete(vehicle)
    setDeleteDialogOpen(true)
  }

  const handleDeleteConfirm = () => {
    if (vehicleToDelete) {
      deleteMutation.mutate(vehicleToDelete.id)
    }
  }

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={6} />
  }

  if (error) {
    return (
      <ContentState
        state="error"
        title="Araçlar yüklenemedi"
        description="Lütfen daha sonra tekrar deneyin."
        actionLabel="Tekrar dene"
        onAction={() => queryClient.refetchQueries({ queryKey: ['vehicles'] })}
      />
    )
  }

  const hasData = data && data.data && data.data.length > 0

  return (
    <Box p={3}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" sx={{ fontWeight: 600 }}>
          Araçlar
        </Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => handleOpenForm()}
        >
          Yeni Araç
        </Button>
      </Box>

      <Paper sx={{ p: 2, mb: 3 }}>
        <TextField
          placeholder="Ara (plaka, araç tipi)..."
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
          title="Araç bulunamadı"
          description="Henüz kayıtlı araç bulunmamaktadır."
        />
      ) : (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell><strong>Plaka Numarası</strong></TableCell>
                <TableCell><strong>Araç Tipi</strong></TableCell>
                <TableCell><strong>Kapasite</strong></TableCell>
                <TableCell><strong>Kayıt Tarihi</strong></TableCell>
                <TableCell><strong>İşlemler</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {data?.data.map((vehicle) => (
                <TableRow key={vehicle.id} hover>
                  <TableCell>{vehicle.plateNumber}</TableCell>
                  <TableCell>{vehicle.vehicleType}</TableCell>
                  <TableCell>{vehicle.capacity}</TableCell>
                  <TableCell>{formatDate(vehicle.createdDate)}</TableCell>
                  <TableCell>
                    <Box sx={{ display: 'flex', gap: 1 }}>
                      <Tooltip title="Düzenle">
                        <IconButton
                          size="small"
                          color="primary"
                          onClick={() => handleOpenForm(vehicle)}
                        >
                          <EditIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Sil">
                        <IconButton
                          size="small"
                          color="error"
                          onClick={() => handleDeleteClick(vehicle)}
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

      <VehicleForm
        open={formOpen}
        onClose={handleCloseForm}
        onSubmit={handleFormSubmit}
        vehicle={editingVehicle}
        isLoading={createMutation.isPending || updateMutation.isPending}
      />

      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>Araç Sil</DialogTitle>
        <DialogContent>
          <DialogContentText>
            {vehicleToDelete && (
              <>
                <strong>{vehicleToDelete.plateNumber}</strong> plakalı aracı silmek istediğinize emin
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

export default VehiclesPage


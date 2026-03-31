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
  Card,
} from '@mui/material'
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Search as SearchIcon,
  Clear as ClearIcon,
} from '@mui/icons-material'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { airportService, CreateAirportRequest, UpdateAirportRequest, AirportFilters, Airport } from '../../services/airportService'
import { formatDate } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'
import AirportForm from '../../components/Airports/AirportForm'
import { useNotification } from '../../hooks/useNotification'

const AirportsPage = () => {
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
  const [formOpen, setFormOpen] = useState(false)
  const [editingAirport, setEditingAirport] = useState<Airport | null>(null)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [airportToDelete, setAirportToDelete] = useState<Airport | null>(null)
  const [searchTerm, setSearchTerm] = useState('')

  const queryClient = useQueryClient()
  const notification = useNotification()

  const filters: AirportFilters = {
    ...(searchTerm && { searchTerm }),
  }

  const { data, isLoading, error } = useQuery({
    queryKey: ['airports', page + 1, rowsPerPage, filters],
    queryFn: () => airportService.getAirports(page + 1, rowsPerPage, filters),
  })

  const createMutation = useMutation({
    mutationFn: (data: CreateAirportRequest) => airportService.createAirport(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['airports'] })
      setFormOpen(false)
      notification.showSuccess('Havalimanı başarıyla eklendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Havalimanı eklenirken bir hata oluştu.')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateAirportRequest }) =>
      airportService.updateAirport(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['airports'] })
      setFormOpen(false)
      setEditingAirport(null)
      notification.showSuccess('Havalimanı başarıyla güncellendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Havalimanı güncellenirken bir hata oluştu.')
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (id: number) => airportService.deleteAirport(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['airports'] })
      setDeleteDialogOpen(false)
      setAirportToDelete(null)
      notification.showSuccess('Havalimanı başarıyla silindi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Havalimanı silinirken bir hata oluştu.')
    },
  })

  const handleChangePage = (_event: unknown, newPage: number) => {
    setPage(newPage)
  }

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10))
    setPage(0)
  }

  const handleOpenForm = (airport?: Airport) => {
    if (airport) {
      setEditingAirport(airport)
    } else {
      setEditingAirport(null)
    }
    setFormOpen(true)
  }

  const handleCloseForm = () => {
    setFormOpen(false)
    setEditingAirport(null)
  }

  const handleFormSubmit = async (data: CreateAirportRequest | UpdateAirportRequest) => {
    if (editingAirport) {
      await updateMutation.mutateAsync({ id: editingAirport.id, data })
    } else {
      await createMutation.mutateAsync(data)
    }
  }

  const handleDeleteClick = (airport: Airport) => {
    setAirportToDelete(airport)
    setDeleteDialogOpen(true)
  }

  const handleDeleteConfirm = () => {
    if (airportToDelete) {
      deleteMutation.mutate(airportToDelete.id)
    }
  }

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={6} />
  }

  if (error) {
    return (
      <ContentState
        state="error"
        title="Havalimanları yüklenemedi"
        description="Lütfen daha sonra tekrar deneyin."
        actionLabel="Tekrar dene"
        onAction={() => queryClient.refetchQueries({ queryKey: ['airports'] })}
      />
    )
  }

  const hasData = data && data.data && data.data.length > 0

  return (
    <Box className="fade-in" p={3}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" component="h1" className="premium-gradient-text" sx={{ fontWeight: 800 }}>
          Havalimanı Yönetimi
        </Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => handleOpenForm()}
          className="premium-gradient"
          sx={{ borderRadius: 2, boxShadow: '0 4px 14px 0 rgba(0,118,255,0.39)' }}
        >
          Yeni Havalimanı
        </Button>
      </Box>

      <Card className="glass-panel" sx={{ p: 2, mb: 3 }}>
        <TextField
          fullWidth
          placeholder="Havalimanı adı ile ara..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          size="small"
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon color="action" />
              </InputAdornment>
            ),
            sx: { borderRadius: 3 }
          }}
        />
      </Card>

      {!hasData ? (
        <ContentState
          state="empty"
          title="Havalimanı bulunamadı"
          description="Henüz kayıtlı havalimanı bulunmamaktadır."
        />
      ) : (
        <Card className="glass-panel">
          <TableContainer>
            <Table>
              <TableHead sx={{ bgcolor: 'rgba(0,0,0,0.02)' }}>
                <TableRow>
                  <TableCell sx={{ fontWeight: 700 }}>Havalimanı Adı</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Şehir</TableCell>
                  <TableCell sx={{ fontWeight: 700 }}>Kayıt Tarihi</TableCell>
                  <TableCell align="right" sx={{ fontWeight: 700 }}>İşlemler</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data?.data.map((airport) => (
                  <TableRow key={airport.id} hover>
                    <TableCell>{airport.airportName}</TableCell>
                    <TableCell>{airport.cityName || '-'}</TableCell>
                    <TableCell>{formatDate(airport.createdDate)}</TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', gap: 1 }}>
                        <Tooltip title="Düzenle">
                          <IconButton
                            size="small"
                            color="primary"
                            onClick={() => handleOpenForm(airport)}
                          >
                            <EditIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Sil">
                          <IconButton
                            size="small"
                            color="error"
                            onClick={() => handleDeleteClick(airport)}
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

      <AirportForm
        open={formOpen}
        onClose={handleCloseForm}
        onSubmit={handleFormSubmit}
        airport={editingAirport}
        isLoading={createMutation.isPending || updateMutation.isPending}
      />

      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>Havalimanı Sil</DialogTitle>
        <DialogContent>
          <DialogContentText>
            {airportToDelete && (
              <>
                <strong>{airportToDelete.airportName}</strong> adlı havalimanını silmek istediğinize emin
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

export default AirportsPage


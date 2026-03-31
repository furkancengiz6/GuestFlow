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
  Grid,
  Card,
  CardContent,
} from '@mui/material'
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Visibility as VisibilityIcon,
  Search as SearchIcon,
  Clear as ClearIcon,
} from '@mui/icons-material'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { reservationService, CreateReservationRequest, UpdateReservationRequest, ReservationFilters, Reservation } from '../../services/reservationService'
import { formatDate } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'
import ReservationForm from '../../components/Reservations/ReservationForm'
import { useNotification } from '../../hooks/useNotification'

const ReservationsPage = () => {
  const navigate = useNavigate()
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
  const [formOpen, setFormOpen] = useState(false)
  const [editingReservation, setEditingReservation] = useState<Reservation | null>(null)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [reservationToDelete, setReservationToDelete] = useState<Reservation | null>(null)
  const [searchTerm, setSearchTerm] = useState('')

  const queryClient = useQueryClient()
  const notification = useNotification()

  const filters: ReservationFilters = {
    ...(searchTerm && { searchTerm }),
  }

  const { data, isLoading, error } = useQuery({
    queryKey: ['reservations', page + 1, rowsPerPage, filters],
    queryFn: () => reservationService.getReservations(page + 1, rowsPerPage, filters),
  })

  const createMutation = useMutation({
    mutationFn: (data: CreateReservationRequest) => reservationService.createReservation(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reservations'] })
      setFormOpen(false)
      notification.showSuccess('Rezervasyon başarıyla eklendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Rezervasyon eklenirken bir hata oluştu.')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateReservationRequest }) =>
      reservationService.updateReservation(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reservations'] })
      setFormOpen(false)
      setEditingReservation(null)
      notification.showSuccess('Rezervasyon başarıyla güncellendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Rezervasyon güncellenirken bir hata oluştu.')
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (id: number) => reservationService.deleteReservation(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reservations'] })
      setDeleteDialogOpen(false)
      setReservationToDelete(null)
      notification.showSuccess('Rezervasyon başarıyla silindi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Rezervasyon silinirken bir hata oluştu.')
    },
  })

  const confirmMutation = useMutation({
    mutationFn: (id: number) => reservationService.confirmReservation(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reservations'] })
      notification.showSuccess('Rezervasyon onaylandı.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Rezervasyon onaylanırken bir hata oluştu.')
    },
  })

  const cancelMutation = useMutation({
    mutationFn: (id: number) => reservationService.cancelReservation(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reservations'] })
      notification.showSuccess('Rezervasyon iptal edildi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Rezervasyon iptal edilirken bir hata oluştu.')
    },
  })

  const handleChangePage = (_event: unknown, newPage: number) => {
    setPage(newPage)
  }

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10))
    setPage(0)
  }

  const handleOpenForm = (reservation?: Reservation) => {
    if (reservation) {
      setEditingReservation(reservation)
    } else {
      setEditingReservation(null)
    }
    setFormOpen(true)
  }

  const handleCloseForm = () => {
    setFormOpen(false)
    setEditingReservation(null)
  }

  const handleFormSubmit = async (data: CreateReservationRequest | UpdateReservationRequest) => {
    if (editingReservation) {
      await updateMutation.mutateAsync({ id: editingReservation.id, data })
    } else {
      await createMutation.mutateAsync(data)
    }
  }

  const handleDeleteClick = (reservation: Reservation) => {
    setReservationToDelete(reservation)
    setDeleteDialogOpen(true)
  }

  const handleDeleteConfirm = () => {
    if (reservationToDelete) {
      deleteMutation.mutate(reservationToDelete.id)
    }
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Confirmed':
        return 'success'
      case 'Cancelled':
        return 'error'
      case 'Completed':
        return 'info'
      default:
        return 'warning'
    }
  }

  const getStatusLabel = (status: string) => {
    switch (status) {
      case 'Pending':
        return 'Beklemede'
      case 'Confirmed':
        return 'Onaylandı'
      case 'Cancelled':
        return 'İptal Edildi'
      case 'Completed':
        return 'Tamamlandı'
      default:
        return status
    }
  }

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={6} />
  }

  if (error) {
    return (
      <ContentState
        state="error"
        title="Rezervasyonlar yüklenemedi"
        description="Lütfen daha sonra tekrar deneyin."
        actionLabel="Tekrar dene"
        onAction={() => queryClient.refetchQueries({ queryKey: ['reservations'] })}
      />
    )
  }

  const hasData = data && data.data && data.data.length > 0

  return (
    <Box className="fade-in" p={3}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" component="h1" className="premium-gradient-text" sx={{ fontWeight: 800 }}>
          Rezervasyon Yönetimi
        </Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => setFormOpen(true)}
          className="premium-gradient"
          sx={{ borderRadius: 2, boxShadow: '0 4px 14px 0 rgba(0,118,255,0.39)' }}
        >
          Yeni Rezervasyon
        </Button>
      </Box>

      <Card className="glass-panel" sx={{ p: 2, mb: 3 }}>
        <TextField
          placeholder="Ara (misafir, personel, durum)..."
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
      </Card>

      {!hasData ? (
        <ContentState
          state="empty"
          title="Rezervasyon bulunamadı"
          description="Henüz kayıtlı rezervasyon bulunmamaktadır."
        />
      ) : (
        <Card className="glass-panel">
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell><strong>Rezervasyon Tarihi</strong></TableCell>
                  <TableCell><strong>Misafir</strong></TableCell>
                  <TableCell><strong>Personel</strong></TableCell>
                  <TableCell><strong>Durum</strong></TableCell>
                  <TableCell><strong>Kayıt Tarihi</strong></TableCell>
                  <TableCell><strong>İşlemler</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data?.data.map((reservation) => (
                  <TableRow key={reservation.id} hover>
                    <TableCell>{formatDate(reservation.reservationDate)}</TableCell>
                    <TableCell>
                      <Button
                        variant="text"
                        onClick={() => navigate(`/guests/${reservation.guestId}`)}
                        sx={{ textTransform: 'none', fontWeight: 500 }}
                      >
                        {reservation.guestName || '-'}
                      </Button>
                    </TableCell>
                    <TableCell>{reservation.personnelName || '-'}</TableCell>
                    <TableCell>
                      <Chip
                        label={getStatusLabel(reservation.status)}
                        color={getStatusColor(reservation.status) as any}
                        size="small"
                      />
                    </TableCell>
                    <TableCell>{formatDate(reservation.createdDate)}</TableCell>
                    <TableCell>
                      <Box sx={{ display: 'flex', gap: 1 }}>
                        <Tooltip title="Detay">
                          <IconButton
                            size="small"
                            color="primary"
                            onClick={() => navigate(`/reservations/${reservation.id}`)}
                          >
                            <VisibilityIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        <Tooltip title="Düzenle">
                          <IconButton
                            size="small"
                            color="primary"
                            onClick={() => handleOpenForm(reservation)}
                          >
                            <EditIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                        {reservation.status === 'Pending' && (
                          <Tooltip title="Onayla">
                            <IconButton
                              size="small"
                              color="success"
                              onClick={() => confirmMutation.mutate(reservation.id)}
                            >
                              ✓
                            </IconButton>
                          </Tooltip>
                        )}
                        {reservation.status !== 'Cancelled' && (
                          <Tooltip title="İptal Et">
                            <IconButton
                              size="small"
                              color="warning"
                              onClick={() => cancelMutation.mutate(reservation.id)}
                            >
                              ✕
                            </IconButton>
                          </Tooltip>
                        )}
                        <Tooltip title="Sil">
                          <IconButton
                            size="small"
                            color="error"
                            onClick={() => handleDeleteClick(reservation)}
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

      <ReservationForm
        open={formOpen}
        onClose={handleCloseForm}
        onSubmit={handleFormSubmit}
        reservation={editingReservation}
        isLoading={createMutation.isPending || updateMutation.isPending}
      />

      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>Rezervasyon Sil</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Bu rezervasyonu silmek istediğinize emin misiniz? Bu işlem geri alınamaz.
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

export default ReservationsPage


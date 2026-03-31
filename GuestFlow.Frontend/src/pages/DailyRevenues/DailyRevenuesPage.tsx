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
import { dailyRevenueService, CreateDailyRevenueRequest, UpdateDailyRevenueRequest, DailyRevenueFilters, DailyRevenue } from '../../services/dailyRevenueService'
import { formatDate, formatCurrency } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'
import DailyRevenueForm from '../../components/DailyRevenues/DailyRevenueForm'
import { useNotification } from '../../hooks/useNotification'

const DailyRevenuesPage = () => {
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
  const [formOpen, setFormOpen] = useState(false)
  const [editingDailyRevenue, setEditingDailyRevenue] = useState<DailyRevenue | null>(null)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [dailyRevenueToDelete, setDailyRevenueToDelete] = useState<DailyRevenue | null>(null)
  const [searchTerm, setSearchTerm] = useState('')

  const queryClient = useQueryClient()
  const notification = useNotification()

  const filters: DailyRevenueFilters = {
    // Add filters as needed
  }

  const { data, isLoading, error } = useQuery({
    queryKey: ['dailyRevenues', page + 1, rowsPerPage, filters],
    queryFn: () => dailyRevenueService.getDailyRevenues(page + 1, rowsPerPage, filters),
  })

  const createMutation = useMutation({
    mutationFn: (data: CreateDailyRevenueRequest) => dailyRevenueService.createDailyRevenue(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['dailyRevenues'] })
      setFormOpen(false)
      notification.showSuccess('Günlük gelir başarıyla eklendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Günlük gelir eklenirken bir hata oluştu.')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateDailyRevenueRequest }) =>
      dailyRevenueService.updateDailyRevenue(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['dailyRevenues'] })
      setFormOpen(false)
      setEditingDailyRevenue(null)
      notification.showSuccess('Günlük gelir başarıyla güncellendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Günlük gelir güncellenirken bir hata oluştu.')
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (id: number) => dailyRevenueService.deleteDailyRevenue(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['dailyRevenues'] })
      setDeleteDialogOpen(false)
      setDailyRevenueToDelete(null)
      notification.showSuccess('Günlük gelir başarıyla silindi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Günlük gelir silinirken bir hata oluştu.')
    },
  })

  const handleChangePage = (_event: unknown, newPage: number) => {
    setPage(newPage)
  }

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10))
    setPage(0)
  }

  const handleOpenForm = (dailyRevenue?: DailyRevenue) => {
    if (dailyRevenue) {
      setEditingDailyRevenue(dailyRevenue)
    } else {
      setEditingDailyRevenue(null)
    }
    setFormOpen(true)
  }

  const handleCloseForm = () => {
    setFormOpen(false)
    setEditingDailyRevenue(null)
  }

  const handleFormSubmit = async (data: CreateDailyRevenueRequest | UpdateDailyRevenueRequest) => {
    if (editingDailyRevenue) {
      await updateMutation.mutateAsync({ id: editingDailyRevenue.id, data })
    } else {
      await createMutation.mutateAsync(data)
    }
  }

  const handleDeleteClick = (dailyRevenue: DailyRevenue) => {
    setDailyRevenueToDelete(dailyRevenue)
    setDeleteDialogOpen(true)
  }

  const handleDeleteConfirm = () => {
    if (dailyRevenueToDelete) {
      deleteMutation.mutate(dailyRevenueToDelete.id)
    }
  }

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={6} />
  }

  if (error) {
    return (
      <ContentState
        state="error"
        title="Günlük gelirler yüklenemedi"
        description="Lütfen daha sonra tekrar deneyin."
        actionLabel="Tekrar dene"
        onAction={() => queryClient.refetchQueries({ queryKey: ['dailyRevenues'] })}
      />
    )
  }

  const hasData = data && data.data && data.data.length > 0

  return (
    <Box p={3}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" sx={{ fontWeight: 600 }}>
          Günlük Gelirler
        </Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => handleOpenForm()}
        >
          Yeni Gelir
        </Button>
      </Box>

      <Card className="glass-panel" sx={{ p: 2, mb: 3 }}>
        <TextField
          placeholder="Ara..."
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

      {
        !hasData ? (
          <ContentState
            state="empty"
            title="Günlük gelir bulunamadı"
            description="Henüz kayıtlı günlük gelir bulunmamaktadır."
          />
        ) : (
          <Card className="glass-panel">
            <TableContainer>
              <Table>
                <TableHead sx={{ bgcolor: 'rgba(0,0,0,0.02)' }}>
                  <TableRow>
                    <TableCell sx={{ fontWeight: 700 }}>Tarih</TableCell>
                    <TableCell sx={{ fontWeight: 700 }}>Gelir Miktarı</TableCell>
                    <TableCell sx={{ fontWeight: 700 }}>Para Birimi</TableCell>
                    <TableCell sx={{ fontWeight: 700 }}>Not</TableCell>
                    <TableCell sx={{ fontWeight: 700 }}>Kayıt Tarihi</TableCell>
                    <TableCell sx={{ fontWeight: 700 }}>İşlemler</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {data?.data.map((dailyRevenue) => (
                    <TableRow key={dailyRevenue.id} hover>
                      <TableCell>{formatDate(dailyRevenue.revenueDate)}</TableCell>
                      <TableCell>{formatCurrency(dailyRevenue.revenueAmount, dailyRevenue.currency)}</TableCell>
                      <TableCell>{dailyRevenue.currency}</TableCell>
                      <TableCell sx={{ maxWidth: 200, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                        {dailyRevenue.note || '-'}
                      </TableCell>
                      <TableCell>{formatDate(dailyRevenue.createdDate)}</TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', gap: 1 }}>
                          <Tooltip title="Düzenle">
                            <IconButton
                              size="small"
                              color="primary"
                              onClick={() => handleOpenForm(dailyRevenue)}
                            >
                              <EditIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Sil">
                            <IconButton
                              size="small"
                              color="error"
                              onClick={() => handleDeleteClick(dailyRevenue)}
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

      <DailyRevenueForm
        open={formOpen}
        onClose={handleCloseForm}
        onSubmit={handleFormSubmit}
        dailyRevenue={editingDailyRevenue}
        isLoading={createMutation.isPending || updateMutation.isPending}
      />

      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>Günlük Gelir Sil</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Bu günlük geliri silmek istediğinize emin misiniz? Bu işlem geri alınamaz.
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

export default DailyRevenuesPage


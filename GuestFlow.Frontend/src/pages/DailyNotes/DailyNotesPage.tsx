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
import { dailyNoteService, CreateDailyNoteRequest, UpdateDailyNoteRequest, DailyNoteFilters, DailyNote } from '../../services/dailyNoteService'
import { formatDate } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'
import DailyNoteForm from '../../components/DailyNotes/DailyNoteForm'
import { useNotification } from '../../hooks/useNotification'

const DailyNotesPage = () => {
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
  const [formOpen, setFormOpen] = useState(false)
  const [editingDailyNote, setEditingDailyNote] = useState<DailyNote | null>(null)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [dailyNoteToDelete, setDailyNoteToDelete] = useState<DailyNote | null>(null)
  const [searchTerm, setSearchTerm] = useState('')

  const queryClient = useQueryClient()
  const notification = useNotification()

  const filters: DailyNoteFilters = {
    ...(searchTerm && { searchTerm }),
  }

  const { data, isLoading, error } = useQuery({
    queryKey: ['dailyNotes', page + 1, rowsPerPage, filters],
    queryFn: () => dailyNoteService.getDailyNotes(page + 1, rowsPerPage, filters),
  })

  const createMutation = useMutation({
    mutationFn: (data: CreateDailyNoteRequest) => dailyNoteService.createDailyNote(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['dailyNotes'] })
      setFormOpen(false)
      notification.showSuccess('Günlük not başarıyla eklendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Günlük not eklenirken bir hata oluştu.')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateDailyNoteRequest }) =>
      dailyNoteService.updateDailyNote(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['dailyNotes'] })
      setFormOpen(false)
      setEditingDailyNote(null)
      notification.showSuccess('Günlük not başarıyla güncellendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Günlük not güncellenirken bir hata oluştu.')
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (id: number) => dailyNoteService.deleteDailyNote(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['dailyNotes'] })
      setDeleteDialogOpen(false)
      setDailyNoteToDelete(null)
      notification.showSuccess('Günlük not başarıyla silindi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Günlük not silinirken bir hata oluştu.')
    },
  })

  const handleChangePage = (_event: unknown, newPage: number) => {
    setPage(newPage)
  }

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10))
    setPage(0)
  }

  const handleOpenForm = (dailyNote?: DailyNote) => {
    if (dailyNote) {
      setEditingDailyNote(dailyNote)
    } else {
      setEditingDailyNote(null)
    }
    setFormOpen(true)
  }

  const handleCloseForm = () => {
    setFormOpen(false)
    setEditingDailyNote(null)
  }

  const handleFormSubmit = async (data: CreateDailyNoteRequest | UpdateDailyNoteRequest) => {
    if (editingDailyNote) {
      await updateMutation.mutateAsync({ id: editingDailyNote.id, data })
    } else {
      await createMutation.mutateAsync(data)
    }
  }

  const handleDeleteClick = (dailyNote: DailyNote) => {
    setDailyNoteToDelete(dailyNote)
    setDeleteDialogOpen(true)
  }

  const handleDeleteConfirm = () => {
    if (dailyNoteToDelete) {
      deleteMutation.mutate(dailyNoteToDelete.id)
    }
  }

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={6} />
  }

  if (error) {
    return (
      <ContentState
        state="error"
        title="Günlük notlar yüklenemedi"
        description="Lütfen daha sonra tekrar deneyin."
        actionLabel="Tekrar dene"
        onAction={() => queryClient.refetchQueries({ queryKey: ['dailyNotes'] })}
      />
    )
  }

  const hasData = data && data.data && data.data.length > 0

  return (
    <Box p={3}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" sx={{ fontWeight: 600 }}>
          Günlük Notlar
        </Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => handleOpenForm()}
        >
          Yeni Not
        </Button>
      </Box>

      <Paper sx={{ p: 2, mb: 3 }}>
        <TextField
          placeholder="Ara (not içeriği)..."
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
          title="Günlük not bulunamadı"
          description="Henüz kayıtlı günlük not bulunmamaktadır."
        />
      ) : (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell><strong>Tarih</strong></TableCell>
                <TableCell><strong>Personel</strong></TableCell>
                <TableCell><strong>Not</strong></TableCell>
                <TableCell><strong>Kayıt Tarihi</strong></TableCell>
                <TableCell><strong>İşlemler</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {data?.data.map((dailyNote) => (
                <TableRow key={dailyNote.id} hover>
                  <TableCell>{formatDate(dailyNote.noteDate)}</TableCell>
                  <TableCell>{dailyNote.personnelName || '-'}</TableCell>
                  <TableCell sx={{ maxWidth: 300, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                    {dailyNote.note}
                  </TableCell>
                  <TableCell>{formatDate(dailyNote.createdDate)}</TableCell>
                  <TableCell>
                    <Box sx={{ display: 'flex', gap: 1 }}>
                      <Tooltip title="Düzenle">
                        <IconButton
                          size="small"
                          color="primary"
                          onClick={() => handleOpenForm(dailyNote)}
                        >
                          <EditIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Sil">
                        <IconButton
                          size="small"
                          color="error"
                          onClick={() => handleDeleteClick(dailyNote)}
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

      <DailyNoteForm
        open={formOpen}
        onClose={handleCloseForm}
        onSubmit={handleFormSubmit}
        dailyNote={editingDailyNote}
        isLoading={createMutation.isPending || updateMutation.isPending}
      />

      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>Günlük Not Sil</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Bu günlük notu silmek istediğinize emin misiniz? Bu işlem geri alınamaz.
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

export default DailyNotesPage


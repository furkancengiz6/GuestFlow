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
  Chip,
  IconButton,
  Tooltip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  InputAdornment,
  Button,
  Grid,
  Card,
  CardContent,
} from '@mui/material'
import { useAuthStore } from '../../stores/authStore'
import {
  Search as SearchIcon,
  Clear as ClearIcon,
  Visibility as VisibilityIcon,
  CheckCircle as CheckCircleIcon,
  Delete as DeleteIcon,
} from '@mui/icons-material'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { notificationService, NotificationFilters, Notification } from '../../services/notificationService'
import { formatDate } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'
import { useNotification } from '../../hooks/useNotification'

const NotificationsPage = () => {
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
  const [viewDialogOpen, setViewDialogOpen] = useState(false)
  const [selectedNotification, setSelectedNotification] = useState<Notification | null>(null)
  const [searchTerm, setSearchTerm] = useState('')
  const [filterRead, setFilterRead] = useState<boolean | undefined>(undefined)

  const queryClient = useQueryClient()
  const notification = useNotification()

  const filters: NotificationFilters = {
    ...(searchTerm && { searchTerm }),
    ...(filterRead !== undefined && { isRead: filterRead }),
  }

  const { isAuthenticated } = useAuthStore()

  const { data, isLoading, error } = useQuery({
    queryKey: ['notifications', page + 1, rowsPerPage, filters],
    queryFn: () => notificationService.getMyNotifications(page + 1, rowsPerPage, filters),
    enabled: isAuthenticated, // Only fetch when authenticated
  })

  const { data: statistics } = useQuery({
    queryKey: ['notification-statistics'],
    queryFn: () => notificationService.getStatistics(),
    enabled: isAuthenticated, // Only fetch when authenticated
  })

  const markAsReadMutation = useMutation({
    mutationFn: (id: number) => notificationService.markAsRead(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] })
      queryClient.invalidateQueries({ queryKey: ['notification-statistics'] })
      notification.showSuccess('Bildirim okundu olarak işaretlendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Bildirim güncellenirken bir hata oluştu.')
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (id: number) => notificationService.deleteNotification(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] })
      queryClient.invalidateQueries({ queryKey: ['notification-statistics'] })
      notification.showSuccess('Bildirim başarıyla silindi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Bildirim silinirken bir hata oluştu.')
    },
  })

  const handleChangePage = (_event: unknown, newPage: number) => {
    setPage(newPage)
  }

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10))
    setPage(0)
  }

  const handleViewNotification = (notif: Notification) => {
    setSelectedNotification(notif)
    setViewDialogOpen(true)
    if (!notif.isRead) {
      markAsReadMutation.mutate(notif.id)
    }
  }

  const getTypeColor = (type: string) => {
    switch (type) {
      case 'Info':
        return 'info'
      case 'Success':
        return 'success'
      case 'Warning':
        return 'warning'
      case 'Error':
        return 'error'
      default:
        return 'default'
    }
  }

  const getTypeLabel = (type: string) => {
    switch (type) {
      case 'Info':
        return 'Bilgi'
      case 'Success':
        return 'Başarılı'
      case 'Warning':
        return 'Uyarı'
      case 'Error':
        return 'Hata'
      default:
        return type
    }
  }

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={6} />
  }

  if (error) {
    return (
      <ContentState
        state="error"
        title="Bildirimler yüklenemedi"
        description="Lütfen daha sonra tekrar deneyin."
        actionLabel="Tekrar dene"
        onAction={() => queryClient.refetchQueries({ queryKey: ['notifications'] })}
      />
    )
  }

  const hasData = data && data.data && data.data.length > 0

  return (
    <Box p={3}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" sx={{ fontWeight: 600 }}>
          Bildirimler
        </Typography>
      </Box>

      {statistics && (
        <Grid container spacing={2} sx={{ mb: 3 }}>
          <Grid item xs={12} sm={6} md={4}>
            <Card>
              <CardContent>
                <Typography variant="body2" color="text.secondary">
                  Toplam Bildirim
                </Typography>
                <Typography variant="h5" sx={{ fontWeight: 600 }}>
                  {statistics.total}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={4}>
            <Card>
              <CardContent>
                <Typography variant="body2" color="text.secondary">
                  Okunmamış
                </Typography>
                <Typography variant="h5" sx={{ fontWeight: 600, color: 'warning.main' }}>
                  {statistics.unread}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={4}>
            <Card>
              <CardContent>
                <Typography variant="body2" color="text.secondary">
                  Okunmuş
                </Typography>
                <Typography variant="h5" sx={{ fontWeight: 600, color: 'success.main' }}>
                  {statistics.read}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      <Paper sx={{ p: 2, mb: 3 }}>
        <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
          <TextField
            placeholder="Ara (başlık, mesaj)..."
            value={searchTerm}
            onChange={(e) => {
              setSearchTerm(e.target.value)
              setPage(0)
            }}
            size="small"
            sx={{ flex: 1 }}
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
            variant={filterRead === false ? 'contained' : 'outlined'}
            onClick={() => setFilterRead(filterRead === false ? undefined : false)}
          >
            Sadece Okunmamışlar
          </Button>
          <Button
            variant={filterRead === true ? 'contained' : 'outlined'}
            onClick={() => setFilterRead(filterRead === true ? undefined : true)}
          >
            Sadece Okunmuşlar
          </Button>
        </Box>
      </Paper>

      {!hasData ? (
        <ContentState
          state="empty"
          title="Bildirim bulunamadı"
          description="Henüz bildirim bulunmamaktadır."
        />
      ) : (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell><strong>Başlık</strong></TableCell>
                <TableCell><strong>Mesaj</strong></TableCell>
                <TableCell><strong>Tip</strong></TableCell>
                <TableCell><strong>Durum</strong></TableCell>
                <TableCell><strong>Tarih</strong></TableCell>
                <TableCell><strong>İşlemler</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {data?.data.map((notif) => (
                <TableRow
                  key={notif.id}
                  hover
                  sx={{
                    backgroundColor: notif.isRead ? 'transparent' : 'action.hover',
                    fontWeight: notif.isRead ? 'normal' : 'bold',
                  }}
                >
                  <TableCell>{notif.title}</TableCell>
                  <TableCell sx={{ maxWidth: 300, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                    {notif.message}
                  </TableCell>
                  <TableCell>
                    <Chip
                      label={getTypeLabel(notif.type)}
                      color={getTypeColor(notif.type) as any}
                      size="small"
                    />
                  </TableCell>
                  <TableCell>
                    {notif.isRead ? (
                      <Chip label="Okundu" color="success" size="small" />
                    ) : (
                      <Chip label="Okunmadı" color="warning" size="small" />
                    )}
                  </TableCell>
                  <TableCell>{formatDate(notif.createdDate)}</TableCell>
                  <TableCell>
                    <Box sx={{ display: 'flex', gap: 1 }}>
                      <Tooltip title="Görüntüle">
                        <IconButton
                          size="small"
                          color="primary"
                          onClick={() => handleViewNotification(notif)}
                        >
                          <VisibilityIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                      {!notif.isRead && (
                        <Tooltip title="Okundu İşaretle">
                          <IconButton
                            size="small"
                            color="success"
                            onClick={() => markAsReadMutation.mutate(notif.id)}
                          >
                            <CheckCircleIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      )}
                      <Tooltip title="Sil">
                        <IconButton
                          size="small"
                          color="error"
                          onClick={() => deleteMutation.mutate(notif.id)}
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

      <Dialog open={viewDialogOpen} onClose={() => setViewDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          {selectedNotification?.title || 'Bildirim Detayı'}
        </DialogTitle>
        <DialogContent>
          {selectedNotification && (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
              <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
                <Chip
                  label={getTypeLabel(selectedNotification.type)}
                  color={getTypeColor(selectedNotification.type) as any}
                  size="small"
                />
                {selectedNotification.isRead ? (
                  <Chip label="Okundu" color="success" size="small" />
                ) : (
                  <Chip label="Okunmadı" color="warning" size="small" />
                )}
              </Box>
              <TextField
                label="Mesaj"
                value={selectedNotification.message}
                multiline
                rows={6}
                fullWidth
                disabled
              />
              <Typography variant="body2" color="text.secondary">
                Oluşturulma Tarihi: {formatDate(selectedNotification.createdDate)}
              </Typography>
              {selectedNotification.readDate && (
                <Typography variant="body2" color="text.secondary">
                  Okunma Tarihi: {formatDate(selectedNotification.readDate)}
                </Typography>
              )}
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          {selectedNotification && !selectedNotification.isRead && (
            <Button
              onClick={() => {
                markAsReadMutation.mutate(selectedNotification.id)
                setViewDialogOpen(false)
              }}
              variant="outlined"
            >
              Okundu İşaretle
            </Button>
          )}
          <Button onClick={() => setViewDialogOpen(false)}>Kapat</Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

export default NotificationsPage


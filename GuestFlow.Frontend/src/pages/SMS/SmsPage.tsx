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
  Button,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  InputAdornment,
  Grid,
  Card,
  CardContent,
} from '@mui/material'
import {
  Send as SendIcon,
  Search as SearchIcon,
  Clear as ClearIcon,
} from '@mui/icons-material'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { smsService, SendSmsRequest, SmsFilters } from '../../services/smsService'
import { formatDate } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'
import { useNotification } from '../../hooks/useNotification'
import { dropdownService } from '../../services/dropdownService'

const SmsPage = () => {
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
  const [sendDialogOpen, setSendDialogOpen] = useState(false)
  const [searchTerm, setSearchTerm] = useState('')
  const [phoneNumber, setPhoneNumber] = useState('')
  const [message, setMessage] = useState('')
  const [selectedGuestId, setSelectedGuestId] = useState<number | undefined>(undefined)

  const queryClient = useQueryClient()
  const notification = useNotification()

  const { data: guests } = useQuery({
    queryKey: ['guests-dropdown'],
    queryFn: () => dropdownService.getGuests(),
  })

  const filters: SmsFilters = {
    ...(searchTerm && { searchTerm }),
  }

  const { data, isLoading, error } = useQuery({
    queryKey: ['sms', page + 1, rowsPerPage, filters],
    queryFn: () => smsService.getSmsHistory(page + 1, rowsPerPage, filters),
  })

  const { data: statistics } = useQuery({
    queryKey: ['sms-statistics'],
    queryFn: () => smsService.getStatistics(),
  })

  const sendMutation = useMutation({
    mutationFn: (data: SendSmsRequest) => smsService.sendSms(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['sms'] })
      queryClient.invalidateQueries({ queryKey: ['sms-statistics'] })
      setSendDialogOpen(false)
      setPhoneNumber('')
      setMessage('')
      setSelectedGuestId(undefined)
      notification.showSuccess('SMS başarıyla gönderildi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'SMS gönderilirken bir hata oluştu.')
    },
  })

  const handleChangePage = (_event: unknown, newPage: number) => {
    setPage(newPage)
  }

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10))
    setPage(0)
  }

  const handleSendSms = () => {
    if (!phoneNumber || !message) {
      notification.showError('Telefon numarası ve mesaj gereklidir.')
      return
    }

    sendMutation.mutate({
      phoneNumber,
      message,
      ...(selectedGuestId && { guestId: selectedGuestId }),
    })
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Sent':
        return 'success'
      case 'Failed':
        return 'error'
      case 'Pending':
        return 'warning'
      default:
        return 'default'
    }
  }

  const getStatusLabel = (status: string) => {
    switch (status) {
      case 'Sent':
        return 'Gönderildi'
      case 'Failed':
        return 'Başarısız'
      case 'Pending':
        return 'Beklemede'
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
        title="SMS geçmişi yüklenemedi"
        description="Lütfen daha sonra tekrar deneyin."
        actionLabel="Tekrar dene"
        onAction={() => queryClient.refetchQueries({ queryKey: ['sms'] })}
      />
    )
  }

  const hasData = data && data.data && data.data.length > 0

  return (
    <Box p={3}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" sx={{ fontWeight: 600 }}>
          SMS Yönetimi
        </Typography>
        <Button
          variant="contained"
          startIcon={<SendIcon />}
          onClick={() => setSendDialogOpen(true)}
        >
          SMS Gönder
        </Button>
      </Box>

      {statistics && (
        <Grid container spacing={2} sx={{ mb: 3 }}>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography variant="body2" color="text.secondary">
                  Toplam Gönderilen
                </Typography>
                <Typography variant="h5" sx={{ fontWeight: 600 }}>
                  {statistics.totalSent}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography variant="body2" color="text.secondary">
                  Başarısız
                </Typography>
                <Typography variant="h5" sx={{ fontWeight: 600, color: 'error.main' }}>
                  {statistics.totalFailed}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography variant="body2" color="text.secondary">
                  Beklemede
                </Typography>
                <Typography variant="h5" sx={{ fontWeight: 600, color: 'warning.main' }}>
                  {statistics.totalPending}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography variant="body2" color="text.secondary">
                  Başarı Oranı
                </Typography>
                <Typography variant="h5" sx={{ fontWeight: 600, color: 'success.main' }}>
                  %{statistics.successRate.toFixed(1)}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      <Paper sx={{ p: 2, mb: 3 }}>
        <TextField
          placeholder="Ara (telefon, mesaj, misafir)..."
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
          title="SMS geçmişi bulunamadı"
          description="Henüz SMS geçmişi bulunmamaktadır."
        />
      ) : (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell><strong>Telefon</strong></TableCell>
                <TableCell><strong>Mesaj</strong></TableCell>
                <TableCell><strong>Misafir</strong></TableCell>
                <TableCell><strong>Durum</strong></TableCell>
                <TableCell><strong>Gönderim Tarihi</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {data?.data.map((sms) => (
                <TableRow key={sms.id} hover>
                  <TableCell>{sms.phoneNumber}</TableCell>
                  <TableCell sx={{ maxWidth: 300, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                    {sms.message}
                  </TableCell>
                  <TableCell>{sms.guestName || '-'}</TableCell>
                  <TableCell>
                    <Chip
                      label={getStatusLabel(sms.status)}
                      color={getStatusColor(sms.status) as any}
                      size="small"
                    />
                  </TableCell>
                  <TableCell>{formatDate(sms.sentDate)}</TableCell>
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

      <Dialog open={sendDialogOpen} onClose={() => setSendDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>SMS Gönder</DialogTitle>
        <DialogContent>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2, pt: 2 }}>
            <TextField
              select
              label="Misafir (Opsiyonel)"
              fullWidth
              SelectProps={{
                native: true,
              }}
              value={selectedGuestId || ''}
              onChange={(e) => {
                const value = e.target.value
                setSelectedGuestId(value ? Number(value) : undefined)
                if (value && guests) {
                  const guest = guests.find((g) => g.id === Number(value))
                  if (guest && guest.phoneNumber) {
                    setPhoneNumber(guest.phoneNumber)
                  }
                }
              }}
            >
              <option value="">Misafir Seçiniz (Opsiyonel)</option>
              {guests?.map((guest) => (
                <option key={guest.id} value={guest.id}>
                  {guest.fullName} ({guest.phoneNumber || 'Telefon yok'})
                </option>
              ))}
            </TextField>
            <TextField
              label="Telefon Numarası"
              fullWidth
              required
              value={phoneNumber}
              onChange={(e) => setPhoneNumber(e.target.value)}
              placeholder="+90 555 123 4567"
            />
            <TextField
              label="Mesaj"
              multiline
              rows={6}
              fullWidth
              required
              value={message}
              onChange={(e) => setMessage(e.target.value)}
              placeholder="Mesajınızı buraya yazın..."
            />
          </Box>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSendDialogOpen(false)} disabled={sendMutation.isPending}>
            İptal
          </Button>
          <Button
            onClick={handleSendSms}
            variant="contained"
            disabled={sendMutation.isPending || !phoneNumber || !message}
          >
            {sendMutation.isPending ? 'Gönderiliyor...' : 'Gönder'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

export default SmsPage


// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

import {
  Box,
  Typography,
  Card,
  CardContent,
  Grid,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  IconButton,
  Tooltip,
  Pagination,
  Alert,
} from '@mui/material'
import {
  WhatsApp as WhatsAppIcon,
  Send as SendIcon,
  Refresh as RefreshIcon,
  Visibility as ViewIcon,
  CheckCircle as DeliveredIcon,
  Cancel as FailedIcon,
  Schedule as PendingIcon,
  MarkEmailRead as ReadIcon,
} from '@mui/icons-material'
import { useState } from 'react'
import { useWhatsAppHistory, useSendWhatsApp, useWhatsAppStatistics } from '../../hooks/useWhatsApp'
import { formatDate } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'
import type { SendWhatsApp as SendWhatsAppType } from '../../types/whatsApp'

const WhatsAppManagementPage = () => {
  const [page, setPage] = useState(1)
  const [pageSize] = useState(50)
  const [sendDialogOpen, setSendDialogOpen] = useState(false)
  const [viewDialogOpen, setViewDialogOpen] = useState(false)
  const [selectedMessage, setSelectedMessage] = useState<any>(null)
  const [filters, setFilters] = useState({
    status: '',
    guestId: '',
  })
  const [sendData, setSendData] = useState<SendWhatsAppType>({
    phoneNumber: '',
    message: '',
    guestId: undefined,
    messageType: 1, // Text
  })

  const { data: history, isLoading, error, refetch } = useWhatsAppHistory({
    pageNumber: page,
    pageSize,
    status: filters.status || undefined,
    guestId: filters.guestId ? parseInt(filters.guestId) : undefined,
  })

  const { data: statistics } = useWhatsAppStatistics()
  const sendMutation = useSendWhatsApp()

  const getStatusIcon = (status: string) => {
    switch (status.toLowerCase()) {
      case 'delivered':
        return <DeliveredIcon fontSize="small" color="success" />
      case 'read':
        return <ReadIcon fontSize="small" color="info" />
      case 'failed':
        return <FailedIcon fontSize="small" color="error" />
      case 'pending':
      case 'sent':
        return <PendingIcon fontSize="small" color="warning" />
      default:
        return undefined
    }
  }

  const getStatusColor = (status: string): 'success' | 'error' | 'warning' | 'info' | 'default' => {
    switch (status.toLowerCase()) {
      case 'delivered':
      case 'read':
        return 'success'
      case 'failed':
        return 'error'
      case 'pending':
      case 'sent':
        return 'warning'
      default:
        return 'default'
    }
  }

  const handleSend = () => {
    sendMutation.mutate(sendData, {
      onSuccess: () => {
        setSendDialogOpen(false)
        setSendData({
          phoneNumber: '',
          message: '',
          guestId: undefined,
          messageType: 1,
        })
        refetch()
      },
    })
  }

  const handleViewMessage = (message: any) => {
    setSelectedMessage(message)
    setViewDialogOpen(true)
  }

  if (isLoading && !history) {
    return <ContentState state="loading" skeletonLines={10} />
  }

  if (error) {
    return (
      <Alert severity="error">
        WhatsApp geçmişi yüklenirken bir hata oluştu.
      </Alert>
    )
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" component="h1">
          WhatsApp Yönetimi
        </Typography>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <Button
            variant="outlined"
            startIcon={<RefreshIcon />}
            onClick={() => refetch()}
          >
            Yenile
          </Button>
          <Button
            variant="contained"
            startIcon={<SendIcon />}
            onClick={() => setSendDialogOpen(true)}
          >
            WhatsApp Gönder
          </Button>
        </Box>
      </Box>

      {/* İstatistikler */}
      {statistics && (
        <Grid container spacing={2} sx={{ mb: 3 }}>
          <Grid item xs={12} md={3}>
            <Card variant="outlined">
              <CardContent>
                <Typography variant="body2" color="text.secondary">
                  Toplam Gönderilen
                </Typography>
                <Typography variant="h5">{statistics.totalSent}</Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} md={3}>
            <Card variant="outlined">
              <CardContent>
                <Typography variant="body2" color="text.secondary">
                  Teslim Edilen
                </Typography>
                <Typography variant="h5" color="success.main">
                  {statistics.totalDelivered}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} md={3}>
            <Card variant="outlined">
              <CardContent>
                <Typography variant="body2" color="text.secondary">
                  Okunan
                </Typography>
                <Typography variant="h5" color="info.main">
                  {statistics.totalRead}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} md={3}>
            <Card variant="outlined">
              <CardContent>
                <Typography variant="body2" color="text.secondary">
                  Başarı Oranı
                </Typography>
                <Typography variant="h5" color="success.main">
                  {statistics.successRate.toFixed(1)}%
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      {/* Filtreler */}
      <Card variant="outlined" sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={2}>
            <Grid item xs={12} md={4}>
              <FormControl fullWidth>
                <InputLabel>Durum</InputLabel>
                <Select
                  value={filters.status}
                  onChange={(e: any) => setFilters({ ...filters, status: e.target.value })}
                  label="Durum"
                >
                  <MenuItem value="">Tümü</MenuItem>
                  <MenuItem value="Pending">Beklemede</MenuItem>
                  <MenuItem value="Sent">Gönderildi</MenuItem>
                  <MenuItem value="Delivered">Teslim Edildi</MenuItem>
                  <MenuItem value="Read">Okundu</MenuItem>
                  <MenuItem value="Failed">Başarısız</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={4}>
              <TextField
                fullWidth
                label="Misafir ID"
                value={filters.guestId}
                onChange={(e: React.ChangeEvent<HTMLInputElement>) => setFilters({ ...filters, guestId: e.target.value })}
                type="number"
              />
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* WhatsApp Geçmişi Tablosu */}
      <TableContainer component={Card}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Tarih</TableCell>
              <TableCell>Telefon</TableCell>
              <TableCell>Mesaj</TableCell>
              <TableCell>Durum</TableCell>
              <TableCell>Misafir</TableCell>
              <TableCell>İşlemler</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {!history || history.data.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} align="center">
                  <Alert severity="info">Henüz WhatsApp mesajı bulunmamaktadır.</Alert>
                </TableCell>
              </TableRow>
            ) : (
              history.data.map((msg) => (
                <TableRow key={msg.id}>
                  <TableCell>{formatDate(msg.sentDate)}</TableCell>
                  <TableCell>{msg.phoneNumber}</TableCell>
                  <TableCell>
                    <Typography variant="body2" noWrap sx={{ maxWidth: 300 }}>
                      {msg.message}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Chip
                      icon={getStatusIcon(msg.status)}
                      label={msg.status}
                      size="small"
                      color={getStatusColor(msg.status)}
                    />
                  </TableCell>
                  <TableCell>
                    {msg.guestName ? (
                      <Chip label={msg.guestName} size="small" variant="outlined" />
                    ) : (
                      <Typography variant="body2" color="text.secondary">
                        -
                      </Typography>
                    )}
                  </TableCell>
                  <TableCell>
                    <Tooltip title="Detayları Görüntüle">
                      <IconButton
                        size="small"
                        onClick={() => handleViewMessage(msg)}
                      >
                        <ViewIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Sayfalama */}
      {history && history.totalCount > pageSize && (
        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 3 }}>
          <Pagination
            count={Math.ceil(history.totalCount / pageSize)}
            page={page}
            onChange={(_, value) => setPage(value)}
            color="primary"
          />
        </Box>
      )}

      {/* Mesaj Gönderme Dialog */}
      <Dialog open={sendDialogOpen} onClose={() => setSendDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <WhatsAppIcon color="success" />
            WhatsApp Mesajı Gönder
          </Box>
        </DialogTitle>
        <DialogContent>
          <TextField
            fullWidth
            margin="normal"
            label="Telefon Numarası"
            value={sendData.phoneNumber}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) => setSendData({ ...sendData, phoneNumber: e.target.value })}
            placeholder="905551234567"
            helperText="Ülke kodu ile birlikte telefon numarası (örn: 905551234567)"
          />
          <TextField
            fullWidth
            margin="normal"
            label="Misafir ID (Opsiyonel)"
            value={sendData.guestId || ''}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              setSendData({
                ...sendData,
                guestId: e.target.value ? parseInt(e.target.value) : undefined,
              })
            }
            type="number"
          />
          <TextField
            fullWidth
            margin="normal"
            label="Mesaj"
            value={sendData.message}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) => setSendData({ ...sendData, message: e.target.value })}
            multiline
            rows={6}
            required
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSendDialogOpen(false)}>İptal</Button>
          <Button
            variant="contained"
            onClick={handleSend}
            disabled={sendMutation.isPending || !sendData.phoneNumber || !sendData.message}
            startIcon={<SendIcon />}
          >
            Gönder
          </Button>
        </DialogActions>
      </Dialog>

      {/* Mesaj Detay Dialog */}
      <Dialog open={viewDialogOpen} onClose={() => setViewDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>WhatsApp Mesaj Detayları</DialogTitle>
        <DialogContent>
          {selectedMessage && (
            <Box sx={{ mt: 2 }}>
              <Grid container spacing={2}>
                <Grid item xs={12}>
                  <Typography variant="subtitle2" color="text.secondary">
                    Telefon Numarası
                  </Typography>
                  <Typography variant="body1">{selectedMessage.phoneNumber}</Typography>
                </Grid>
                <Grid item xs={12}>
                  <Typography variant="subtitle2" color="text.secondary">
                    Mesaj
                  </Typography>
                  <Typography variant="body1">{selectedMessage.message}</Typography>
                </Grid>
                <Grid item xs={12}>
                  <Typography variant="subtitle2" color="text.secondary">
                    Durum
                  </Typography>
                  <Chip
                    icon={getStatusIcon(selectedMessage.status)}
                    label={selectedMessage.status}
                    size="small"
                    color={getStatusColor(selectedMessage.status)}
                  />
                </Grid>
                <Grid item xs={12}>
                  <Typography variant="subtitle2" color="text.secondary">
                    Gönderim Tarihi
                  </Typography>
                  <Typography variant="body1">{formatDate(selectedMessage.sentDate)}</Typography>
                </Grid>
                {selectedMessage.deliveredDate && (
                  <Grid item xs={12}>
                    <Typography variant="subtitle2" color="text.secondary">
                      Teslim Tarihi
                    </Typography>
                    <Typography variant="body1">{formatDate(selectedMessage.deliveredDate)}</Typography>
                  </Grid>
                )}
                {selectedMessage.readDate && (
                  <Grid item xs={12}>
                    <Typography variant="subtitle2" color="text.secondary">
                      Okunma Tarihi
                    </Typography>
                    <Typography variant="body1">{formatDate(selectedMessage.readDate)}</Typography>
                  </Grid>
                )}
                {selectedMessage.errorMessage && (
                  <Grid item xs={12}>
                    <Typography variant="subtitle2" color="text.secondary">
                      Hata Mesajı
                    </Typography>
                    <Alert severity="error">{selectedMessage.errorMessage}</Alert>
                  </Grid>
                )}
              </Grid>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setViewDialogOpen(false)}>Kapat</Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

export default WhatsAppManagementPage

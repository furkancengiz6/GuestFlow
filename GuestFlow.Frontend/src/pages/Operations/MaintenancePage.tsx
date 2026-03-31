// Copyright (c) 2026 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

import { useState, useEffect, useCallback } from 'react'
import {
  Box,
  Card,
  CardContent,
  Typography,
  Chip,
  Button,
  Grid,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  MenuItem,
  Stack,
  alpha,
  useTheme,
  IconButton,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Tooltip
} from '@mui/material'
import {
  Engineering as MaintenanceIcon,
  Add as AddIcon,
  CheckCircle as ResolvedIcon,
  Refresh as RefreshIcon
} from '@mui/icons-material'
import housekeepingService, { MaintenanceRequest, MaintenanceStatus, MaintenancePriority } from '../../services/housekeepingService'
import { formatDateTime } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'

const MaintenancePage = () => {
  const theme = useTheme()
  const [requests, setRequests] = useState<MaintenanceRequest[]>([])
  const [loading, setLoading] = useState(true)

  // Dialog states
  const [createDialog, setCreateDialog] = useState(false)
  const [resolveDialog, setResolveDialog] = useState<{ open: boolean, requestId?: number }>({ open: false })
  const [newRequest, setNewRequest] = useState({
    roomNumber: '',
    issueDescription: '',
    priority: MaintenancePriority.Medium,
    hotelId: 1
  })
  const [resolutionNotes, setResolutionNotes] = useState('')

  const fetchRequests = useCallback(async () => {
    setLoading(true)
    try {
      const response = await housekeepingService.getMaintenanceRequests()
      if (response.success) {
        setRequests(response.data || [])
      }
    } catch (err) {
      console.error('Bakım talepleri yüklenirken bir hata oluştu.', err)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchRequests()
  }, [fetchRequests])

  const handleCreateRequest = async () => {
    try {
      const response = await housekeepingService.createMaintenanceRequest(newRequest)
      if (response.success) {
        setCreateDialog(false)
        fetchRequests()
      }
    } catch (err) {
      console.error('Create request failed', err)
    }
  }

  const handleResolveRequest = async () => {
    if (!resolveDialog.requestId) return

    try {
      const response = await housekeepingService.resolveMaintenanceRequest(resolveDialog.requestId, resolutionNotes)
      if (response.success) {
        setResolveDialog({ open: false })
        fetchRequests()
      }
    } catch (err) {
      console.error('Resolve failed', err)
    }
  }

  const getPriorityColor = (priority: MaintenancePriority) => {
    switch (priority) {
      case MaintenancePriority.Urgent: return 'error'
      case MaintenancePriority.High: return 'warning'
      case MaintenancePriority.Medium: return 'info'
      case MaintenancePriority.Low: return 'success'
      default: return 'default'
    }
  }

  const getStatusColor = (status: MaintenanceStatus) => {
    switch (status) {
      case MaintenanceStatus.Resolved: return 'success'
      case MaintenanceStatus.InProgress: return 'warning'
      case MaintenanceStatus.Pending: return 'info'
      case MaintenanceStatus.Cancelled: return 'error'
      default: return 'default'
    }
  }

  if (loading && requests.length === 0) {
    return <ContentState state="loading" />
  }

  return (
    <Box sx={{ p: 4 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 4 }}>
        <Box>
          <Typography variant="h4" sx={{ fontWeight: 800, color: 'primary.main', mb: 1, display: 'flex', alignItems: 'center', gap: 2 }}>
            <MaintenanceIcon fontSize="large" />
            Teknik Bakım Takibi
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Oda ve tesis genelindeki teknik arızaları yönetin ve çözüm süreçlerini izleyin.
          </Typography>
        </Box>
        <Stack direction="row" spacing={2}>
          <Button 
            variant="contained" 
            startIcon={<AddIcon />} 
            onClick={() => setCreateDialog(true)}
            sx={{ borderRadius: 3, px: 3, py: 1.2 }}
          >
            Yeni Talep
          </Button>
          <Tooltip title="Yenile">
            <IconButton onClick={fetchRequests} sx={{ bgcolor: alpha(theme.palette.primary.main, 0.1) }}>
              <RefreshIcon color="primary" />
            </IconButton>
          </Tooltip>
        </Stack>
      </Box>

      {/* Stats Cards */}
      <Grid container spacing={3} sx={{ mb: 4 }}>
        <Grid item xs={12} md={3}>
          <Card sx={{ borderRadius: 4, bgcolor: alpha(theme.palette.error.main, 0.05), border: '1px solid', borderColor: alpha(theme.palette.error.main, 0.1) }}>
            <CardContent>
              <Typography variant="h6" sx={{ fontWeight: 700, mb: 1 }}>{requests.filter(r => r.priority === MaintenancePriority.Urgent).length}</Typography>
              <Typography variant="body2" color="text.secondary">Acil Talepler</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={3}>
          <Card sx={{ borderRadius: 4, bgcolor: alpha(theme.palette.info.main, 0.05), border: '1px solid', borderColor: alpha(theme.palette.info.main, 0.1) }}>
            <CardContent>
              <Typography variant="h6" sx={{ fontWeight: 700, mb: 1 }}>{requests.filter(r => r.status === MaintenanceStatus.Pending).length}</Typography>
              <Typography variant="body2" color="text.secondary">Bekleyenler</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={3}>
          <Card sx={{ borderRadius: 4, bgcolor: alpha(theme.palette.warning.main, 0.05), border: '1px solid', borderColor: alpha(theme.palette.warning.main, 0.1) }}>
            <CardContent>
              <Typography variant="h6" sx={{ fontWeight: 700, mb: 1 }}>{requests.filter(r => r.status === MaintenanceStatus.InProgress).length}</Typography>
              <Typography variant="body2" color="text.secondary">Devam Edenler</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={3}>
          <Card sx={{ borderRadius: 4, bgcolor: alpha(theme.palette.success.main, 0.05), border: '1px solid', borderColor: alpha(theme.palette.success.main, 0.1) }}>
            <CardContent>
              <Typography variant="h6" sx={{ fontWeight: 700, mb: 1 }}>{requests.filter(r => r.status === MaintenanceStatus.Resolved).length}</Typography>
              <Typography variant="body2" color="text.secondary">Çözülenler</Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Requests Table */}
      <TableContainer component={Paper} sx={{ borderRadius: 4, boxShadow: '0 4px 20px rgba(0,0,0,0.05)', overflow: 'hidden' }}>
        <Table>
          <TableHead sx={{ bgcolor: alpha(theme.palette.primary.main, 0.02) }}>
            <TableRow>
              <TableCell sx={{ fontWeight: 800 }}>Oda No</TableCell>
              <TableCell sx={{ fontWeight: 800 }}>Açıklama</TableCell>
              <TableCell sx={{ fontWeight: 800 }}>Öncelik</TableCell>
              <TableCell sx={{ fontWeight: 800 }}>Durum</TableCell>
              <TableCell sx={{ fontWeight: 800 }}>Tarih</TableCell>
              <TableCell sx={{ fontWeight: 800 }} align="right">İşlemler</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {requests.map((request) => (
              <TableRow key={request.id} hover>
                <TableCell sx={{ fontWeight: 700 }}>{request.roomNumber}</TableCell>
                <TableCell>
                  <Typography variant="body2" sx={{ maxWidth: 300 }} noWrap title={request.issueDescription}>
                    {request.issueDescription}
                  </Typography>
                </TableCell>
                <TableCell>
                  <Chip size="small" label={request.priority} color={getPriorityColor(request.priority)} variant="outlined" />
                </TableCell>
                <TableCell>
                  <Chip size="small" label={request.status} color={getStatusColor(request.status)} />
                </TableCell>
                <TableCell>
                  <Typography variant="caption" color="text.secondary">
                    {formatDateTime(request.reportedDate)}
                  </Typography>
                </TableCell>
                <TableCell align="right">
                  <Button 
                    size="small" 
                    variant="text" 
                    startIcon={<ResolvedIcon />}
                    onClick={() => setResolveDialog({ open: true, requestId: request.id })}
                    disabled={request.status === MaintenanceStatus.Resolved}
                  >
                    Çözüldü
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Create Dialog */}
      <Dialog open={createDialog} onClose={() => setCreateDialog(false)} PaperProps={{ sx: { borderRadius: 4, width: '100%', maxWidth: 450 } }}>
        <DialogTitle sx={{ fontWeight: 800 }}>Yeni Bakım Talebi</DialogTitle>
        <DialogContent>
          <Stack spacing={3} sx={{ mt: 1 }}>
            <TextField 
              fullWidth 
              label="Oda No" 
              value={newRequest.roomNumber}
              onChange={(e) => setNewRequest({ ...newRequest, roomNumber: e.target.value })}
            />
            <TextField
              select
              fullWidth
              label="Öncelik"
              value={newRequest.priority}
              onChange={(e) => setNewRequest({ ...newRequest, priority: e.target.value as MaintenancePriority })}
            >
              {Object.values(MaintenancePriority).map((p) => (
                <MenuItem key={p} value={p}>{p}</MenuItem>
              ))}
            </TextField>
            <TextField 
              fullWidth 
              multiline 
              rows={3} 
              label="Sorun Açıklaması" 
              value={newRequest.issueDescription}
              onChange={(e) => setNewRequest({ ...newRequest, issueDescription: e.target.value })}
            />
          </Stack>
        </DialogContent>
        <DialogActions sx={{ p: 3 }}>
          <Button onClick={() => setCreateDialog(false)} color="inherit">İptal</Button>
          <Button onClick={handleCreateRequest} variant="contained" sx={{ borderRadius: 2 }}>Kaydet</Button>
        </DialogActions>
      </Dialog>

      {/* Resolve Dialog */}
      <Dialog open={resolveDialog.open} onClose={() => setResolveDialog({ open: false })} PaperProps={{ sx: { borderRadius: 4, width: '100%', maxWidth: 450 } }}>
        <DialogTitle sx={{ fontWeight: 800 }}>Bakım Tamamlandı</DialogTitle>
        <DialogContent>
          <TextField 
            fullWidth 
            multiline 
            rows={4} 
            label="Çözüm Notları" 
            sx={{ mt: 1 }}
            value={resolutionNotes}
            onChange={(e) => setResolutionNotes(e.target.value)}
            placeholder="Ne yapıldı? Hangi parçalar değişti?"
          />
        </DialogContent>
        <DialogActions sx={{ p: 3 }}>
          <Button onClick={() => setResolveDialog({ open: false })} color="inherit">İptal</Button>
          <Button onClick={handleResolveRequest} variant="contained" color="success" sx={{ borderRadius: 2 }}>Tamamla</Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

export default MaintenancePage

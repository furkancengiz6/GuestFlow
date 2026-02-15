import { useState } from 'react'
import {
  Box,
  Typography,
  Grid,
  Card,
  CardContent,
  Button,
  TextField,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Alert,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  CircularProgress,
} from '@mui/material'
import {
  Security as SecurityIcon,
  Delete as DeleteIcon,
  VisibilityOff as VisibilityOffIcon,
  History as HistoryIcon,
} from '@mui/icons-material'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { privacyService, PrivacyActionHistory } from '../../services/privacyService'
import { guestService } from '../../services/guestService'
import { formatDate } from '../../utils/formatters'
import { toast } from 'react-toastify'

const PrivacyManagementPage = () => {
  const queryClient = useQueryClient()
  const [anonymizeDialogOpen, setAnonymizeDialogOpen] = useState(false)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [selectedGuestId, setSelectedGuestId] = useState<number | null>(null)
  const [reason, setReason] = useState('')
  const [guestSearch, setGuestSearch] = useState('')
  const [guestIdFilter, setGuestIdFilter] = useState<number | ''>('')
  const [startDate, setStartDate] = useState('')
  const [endDate, setEndDate] = useState('')

  // Fetch privacy action history
  const { data: history = [], isLoading: loadingHistory } = useQuery({
    queryKey: ['privacy-history', startDate, endDate, guestIdFilter],
    queryFn: () =>
      privacyService.getPrivacyActionHistory(
        startDate || undefined,
        endDate || undefined,
        guestIdFilter ? Number(guestIdFilter) : undefined
      ),
  })

  // Search guest by ID or name
  const { data: guest, isLoading: loadingGuest } = useQuery({
    queryKey: ['guest-search', guestSearch],
    queryFn: () => {
      if (!guestSearch) return null
      const guestId = parseInt(guestSearch, 10)
      if (!isNaN(guestId)) {
        return guestService.getGuestDetail(guestId)
      }
      return null
    },
    enabled: !!guestSearch && !isNaN(parseInt(guestSearch, 10)),
  })

  // Anonymize mutation
  const anonymizeMutation = useMutation({
    mutationFn: (request: { guestId: number; reason: string }) =>
      privacyService.anonymizeGuest(request),
    onSuccess: () => {
      toast.success('Misafir verisi başarıyla anonymize edildi.')
      setAnonymizeDialogOpen(false)
      setReason('')
      setSelectedGuestId(null)
      queryClient.invalidateQueries({ queryKey: ['privacy-history'] })
      queryClient.invalidateQueries({ queryKey: ['guest-search'] })
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.message || 'Anonymize işlemi başarısız oldu.')
    },
  })

  // Delete mutation
  const deleteMutation = useMutation({
    mutationFn: (request: { guestId: number; reason: string; confirmDeletion: boolean }) =>
      privacyService.deleteGuest({ ...request, confirmDeletion: true }),
    onSuccess: () => {
      toast.success('Misafir verisi başarıyla silindi.')
      setDeleteDialogOpen(false)
      setReason('')
      setSelectedGuestId(null)
      queryClient.invalidateQueries({ queryKey: ['privacy-history'] })
      queryClient.invalidateQueries({ queryKey: ['guest-search'] })
    },
    onError: (error: any) => {
      toast.error(error?.response?.data?.message || 'Silme işlemi başarısız oldu.')
    },
  })

  const handleAnonymize = () => {
    if (selectedGuestId && reason.trim()) {
      anonymizeMutation.mutate({ guestId: selectedGuestId, reason: reason.trim() })
    }
  }

  const handleDelete = () => {
    if (selectedGuestId && reason.trim()) {
      deleteMutation.mutate({
        guestId: selectedGuestId,
        reason: reason.trim(),
        confirmDeletion: true,
      })
    }
  }

  const getActionTypeColor = (actionType: string) => {
    switch (actionType) {
      case 'Anonymize':
        return 'warning'
      case 'Delete':
        return 'error'
      default:
        return 'default'
    }
  }

  return (
    <Box sx={{ p: 3 }}>
      <Box sx={{ mb: 3, display: 'flex', alignItems: 'center', gap: 2 }}>
        <SecurityIcon sx={{ fontSize: 40 }} color="primary" />
        <Box>
          <Typography variant="h4" component="h1">
            PII Yönetimi (KVKK/GDPR)
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Misafir verilerini anonymize etme ve silme işlemleri
          </Typography>
        </Box>
      </Box>

      <Grid container spacing={3}>
        {/* Guest Search Section */}
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Misafir Ara
              </Typography>
              <Box sx={{ display: 'flex', gap: 2, mb: 2 }}>
                <TextField
                  label="Misafir ID"
                  value={guestSearch}
                  onChange={(e) => setGuestSearch(e.target.value)}
                  placeholder="Misafir ID girin"
                  type="number"
                  sx={{ flex: 1 }}
                />
                {guest && (
                  <Box sx={{ flex: 2, display: 'flex', alignItems: 'center', gap: 2 }}>
                    <Typography variant="body1">
                      <strong>{guest.fullName}</strong> - {guest.email}
                    </Typography>
                    {guest.isAnonymized && (
                      <Chip label="Anonymize Edilmiş" color="warning" size="small" />
                    )}
                    <Button
                      variant="outlined"
                      color="warning"
                      startIcon={<VisibilityOffIcon />}
                      onClick={() => {
                        setSelectedGuestId(guest.id)
                        setAnonymizeDialogOpen(true)
                      }}
                      disabled={guest.isAnonymized}
                    >
                      Anonymize Et
                    </Button>
                    <Button
                      variant="outlined"
                      color="error"
                      startIcon={<DeleteIcon />}
                      onClick={() => {
                        setSelectedGuestId(guest.id)
                        setDeleteDialogOpen(true)
                      }}
                    >
                      Sil
                    </Button>
                  </Box>
                )}
                {loadingGuest && <CircularProgress size={24} />}
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* Privacy Action History */}
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 2 }}>
                <HistoryIcon />
                <Typography variant="h6">Privacy Action History</Typography>
              </Box>

              {/* Filters */}
              <Grid container spacing={2} sx={{ mb: 2 }}>
                <Grid item xs={12} sm={3}>
                  <TextField
                    label="Başlangıç Tarihi"
                    type="date"
                    value={startDate}
                    onChange={(e) => setStartDate(e.target.value)}
                    fullWidth
                    InputLabelProps={{ shrink: true }}
                  />
                </Grid>
                <Grid item xs={12} sm={3}>
                  <TextField
                    label="Bitiş Tarihi"
                    type="date"
                    value={endDate}
                    onChange={(e) => setEndDate(e.target.value)}
                    fullWidth
                    InputLabelProps={{ shrink: true }}
                  />
                </Grid>
                <Grid item xs={12} sm={3}>
                  <TextField
                    label="Misafir ID"
                    type="number"
                    value={guestIdFilter}
                    onChange={(e) => setGuestIdFilter(e.target.value ? Number(e.target.value) : '')}
                    fullWidth
                  />
                </Grid>
                <Grid item xs={12} sm={3}>
                  <Button
                    variant="outlined"
                    onClick={() => {
                      setStartDate('')
                      setEndDate('')
                      setGuestIdFilter('')
                    }}
                    fullWidth
                  >
                    Filtreleri Temizle
                  </Button>
                </Grid>
              </Grid>

              {loadingHistory ? (
                <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
                  <CircularProgress />
                </Box>
              ) : history.length === 0 ? (
                <Alert severity="info">Henüz privacy action history kaydı yok.</Alert>
              ) : (
                <TableContainer>
                  <Table>
                    <TableHead>
                      <TableRow>
                        <TableCell>ID</TableCell>
                        <TableCell>Misafir ID</TableCell>
                        <TableCell>Action Type</TableCell>
                        <TableCell>Reason</TableCell>
                        <TableCell>Requested By</TableCell>
                        <TableCell>Action Date</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {history.map((item: PrivacyActionHistory) => (
                        <TableRow key={item.id}>
                          <TableCell>{item.id}</TableCell>
                          <TableCell>{item.guestId}</TableCell>
                          <TableCell>
                            <Chip
                              label={item.actionType}
                              color={getActionTypeColor(item.actionType)}
                              size="small"
                            />
                          </TableCell>
                          <TableCell>{item.reason}</TableCell>
                          <TableCell>
                            {item.requestedByPersonnelName || item.requestedByPersonnelId || 'N/A'}
                          </TableCell>
                          <TableCell>{formatDate(item.actionDate)}</TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Anonymize Dialog */}
      <Dialog open={anonymizeDialogOpen} onClose={() => setAnonymizeDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Misafir Verisini Anonymize Et</DialogTitle>
        <DialogContent>
          <Alert severity="warning" sx={{ mb: 2 }}>
            Bu işlem geri alınamaz. Misafir verileri anonymize edilecek ve PII bilgileri kaldırılacak.
          </Alert>
          <TextField
            label="Sebep (Zorunlu)"
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            fullWidth
            multiline
            rows={4}
            required
            placeholder="Anonymize etme sebebini açıklayın (KVKK/GDPR uyumu için)"
            sx={{ mt: 2 }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setAnonymizeDialogOpen(false)}>İptal</Button>
          <Button
            onClick={handleAnonymize}
            variant="contained"
            color="warning"
            disabled={!reason.trim() || anonymizeMutation.isPending}
          >
            {anonymizeMutation.isPending ? 'İşleniyor...' : 'Anonymize Et'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Dialog */}
      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Misafir Verisini Sil</DialogTitle>
        <DialogContent>
          <Alert severity="error" sx={{ mb: 2 }}>
            <strong>DİKKAT:</strong> Bu işlem geri alınamaz. Misafir verileri kalıcı olarak silinecek (soft delete).
          </Alert>
          <TextField
            label="Sebep (Zorunlu)"
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            fullWidth
            multiline
            rows={4}
            required
            placeholder="Silme sebebini açıklayın (KVKK/GDPR uyumu için)"
            sx={{ mt: 2 }}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>İptal</Button>
          <Button
            onClick={handleDelete}
            variant="contained"
            color="error"
            disabled={!reason.trim() || deleteMutation.isPending}
          >
            {deleteMutation.isPending ? 'İşleniyor...' : 'Sil'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

export default PrivacyManagementPage

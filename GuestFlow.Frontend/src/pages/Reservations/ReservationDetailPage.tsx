import { useParams, useNavigate } from 'react-router-dom'
import {
  Box,
  Typography,
  Button,
  Chip,
  Grid,
  Card,
  CardContent,
  Divider,
} from '@mui/material'
import {
  ArrowBack as ArrowBackIcon,
} from '@mui/icons-material'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { reservationService } from '../../services/reservationService'
import { formatDate } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'
import { useNotification } from '../../hooks/useNotification'

const ReservationDetailPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const notification = useNotification()

  const { data, isLoading, error } = useQuery({
    queryKey: ['reservation-detail', id],
    queryFn: () => reservationService.getReservationDetail(Number(id)),
    enabled: !!id,
  })

  const confirmMutation = useMutation({
    mutationFn: (id: number) => reservationService.confirmReservation(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reservation-detail', id] })
      queryClient.invalidateQueries({ queryKey: ['reservations'] })
      notification.showSuccess('Rezervasyon onaylandı.')
    },
  })

  const cancelMutation = useMutation({
    mutationFn: (id: number) => reservationService.cancelReservation(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['reservation-detail', id] })
      queryClient.invalidateQueries({ queryKey: ['reservations'] })
      notification.showSuccess('Rezervasyon iptal edildi.')
    },
  })

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

  if (error || !data) {
    return (
      <ContentState
        state="error"
        title="Rezervasyon yüklenemedi"
        description="Lütfen daha sonra tekrar deneyin."
        actionLabel="Geri Dön"
        onAction={() => navigate('/reservations')}
      />
    )
  }

  return (
    <Box p={3}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <Button
            startIcon={<ArrowBackIcon />}
            onClick={() => navigate('/reservations')}
          >
            Geri
          </Button>
          <Typography variant="h4" sx={{ fontWeight: 600 }}>
            Rezervasyon Detayı
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 1 }}>
          {data.status === 'Pending' && (
            <Button
              variant="contained"
              color="success"
              onClick={() => confirmMutation.mutate(data.id)}
              disabled={confirmMutation.isPending}
            >
              Onayla
            </Button>
          )}
          {data.status !== 'Cancelled' && (
            <Button
              variant="outlined"
              color="warning"
              onClick={() => cancelMutation.mutate(data.id)}
              disabled={cancelMutation.isPending}
            >
              İptal Et
            </Button>
          )}
        </Box>
      </Box>

      <Grid container spacing={3}>
        <Grid item xs={12} md={8}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Rezervasyon Bilgileri
              </Typography>
              <Divider sx={{ mb: 2 }} />
              <Grid container spacing={2}>
                <Grid item xs={12} sm={6}>
                  <Typography variant="body2" color="text.secondary">
                    Rezervasyon Tarihi
                  </Typography>
                  <Typography variant="body1" sx={{ fontWeight: 500 }}>
                    {formatDate(data.reservationDate)}
                  </Typography>
                </Grid>
                <Grid item xs={12} sm={6}>
                  <Typography variant="body2" color="text.secondary">
                    Durum
                  </Typography>
                  <Chip
                    label={getStatusLabel(data.status)}
                    color={getStatusColor(data.status) as any}
                    size="small"
                    sx={{ mt: 0.5 }}
                  />
                </Grid>
                <Grid item xs={12} sm={6}>
                  <Typography variant="body2" color="text.secondary">
                    Misafir
                  </Typography>
                  <Button
                    variant="text"
                    onClick={() => navigate(`/guests/${data.guestId}`)}
                    sx={{ textTransform: 'none', fontWeight: 500, p: 0, minWidth: 'auto' }}
                  >
                    {data.guestName || '-'}
                  </Button>
                </Grid>
                <Grid item xs={12} sm={6}>
                  <Typography variant="body2" color="text.secondary">
                    Personel
                  </Typography>
                  <Typography variant="body1" sx={{ fontWeight: 500 }}>
                    {data.personnelName || '-'}
                  </Typography>
                </Grid>
                {data.note && (
                  <Grid item xs={12}>
                    <Typography variant="body2" color="text.secondary">
                      Not
                    </Typography>
                    <Typography variant="body1" sx={{ fontWeight: 500 }}>
                      {data.note}
                    </Typography>
                  </Grid>
                )}
                <Grid item xs={12} sm={6}>
                  <Typography variant="body2" color="text.secondary">
                    Kayıt Tarihi
                  </Typography>
                  <Typography variant="body1" sx={{ fontWeight: 500 }}>
                    {formatDate(data.createdDate)}
                  </Typography>
                </Grid>
              </Grid>
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} md={4}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                İlişkili Kayıtlar
              </Typography>
              <Divider sx={{ mb: 2 }} />
              {data.transfers && data.transfers.length > 0 && (
                <Box sx={{ mb: 2 }}>
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    Transferler ({data.transfers.length})
                  </Typography>
                  {data.transfers.map((transfer: any) => (
                    <Button
                      key={transfer.id}
                      variant="text"
                      onClick={() => navigate(`/transfers/${transfer.id}`)}
                      sx={{ textTransform: 'none', display: 'block', textAlign: 'left' }}
                    >
                      {transfer.pickupAddress} → {transfer.dropoffAddress}
                    </Button>
                  ))}
                </Box>
              )}
              {data.tours && data.tours.length > 0 && (
                <Box sx={{ mb: 2 }}>
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    Turlar ({data.tours.length})
                  </Typography>
                  {data.tours.map((tour: any) => (
                    <Button
                      key={tour.id}
                      variant="text"
                      onClick={() => navigate(`/tours/${tour.type}/${tour.id}`)}
                      sx={{ textTransform: 'none', display: 'block', textAlign: 'left' }}
                    >
                      {tour.name || `Tur #${tour.id}`}
                    </Button>
                  ))}
                </Box>
              )}
              {data.invoices && data.invoices.length > 0 && (
                <Box>
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    Faturalar ({data.invoices.length})
                  </Typography>
                  {data.invoices.map((invoice: any) => (
                    <Button
                      key={invoice.id}
                      variant="text"
                      onClick={() => navigate(`/invoices/${invoice.id}`)}
                      sx={{ textTransform: 'none', display: 'block', textAlign: 'left' }}
                    >
                      {invoice.invoiceNumber || `Fatura #${invoice.id}`}
                    </Button>
                  ))}
                </Box>
              )}
              {(!data.transfers || data.transfers.length === 0) &&
                (!data.tours || data.tours.length === 0) &&
                (!data.invoices || data.invoices.length === 0) && (
                  <Typography variant="body2" color="text.secondary">
                    İlişkili kayıt bulunamadı.
                  </Typography>
                )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  )
}

export default ReservationDetailPage


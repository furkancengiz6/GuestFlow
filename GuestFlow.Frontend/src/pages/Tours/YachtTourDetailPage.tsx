import { useParams, useNavigate } from 'react-router-dom'
import {
  Box,
  Card,
  CardContent,
  Typography,
  Grid,
  Chip,
  Button,
  Divider,
  IconButton,
} from '@mui/material'
import { useLiveUpdates } from '../../hooks/useLiveUpdates'
import {
  ArrowBack as ArrowBackIcon,
  Person as PersonIcon,
  LocationOn as LocationOnIcon,
  AttachMoney as AttachMoneyIcon,
  DirectionsBoat as DirectionsBoatIcon,
  People as PeopleIcon,
  Edit as EditIcon,
  Receipt as ReceiptIcon,
  Email as EmailIcon,
} from '@mui/icons-material'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { tourService } from '../../services/tourService'
import { useNotification } from '../../hooks/useNotification'
import { formatDate, formatCurrency, formatDateTime } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'

const YachtTourDetailPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const tourId = id ? parseInt(id, 10) : 0

  // Enable real-time updates for yacht tour changes
  useLiveUpdates(['yachttour'])

  const { data: tour, isLoading, error } = useQuery({
    queryKey: ['yacht-tour-detail', tourId],
    queryFn: () => tourService.getYachtTourDetail(tourId),
    enabled: !!tourId && !isNaN(tourId),
  })

  const queryClient = useQueryClient()
  const notification = useNotification()

  // Mutations for action buttons
  const markCompletedMutation = useMutation({
    mutationFn: () => tourService.markYachtTourCompleted(tourId),
    onSuccess: () => {
      notification.showSuccess('Yat turu tamamlandı olarak işaretlendi')
      queryClient.invalidateQueries({ queryKey: ['yacht-tour-detail', tourId] })
      queryClient.invalidateQueries({ queryKey: ['yacht-tours'] })
    },
    onError: (error: any) => {
      notification.showError(`Hata: ${error.response?.data?.message || 'Durum güncellenemedi'}`)
    }
  })

  const cancelTourMutation = useMutation({
    mutationFn: () => tourService.cancelYachtTour(tourId),
    onSuccess: () => {
      notification.showSuccess('Yat turu iptal edildi')
      queryClient.invalidateQueries({ queryKey: ['yacht-tour-detail', tourId] })
      queryClient.invalidateQueries({ queryKey: ['yacht-tours'] })
    },
    onError: (error: any) => {
      notification.showError(`Hata: ${error.response?.data?.message || 'Tur iptal edilemedi'}`)
    }
  })

  const createYachtTourInvoiceMutation = useMutation({
    mutationFn: () => tourService.createYachtTourInvoice(tourId),
    onSuccess: () => {
      notification.showSuccess('Fatura başarıyla oluşturuldu')
      queryClient.invalidateQueries({ queryKey: ['yacht-tour-detail', tourId] })
      queryClient.invalidateQueries({ queryKey: ['invoices'] })
    },
    onError: (error: any) => {
      notification.showError(`Hata: ${error.response?.data?.message || 'Fatura oluşturulamadı'}`)
    }
  })

  const sendYachtTourConfirmationMutation = useMutation({
    mutationFn: () => tourService.sendYachtTourConfirmation(tourId),
    onSuccess: () => {
      notification.showSuccess('Onay maili gönderildi')
    },
    onError: (error: any) => {
      notification.showError(`Hata: ${error.response?.data?.message || 'Mail gönderilemedi'}`)
    }
  })

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={8} />
  }

  if (error || !tour) {
    return (
      <ContentState
        state="error"
        title="Yat turu detayı yüklenemedi"
        description="Lütfen daha sonra tekrar deneyin."
        actionLabel="Geri dön"
        onAction={() => navigate('/tours')}
      />
    )
  }

  return (
    <Box p={3}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
        <IconButton onClick={() => navigate('/tours')} color="primary">
          <ArrowBackIcon />
        </IconButton>
        <Typography variant="h4" sx={{ fontWeight: 600 }}>
          Yat Turu Detayı
        </Typography>
      </Box>

      {/* ACTION BUTTONS */}
      <Box sx={{ mb: 3, display: 'flex', gap: 1, flexWrap: 'wrap' }}>
        <Button
          variant="contained"
          color="primary"
          startIcon={<EditIcon />}
          onClick={() => navigate(`/tours/yacht/${tourId}/edit`)}
        >
          Düzenle
        </Button>
        <Button
          variant="outlined"
          color="success"
          startIcon={<PersonIcon />}
          onClick={() => markCompletedMutation.mutate()}
          disabled={tour.status === 'Completed' || markCompletedMutation.isPending}
        >
          {markCompletedMutation.isPending ? 'İşleniyor...' : 'Tamamlandı İşaretle'}
        </Button>
        <Button
          variant="outlined"
          color="warning"
          startIcon={<ReceiptIcon />}
          onClick={() => createYachtTourInvoiceMutation.mutate()}
          disabled={createYachtTourInvoiceMutation.isPending}
        >
          {createYachtTourInvoiceMutation.isPending ? 'Oluşturuluyor...' : 'Fatura Oluştur'}
        </Button>
        <Button
          variant="outlined"
          color="info"
          startIcon={<EmailIcon />}
          onClick={() => sendYachtTourConfirmationMutation.mutate()}
          disabled={sendYachtTourConfirmationMutation.isPending}
        >
          {sendYachtTourConfirmationMutation.isPending ? 'Gönderiliyor...' : 'Onay Gönder'}
        </Button>
        <Button
          variant="outlined"
          color="error"
          onClick={() => {
            if (window.confirm('Bu yat turunu iptal etmek istediğinizden emin misiniz?')) {
              cancelTourMutation.mutate()
            }
          }}
          disabled={tour.status === 'Cancelled' || cancelTourMutation.isPending}
        >
          {cancelTourMutation.isPending ? 'İptal Ediliyor...' : 'İptal Et'}
        </Button>
      </Box>

      {/* Yat Turu Bilgileri */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
            <Box>
              <Typography variant="h5" gutterBottom>
                Yat Turu #{tour.id}
              </Typography>
            </Box>
            <Box sx={{ textAlign: 'right' }}>
              <Typography variant="body2" color="text.secondary">
                Oluşturulma Tarihi
              </Typography>
              <Typography variant="body1">{formatDate(tour.createdDate)}</Typography>
            </Box>
          </Box>
          <Divider sx={{ my: 2 }} />
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Tur Tarihi
              </Typography>
              <Typography variant="body1" fontWeight="medium">
                {formatDateTime(tour.tourDate)}
              </Typography>
            </Grid>
            <Grid item xs={12} md={6}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <AttachMoneyIcon color="action" />
                <Typography variant="body2" color="text.secondary">
                  Fiyat
                </Typography>
              </Box>
              <Typography variant="body1" fontWeight="medium" color="primary">
                {formatCurrency(tour.finalPrice)} (Orijinal: {formatCurrency(tour.price)})
              </Typography>
            </Grid>
            {tour.paymentStatus && (
              <Grid item xs={12}>
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  Ödeme Durumu
                </Typography>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                  <Chip
                    label={tour.paymentStatus === 'Paid' ? 'Ödendi' :
                           tour.paymentStatus === 'PartiallyPaid' ? 'Kısmi Ödeme' : 'Ödenmedi'}
                    color={tour.paymentStatus === 'Paid' ? 'success' :
                           tour.paymentStatus === 'PartiallyPaid' ? 'warning' : 'error'}
                    size="small"
                  />
                  {tour.paidAmount !== undefined && tour.remainingAmount !== undefined && (
                    <Box sx={{ display: 'flex', gap: 2, ml: 2 }}>
                      <Typography variant="body2" color="text.secondary">
                        Ödenen: <span style={{ fontWeight: 'bold', color: '#2e7d32' }}>{formatCurrency(tour.paidAmount)}</span>
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Kalan: <span style={{ fontWeight: 'bold', color: '#d32f2f' }}>{formatCurrency(tour.remainingAmount)}</span>
                      </Typography>
                    </Box>
                  )}
                </Box>
                {tour.paidAmountByCurrency && Object.keys(tour.paidAmountByCurrency).length > 0 && (
                  <Box sx={{ mt: 1 }}>
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                      Para Birimine Göre Ödemeler:
                    </Typography>
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                      {Object.entries(tour.paidAmountByCurrency).map(([currency, amount]) => (
                        <Chip
                          key={currency}
                          label={`${currency}: ${formatCurrency(amount)}`}
                          variant="outlined"
                          size="small"
                        />
                      ))}
                    </Box>
                  </Box>
                )}
              </Grid>
            )}
            <Grid item xs={12} md={6}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <DirectionsBoatIcon color="action" />
                <Typography variant="body2" color="text.secondary">
                  Yat Adı
                </Typography>
              </Box>
              <Typography variant="body1" fontWeight="medium">{tour.yachtName}</Typography>
            </Grid>
            <Grid item xs={12} md={6}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <PeopleIcon color="action" />
                <Typography variant="body2" color="text.secondary">
                  Kişi Sayısı
                </Typography>
              </Box>
              <Typography variant="body1">{tour.numberOfPeople} kişi</Typography>
            </Grid>
            {tour.specialRequest && (
              <Grid item xs={12}>
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  Özel İstek
                </Typography>
                <Typography variant="body1">{tour.specialRequest}</Typography>
              </Grid>
            )}
          </Grid>
        </CardContent>
      </Card>

      <Grid container spacing={3}>
        {/* Misafir Bilgileri */}
        {tour.guest && (
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                  <PersonIcon color="primary" />
                  <Typography variant="h6">Misafir</Typography>
                </Box>
                <Divider sx={{ mb: 2 }} />
                <Typography variant="body2" color="text.secondary">
                  Ad Soyad
                </Typography>
                <Typography variant="body1" fontWeight="medium" gutterBottom>
                  {tour.guest.fullName}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Misafir Kodu
                </Typography>
                <Typography variant="body1" gutterBottom>
                  {tour.guest.guestCode}
                </Typography>
                {tour.guest.email && (
                  <>
                    <Typography variant="body2" color="text.secondary">
                      E-posta
                    </Typography>
                    <Typography variant="body1" gutterBottom>
                      {tour.guest.email}
                    </Typography>
                  </>
                )}
                {tour.guest.phoneNumber && (
                  <>
                    <Typography variant="body2" color="text.secondary">
                      Telefon
                    </Typography>
                    <Typography variant="body1" gutterBottom>
                      {tour.guest.phoneNumber}
                    </Typography>
                  </>
                )}
                <Typography variant="body2" color="text.secondary">
                  Uyruk
                </Typography>
                <Typography variant="body1" gutterBottom>
                  {tour.guest.nationality}
                </Typography>
                {tour.guest.isSpecialGuest && (
                  <Chip label="Özel Misafir" color="primary" size="small" sx={{ mt: 1 }} />
                )}
                <Button
                  variant="outlined"
                  size="small"
                  sx={{ mt: 2 }}
                  onClick={() => navigate(`/guests/${tour.guest!.id}`)}
                >
                  Misafir Detayı
                </Button>
              </CardContent>
            </Card>
          </Grid>
        )}

        {/* Personel Bilgileri */}
        {tour.personnel && (
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                  <PersonIcon color="primary" />
                  <Typography variant="h6">Personel</Typography>
                </Box>
                <Divider sx={{ mb: 2 }} />
                <Typography variant="body2" color="text.secondary">
                  Ad Soyad
                </Typography>
                <Typography variant="body1" fontWeight="medium" gutterBottom>
                  {tour.personnel.fullName}
                </Typography>
                {tour.personnel.email && (
                  <>
                    <Typography variant="body2" color="text.secondary">
                      E-posta
                    </Typography>
                    <Typography variant="body1" gutterBottom>
                      {tour.personnel.email}
                    </Typography>
                  </>
                )}
                <Typography variant="body2" color="text.secondary">
                  Rol
                </Typography>
                <Typography variant="body1">{tour.personnel.userType}</Typography>
              </CardContent>
            </Card>
          </Grid>
        )}

        {/* Şehir Bilgileri */}
        {tour.city && (
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                  <LocationOnIcon color="primary" />
                  <Typography variant="h6">Şehir</Typography>
                </Box>
                <Divider sx={{ mb: 2 }} />
                <Typography variant="body2" color="text.secondary">
                  Şehir Adı
                </Typography>
                <Typography variant="body1" fontWeight="medium" gutterBottom>
                  {tour.city.cityName}
                </Typography>
                {tour.city.country && (
                  <>
                    <Typography variant="body2" color="text.secondary">
                      Ülke
                    </Typography>
                    <Typography variant="body1">{tour.city.country}</Typography>
                  </>
                )}
              </CardContent>
            </Card>
          </Grid>
        )}
      </Grid>
    </Box>
  )
}

export default YachtTourDetailPage

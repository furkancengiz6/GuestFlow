import { useParams, useNavigate } from 'react-router-dom'
import {
  Box,
  Typography,
  Grid,
  Card,
  CardContent,
  Chip,
  Button,
  Divider,
  IconButton,
} from '@mui/material'
import { useLiveUpdates } from '../../hooks/useLiveUpdates'
import {
  ArrowBack as ArrowBackIcon,
  Person as PersonIcon,
  DirectionsCar as DirectionsCarIcon,
  FlightTakeoff as FlightTakeoffIcon,
  LocationOn as LocationOnIcon,
  Edit as EditIcon,
  Receipt as ReceiptIcon,
  Email as EmailIcon,
  AttachMoney as AttachMoneyIcon,
} from '@mui/icons-material'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { transferService } from '../../services/transferService'
import { useNotification } from '../../hooks/useNotification'
import { formatDate, formatCurrency, formatDateTime } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'

const getStatusColor = (status: string) => {
  switch (status.toLowerCase()) {
    case 'completed':
      return 'success'
    case 'pending':
      return 'warning'
    case 'inprogress':
      return 'info'
    case 'cancelled':
      return 'error'
    default:
      return 'default'
  }
}

const getStatusLabel = (status: string) => {
  switch (status.toLowerCase()) {
    case 'completed':
      return 'Tamamlandı'
    case 'pending':
      return 'Beklemede'
    case 'inprogress':
      return 'Devam Ediyor'
    case 'cancelled':
      return 'İptal Edildi'
    default:
      return status
  }
}

const TransferDetailPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const transferId = id ? parseInt(id, 10) : 0

  // Enable real-time updates for transfer changes
  useLiveUpdates(['transfer'])

  const { data: transfer, isLoading, error } = useQuery({
    queryKey: ['transfer-detail', transferId],
    queryFn: () => transferService.getTransferDetail(transferId),
    enabled: !!transferId && !isNaN(transferId),
  })

  const queryClient = useQueryClient()
  const notification = useNotification()

  // Mutations for action buttons
  const markCompletedMutation = useMutation({
    mutationFn: () => transferService.markTransferCompleted(transferId),
    onSuccess: () => {
      notification.showSuccess('Transfer tamamlandı olarak işaretlendi')
      queryClient.invalidateQueries({ queryKey: ['transfer-detail', transferId] })
      queryClient.invalidateQueries({ queryKey: ['transfers'] })
    },
    onError: (error: any) => {
      notification.showError(`Hata: ${error.response?.data?.message || 'Transfer tamamlanamadı'}`)
    }
  })

  const cancelTransferMutation = useMutation({
    mutationFn: () => transferService.cancelTransfer(transferId),
    onSuccess: () => {
      notification.showSuccess('Transfer iptal edildi')
      queryClient.invalidateQueries({ queryKey: ['transfer-detail', transferId] })
      queryClient.invalidateQueries({ queryKey: ['transfers'] })
    },
    onError: (error: any) => {
      notification.showError(`Hata: ${error.response?.data?.message || 'Transfer iptal edilemedi'}`)
    }
  })

  const createInvoiceMutation = useMutation({
    mutationFn: () => transferService.createTransferInvoice(transferId),
    onSuccess: () => {
      notification.showSuccess('Fatura başarıyla oluşturuldu')
      queryClient.invalidateQueries({ queryKey: ['transfer-detail', transferId] })
      queryClient.invalidateQueries({ queryKey: ['invoices'] })
    },
    onError: (error: any) => {
      notification.showError(`Hata: ${error.response?.data?.message || 'Fatura oluşturulamadı'}`)
    }
  })

  const sendConfirmationMutation = useMutation({
    mutationFn: () => transferService.sendTransferConfirmation(transferId),
    onSuccess: () => {
      notification.showSuccess('Onay maili gönderildi')
    },
    onError: (error: any) => {
      notification.showError(`Hata: ${error.response?.data?.message || 'Mail gönderilemedi'}`)
    }
  })

  const createRoundTripMutation = useMutation({
    mutationFn: () => transferService.createRoundTrip(transferId),
    onSuccess: () => {
      notification.showSuccess('Gidiş-dönüş transfer oluşturuldu')
      queryClient.invalidateQueries({ queryKey: ['transfers'] })
    },
    onError: (error: any) => {
      notification.showError(`Hata: ${error.response?.data?.message || 'Gidiş-dönüş transfer oluşturulamadı'}`)
    }
  })

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={8} />
  }

  if (error || !transfer) {
    return (
      <ContentState
        state="error"
        title="Transfer detayı yüklenemedi"
        description="Lütfen daha sonra tekrar deneyin."
        actionLabel="Geri dön"
        onAction={() => navigate('/transfers')}
      />
    )
  }

  return (
    <Box p={3}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
        <IconButton onClick={() => navigate('/transfers')} color="primary">
          <ArrowBackIcon />
        </IconButton>
        <Typography variant="h4" sx={{ fontWeight: 600 }}>
          Transfer Detayı
        </Typography>
      </Box>

      {/* HEADER - Transfer ID, Priority & Status */}
      <Card sx={{ mb: 3, backgroundColor: 'primary.light', color: 'white' }}>
        <CardContent>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <Box>
              <Typography variant="h4" sx={{ fontWeight: 'bold', mb: 1 }}>
                Transfer #{transfer.id}
              </Typography>
              <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
                <Chip
                  label="ACİL"
                  color="error"
                  size="small"
                  sx={{ backgroundColor: 'white', color: 'error.main', fontWeight: 'bold' }}
                />
                <Chip
                  label={getStatusLabel(transfer.status || 'pending')}
                  color={getStatusColor(transfer.status || 'pending') as any}
                  size="small"
                  sx={{ backgroundColor: 'white' }}
                />
                <Chip
                  label="VIP"
                  color="warning"
                  size="small"
                  sx={{ backgroundColor: 'white', color: 'warning.dark' }}
                />
              </Box>
            </Box>
            <Box sx={{ textAlign: 'right' }}>
              <Typography variant="body2" sx={{ opacity: 0.8 }}>
                Oluşturulma Tarihi
              </Typography>
              <Typography variant="body1" sx={{ fontWeight: 'medium' }}>
                {formatDate(transfer.createdDate)}
              </Typography>
            </Box>
          </Box>
        </CardContent>
      </Card>

      {/* ACTION BUTTONS */}
      <Box sx={{ mb: 3, display: 'flex', gap: 1, flexWrap: 'wrap' }}>
        <Button
          variant="contained"
          color="primary"
          startIcon={<EditIcon />}
          onClick={() => navigate(`/transfers/${transferId}/edit`)}
        >
          Düzenle
        </Button>
        <Button
          variant="outlined"
          color="success"
          startIcon={<PersonIcon />}
          onClick={() => markCompletedMutation.mutate()}
          disabled={transfer.status === 'Completed' || markCompletedMutation.isPending}
        >
          {markCompletedMutation.isPending ? 'İşleniyor...' : 'Tamamlandı İşaretle'}
        </Button>
        <Button
          variant="outlined"
          color="warning"
          startIcon={<ReceiptIcon />}
          onClick={() => createInvoiceMutation.mutate()}
          disabled={createInvoiceMutation.isPending}
        >
          {createInvoiceMutation.isPending ? 'Oluşturuluyor...' : 'Fatura Oluştur'}
        </Button>
        <Button
          variant="outlined"
          color="info"
          startIcon={<EmailIcon />}
          onClick={() => sendConfirmationMutation.mutate()}
          disabled={sendConfirmationMutation.isPending}
        >
          {sendConfirmationMutation.isPending ? 'Gönderiliyor...' : 'Onay Gönder'}
        </Button>
        <Button
          variant="outlined"
          color="secondary"
          onClick={() => createRoundTripMutation.mutate()}
          disabled={createRoundTripMutation.isPending}
        >
          {createRoundTripMutation.isPending ? 'Oluşturuluyor...' : 'Gidiş-Dönüş Oluştur'}
        </Button>
        <Button
          variant="outlined"
          color="error"
          onClick={() => {
            if (window.confirm('Bu transferi iptal etmek istediğinizden emin misiniz?')) {
              cancelTransferMutation.mutate()
            }
          }}
          disabled={transfer.status === 'Cancelled' || cancelTransferMutation.isPending}
        >
          {cancelTransferMutation.isPending ? 'İptal Ediliyor...' : 'İptal Et'}
        </Button>
      </Box>

      {/* GUEST INFO CARD */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <PersonIcon color="primary" />
            MİSAFİR BİLGİLERİ
          </Typography>
          <Divider sx={{ mb: 2 }} />
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <Typography variant="body2" color="text.secondary">Misafir Adı</Typography>
              <Typography variant="body1" fontWeight="medium">
                {transfer.guest?.fullName || 'Bilinmiyor'}
              </Typography>
            </Grid>
            <Grid item xs={12} md={6}>
              <Typography variant="body2" color="text.secondary">İletişim</Typography>
              <Typography variant="body1">{transfer.guest?.phoneNumber || '-'}</Typography>
              <Typography variant="body2">{transfer.guest?.email || '-'}</Typography>
            </Grid>
            <Grid item xs={12} md={6}>
              <Typography variant="body2" color="text.secondary">Konuşulan Dil</Typography>
              <Typography variant="body1">{(transfer as any).guestLanguage || 'Türkçe'}</Typography>
            </Grid>
            <Grid item xs={12} md={6}>
              <Typography variant="body2" color="text.secondary">Acil İletişim</Typography>
              <Typography variant="body1">{(transfer as any).emergencyContactPhone || '-'}</Typography>
            </Grid>
            <Grid item xs={12} md={6}>
              <Typography variant="body2" color="text.secondary">Grup Boyutu</Typography>
              <Typography variant="body1">{(transfer as any).groupSize || 1} kişi</Typography>
            </Grid>
            <Grid item xs={12} md={6}>
              <Typography variant="body2" color="text.secondary">Çocuk/Bebek</Typography>
              <Typography variant="body1">
                {(transfer as any).childCount || 0} çocuk, {(transfer as any).infantCount || 0} bebek
              </Typography>
            </Grid>
            <Grid item xs={12}>
              <Typography variant="body2" color="text.secondary">Özel İhtiyaçlar</Typography>
              <Typography variant="body1">{(transfer as any).accessibilityRequirements || 'Yok'}</Typography>
            </Grid>
            <Grid item xs={12}>
              <Typography variant="body2" color="text.secondary">Misafir Notları</Typography>
              <Typography variant="body1">{(transfer as any).guestVisibleNotes || 'Yok'}</Typography>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* LOGISTICS CARD */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <DirectionsCarIcon color="primary" />
            LOJİSTİK BİLGİLERİ
          </Typography>
          <Divider sx={{ mb: 2 }} />
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <Typography variant="body2" color="text.secondary">Rota</Typography>
              <Typography variant="body1" fontWeight="medium">
                {transfer.pickupAddress} → {transfer.dropoffAddress}
              </Typography>
            </Grid>
            <Grid item xs={12} md={6}>
              <Typography variant="body2" color="text.secondary">Transfer Tarihi</Typography>
              <Typography variant="body1">{formatDateTime(transfer.transferDate)}</Typography>
            </Grid>
            <Grid item xs={12} md={6}>
              <Typography variant="body2" color="text.secondary">Alış Saati</Typography>
              <Typography variant="body1">{transfer.pickupTime || '-'}</Typography>
            </Grid>
            <Grid item xs={12} md={6}>
              <Typography variant="body2" color="text.secondary">Servis Başlangıç</Typography>
              <Typography variant="body1">{transfer.serviceStartTime || '-'}</Typography>
            </Grid>
            <Grid item xs={12} md={6}>
              <Typography variant="body2" color="text.secondary">Onay Zamanları</Typography>
              <Typography variant="body1">
                Alış: {(transfer as any).pickupConfirmationTime ? formatDateTime((transfer as any).pickupConfirmationTime) : '-'}
              </Typography>
              <Typography variant="body1">
                Bırakış: {(transfer as any).dropoffConfirmationTime ? formatDateTime((transfer as any).dropoffConfirmationTime) : '-'}
              </Typography>
            </Grid>
            <Grid item xs={12} md={6}>
              <Typography variant="body2" color="text.secondary">Şoför Ataması</Typography>
              <Typography variant="body1">{(transfer as any).driverName || 'Atanmamış'}</Typography>
              <Typography variant="body2" color="text.secondary">Şoför ID: {(transfer as any).driverId || '-'}</Typography>
            </Grid>
            <Grid item xs={12} md={6}>
              <Typography variant="body2" color="text.secondary">Araç Ataması</Typography>
              <Typography variant="body1">{(transfer as any).vehicleId ? `Araç #${(transfer as any).vehicleId}` : 'Atanmamış'}</Typography>
            </Grid>
            <Grid item xs={12}>
              <Typography variant="body2" color="text.secondary">Koordinasyon Noktası</Typography>
              <Typography variant="body1">{(transfer as any).meetingPointDetails || 'Belirtilmemiş'}</Typography>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* FINANCIAL CARD */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <AttachMoneyIcon color="primary" />
            FİNANSAL BİLGİLER
          </Typography>
          <Divider sx={{ mb: 2 }} />
          <Grid container spacing={3}>
            <Grid item xs={12} md={4}>
              <Typography variant="body2" color="text.secondary">Taban Fiyat</Typography>
              <Typography variant="h6" color="primary.main">
                {formatCurrency(transfer.price)}
              </Typography>
            </Grid>
            <Grid item xs={12} md={4}>
              <Typography variant="body2" color="text.secondary">İndirim</Typography>
              <Typography variant="h6" color="error.main">
                -{formatCurrency((transfer.price - transfer.finalPrice))}
              </Typography>
            </Grid>
            <Grid item xs={12} md={4}>
              <Typography variant="body2" color="text.secondary">Final Fiyat</Typography>
              <Typography variant="h6" color="success.main" fontWeight="bold">
                {formatCurrency(transfer.finalPrice)}
              </Typography>
            </Grid>
            <Grid item xs={12} md={6}>
              <Typography variant="body2" color="text.secondary">Ödeme Durumu</Typography>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <Chip
                  label={transfer.paymentStatus === 'Paid' ? 'Ödendi' :
                         transfer.paymentStatus === 'PartiallyPaid' ? 'Kısmi Ödeme' : 'Ödenmedi'}
                  color={transfer.paymentStatus === 'Paid' ? 'success' :
                         transfer.paymentStatus === 'PartiallyPaid' ? 'warning' : 'error'}
                  size="small"
                />
                {(transfer as any).remainingAmount && (transfer as any).remainingAmount > 0 && (
                  <Typography variant="body2" color="error.main">
                    Kalan: {formatCurrency(Number((transfer as any).remainingAmount))}
                  </Typography>
                )}
              </Box>
            </Grid>
            <Grid item xs={12} md={6}>
              <Typography variant="body2" color="text.secondary">Tedarikçi Maliyeti</Typography>
              <Typography variant="body1">
                {(transfer as any).supplierCost ? formatCurrency((transfer as any).supplierCost) : '-'}
              </Typography>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* COORDINATION CARD - INTERNAL ONLY */}
      <Card sx={{ mb: 3, border: '2px solid', borderColor: 'warning.main' }}>
        <CardContent>
          <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1, color: 'warning.main' }}>
            🔒 İÇ KOORDİNASYON (PERSONEL GÖRÜNÜMLÜ)
          </Typography>
          <Divider sx={{ mb: 2 }} />
          <Grid container spacing={3}>
            <Grid item xs={12}>
              <Typography variant="body2" color="text.secondary">Concierge Notları</Typography>
              <Typography variant="body1">{(transfer as any).conciergeInternalNotes || 'Not yok'}</Typography>
            </Grid>
            <Grid item xs={12} md={6}>
              <Typography variant="body2" color="text.secondary">Tedarikçi İletişim</Typography>
              <Typography variant="body1">{(transfer as any).supplierContactPhone || '-'}</Typography>
              <Typography variant="body2">{(transfer as any).supplierEmergencyContact || '-'}</Typography>
            </Grid>
            <Grid item xs={12} md={6}>
              <Typography variant="body2" color="text.secondary">Son Güncelleme</Typography>
              <Typography variant="body1">{formatDateTime((transfer as any).updatedDate)}</Typography>
              <Typography variant="body2" color="text.secondary">
                Personel ID: {(transfer as any).updatedByPersonnelId || (transfer as any).createdByPersonnelId}
              </Typography>
            </Grid>
          </Grid>
        </CardContent>
      </Card>
    </Box>
  )
}

export default TransferDetailPage

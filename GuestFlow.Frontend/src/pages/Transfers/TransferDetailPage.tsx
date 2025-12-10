import { useParams, useNavigate } from 'react-router-dom'
import {
  Box,
  Paper,
  Typography,
  Grid,
  Card,
  CardContent,
  Chip,
  Button,
  Divider,
  IconButton,
} from '@mui/material'
import {
  ArrowBack as ArrowBackIcon,
  Person as PersonIcon,
  DirectionsCar as DirectionsCarIcon,
  FlightTakeoff as FlightTakeoffIcon,
  LocationOn as LocationOnIcon,
  AttachMoney as AttachMoneyIcon,
} from '@mui/icons-material'
import { useQuery } from '@tanstack/react-query'
import { transferService } from '../../services/transferService'
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

  const { data: transfer, isLoading, error } = useQuery({
    queryKey: ['transfer-detail', transferId],
    queryFn: () => transferService.getTransferDetail(transferId),
    enabled: !!transferId && !isNaN(transferId),
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

      {/* Transfer Bilgileri */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
            <Box>
              <Typography variant="h5" gutterBottom>
                Transfer #{transfer.id}
              </Typography>
              <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', mt: 2 }}>
                <Chip
                  label={getStatusLabel(transfer.status)}
                  color={getStatusColor(transfer.status) as any}
                  size="medium"
                />
                {transfer.isFromAirport && (
                  <Chip icon={<FlightTakeoffIcon />} label="Havaalanından" color="info" />
                )}
              </Box>
            </Box>
            <Box sx={{ textAlign: 'right' }}>
              <Typography variant="body2" color="text.secondary">
                Oluşturulma Tarihi
              </Typography>
              <Typography variant="body1">{formatDate(transfer.createdDate)}</Typography>
            </Box>
          </Box>
          <Divider sx={{ my: 2 }} />
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Transfer Tarihi
              </Typography>
              <Typography variant="body1" fontWeight="medium">
                {formatDateTime(transfer.transferDate)}
              </Typography>
            </Grid>
            <Grid item xs={12} md={6}>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Fiyat
              </Typography>
              <Typography variant="body1" fontWeight="medium" color="primary">
                {formatCurrency(transfer.finalPrice)} (Orijinal: {formatCurrency(transfer.price)})
              </Typography>
            </Grid>
            <Grid item xs={12} md={6}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <LocationOnIcon color="action" />
                <Typography variant="body2" color="text.secondary">
                  Alış Adresi
                </Typography>
              </Box>
              <Typography variant="body1">{transfer.pickupAddress}</Typography>
              {transfer.pickupCity && (
                <Typography variant="body2" color="text.secondary">
                  {transfer.pickupCity.cityName} {transfer.pickupCity.country && `- ${transfer.pickupCity.country}`}
                </Typography>
              )}
            </Grid>
            <Grid item xs={12} md={6}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <LocationOnIcon color="action" />
                <Typography variant="body2" color="text.secondary">
                  Bırakış Adresi
                </Typography>
              </Box>
              <Typography variant="body1">{transfer.dropoffAddress}</Typography>
              {transfer.dropoffCity && (
                <Typography variant="body2" color="text.secondary">
                  {transfer.dropoffCity.cityName} {transfer.dropoffCity.country && `- ${transfer.dropoffCity.country}`}
                </Typography>
              )}
            </Grid>
            {transfer.note && (
              <Grid item xs={12}>
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  Not
                </Typography>
                <Typography variant="body1">{transfer.note}</Typography>
              </Grid>
            )}
          </Grid>
        </CardContent>
      </Card>

      <Grid container spacing={3}>
        {/* Misafir Bilgileri */}
        {transfer.guest && (
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
                  {transfer.guest.fullName}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Misafir Kodu
                </Typography>
                <Typography variant="body1" gutterBottom>
                  {transfer.guest.guestCode}
                </Typography>
                {transfer.guest.email && (
                  <>
                    <Typography variant="body2" color="text.secondary">
                      E-posta
                    </Typography>
                    <Typography variant="body1" gutterBottom>
                      {transfer.guest.email}
                    </Typography>
                  </>
                )}
                {transfer.guest.phoneNumber && (
                  <>
                    <Typography variant="body2" color="text.secondary">
                      Telefon
                    </Typography>
                    <Typography variant="body1" gutterBottom>
                      {transfer.guest.phoneNumber}
                    </Typography>
                  </>
                )}
                <Typography variant="body2" color="text.secondary">
                  Uyruk
                </Typography>
                <Typography variant="body1" gutterBottom>
                  {transfer.guest.nationality}
                </Typography>
                {transfer.guest.isSpecialGuest && (
                  <Chip label="Özel Misafir" color="primary" size="small" sx={{ mt: 1 }} />
                )}
                <Button
                  variant="outlined"
                  size="small"
                  sx={{ mt: 2 }}
                  onClick={() => navigate(`/guests/${transfer.guest!.id}`)}
                >
                  Misafir Detayı
                </Button>
              </CardContent>
            </Card>
          </Grid>
        )}

        {/* Personel Bilgileri */}
        {transfer.personnel && (
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
                  {transfer.personnel.fullName}
                </Typography>
                {transfer.personnel.email && (
                  <>
                    <Typography variant="body2" color="text.secondary">
                      E-posta
                    </Typography>
                    <Typography variant="body1" gutterBottom>
                      {transfer.personnel.email}
                    </Typography>
                  </>
                )}
                <Typography variant="body2" color="text.secondary">
                  Rol
                </Typography>
                <Typography variant="body1">{transfer.personnel.userType}</Typography>
              </CardContent>
            </Card>
          </Grid>
        )}

        {/* Araç Bilgileri */}
        {transfer.vehicle && (
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                  <DirectionsCarIcon color="primary" />
                  <Typography variant="h6">Araç</Typography>
                </Box>
                <Divider sx={{ mb: 2 }} />
                <Typography variant="body2" color="text.secondary">
                  Araç Tipi
                </Typography>
                <Typography variant="body1" fontWeight="medium" gutterBottom>
                  {transfer.vehicle.vehicleType}
                </Typography>
                {transfer.vehicle.licensePlate && (
                  <>
                    <Typography variant="body2" color="text.secondary">
                      Plaka
                    </Typography>
                    <Typography variant="body1" gutterBottom>
                      {transfer.vehicle.licensePlate}
                    </Typography>
                  </>
                )}
                <Typography variant="body2" color="text.secondary">
                  Kapasite
                </Typography>
                <Typography variant="body1">{transfer.vehicle.capacity} kişi</Typography>
              </CardContent>
            </Card>
          </Grid>
        )}

        {/* Havaalanı Bilgileri */}
        {transfer.airport && (
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                  <FlightTakeoffIcon color="primary" />
                  <Typography variant="h6">Havaalanı</Typography>
                </Box>
                <Divider sx={{ mb: 2 }} />
                <Typography variant="body2" color="text.secondary">
                  Havaalanı Adı
                </Typography>
                <Typography variant="body1" fontWeight="medium" gutterBottom>
                  {transfer.airport.airportName}
                </Typography>
                {transfer.airport.cityName && (
                  <>
                    <Typography variant="body2" color="text.secondary">
                      Şehir
                    </Typography>
                    <Typography variant="body1" gutterBottom>
                      {transfer.airport.cityName}
                    </Typography>
                  </>
                )}
                {transfer.airport.country && (
                  <>
                    <Typography variant="body2" color="text.secondary">
                      Ülke
                    </Typography>
                    <Typography variant="body1">{transfer.airport.country}</Typography>
                  </>
                )}
              </CardContent>
            </Card>
          </Grid>
        )}

        {/* İstatistikler */}
        {transfer.statistics && (
          <Grid item xs={12}>
            <Card>
              <CardContent>
                <Typography variant="h6" gutterBottom>
                  İstatistikler
                </Typography>
                <Divider sx={{ mb: 2 }} />
                <Grid container spacing={3}>
                  <Grid item xs={12} sm={6} md={3}>
                    <Typography variant="body2" color="text.secondary">
                      Toplam Transfer
                    </Typography>
                    <Typography variant="h5">{transfer.statistics.totalTransfers}</Typography>
                  </Grid>
                  <Grid item xs={12} sm={6} md={3}>
                    <Typography variant="body2" color="text.secondary">
                      Tamamlanan
                    </Typography>
                    <Typography variant="h5" color="success.main">
                      {transfer.statistics.completedTransfers}
                    </Typography>
                  </Grid>
                  <Grid item xs={12} sm={6} md={3}>
                    <Typography variant="body2" color="text.secondary">
                      Toplam Gelir
                    </Typography>
                    <Typography variant="h5" color="primary">
                      {formatCurrency(transfer.statistics.totalRevenue)}
                    </Typography>
                  </Grid>
                  <Grid item xs={12} sm={6} md={3}>
                    <Typography variant="body2" color="text.secondary">
                      Ortalama Fiyat
                    </Typography>
                    <Typography variant="h5">
                      {formatCurrency(transfer.statistics.averagePrice)}
                    </Typography>
                  </Grid>
                </Grid>
              </CardContent>
            </Card>
          </Grid>
        )}
      </Grid>
    </Box>
  )
}

export default TransferDetailPage


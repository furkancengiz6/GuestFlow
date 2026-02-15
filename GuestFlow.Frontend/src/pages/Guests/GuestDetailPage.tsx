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
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Divider,
  IconButton,
  Tooltip,
} from '@mui/material'
import {
  ArrowBack as ArrowBackIcon,
  Email as EmailIcon,
  Phone as PhoneIcon,
  Language as LanguageIcon,
  Star as StarIcon,
  Receipt as ReceiptIcon,
  DirectionsCar as DirectionsCarIcon,
  Tour as TourIcon,
  AttachMoney as AttachMoneyIcon,
  PictureAsPdf as PictureAsPdfIcon,
  Edit as EditIcon,
  Add as AddIcon,
  Hotel as HotelIcon,
} from '@mui/icons-material'
import { useQuery } from '@tanstack/react-query'
import { guestService } from '../../services/guestService'
import { privacyService } from '../../services/privacyService'
import { pmsService } from '../../services/pmsService'
import { formatDate, formatCurrency } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'
import UnifiedGuestProfile from '../../components/Guests/UnifiedGuestProfile'
import { Tabs, Tab } from '@mui/material'
import { useState } from 'react'

const GuestDetailPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const guestId = id ? parseInt(id, 10) : 0
  const [viewMode, setViewMode] = useState<'standard' | 'unified'>('unified')

  const { data: guest, isLoading, error } = useQuery({
    queryKey: ['guest-detail', guestId],
    queryFn: () => guestService.getGuestDetail(guestId),
    enabled: !!guestId && !isNaN(guestId),
  })

  // Check if guest is anonymized
  const { data: isAnonymized = false } = useQuery({
    queryKey: ['guest-anonymized', guestId],
    queryFn: () => privacyService.checkAnonymized(guestId),
    enabled: !!guestId && !isNaN(guestId),
  })

  // Get masked email (async)
  const { data: maskedEmail } = useQuery({
    queryKey: ['masked-email', guest?.email, isAnonymized || guest?.isAnonymized],
    queryFn: () => {
      if (!guest?.email) return ''
      if (isAnonymized || guest.isAnonymized) {
        return privacyService.maskEmail(guest.email)
      }
      return guest.email
    },
    enabled: !!guest?.email && (isAnonymized || guest?.isAnonymized || false),
    initialData: guest?.email || '',
  })

  // Get masked phone (async)
  const { data: maskedPhone } = useQuery({
    queryKey: ['masked-phone', guest?.phoneNumber, isAnonymized || guest?.isAnonymized],
    queryFn: () => {
      if (!guest?.phoneNumber) return ''
      if (isAnonymized || guest.isAnonymized) {
        return privacyService.maskPhone(guest.phoneNumber)
      }
      return guest.phoneNumber
    },
    enabled: !!guest?.phoneNumber && (isAnonymized || guest?.isAnonymized || false),
    initialData: guest?.phoneNumber || '',
  })

  // Helper function to get display email
  const getDisplayEmail = () => {
    if (!guest?.email) return ''
    if (isAnonymized || guest.isAnonymized) {
      return maskedEmail || guest.email
    }
    return guest.email
  }

  // Helper function to get display phone
  const getDisplayPhone = () => {
    if (!guest?.phoneNumber) return ''
    if (isAnonymized || guest.isAnonymized) {
      return maskedPhone || guest.phoneNumber
    }
    return guest.phoneNumber
  }

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={8} />
  }

  if (error || !guest) {
    return (
      <ContentState
        state="error"
        title="Misafir detayı yüklenemedi"
        description="Lütfen daha sonra tekrar deneyin."
        actionLabel="Geri dön"
        onAction={() => navigate('/guests')}
      />
    )
  }

  const getTimelineIcon = (type: string) => {
    switch (type) {
      case 'Transfer':
        return <DirectionsCarIcon />
      case 'CityTour':
      case 'YachtTour':
        return <TourIcon />
      case 'Invoice':
        return <ReceiptIcon />
      case 'Stay':
        return <HotelIcon />
      default:
        return <AttachMoneyIcon />
    }
  }

  const getTimelineColor = (type: string) => {
    switch (type) {
      case 'Transfer':
        return 'primary'
      case 'CityTour':
        return 'success'
      case 'YachtTour':
        return 'info'
      case 'Stay':
        return 'secondary'
      case 'Invoice':
        return 'warning'
      default:
        return 'grey'
    }
  }

  return (
    <Box p={3}>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 3 }}>
        <IconButton onClick={() => navigate('/guests')} color="primary">
          <ArrowBackIcon />
        </IconButton>
        <Typography variant="h4" sx={{ fontWeight: 600 }}>
          Misafir Detayı
        </Typography>
      </Box>

      {/* View Mode Toggle */}
      <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <Tabs
          value={viewMode}
          onChange={(_, newValue) => setViewMode(newValue)}
          sx={{ minHeight: 'auto' }}
        >
          <Tab label="Unified Profile (PMS + GuestFlow)" value="unified" />
          <Tab label="Standart Görünüm" value="standard" />
        </Tabs>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button
            variant="contained"
            color="primary"
            size="small"
            startIcon={<EditIcon />}
            onClick={() => navigate(`/guests/${guestId}/edit`)}
          >
            Düzenle
          </Button>
          <Button
            variant="outlined"
            color="secondary"
            size="small"
            startIcon={<AddIcon />}
            onClick={() => navigate('/transfers', { state: { preselectedGuestId: guestId } })}
          >
            Transfer
          </Button>
          <Button
            variant="outlined"
            color="info"
            size="small"
            startIcon={<TourIcon />}
            onClick={() => navigate('/tours', { state: { preselectedGuestId: guestId } })}
          >
            Tur
          </Button>
        </Box>
      </Box>

      {/* Unified Guest Profile View */}
      {viewMode === 'unified' && <UnifiedGuestProfile guestId={guestId} />}

      {/* Sync from PMS Button and View Mode Toggle */}
      {guest?.pmsIntegrationId && guest?.pmsGuestId && (
        <Box sx={{ mb: 2, display: 'flex', justifyContent: 'flex-end' }}>
          <Button
            variant="outlined"
            color="secondary"
            size="small"
            startIcon={<EditIcon />} // Using EditIcon for now, could act as a 'Refresh' icon visually if changed
            onClick={async () => {
              try {
                if (guest.pmsIntegrationId && guest.pmsGuestId) {
                  await pmsService.syncGuest(guest.pmsIntegrationId, guest.pmsGuestId);
                  // Refresh the page or invalidate query
                  window.location.reload();
                }
              } catch (error) {
                console.error('Failed to sync guest', error);
                alert('Senkronizasyon başarısız oldu.');
              }
            }}
          >
            PMS'den Senkronize Et
          </Button>
        </Box>
      )}

      {/* Standard View */}
      {viewMode === 'standard' && (
        <>

          {/* Misafir Bilgileri */}
          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start' }}>
                <Box>
                  <Typography variant="h5" gutterBottom>
                    {guest.fullName}
                  </Typography>
                  <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap', mt: 2 }}>
                    <Chip label={`Misafir Kodu: ${guest.guestCode}`} variant="outlined" />
                    {guest.isSpecialGuest && (
                      <Chip
                        icon={<StarIcon />}
                        label="Özel Misafir"
                        color="primary"
                        variant="filled"
                      />
                    )}
                    {(isAnonymized || guest.isAnonymized) && (
                      <Chip
                        label="Anonymize Edilmiş"
                        color="warning"
                        variant="filled"
                      />
                    )}
                    <Chip
                      icon={<LanguageIcon />}
                      label={guest.nationality}
                      variant="outlined"
                    />
                  </Box>
                </Box>
                <Box sx={{ textAlign: 'right' }}>
                  <Typography variant="body2" color="text.secondary">
                    Kayıt Tarihi
                  </Typography>
                  <Typography variant="body1">{formatDate(guest.createdDate)}</Typography>
                </Box>
              </Box>
              <Divider sx={{ my: 2 }} />
              <Grid container spacing={2}>
                {guest.email && (
                  <Grid item xs={12} sm={6}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <EmailIcon color="action" />
                      <Typography variant="body2" color="text.secondary">
                        E-posta:
                      </Typography>
                      <Typography variant="body1">{getDisplayEmail()}</Typography>
                    </Box>
                  </Grid>
                )}
                {guest.phoneNumber && (
                  <Grid item xs={12} sm={6}>
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <PhoneIcon color="action" />
                      <Typography variant="body2" color="text.secondary">
                        Telefon:
                      </Typography>
                      <Typography variant="body1">{getDisplayPhone()}</Typography>
                    </Box>
                  </Grid>
                )}
              </Grid>
            </CardContent>
          </Card>

          {/* İstatistikler */}
          <Grid container spacing={3} sx={{ mb: 3 }}>
            <Grid item xs={12} sm={6} md={3}>
              <Card>
                <CardContent>
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    Toplam Transfer
                  </Typography>
                  <Typography variant="h4">{guest.statistics.totalTransfers}</Typography>
                </CardContent>
              </Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card>
                <CardContent>
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    Toplam Tur
                  </Typography>
                  <Typography variant="h4">
                    {guest.statistics.totalCityTours + guest.statistics.totalYachtTours}
                  </Typography>
                </CardContent>
              </Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card>
                <CardContent>
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    Toplam Fatura
                  </Typography>
                  <Typography variant="h4">{guest.statistics.totalInvoices}</Typography>
                </CardContent>
              </Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card>
                <CardContent>
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    Toplam Harcama
                  </Typography>
                  <Typography variant="h4">
                    {formatCurrency(guest.statistics.totalSpent)}
                  </Typography>
                </CardContent>
              </Card>
            </Grid>
          </Grid>

          <Grid container spacing={3}>
            {/* Transferler */}
            <Grid item xs={12} md={6}>
              <Paper sx={{ p: 2 }}>
                <Typography variant="h6" gutterBottom>
                  Transferler ({guest.transfers.length})
                </Typography>
                {guest.transfers.length === 0 ? (
                  <Typography variant="body2" color="text.secondary" sx={{ py: 2 }}>
                    Transfer bulunamadı
                  </Typography>
                ) : (
                  <TableContainer>
                    <Table size="small">
                      <TableHead>
                        <TableRow>
                          <TableCell>Tarih</TableCell>
                          <TableCell>Güzergah</TableCell>
                          <TableCell>Fiyat</TableCell>
                          <TableCell>Durum</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {guest.transfers.map((transfer) => (
                          <TableRow key={transfer.id}>
                            <TableCell>{formatDate(transfer.transferDate)}</TableCell>
                            <TableCell>
                              {transfer.pickupAddress} → {transfer.dropoffAddress}
                            </TableCell>
                            <TableCell>{formatCurrency(transfer.finalPrice)}</TableCell>
                            <TableCell>
                              <Chip label={transfer.status} size="small" />
                            </TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </TableContainer>
                )}
              </Paper>
            </Grid>

            {/* Turlar */}
            <Grid item xs={12} md={6}>
              <Paper sx={{ p: 2 }}>
                <Typography variant="h6" gutterBottom>
                  Turlar ({guest.cityTours.length + guest.yachtTours.length})
                </Typography>
                {guest.cityTours.length === 0 && guest.yachtTours.length === 0 ? (
                  <Typography variant="body2" color="text.secondary" sx={{ py: 2 }}>
                    Tur bulunamadı
                  </Typography>
                ) : (
                  <TableContainer>
                    <Table size="small">
                      <TableHead>
                        <TableRow>
                          <TableCell>Tarih</TableCell>
                          <TableCell>Tur Tipi</TableCell>
                          <TableCell>Kişi</TableCell>
                          <TableCell>Fiyat</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {guest.cityTours.map((tour) => (
                          <TableRow key={`city-${tour.id}`}>
                            <TableCell>{formatDate(tour.tourDate)}</TableCell>
                            <TableCell>Şehir Turu {tour.cityName && `- ${tour.cityName}`}</TableCell>
                            <TableCell>{tour.numberOfPeople}</TableCell>
                            <TableCell>{formatCurrency(tour.finalPrice)}</TableCell>
                          </TableRow>
                        ))}
                        {guest.yachtTours.map((tour) => (
                          <TableRow key={`yacht-${tour.id}`}>
                            <TableCell>{formatDate(tour.tourDate)}</TableCell>
                            <TableCell>
                              Yat Turu {tour.yachtName && `- ${tour.yachtName}`}
                            </TableCell>
                            <TableCell>{tour.numberOfPeople}</TableCell>
                            <TableCell>{formatCurrency(tour.finalPrice)}</TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </TableContainer>
                )}
              </Paper>
            </Grid>

            {/* Faturalar */}
            <Grid item xs={12} md={6}>
              <Paper sx={{ p: 2 }}>
                <Typography variant="h6" gutterBottom>
                  Faturalar ({guest.invoices.length})
                </Typography>
                {guest.invoices.length === 0 ? (
                  <Typography variant="body2" color="text.secondary" sx={{ py: 2 }}>
                    Fatura bulunamadı
                  </Typography>
                ) : (
                  <TableContainer>
                    <Table size="small">
                      <TableHead>
                        <TableRow>
                          <TableCell>Fatura No</TableCell>
                          <TableCell>Tarih</TableCell>
                          <TableCell>Tutar</TableCell>
                          <TableCell>İşlem</TableCell>
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {guest.invoices.map((invoice) => (
                          <TableRow key={invoice.id}>
                            <TableCell>#{invoice.invoiceNumber}</TableCell>
                            <TableCell>{formatDate(invoice.issueDate)}</TableCell>
                            <TableCell>
                              {formatCurrency(invoice.totalAmount)} {invoice.currency}
                            </TableCell>
                            <TableCell>
                              {invoice.hasPdf && (
                                <Tooltip title="PDF İndir">
                                  <IconButton
                                    size="small"
                                    onClick={() => window.open(invoice.pdfUrl, '_blank')}
                                  >
                                    <PictureAsPdfIcon fontSize="small" />
                                  </IconButton>
                                </Tooltip>
                              )}
                            </TableCell>
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </TableContainer>
                )}
              </Paper>
            </Grid>

            {/* Zaman Çizelgesi */}
            <Grid item xs={12} md={6}>
              <Paper sx={{ p: 2 }}>
                <Typography variant="h6" gutterBottom>
                  Zaman Çizelgesi ({guest.timeline.length})
                </Typography>
                {guest.timeline.length === 0 ? (
                  <Typography variant="body2" color="text.secondary" sx={{ py: 2 }}>
                    Aktivite bulunamadı
                  </Typography>
                ) : (
                  <Box sx={{ maxHeight: 400, overflowY: 'auto' }}>
                    {guest.timeline.map((item, index) => (
                      <Box key={item.id} sx={{ mb: 2 }}>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
                          <Chip
                            icon={getTimelineIcon(item.type)}
                            label={item.type}
                            size="small"
                            color={getTimelineColor(item.type) as any}
                          />
                          <Typography variant="caption" color="text.secondary">
                            {formatDate(item.date)}
                          </Typography>
                        </Box>
                        <Typography variant="body2" fontWeight="medium">
                          {item.title}
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          {item.description}
                        </Typography>
                        {item.amount && (
                          <Typography variant="body2" color="primary" sx={{ mt: 0.5 }}>
                            {formatCurrency(item.amount)}
                          </Typography>
                        )}
                        {index < guest.timeline.length - 1 && <Divider sx={{ mt: 2 }} />}
                      </Box>
                    ))}
                  </Box>
                )}
              </Paper>
            </Grid>
          </Grid>
        </>
      )}
    </Box>
  )
}

export default GuestDetailPage


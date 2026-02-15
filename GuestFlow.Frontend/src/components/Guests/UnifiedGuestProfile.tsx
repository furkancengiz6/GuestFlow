import {
  Box,
  Typography,
  Grid,
  Card,
  CardContent,
  Chip,
  Divider,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tabs,
  Tab,
  Alert,
  AlertTitle,
  Tooltip,
  IconButton,
} from '@mui/material'
import {
  Person as PersonIcon,
  Room as RoomIcon,
  Email as EmailIcon,
  Phone as PhoneIcon,
  Star as VIPIcon,
  Sync as SyncIcon,
  CalendarToday as CalendarIcon,
  Receipt as ReceiptIcon,
  PictureAsPdf as PictureAsPdfIcon,
} from '@mui/icons-material'
import { useState } from 'react'
import { useUnifiedGuestProfile, useGuestHistoryDashboard } from '../../hooks/useConciergeDashboard'
import { formatDate, formatCurrency } from '../../utils/formatters'
import ContentState from '../Feedback/ContentState'
import GuestPreferences from './GuestPreferences'
import CommunicationHistory from './CommunicationHistory'
import {
  BarChart as BarChartIcon,
  PieChart as PieChartIcon,
  TrendingUp as TrendingUpIcon,
  Hotel as HotelIcon,
  Message as MessageIcon,
} from '@mui/icons-material'

interface UnifiedGuestProfileProps {
  guestId: number
}

const UnifiedGuestProfile = ({ guestId }: UnifiedGuestProfileProps) => {
  const { data: profile, isLoading, error } = useUnifiedGuestProfile(guestId)
  const { data: historyDashboard, isLoading: isLoadingHistory } = useGuestHistoryDashboard(guestId)
  const [activeTab, setActiveTab] = useState(0)

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={5} />
  }

  if (error || !profile) {
    return (
      <ContentState
        state="error"
        title="Misafir profili yüklenemedi"
        description="Lütfen daha sonra tekrar deneyin."
      />
    )
  }

  return (
    <Box>
      {/* Header Section */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
              <PersonIcon color="primary" sx={{ fontSize: 40 }} />
              <Box>
                <Typography variant="h5" gutterBottom>
                  {profile.guestName}
                  {profile.isVIP && (
                    <VIPIcon color="warning" sx={{ ml: 1, verticalAlign: 'middle' }} />
                  )}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Misafir Kodu: {profile.guestCode}
                </Typography>
              </Box>
            </Box>
            <Box>
              {profile.isVIP && (
                <Chip
                  label="VIP"
                  color="warning"
                  icon={<VIPIcon />}
                  sx={{ mr: 1 }}
                />
              )}
              {profile.pmsData && profile.pmsData.length > 0 && (
                <Chip
                  label={`${profile.pmsData.length} PMS Entegrasyonu`}
                  color="primary"
                  icon={<SyncIcon />}
                />
              )}
            </Box>
          </Box>

          <Divider sx={{ my: 2 }} />

          {/* Quick Info Grid */}
          <Grid container spacing={2}>
            <Grid item xs={12} md={6}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <RoomIcon fontSize="small" color="action" />
                <Typography variant="body2" color="text.secondary">
                  Oda:
                </Typography>
                <Typography variant="body2" fontWeight="medium">
                  {profile.roomNumber || '-'}
                </Typography>
                {profile.roomType && (
                  <Chip label={profile.roomType} size="small" variant="outlined" />
                )}
              </Box>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <CalendarIcon fontSize="small" color="action" />
                <Typography variant="body2" color="text.secondary">
                  Check-in:
                </Typography>
                <Typography variant="body2" fontWeight="medium">
                  {profile.checkInDate ? formatDate(profile.checkInDate) : '-'}
                </Typography>
              </Box>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <CalendarIcon fontSize="small" color="action" />
                <Typography variant="body2" color="text.secondary">
                  Check-out:
                </Typography>
                <Typography variant="body2" fontWeight="medium">
                  {profile.checkOutDate ? formatDate(profile.checkOutDate) : '-'}
                </Typography>
              </Box>
            </Grid>
            <Grid item xs={12} md={6}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <EmailIcon fontSize="small" color="action" />
                <Typography variant="body2" color="text.secondary">
                  E-posta:
                </Typography>
                <Typography variant="body2" fontWeight="medium">
                  {profile.email || '-'}
                </Typography>
              </Box>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                <PhoneIcon fontSize="small" color="action" />
                <Typography variant="body2" color="text.secondary">
                  Telefon:
                </Typography>
                <Typography variant="body2" fontWeight="medium">
                  {profile.phoneNumber || '-'}
                </Typography>
              </Box>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Tabs Section */}
      <Card>
        <Tabs
          value={activeTab}
          onChange={(_, newValue) => setActiveTab(newValue)}
          sx={{ borderBottom: 1, borderColor: 'divider' }}
        >
          <Tab label="Genel Bilgiler" />
          <Tab label="PMS Verileri" />
          <Tab label="Hizmet Geçmişi" />
          <Tab label="Geçmiş & Analiz" icon={<BarChartIcon />} iconPosition="start" />
          <Tab label="Tercihler" />
          <Tab label="İletişim" icon={<MessageIcon />} iconPosition="start" />
          <Tab label="Finansal" icon={<ReceiptIcon />} iconPosition="start" />
        </Tabs>

        {/* Tab Panel 0: Genel Bilgiler */}
        {activeTab === 0 && (
          <CardContent>
            <Grid container spacing={3}>
              {/* GuestFlow Data */}
              {profile.guestFlowData && (
                <Grid item xs={12} md={6}>
                  <Typography variant="h6" gutterBottom>
                    GuestFlow Verileri
                  </Typography>
                  <Box sx={{ pl: 2 }}>
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                      Oda: {profile.guestFlowData.roomNumber || '-'}
                    </Typography>
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                      Check-in: {profile.guestFlowData.checkInDate ? formatDate(profile.guestFlowData.checkInDate) : '-'}
                    </Typography>
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                      Check-out: {profile.guestFlowData.checkOutDate ? formatDate(profile.guestFlowData.checkOutDate) : '-'}
                    </Typography>
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                      E-posta: {profile.guestFlowData.email || '-'}
                    </Typography>
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                      Telefon: {profile.guestFlowData.phoneNumber || '-'}
                    </Typography>
                    {profile.guestFlowData.isVIP && (
                      <Chip label="VIP" color="warning" size="small" sx={{ mt: 1 }} />
                    )}
                  </Box>
                </Grid>
              )}

              {/* Summary */}
              <Grid item xs={12} md={6}>
                <Typography variant="h6" gutterBottom>
                  Özet
                </Typography>
                <Box sx={{ pl: 2 }}>
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    Toplam Hizmet: {profile.guestFlowData?.serviceHistory?.length || 0}
                  </Typography>
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    PMS Entegrasyonları: {profile.pmsData?.length || 0}
                  </Typography>
                  {profile.guestFlowData?.serviceHistory && profile.guestFlowData.serviceHistory.length > 0 && (
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                      Toplam Harcama: {formatCurrency(
                        profile.guestFlowData.serviceHistory.reduce((sum, s) => sum + (s.amount || 0), 0),
                        'TRY'
                      )}
                    </Typography>
                  )}
                </Box>
              </Grid>
            </Grid>
          </CardContent>
        )}

        {/* Tab Panel 1: PMS Verileri */}
        {activeTab === 1 && (
          <CardContent>
            {profile.pmsData && profile.pmsData.length > 0 ? (
              <Grid container spacing={3}>
                {profile.pmsData.map((pms, index) => (
                  <Grid item xs={12} md={6} key={index}>
                    <Card variant="outlined">
                      <CardContent>
                        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 2 }}>
                          <Typography variant="h6">
                            {pms.providerName}
                          </Typography>
                          <Chip
                            label="PMS"
                            color="primary"
                            size="small"
                            icon={<SyncIcon />}
                          />
                        </Box>
                        <Divider sx={{ mb: 2 }} />
                        <Box>
                          <Typography variant="body2" color="text.secondary" gutterBottom>
                            <strong>PMS Guest ID:</strong> {pms.pmsGuestId || '-'}
                          </Typography>
                          <Typography variant="body2" color="text.secondary" gutterBottom>
                            <strong>Rezervasyon ID:</strong> {pms.pmsReservationId || '-'}
                          </Typography>
                          <Typography variant="body2" color="text.secondary" gutterBottom>
                            <strong>Oda:</strong> {pms.roomNumber || '-'} {pms.roomType && `(${pms.roomType})`}
                          </Typography>
                          <Typography variant="body2" color="text.secondary" gutterBottom>
                            <strong>Check-in:</strong> {pms.checkInDate ? formatDate(pms.checkInDate) : '-'}
                          </Typography>
                          <Typography variant="body2" color="text.secondary" gutterBottom>
                            <strong>Check-out:</strong> {pms.checkOutDate ? formatDate(pms.checkOutDate) : '-'}
                          </Typography>
                          <Typography variant="body2" color="text.secondary" gutterBottom>
                            <strong>E-posta:</strong> {pms.email || '-'}
                          </Typography>
                          <Typography variant="body2" color="text.secondary" gutterBottom>
                            <strong>Telefon:</strong> {pms.phoneNumber || '-'}
                          </Typography>
                          {pms.isVIP && (
                            <Chip label="VIP" color="warning" size="small" sx={{ mt: 1 }} />
                          )}
                          {pms.lastSyncedAt && (
                            <Typography variant="caption" color="text.secondary" display="block" sx={{ mt: 1 }}>
                              Son senkronizasyon: {formatDate(pms.lastSyncedAt)}
                            </Typography>
                          )}
                        </Box>
                      </CardContent>
                    </Card>
                  </Grid>
                ))}
              </Grid>
            ) : (
              <Alert severity="info">
                <AlertTitle>PMS Verisi Yok</AlertTitle>
                Bu misafir için henüz PMS entegrasyonu bulunmamaktadır.
              </Alert>
            )}
          </CardContent>
        )}

        {/* Tab Panel 2: Hizmet Geçmişi */}
        {activeTab === 2 && (
          <CardContent>
            {profile.guestFlowData?.serviceHistory && profile.guestFlowData.serviceHistory.length > 0 ? (
              <TableContainer>
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>Tarih</TableCell>
                      <TableCell>Servis Tipi</TableCell>
                      <TableCell>Açıklama</TableCell>
                      <TableCell>Tutar</TableCell>
                      <TableCell>Durum</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {profile.guestFlowData.serviceHistory.map((service, index) => (
                      <TableRow key={index} hover>
                        <TableCell>
                          {formatDate(service.serviceDate)}
                        </TableCell>
                        <TableCell>
                          <Chip
                            label={service.serviceType}
                            size="small"
                            color={
                              service.serviceType === 'Transfer'
                                ? 'primary'
                                : service.serviceType === 'CityTour'
                                  ? 'secondary'
                                  : service.serviceType === 'YachtTour'
                                    ? 'success'
                                    : 'default'
                            }
                          />
                        </TableCell>
                        <TableCell>
                          {service.description || '-'}
                        </TableCell>
                        <TableCell>
                          {service.amount ? formatCurrency(service.amount, 'TRY') : '-'}
                        </TableCell>
                        <TableCell>
                          {service.status ? (
                            <Chip
                              label={service.status}
                              size="small"
                              color={
                                service.status === 'Completed' || service.status === 'Confirmed'
                                  ? 'success'
                                  : service.status === 'Pending'
                                    ? 'warning'
                                    : 'default'
                              }
                            />
                          ) : (
                            '-'
                          )}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            ) : (
              <Alert severity="info">
                <AlertTitle>Hizmet Geçmişi Yok</AlertTitle>
                Bu misafir için henüz hizmet kaydı bulunmamaktadır.
              </Alert>
            )}
          </CardContent>
        )}

        {/* Tab Panel 3: Guest History Dashboard */}
        {activeTab === 3 && (
          <CardContent>
            {isLoadingHistory ? (
              <ContentState state="loading" skeletonLines={5} />
            ) : historyDashboard ? (
              <Grid container spacing={3}>
                {/* Önceki Konaklamalar */}
                <Grid item xs={12} md={6}>
                  <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <HotelIcon color="primary" />
                    Önceki Konaklamalar ({historyDashboard.previousStays.length})
                  </Typography>
                  {historyDashboard.previousStays.length > 0 ? (
                    <TableContainer sx={{ mt: 2 }}>
                      <Table size="small">
                        <TableHead>
                          <TableRow>
                            <TableCell>Tarih</TableCell>
                            <TableCell>Oda</TableCell>
                            <TableCell>Gece</TableCell>
                            <TableCell>Tutar</TableCell>
                          </TableRow>
                        </TableHead>
                        <TableBody>
                          {historyDashboard.previousStays.map((stay, index) => (
                            <TableRow key={index} hover>
                              <TableCell>
                                <Typography variant="body2">
                                  {formatDate(stay.checkInDate)} - {formatDate(stay.checkOutDate)}
                                </Typography>
                              </TableCell>
                              <TableCell>
                                {stay.roomNumber || '-'}
                                {stay.roomType && (
                                  <Typography variant="caption" color="text.secondary" display="block">
                                    {stay.roomType}
                                  </Typography>
                                )}
                              </TableCell>
                              <TableCell>{stay.numberOfNights} gece</TableCell>
                              <TableCell>
                                {stay.totalAmount
                                  ? formatCurrency(stay.totalAmount, stay.currency || 'TRY')
                                  : '-'}
                              </TableCell>
                            </TableRow>
                          ))}
                        </TableBody>
                      </Table>
                    </TableContainer>
                  ) : (
                    <Alert severity="info" sx={{ mt: 2 }}>
                      Önceki konaklama kaydı bulunmamaktadır.
                    </Alert>
                  )}
                </Grid>

                {/* Harcama Analizi */}
                <Grid item xs={12} md={6}>
                  <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <TrendingUpIcon color="primary" />
                    Harcama Analizi
                  </Typography>
                  <Card variant="outlined" sx={{ mt: 2 }}>
                    <CardContent>
                      <Box sx={{ mb: 2 }}>
                        <Typography variant="h4" color="primary">
                          {formatCurrency(historyDashboard.spendingAnalysis.totalSpending, historyDashboard.spendingAnalysis.currency)}
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          Toplam Harcama
                        </Typography>
                      </Box>
                      <Divider sx={{ my: 2 }} />
                      <Grid container spacing={2}>
                        <Grid item xs={6}>
                          <Typography variant="body2" color="text.secondary">
                            Konaklama
                          </Typography>
                          <Typography variant="body1" fontWeight="medium">
                            {historyDashboard.spendingAnalysis.pmsSpending
                              ? formatCurrency(historyDashboard.spendingAnalysis.pmsSpending, historyDashboard.spendingAnalysis.currency)
                              : '-'}
                          </Typography>
                        </Grid>
                        <Grid item xs={6}>
                          <Typography variant="body2" color="text.secondary">
                            Hizmetler
                          </Typography>
                          <Typography variant="body1" fontWeight="medium">
                            {formatCurrency(historyDashboard.spendingAnalysis.guestFlowSpending, historyDashboard.spendingAnalysis.currency)}
                          </Typography>
                        </Grid>
                        <Grid item xs={6}>
                          <Typography variant="body2" color="text.secondary">
                            Toplam Konaklama
                          </Typography>
                          <Typography variant="body1" fontWeight="medium">
                            {historyDashboard.spendingAnalysis.totalStays}
                          </Typography>
                        </Grid>
                        <Grid item xs={6}>
                          <Typography variant="body2" color="text.secondary">
                            Toplam Hizmet
                          </Typography>
                          <Typography variant="body1" fontWeight="medium">
                            {historyDashboard.spendingAnalysis.totalServices}
                          </Typography>
                        </Grid>
                      </Grid>
                      {historyDashboard.spendingAnalysis.spendingByCategory.length > 0 && (
                        <>
                          <Divider sx={{ my: 2 }} />
                          <Typography variant="subtitle2" gutterBottom>
                            Kategori Bazlı Harcama
                          </Typography>
                          {historyDashboard.spendingAnalysis.spendingByCategory.map((category, index) => (
                            <Box key={index} sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                              <Typography variant="body2">{category.category}</Typography>
                              <Typography variant="body2" fontWeight="medium">
                                {formatCurrency(category.amount, historyDashboard.spendingAnalysis.currency)} ({category.count})
                              </Typography>
                            </Box>
                          ))}
                        </>
                      )}
                    </CardContent>
                  </Card>
                </Grid>

                {/* Tercih Analizi */}
                <Grid item xs={12}>
                  <Typography variant="h6" gutterBottom sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <PieChartIcon color="primary" />
                    Tercih Analizi
                  </Typography>
                  <Grid container spacing={2} sx={{ mt: 1 }}>
                    {/* Oda Tercihleri */}
                    <Grid item xs={12} md={6}>
                      <Card variant="outlined">
                        <CardContent>
                          <Typography variant="subtitle1" gutterBottom>
                            Oda Tercihleri
                          </Typography>
                          {historyDashboard.preferenceAnalysis.roomPreferences.length > 0 ? (
                            <Box>
                              {historyDashboard.preferenceAnalysis.roomPreferences.map((pref, index) => (
                                <Box key={index} sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                                  <Typography variant="body2">{pref.roomType}</Typography>
                                  <Chip label={`${pref.stayCount} konaklama`} size="small" />
                                </Box>
                              ))}
                            </Box>
                          ) : (
                            <Typography variant="body2" color="text.secondary">
                              Oda tercihi kaydı bulunmamaktadır.
                            </Typography>
                          )}
                        </CardContent>
                      </Card>
                    </Grid>

                    {/* Servis Tercihleri */}
                    <Grid item xs={12} md={6}>
                      <Card variant="outlined">
                        <CardContent>
                          <Typography variant="subtitle1" gutterBottom>
                            Servis Tercihleri
                          </Typography>
                          {historyDashboard.preferenceAnalysis.servicePreferences.length > 0 ? (
                            <Box>
                              {historyDashboard.preferenceAnalysis.servicePreferences.map((pref, index) => (
                                <Box key={index} sx={{ display: 'flex', justifyContent: 'space-between', mb: 1 }}>
                                  <Typography variant="body2">{pref.serviceType}</Typography>
                                  <Box sx={{ display: 'flex', gap: 1, alignItems: 'center' }}>
                                    <Chip label={`${pref.usageCount} kullanım`} size="small" />
                                    {pref.totalSpending && (
                                      <Typography variant="caption" color="text.secondary">
                                        {formatCurrency(pref.totalSpending, 'TRY')}
                                      </Typography>
                                    )}
                                  </Box>
                                </Box>
                              ))}
                            </Box>
                          ) : (
                            <Typography variant="body2" color="text.secondary">
                              Servis tercihi kaydı bulunmamaktadır.
                            </Typography>
                          )}
                        </CardContent>
                      </Card>
                    </Grid>
                  </Grid>
                </Grid>
              </Grid>
            ) : (
              <Alert severity="error">
                Geçmiş verileri yüklenemedi.
              </Alert>
            )}
          </CardContent>
        )}

        {/* Tab Panel 4: Guest Preferences */}
        {activeTab === 4 && (
          <CardContent>
            <GuestPreferences guestId={guestId} readOnly={false} />
          </CardContent>
        )}

        {/* Tab Panel 5: İletişim */}
        {activeTab === 5 && (
          <CardContent>
            <CommunicationHistory guestId={guestId} />
          </CardContent>
        )}

        {/* Tab Panel 6: Finansal */}
        {activeTab === 6 && (
          <CardContent>
            {profile.guestFlowData?.invoiceHistory && profile.guestFlowData.invoiceHistory.length > 0 ? (
              <TableContainer>
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>Fatura No</TableCell>
                      <TableCell>Tarih</TableCell>
                      <TableCell>Tutar</TableCell>
                      <TableCell>Durum</TableCell>
                      <TableCell>İşlem</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {profile.guestFlowData.invoiceHistory.map((invoice, index) => (
                      <TableRow key={index} hover>
                        <TableCell>
                          #{invoice.invoiceNumber}
                        </TableCell>
                        <TableCell>
                          {formatDate(invoice.issueDate)}
                        </TableCell>
                        <TableCell>
                          {formatCurrency(invoice.totalAmount, invoice.currency)}
                        </TableCell>
                        <TableCell>
                          <Chip
                            label={invoice.status}
                            size="small"
                            color={invoice.status === 'Paid' ? 'success' : 'warning'}
                          />
                        </TableCell>
                        <TableCell>
                          {invoice.pdfUrl && (
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
            ) : (
              <Alert severity="info">
                <AlertTitle>Fatura Bulunamadı</AlertTitle>
                Bu misafir için henüz fatura kaydı bulunmamaktadır.
              </Alert>
            )}
          </CardContent>
        )}
      </Card>
    </Box>
  )
}

export default UnifiedGuestProfile

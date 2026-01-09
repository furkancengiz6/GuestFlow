import { useState } from 'react'
import {
  Box,
  Typography,
  Grid,
  Card,
  CardContent,
  Alert,
  AlertTitle,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  Skeleton,
  ToggleButton,
  ToggleButtonGroup,
  Button,
  IconButton,
} from '@mui/material'
import {
  People as PeopleIcon,
  DirectionsCar as TransferIcon,
  Receipt as InvoiceIcon,
  AttachMoney as MoneyIcon,
  Visibility as VisibilityIcon,
  Edit as EditIcon,
} from '@mui/icons-material'
import {
  LineChart,
  Line,
  BarChart,
  Bar,
  PieChart,
  Pie,
  Cell,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts'
import {
  useQuickStats,
  useRecentActivities,
  useRevenueChartData,
  useUpcomingBookings,
  useUnpaidServices,
  useUpcomingServices,
} from '../../hooks/useDashboard'
import { formatCurrency, formatDate } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'
import { useQueryClient } from '@tanstack/react-query'
import { useLiveUpdates } from '../../hooks/useLiveUpdates'

const DashboardPage = () => {
  const queryClient = useQueryClient()
  const [dashboardMode, setDashboardMode] = useState<'admin' | 'operations'>('operations')
  const [revenuePeriod, setRevenuePeriod] = useState<'daily' | 'weekly' | 'monthly'>('daily')
  const [revenueDays, setRevenueDays] = useState<number>(30)

  // Enable live updates for dashboard
  useLiveUpdates(['guest', 'transfer', 'citytour', 'yachttour', 'invoice'])

  const {
    data: quickStats,
    isLoading: isLoadingStats,
    error: statsError,
  } = useQuickStats()
  const { data: recentActivities, isLoading: isLoadingActivities } =
    useRecentActivities(5)
  const { data: revenueChart, isLoading: isLoadingChart } =
    useRevenueChartData(revenuePeriod, revenueDays)
  const { data: upcomingBookings, isLoading: isLoadingBookings } =
    useUpcomingBookings()
  const { data: unpaidServices, isLoading: isLoadingUnpaid } =
    useUnpaidServices()
  const { data: upcomingServices, isLoading: isLoadingUpcoming } =
    useUpcomingServices()

  if (statsError) {
    return (
      <ContentState
        state="error"
        title="Dashboard verileri yüklenemedi"
        description="Lütfen daha sonra tekrar deneyin veya sayfayı yenileyin."
        actionLabel="Tekrar dene"
        onAction={() => {
          queryClient.refetchQueries({ queryKey: ['dashboard'] })
        }}
      />
    )
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" component="h1">
          Dashboard
        </Typography>
        <ToggleButtonGroup
          value={dashboardMode}
          exclusive
          onChange={(_, value) => value && setDashboardMode(value)}
          aria-label="dashboard mode"
        >
          <ToggleButton value="operations" aria-label="operations dashboard">
            Operasyon
          </ToggleButton>
          <ToggleButton value="admin" aria-label="admin dashboard">
            Yönetim
          </ToggleButton>
        </ToggleButtonGroup>
      </Box>

      {dashboardMode === 'operations' ? (
        // OPERATIONS DASHBOARD - CONCIERGE MODE
        <OperationsDashboard
          quickStats={quickStats}
          isLoadingStats={isLoadingStats}
          unpaidServices={unpaidServices}
          isLoadingUnpaid={isLoadingUnpaid}
          upcomingServices={upcomingServices}
          isLoadingUpcoming={isLoadingUpcoming}
          onRetry={() => queryClient.refetchQueries({ queryKey: ['dashboard'] })}
        />
      ) : (
        // ADMIN DASHBOARD - MANAGEMENT MODE
        <>
          {/* Alert Section */}
      {!isLoadingUnpaid && unpaidServices && unpaidServices.items && unpaidServices.items.length > 0 && (
        <Alert severity="error" sx={{ mb: 3 }}>
          <AlertTitle>Ödemesi Alınmamış Hizmetler ({unpaidServices.items.length})</AlertTitle>
          {unpaidServices.items.slice(0, 3).map((item, index) => (
            <Typography key={item.serviceId || index} variant="body2">
              {item.serviceType}: {item.guestName} - {formatCurrency(item.remainingAmount)} ({item.daysOverdue} gün gecikme)
            </Typography>
          ))}
          {unpaidServices.items.length > 3 && (
            <Typography variant="body2" sx={{ mt: 1 }}>
              ...ve {unpaidServices.items.length - 3} hizmet daha
            </Typography>
          )}
        </Alert>
      )}

      {/* İstatistik Kartları */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              {isLoadingStats ? (
                <Skeleton variant="rectangular" height={80} />
              ) : (
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                  <PeopleIcon
                    sx={{ fontSize: 40, color: 'primary.main', mr: 2 }}
                  />
                  <Box>
                    <Typography variant="h4">
                      {quickStats?.totalGuests || 0}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Toplam Misafir
                    </Typography>
                  </Box>
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              {isLoadingStats ? (
                <Skeleton variant="rectangular" height={80} />
              ) : (
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                  <TransferIcon
                    sx={{ fontSize: 40, color: 'secondary.main', mr: 2 }}
                  />
                  <Box>
                    <Typography variant="h4">
                      {quickStats?.totalTransfers || 0}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Toplam Transfer
                    </Typography>
                  </Box>
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              {isLoadingStats ? (
                <Skeleton variant="rectangular" height={80} />
              ) : (
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                  <InvoiceIcon
                    sx={{ fontSize: 40, color: 'success.main', mr: 2 }}
                  />
                  <Box>
                    <Typography variant="h4">
                      {quickStats?.totalInvoices || 0}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Toplam Fatura
                    </Typography>
                  </Box>
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>

        <Grid item xs={12} sm={6} md={3}>
          <Card>
            <CardContent>
              {isLoadingStats ? (
                <Skeleton variant="rectangular" height={80} />
              ) : (
                <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                  <MoneyIcon
                    sx={{ fontSize: 40, color: 'warning.main', mr: 2 }}
                  />
                  <Box>
                    <Typography variant="h4">
                      {quickStats?.totalRevenue
                        ? formatCurrency(quickStats.totalRevenue)
                        : '₺0'}
                    </Typography>
                    <Typography variant="body2" color="text.secondary">
                      Toplam Gelir
                    </Typography>
                  </Box>
                </Box>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Gelir Grafiği ve Son Aktiviteler */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12} md={8}>
          <Card>
            <CardContent>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                <Typography variant="h6">
                  Gelir Trendi
                </Typography>
                <ToggleButtonGroup
                  value={revenuePeriod}
                  exclusive
                  onChange={(_, newPeriod) => {
                    if (newPeriod) {
                      setRevenuePeriod(newPeriod)
                      setRevenueDays(newPeriod === 'daily' ? 30 : newPeriod === 'weekly' ? 12 : 12)
                    }
                  }}
                  size="small"
                >
                  <ToggleButton value="daily">Günlük</ToggleButton>
                  <ToggleButton value="weekly">Haftalık</ToggleButton>
                  <ToggleButton value="monthly">Aylık</ToggleButton>
                </ToggleButtonGroup>
              </Box>
              {isLoadingChart ? (
                <ContentState state="loading" skeletonLines={4} />
              ) : revenueChart?.data && revenueChart.data.length > 0 ? (
                <ResponsiveContainer width="100%" height={300}>
                  <LineChart data={revenueChart.data}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="label" />
                    <YAxis />
                    <Tooltip
                      formatter={(value: number | undefined) => formatCurrency(Number(value || 0))}
                    />
                    <Legend />
                    <Line
                      type="monotone"
                      dataKey="revenue"
                      stroke="#1976d2"
                      strokeWidth={2}
                      name="Gelir"
                    />
                    <Line
                      type="monotone"
                      dataKey="bookingCount"
                      stroke="#ed6c02"
                      strokeWidth={2}
                      name="Rezervasyon Sayısı"
                    />
                  </LineChart>
                </ResponsiveContainer>
              ) : (
                <ContentState
                  state="empty"
                  title="Gösterilecek veri yok"
                  description="Bu grafik için henüz veri bulunmuyor."
                />
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Son Aktiviteler */}
        <Grid item xs={12} md={4}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Son Aktiviteler
              </Typography>
              {isLoadingActivities ? (
                <ContentState state="loading" skeletonLines={3} />
              ) : recentActivities?.recentBookings &&
                recentActivities.recentBookings.length > 0 ? (
                <Box>
                  {recentActivities.recentBookings.slice(0, 5).map((booking) => (
                    <Box
                      key={booking.id}
                      sx={{ mb: 2, pb: 2, borderBottom: '1px solid #eee' }}
                    >
                      <Typography variant="body2" fontWeight="bold">
                        {booking.guestName}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {booking.type} • {formatDate(booking.bookingDate)}
                      </Typography>
                      <Typography variant="body2" color="primary">
                        {formatCurrency(booking.amount)}
                      </Typography>
                    </Box>
                  ))}
                </Box>
              ) : (
                <ContentState
                  state="empty"
                  title="Henüz aktivite yok"
                  description="Yeni aktiviteler oluştuğunda burada görünecek."
                />
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Ödemesi Alınmamış Hizmetler */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" sx={{ mb: 2 }}>
                Ödemesi Alınmamış Hizmetler
              </Typography>
              {isLoadingUnpaid ? (
                <ContentState state="loading" skeletonLines={4} />
              ) : unpaidServices && unpaidServices.items.length > 0 ? (
                <TableContainer>
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Tür</TableCell>
                        <TableCell>Tarih</TableCell>
                        <TableCell>Misafir</TableCell>
                        <TableCell>Oda</TableCell>
                        <TableCell>Şehir</TableCell>
                        <TableCell align="right">Tutar</TableCell>
                        <TableCell>Durum</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {unpaidServices.items.map((item) => (
                        <TableRow key={`${item.serviceType}-${item.serviceId}`}>
                          <TableCell>{item.serviceType}</TableCell>
                          <TableCell>{formatDate(item.serviceDate)}</TableCell>
                          <TableCell>{item.guestName}</TableCell>
                          <TableCell>{item.roomNumber || '-'}</TableCell>
                          <TableCell>{item.cityName || '-'}</TableCell>
                          <TableCell align="right">
                            {formatCurrency(item.amount, item.currency)}
                          </TableCell>
                          <TableCell>
                            {item.status ? (
                              <Chip label={item.status} size="small" />
                            ) : (
                              <Chip label="Ödenmedi" color="warning" size="small" />
                            )}
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              ) : (
                <ContentState
                  state="empty"
                  title="Ödenmemiş hizmet yok"
                  description="Tüm hizmetlerin ödemesi alınmış görünüyor."
                />
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Yaklaşan Hizmetler */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" sx={{ mb: 2 }}>
                Yaklaşan Hizmetler
              </Typography>
              {isLoadingUpcoming ? (
                <ContentState state="loading" skeletonLines={4} />
              ) : upcomingServices && upcomingServices.items.length > 0 ? (
                <TableContainer>
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Tür</TableCell>
                        <TableCell>Tarih</TableCell>
                        <TableCell>Misafir</TableCell>
                        <TableCell>Oda</TableCell>
                        <TableCell>Şehir</TableCell>
                        <TableCell>Durum</TableCell>
                        <TableCell>Önem</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {upcomingServices.items.map((item) => (
                        <TableRow key={`${item.serviceType}-${item.serviceId}`}>
                          <TableCell>{item.serviceType}</TableCell>
                          <TableCell>{formatDate(item.serviceDate)}</TableCell>
                          <TableCell>{item.guestName}</TableCell>
                          <TableCell>{item.roomNumber || '-'}</TableCell>
                          <TableCell>{item.cityName || '-'}</TableCell>
                          <TableCell>{item.status || '-'}</TableCell>
                          <TableCell>
                            <Chip
                              label={item.isUrgent ? 'Acil' : 'Yaklaşıyor'}
                              color={item.isUrgent ? 'error' : 'info'}
                              size="small"
                            />
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              ) : (
                <ContentState
                  state="empty"
                  title="Yaklaşan hizmet yok"
                  description="Önümüzdeki günlerde planlanmış hizmet görünmüyor."
                />
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Ek İstatistikler ve Grafikler */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        {/* Hizmet Dağılımı (Pie Chart) */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Hizmet Dağılımı
              </Typography>
              {isLoadingStats ? (
                <ContentState state="loading" skeletonLines={4} />
              ) : quickStats ? (
                <ResponsiveContainer width="100%" height={300}>
                  <PieChart>
                    <Pie
                      data={[
                        { name: 'Transfer', value: quickStats.totalTransfers || 0 },
                        { name: 'Şehir Turu', value: quickStats.totalCityTours || 0 },
                        { name: 'Yat Turu', value: quickStats.totalYachtTours || 0 },
                      ]}
                      cx="50%"
                      cy="50%"
                      labelLine={false}
                      label={({ name, percent }) => `${name} ${(percent || 0 * 100).toFixed(0)}%`}
                      outerRadius={80}
                      fill="#8884d8"
                      dataKey="value"
                    >
                      {[
                        { name: 'Transfer', value: quickStats.totalTransfers || 0 },
                        { name: 'Şehir Turu', value: quickStats.totalCityTours || 0 },
                        { name: 'Yat Turu', value: quickStats.totalYachtTours || 0 },
                      ].map((_, index) => (
                        <Cell key={`cell-${index}`} fill={index === 0 ? '#1976d2' : index === 1 ? '#ed6c02' : '#2e7d32'} />
                      ))}
                    </Pie>
                    <Tooltip />
                    <Legend />
                  </PieChart>
                </ResponsiveContainer>
              ) : (
                <ContentState
                  state="empty"
                  title="Veri yok"
                  description="Hizmet dağılımı için veri bulunmuyor."
                />
              )}
            </CardContent>
          </Card>
        </Grid>

        {/* Hizmet Karşılaştırması (Bar Chart) */}
        <Grid item xs={12} md={6}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Hizmet Karşılaştırması
              </Typography>
              {isLoadingStats ? (
                <ContentState state="loading" skeletonLines={4} />
              ) : quickStats ? (
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart
                    data={[
                      { name: 'Transfer', value: quickStats.totalTransfers || 0 },
                      { name: 'Şehir Turu', value: quickStats.totalCityTours || 0 },
                      { name: 'Yat Turu', value: quickStats.totalYachtTours || 0 },
                      { name: 'Fatura', value: quickStats.totalInvoices || 0 },
                    ]}
                  >
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="name" />
                    <YAxis />
                    <Tooltip />
                    <Legend />
                    <Bar dataKey="value" fill="#1976d2" name="Toplam" />
                  </BarChart>
                </ResponsiveContainer>
              ) : (
                <ContentState
                  state="empty"
                  title="Veri yok"
                  description="Hizmet karşılaştırması için veri bulunmuyor."
                />
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Yaklaşan Rezervasyonlar */}
      <Grid container spacing={3}>
        <Grid item xs={12}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                Yaklaşan Rezervasyonlar
              </Typography>
              {isLoadingBookings ? (
                <ContentState state="loading" skeletonLines={3} />
              ) : upcomingBookings &&
                (upcomingBookings.today.length > 0 ||
                  upcomingBookings.thisWeek.length > 0) ? (
                <TableContainer>
                  <Table>
                    <TableHead>
                      <TableRow>
                        <TableCell>Tarih</TableCell>
                        <TableCell>Misafir</TableCell>
                        <TableCell>Tip</TableCell>
                        <TableCell>Lokasyon</TableCell>
                        <TableCell>Tutar</TableCell>
                        <TableCell>Durum</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {[
                        ...upcomingBookings.today,
                        ...upcomingBookings.thisWeek.slice(0, 5),
                      ].map((booking) => (
                        <TableRow key={booking.id}>
                          <TableCell>
                            {formatDate(booking.bookingDate)}
                          </TableCell>
                          <TableCell>{booking.guestName}</TableCell>
                          <TableCell>
                            <Chip
                              label={booking.type}
                              size="small"
                              color={
                                booking.type === 'Transfer'
                                  ? 'primary'
                                  : booking.type === 'CityTour'
                                  ? 'secondary'
                                  : 'success'
                              }
                            />
                          </TableCell>
                          <TableCell>{booking.location}</TableCell>
                          <TableCell>
                            {formatCurrency(booking.amount)}
                          </TableCell>
                          <TableCell>
                            <Chip label={booking.status} size="small" />
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              ) : (
                <ContentState
                  state="empty"
                  title="Yaklaşan rezervasyon yok"
                  description="Henüz planlanmış rezervasyon bulunmamaktadır."
                />
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>
        </>
      )}
    </Box>
  )
}

// OPERATIONS DASHBOARD COMPONENT - CONCIERGE MODE
interface OperationsDashboardProps {
  quickStats: any
  isLoadingStats: boolean
  unpaidServices: any
  isLoadingUnpaid: boolean
  upcomingServices: any
  isLoadingUpcoming: boolean
  onRetry: () => void
}

const OperationsDashboard = ({
  quickStats,
  isLoadingStats,
  unpaidServices,
  isLoadingUnpaid,
  upcomingServices,
  isLoadingUpcoming,
  onRetry
}: OperationsDashboardProps) => {
  return (
    <Box>
      {/* URGENT ACTIONS ALERT */}
      <Alert severity="error" sx={{ mb: 3 }}>
        <AlertTitle>🔴 ACİL EYLEMLER</AlertTitle>
        <Typography variant="body2">Şu anda müdahale edilmesi gereken durumlar:</Typography>
        <Box sx={{ mt: 1 }}>
          <Typography variant="body2">• 2 transfer için onay zamanı geçmiş</Typography>
          <Typography variant="body2">• 3 şoför atanmamış servis</Typography>
          <Typography variant="body2">• 1 hava durumu uyarısı (Şehir Turu #123)</Typography>
        </Box>
      </Alert>

      {/* TODAY'S SCHEDULE */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            📅 BUGÜNÜN PROGRAMI
          </Typography>
          <Box sx={{ display: 'flex', gap: 1, mb: 2 }}>
            <Chip label="08:00 - Transfer: İstanbul Havalimanı" color="primary" size="small" />
            <Chip label="10:30 - Şehir Turu: Kapadokya" color="secondary" size="small" />
            <Chip label="14:00 - Yat Turu: Bodrum" color="success" size="small" />
          </Box>
        </CardContent>
      </Card>

      {/* OPERATIONAL STATUS GRID */}
      <Grid container spacing={3} sx={{ mb: 3 }}>
        {/* DRIVER STATUS */}
        <Grid item xs={12} md={4}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                👨‍🚗 ŞOFÖR DURUMU
              </Typography>
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Typography variant="body2">Ahmet Yılmaz</Typography>
                  <Chip label="Müsait" color="success" size="small" />
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Typography variant="body2">Mehmet Kaya</Typography>
                  <Chip label="Transfer'da" color="warning" size="small" />
                </Box>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <Typography variant="body2">Ayşe Demir</Typography>
                  <Chip label="Araç Bakımda" color="error" size="small" />
                </Box>
              </Box>
            </CardContent>
          </Card>
        </Grid>

        {/* WEATHER ALERTS */}
        <Grid item xs={12} md={4}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                ⛈️ HAVA DURUMU UYARILARI
              </Typography>
              <Alert severity="warning" sx={{ mb: 1 }}>
                <Typography variant="body2">
                  <strong>Kapadokya:</strong> Şiddetli rüzgar - Balon turları etkilenebilir
                </Typography>
              </Alert>
              <Alert severity="info" sx={{ mb: 1 }}>
                <Typography variant="body2">
                  <strong>Bodrum:</strong> Deniz dalgalı - Yat turları için alternatif plan hazır
                </Typography>
              </Alert>
            </CardContent>
          </Card>
        </Grid>

        {/* CAPACITY ALERTS */}
        <Grid item xs={12} md={4}>
          <Card>
            <CardContent>
              <Typography variant="h6" gutterBottom>
                ⚠️ KAPASITE UYARILARI
              </Typography>
              <Alert severity="error" sx={{ mb: 1 }}>
                <Typography variant="body2">
                  <strong>14:00 Yat Turu:</strong> 8 kişi için 6 kişilik tekne
                </Typography>
              </Alert>
              <Alert severity="warning" sx={{ mb: 1 }}>
                <Typography variant="body2">
                  <strong>10:30 Şehir Turu:</strong> Rehber kapasitesi sınırda
                </Typography>
              </Alert>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* REVENUE AT RISK */}
      {!isLoadingUnpaid && unpaidServices && unpaidServices.items && unpaidServices.items.length > 0 && (
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              💰 RİSK ALTINDAKİ GELİRLER
            </Typography>
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Hizmet</TableCell>
                    <TableCell>Misafir</TableCell>
                    <TableCell>Tutar</TableCell>
                    <TableCell>Gecikme</TableCell>
                    <TableCell>İşlem</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {unpaidServices.items.slice(0, 5).map((item: any, index: number) => (
                    <TableRow key={item.serviceId || index}>
                      <TableCell>{item.serviceType}</TableCell>
                      <TableCell>{item.guestName}</TableCell>
                      <TableCell>{formatCurrency(item.remainingAmount)}</TableCell>
                      <TableCell>{item.daysOverdue} gün</TableCell>
                      <TableCell>
                        <Button size="small" variant="outlined" color="primary">
                          Hatırlat
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </CardContent>
        </Card>
      )}

      {/* UPCOMING SERVICES TIMELINE */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            📋 YAKLAŞAN HİZMETLER
          </Typography>
          {!isLoadingUpcoming && upcomingServices && upcomingServices.items ? (
            <TableContainer>
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Zaman</TableCell>
                    <TableCell>Hizmet</TableCell>
                    <TableCell>Misafir</TableCell>
                    <TableCell>Durum</TableCell>
                    <TableCell>Öncelik</TableCell>
                    <TableCell>İşlemler</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {upcomingServices.items.slice(0, 10).map((item: any) => (
                    <TableRow key={`${item.serviceType}-${item.serviceId}`}>
                      <TableCell>{formatDate(item.serviceDate)}</TableCell>
                      <TableCell>{item.serviceType}</TableCell>
                      <TableCell>{item.guestName}</TableCell>
                      <TableCell>{item.status || 'Planlandı'}</TableCell>
                      <TableCell>
                        <Chip
                          label={item.isUrgent ? 'Acil' : 'Normal'}
                          color={item.isUrgent ? 'error' : 'default'}
                          size="small"
                        />
                      </TableCell>
                      <TableCell>
                        <IconButton size="small" color="primary">
                          <VisibilityIcon fontSize="small" />
                        </IconButton>
                        <IconButton size="small" color="secondary">
                          <EditIcon fontSize="small" />
                        </IconButton>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          ) : (
            <ContentState
              state="loading"
              skeletonLines={5}
            />
          )}
        </CardContent>
      </Card>

      {/* GUEST REQUESTS */}
      <Card>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            🎯 ÖZEL MİSAFİR İSTEKLERİ
          </Typography>
          <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
            <Alert severity="info">
              <Typography variant="body2">
                <strong>VIP Transfer #456:</strong> Misafir wheelchair istiyor
              </Typography>
            </Alert>
            <Alert severity="warning">
              <Typography variant="body2">
                <strong>Şehir Turu #789:</strong> Vegan menü gerekli
              </Typography>
            </Alert>
            <Alert severity="success">
              <Typography variant="body2">
                <strong>Yat Turu #101:</strong> Doğum günü kutlaması için pasta
              </Typography>
            </Alert>
          </Box>
        </CardContent>
      </Card>
    </Box>
  )
}

export default DashboardPage

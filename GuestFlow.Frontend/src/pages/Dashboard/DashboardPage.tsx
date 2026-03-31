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
} from '@mui/material'
import { useNavigate } from 'react-router-dom'
import {
  People as PeopleIcon,
  DirectionsCar as TransferIcon,
  Receipt as InvoiceIcon,
  AttachMoney as MoneyIcon,
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
import ConciergeDashboard from '../../components/ConciergeDashboard/ConciergeDashboard'
import RealTimeKpiCards from '../../components/Analytics/RealTimeKpiCards'
import LiveOpsFeed from '../../components/Dashboard/LiveOpsFeed'
import RevenueKpiCards from '../../components/Revenue/RevenueKpiCards'

const DashboardPage = () => {
  const queryClient = useQueryClient()
  const navigate = useNavigate()
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
        <Typography variant="h4" component="h1" className="premium-gradient-text" sx={{ fontWeight: 800 }}>
          Dashboard
        </Typography>
        <ToggleButtonGroup
          value={dashboardMode}
          exclusive
          onChange={(_, value) => value && setDashboardMode(value)}
          aria-label="dashboard mode"
          size="small"
          sx={{
            bgcolor: 'background.paper',
            borderRadius: 2,
            p: 0.5,
            '& .MuiToggleButton-root': {
              border: 'none',
              borderRadius: 1.5,
              px: 3,
              '&.Mui-selected': {
                bgcolor: 'primary.main',
                color: 'white',
                '&:hover': { bgcolor: 'primary.dark' },
              }
            }
          }}
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
        // OPERATIONS DASHBOARD - CONCIERGE MODE (PMS Entegrasyonlu)
        <ConciergeDashboard />
      ) : (
        // ADMIN DASHBOARD - MANAGEMENT MODE
        <>
          {/* Real-Time Analytics KPIs */}
          <RealTimeKpiCards />

          {/* Revenue KPI Cards - ADR, RevPAR, Occupancy */}
          <RevenueKpiCards />

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
              <Card className="glass-panel" sx={{ transition: 'transform 0.2s', '&:hover': { transform: 'translateY(-4px)' } }}>
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
              <Card className="glass-panel" sx={{ transition: 'transform 0.2s', '&:hover': { transform: 'translateY(-4px)' } }}>
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
              <Card className="glass-panel" sx={{ transition: 'transform 0.2s', '&:hover': { transform: 'translateY(-4px)' } }}>
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
              <Card className="glass-panel">
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

            {/* Live Operations Feed */}
            <Grid item xs={12} md={4}>
              <LiveOpsFeed />
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
                            <TableCell>Gecikme</TableCell>
                            <TableCell>Durum</TableCell>
                            <TableCell align="right">İşlem</TableCell>
                          </TableRow>
                        </TableHead>
                        <TableBody>
                          {unpaidServices.items.map((item) => (
                            <TableRow key={`${item.serviceType}-${item.serviceId}`} hover>
                              <TableCell>{item.serviceType}</TableCell>
                              <TableCell>{formatDate(item.serviceDate)}</TableCell>
                              <TableCell>
                                <Typography variant="body2" sx={{ fontWeight: 600 }}>{item.guestName}</Typography>
                              </TableCell>
                              <TableCell>{item.roomNumber || '-'}</TableCell>
                              <TableCell>{item.cityName || '-'}</TableCell>
                              <TableCell align="right" sx={{ fontWeight: 600, color: 'error.main' }}>
                                {formatCurrency(item.amount, item.currency)}
                              </TableCell>
                              <TableCell>
                                <Chip
                                  label={`${item.daysOverdue} gün`}
                                  color={item.daysOverdue > 7 ? "error" : "warning"}
                                  size="small"
                                  variant="outlined"
                                />
                              </TableCell>
                              <TableCell>
                                {item.status ? (
                                  <Chip label={item.status} size="small" />
                                ) : (
                                  <Chip label="Ödenmedi" color="error" size="small" variant="outlined" />
                                )}
                              </TableCell>
                              <TableCell align="right">
                                <Button
                                  size="small"
                                  variant="outlined"
                                  onClick={() => navigate(`/finance/payments?serviceId=${item.serviceId}`)}
                                >
                                  Öde
                                </Button>
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
              <Card className="glass-panel">
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
              <Card className="glass-panel">
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
              <Card className="glass-panel">
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
              <Card className="glass-panel">
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

export default DashboardPage

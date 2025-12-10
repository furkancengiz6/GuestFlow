import { useState } from 'react'
import {
  Box,
  Typography,
  Grid,
  Card,
  CardContent,
  CircularProgress,
  Alert,
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
import {
  People as PeopleIcon,
  DirectionsCar as TransferIcon,
  Receipt as InvoiceIcon,
  AttachMoney as MoneyIcon,
  Tour as TourIcon,
  TrendingUp as TrendingUpIcon,
  CalendarMonth as CalendarMonthIcon,
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
} from '../../hooks/useDashboard'
import { formatCurrency, formatDate } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'

const DashboardPage = () => {
  const [revenuePeriod, setRevenuePeriod] = useState<'daily' | 'weekly' | 'monthly'>('daily')
  const [revenueDays, setRevenueDays] = useState<number>(30)

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

  if (statsError) {
    return (
      <ContentState
        state="error"
        title="Dashboard verileri yüklenemedi"
        description="Lütfen daha sonra tekrar deneyin veya sayfayı yenileyin."
        actionLabel="Tekrar dene"
        onAction={() => window.location.reload()}
      />
    )
  }

  return (
    <Box>
      <Typography variant="h4" component="h1" gutterBottom sx={{ mb: 3 }}>
        Dashboard
      </Typography>

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
                      formatter={(value: number) => formatCurrency(value)}
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
                      label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
                      outerRadius={80}
                      fill="#8884d8"
                      dataKey="value"
                    >
                      {[
                        { name: 'Transfer', value: quickStats.totalTransfers || 0 },
                        { name: 'Şehir Turu', value: quickStats.totalCityTours || 0 },
                        { name: 'Yat Turu', value: quickStats.totalYachtTours || 0 },
                      ].map((entry, index) => (
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
                  description="Planlanmış rezervasyonlar burada listelenecek."
                />
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  )
}

export default DashboardPage

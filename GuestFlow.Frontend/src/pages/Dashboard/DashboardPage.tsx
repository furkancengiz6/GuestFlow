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
} from '@mui/material'
import {
  People as PeopleIcon,
  DirectionsCar as TransferIcon,
  Receipt as InvoiceIcon,
  AttachMoney as MoneyIcon,
} from '@mui/icons-material'
import {
  LineChart,
  Line,
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

const DashboardPage = () => {
  const {
    data: quickStats,
    isLoading: isLoadingStats,
    error: statsError,
  } = useQuickStats()
  const { data: recentActivities, isLoading: isLoadingActivities } =
    useRecentActivities(5)
  const { data: revenueChart, isLoading: isLoadingChart } =
    useRevenueChartData('daily', 30)
  const { data: upcomingBookings, isLoading: isLoadingBookings } =
    useUpcomingBookings()

  if (statsError) {
    return (
      <Box>
        <Alert severity="error">
          Dashboard verileri yüklenirken bir hata oluştu.
        </Alert>
      </Box>
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
              <Typography variant="h6" gutterBottom>
                Gelir Trendi (Son 30 Gün)
              </Typography>
              {isLoadingChart ? (
                <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
                  <CircularProgress />
                </Box>
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
                  </LineChart>
                </ResponsiveContainer>
              ) : (
                <Box sx={{ p: 3, textAlign: 'center' }}>
                  <Typography color="text.secondary">
                    Grafik verisi bulunamadı
                  </Typography>
                </Box>
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
                <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
                  <CircularProgress />
                </Box>
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
                <Typography color="text.secondary">
                  Aktivite bulunamadı
                </Typography>
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
                <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
                  <CircularProgress />
                </Box>
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
                <Typography color="text.secondary">
                  Yaklaşan rezervasyon bulunamadı
                </Typography>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  )
}

export default DashboardPage

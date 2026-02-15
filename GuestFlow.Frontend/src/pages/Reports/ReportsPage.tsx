import { useState } from 'react'
import {
  Box,
  Paper,
  Typography,
  Grid,
  Card,
  CardContent,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  Collapse,
} from '@mui/material'
import {
  FilterList as FilterListIcon,
  Clear as ClearIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon,
  AutoAwesome as AutoAwesomeIcon,
} from '@mui/icons-material'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns'
import { tr } from 'date-fns/locale'
import { useQuery } from '@tanstack/react-query'
import { reportsService, ReportFilters } from '../../services/reportsService'
import { dropdownService } from '../../services/dropdownService'
import { formatCurrency } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'

const ReportsPage = () => {
  const [filtersOpen, setFiltersOpen] = useState(true)
  const [startDate, setStartDate] = useState<Date | null>(null)
  const [endDate, setEndDate] = useState<Date | null>(null)
  const [serviceType, setServiceType] = useState<string>('')
  const [personnelId, setPersonnelId] = useState<number | undefined>(undefined)

  // Build filters object
  const filters: ReportFilters = {
    ...(startDate && { startDate: startDate.toISOString().split('T')[0] }),
    ...(endDate && { endDate: endDate.toISOString().split('T')[0] }),
    ...(serviceType && { serviceType }),
    ...(personnelId && { personnelId }),
  }

  // Fetch dropdown data
  const { data: personnel } = useQuery({
    queryKey: ['personnel-dropdown'],
    queryFn: () => dropdownService.getPersonnel(),
  })

  // Fetch reports
  const { data: revenueSummary, isLoading: revenueLoading, error: revenueError } = useQuery({
    queryKey: ['reports', 'revenue-summary', filters],
    queryFn: () => reportsService.getRevenueSummary(filters),
  })

  const { data: transferStats, isLoading: transferLoading, error: transferError } = useQuery({
    queryKey: ['reports', 'transfer-statistics', filters],
    queryFn: () => reportsService.getTransferStatistics(filters),
  })

  const { data: personnelPerformance, isLoading: personnelLoading, error: personnelError } = useQuery({
    queryKey: ['reports', 'personnel-performance', filters],
    queryFn: () => reportsService.getPersonnelPerformance(filters),
  })

  const { data: aiInsights } = useQuery({
    queryKey: ['reports', 'revenue-ai-insights', filters],
    queryFn: () => reportsService.getRevenueAIInsights(filters),
    enabled: !!revenueSummary, // Only fetch if we have data to analyze
  })

  const handleClearFilters = () => {
    setStartDate(null)
    setEndDate(null)
    setServiceType('')
    setPersonnelId(undefined)
  }

  const hasActiveFilters = startDate || endDate || serviceType || personnelId

  return (
    <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={tr}>
      <Box p={3}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Typography variant="h5" component="h1">
            Raporlar
          </Typography>
          <Button
            variant="outlined"
            startIcon={filtersOpen ? <ExpandLessIcon /> : <ExpandMoreIcon />}
            onClick={() => setFiltersOpen(!filtersOpen)}
          >
            {filtersOpen ? 'Filtreleri Gizle' : 'Filtreleri Göster'}
          </Button>
        </Box>

        {/* Filters */}
        <Collapse in={filtersOpen}>
          <Paper sx={{ p: 3, mb: 3 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <FilterListIcon sx={{ mr: 1 }} />
              <Typography variant="h6">Filtreler</Typography>
            </Box>
            <Grid container spacing={2}>
              <Grid item xs={12} md={3}>
                <DatePicker
                  label="Başlangıç Tarihi"
                  value={startDate}
                  onChange={setStartDate}
                  slotProps={{ textField: { fullWidth: true } }}
                />
              </Grid>
              <Grid item xs={12} md={3}>
                <DatePicker
                  label="Bitiş Tarihi"
                  value={endDate}
                  onChange={setEndDate}
                  slotProps={{ textField: { fullWidth: true } }}
                />
              </Grid>
              <Grid item xs={12} md={3}>
                <FormControl fullWidth>
                  <InputLabel>Servis Tipi</InputLabel>
                  <Select
                    value={serviceType}
                    label="Servis Tipi"
                    onChange={(e) => setServiceType(e.target.value)}
                  >
                    <MenuItem value="">Tümü</MenuItem>
                    <MenuItem value="Transfer">Transfer</MenuItem>
                    <MenuItem value="CityTour">Şehir Turu</MenuItem>
                    <MenuItem value="YachtTour">Yat Turu</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} md={3}>
                <FormControl fullWidth>
                  <InputLabel>Personel</InputLabel>
                  <Select
                    value={personnelId || ''}
                    label="Personel"
                    onChange={(e) => setPersonnelId(e.target.value ? Number(e.target.value) : undefined)}
                  >
                    <MenuItem value="">Tümü</MenuItem>
                    {personnel?.map((p) => (
                      <MenuItem key={p.id} value={p.id}>
                        {p.fullName}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              {hasActiveFilters && (
                <Grid item xs={12}>
                  <Button
                    variant="outlined"
                    startIcon={<ClearIcon />}
                    onClick={handleClearFilters}
                    size="small"
                  >
                    Filtreleri Temizle
                  </Button>
                </Grid>
              )}
            </Grid>
          </Paper>
        </Collapse>

        {/* AI Insights Section */}
        {aiInsights && (
          <Paper
            sx={{
              p: 3,
              mb: 3,
              background: 'linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%)',
              borderLeft: '6px solid #1976d2',
              borderRadius: 2
            }}
          >
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 1 }}>
              <AutoAwesomeIcon sx={{ mr: 1, color: '#1976d2' }} />
              <Typography variant="h6" color="#1565c0">AI Rapor Analizi & Öneriler</Typography>
            </Box>
            <Typography variant="body1" sx={{ whiteSpace: 'pre-line', color: '#37474f', fontStyle: 'italic' }}>
              {aiInsights}
            </Typography>
          </Paper>
        )}

        {/* Revenue Summary Cards */}
        {revenueLoading ? (
          <ContentState state="loading" skeletonLines={4} />
        ) : revenueError || !revenueSummary ? (
          <ContentState
            state="error"
            title="Gelir özeti yüklenemedi"
            description="Lütfen daha sonra tekrar deneyin."
          />
        ) : (
          <Grid container spacing={3} sx={{ mb: 3 }}>
            <Grid item xs={12}>
              <Typography variant="h6" gutterBottom>
                Gelir Özeti
              </Typography>
            </Grid>
            {Object.entries(revenueSummary.totalRevenueByCurrency).map(([currency, amount]) => (
              <Grid item xs={12} sm={6} md={3} key={currency}>
                <Card>
                  <CardContent>
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                      Toplam Gelir ({currency})
                    </Typography>
                    <Typography variant="h5" component="div">
                      {formatCurrency(amount, currency)}
                    </Typography>
                  </CardContent>
                </Card>
              </Grid>
            ))}
            <Grid item xs={12} sm={6} md={3}>
              <Card>
                <CardContent>
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    Toplam Rezervasyon
                  </Typography>
                  <Typography variant="h5" component="div">
                    {revenueSummary.totalBookings}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    Transfer: {revenueSummary.transferCount} | Şehir Turu: {revenueSummary.cityTourCount} | Yat Turu: {revenueSummary.yachtTourCount}
                  </Typography>
                </CardContent>
              </Card>
            </Grid>
            <Grid item xs={12} sm={6} md={3}>
              <Card>
                <CardContent>
                  <Typography variant="body2" color="text.secondary" gutterBottom>
                    Toplam Ödeme Sayısı
                  </Typography>
                  <Typography variant="h5" component="div">
                    {revenueSummary.totalPaymentCount}
                  </Typography>
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        )}

        {/* Transfer Statistics */}
        {transferLoading ? (
          <ContentState state="loading" skeletonLines={2} />
        ) : transferError || !transferStats ? (
          <ContentState
            state="error"
            title="Transfer istatistikleri yüklenemedi"
            description="Lütfen daha sonra tekrar deneyin."
          />
        ) : (
          <Paper sx={{ p: 3, mb: 3 }}>
            <Typography variant="h6" gutterBottom>
              Transfer İstatistikleri
            </Typography>
            <Grid container spacing={2}>
              <Grid item xs={12} sm={6} md={3}>
                <Card variant="outlined">
                  <CardContent>
                    <Typography variant="body2" color="text.secondary">
                      Toplam Transfer
                    </Typography>
                    <Typography variant="h6">{transferStats.totalTransfers}</Typography>
                  </CardContent>
                </Card>
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <Card variant="outlined">
                  <CardContent>
                    <Typography variant="body2" color="text.secondary">
                      Toplam Gelir
                    </Typography>
                    <Typography variant="h6">{formatCurrency(transferStats.totalRevenue)}</Typography>
                  </CardContent>
                </Card>
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <Card variant="outlined">
                  <CardContent>
                    <Typography variant="body2" color="text.secondary">
                      Ortalama Fiyat
                    </Typography>
                    <Typography variant="h6">{formatCurrency(transferStats.averagePrice)}</Typography>
                  </CardContent>
                </Card>
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <Card variant="outlined">
                  <CardContent>
                    <Typography variant="body2" color="text.secondary">
                      Tamamlanan
                    </Typography>
                    <Typography variant="h6">{transferStats.completedTransfers}</Typography>
                  </CardContent>
                </Card>
              </Grid>
            </Grid>
          </Paper>
        )}

        {/* Personnel Performance Table */}
        {personnelLoading ? (
          <ContentState state="loading" skeletonLines={5} />
        ) : personnelError || !personnelPerformance ? (
          <ContentState
            state="error"
            title="Personel performans raporu yüklenemedi"
            description="Lütfen daha sonra tekrar deneyin."
          />
        ) : (
          <Paper sx={{ p: 3 }}>
            <Typography variant="h6" gutterBottom>
              Personel Performans Raporu
            </Typography>
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Personel</TableCell>
                    <TableCell>Kullanıcı Tipi</TableCell>
                    <TableCell align="right">Toplam Rezervasyon</TableCell>
                    <TableCell align="right">Transfer</TableCell>
                    <TableCell align="right">Şehir Turu</TableCell>
                    <TableCell align="right">Yat Turu</TableCell>
                    <TableCell align="right">Toplam Gelir</TableCell>
                    <TableCell align="right">Ortalama Değer</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {personnelPerformance.length === 0 ? (
                    <TableRow>
                      <TableCell colSpan={8} align="center">
                        <Typography color="text.secondary">Veri bulunamadı</Typography>
                      </TableCell>
                    </TableRow>
                  ) : (
                    personnelPerformance.map((perf) => (
                      <TableRow key={perf.personnelId}>
                        <TableCell>{perf.fullName}</TableCell>
                        <TableCell>
                          <Chip label={perf.userType} size="small" />
                        </TableCell>
                        <TableCell align="right">{perf.totalBookings}</TableCell>
                        <TableCell align="right">{perf.transferCount}</TableCell>
                        <TableCell align="right">{perf.cityTourCount}</TableCell>
                        <TableCell align="right">{perf.yachtTourCount}</TableCell>
                        <TableCell align="right">{formatCurrency(perf.totalRevenue)}</TableCell>
                        <TableCell align="right">{formatCurrency(perf.averageBookingValue)}</TableCell>
                      </TableRow>
                    ))
                  )}
                </TableBody>
              </Table>
            </TableContainer>
          </Paper>
        )}
      </Box>
    </LocalizationProvider>
  )
}

export default ReportsPage

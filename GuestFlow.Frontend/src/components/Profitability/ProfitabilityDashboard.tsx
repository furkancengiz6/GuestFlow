import React, { useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Grid,
  TextField,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Alert,
  CircularProgress,
  Chip
} from '@mui/material';
import {
  TrendingUp as TrendingUpIcon,
  AttachMoney as MoneyIcon,
  Business as BusinessIcon,
  Assessment as AssessmentIcon
} from '@mui/icons-material';
import { useQuery } from '@tanstack/react-query';
import { supplierService } from '../../services/supplierService';
import { DatePicker } from '@mui/x-date-pickers/DatePicker';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns';
import { tr } from 'date-fns/locale';

interface ProfitabilityReport {
  totalRevenue: number;
  totalCost: number;
  totalProfit: number;
  profitMargin: number;
  supplierBreakdown: SupplierBreakdown[];
  serviceTypeBreakdown: ServiceTypeBreakdown[];
}

interface SupplierBreakdown {
  supplierName: string;
  serviceCount: number;
  revenue: number;
  cost: number;
  profit: number;
  profitMargin: number;
}

interface ServiceTypeBreakdown {
  serviceType: string;
  serviceCount: number;
  revenue: number;
  cost: number;
}

const ProfitabilityDashboard: React.FC = () => {
  const [startDate, setStartDate] = useState<Date | null>(
    new Date(new Date().getFullYear(), new Date().getMonth(), 1)
  );
  const [endDate, setEndDate] = useState<Date | null>(new Date());

  const { data: reportData, isLoading, error, refetch } = useQuery({
    queryKey: ['profitability-report', startDate, endDate],
    queryFn: () => supplierService.getProfitabilityReport(
      startDate || new Date(),
      endDate || new Date()
    ),
    enabled: false // Only run when button is clicked
  });

  const { data: topSuppliersData } = useQuery({
    queryKey: ['top-suppliers', startDate, endDate],
    queryFn: () => supplierService.getTopSuppliersByProfit(
      startDate || new Date(),
      endDate || new Date(),
      10
    ),
    enabled: false
  });

  const handleGenerateReport = () => {
    refetch();
  };

  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat('tr-TR', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  };

  const formatPercentage = (value: number) => {
    return `${value.toFixed(1)}%`;
  };

  const report: ProfitabilityReport | null = reportData?.data;
  const topSuppliers = topSuppliersData?.data || [];

  return (
    <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={tr}>
      <Box sx={{ p: 3 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Kârlılık Analizi
        </Typography>

        {/* Date Filters */}
        <Paper sx={{ p: 3, mb: 3 }}>
          <Typography variant="h6" gutterBottom>
            Rapor Dönemi Seçin
          </Typography>
          <Grid container spacing={2} alignItems="center">
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
              <Button
                variant="contained"
                onClick={handleGenerateReport}
                disabled={!startDate || !endDate || isLoading}
                fullWidth
              >
                {isLoading ? 'Rapor Hazırlanıyor...' : 'Rapor Oluştur'}
              </Button>
            </Grid>
          </Grid>
        </Paper>

        {error && (
          <Alert severity="error" sx={{ mb: 3 }}>
            Rapor oluşturulurken hata oluştu: {error.message}
          </Alert>
        )}

        {/* Summary Cards */}
        {report && (
          <>
            <Grid container spacing={3} sx={{ mb: 3 }}>
              <Grid item xs={12} md={3}>
                <Card>
                  <CardContent>
                    <Box display="flex" alignItems="center" mb={1}>
                      <MoneyIcon color="primary" sx={{ mr: 1 }} />
                      <Typography variant="h6">Toplam Gelir</Typography>
                    </Box>
                    <Typography variant="h4" color="primary">
                      {formatCurrency(report.totalRevenue)}
                    </Typography>
                  </CardContent>
                </Card>
              </Grid>
              <Grid item xs={12} md={3}>
                <Card>
                  <CardContent>
                    <Box display="flex" alignItems="center" mb={1}>
                      <AssessmentIcon color="error" sx={{ mr: 1 }} />
                      <Typography variant="h6">Toplam Maliyet</Typography>
                    </Box>
                    <Typography variant="h4" color="error">
                      {formatCurrency(report.totalCost)}
                    </Typography>
                  </CardContent>
                </Card>
              </Grid>
              <Grid item xs={12} md={3}>
                <Card>
                  <CardContent>
                    <Box display="flex" alignItems="center" mb={1}>
                      <TrendingUpIcon color="success" sx={{ mr: 1 }} />
                      <Typography variant="h6">Net Kâr</Typography>
                    </Box>
                    <Typography variant="h4" color="success.main">
                      {formatCurrency(report.totalProfit)}
                    </Typography>
                  </CardContent>
                </Card>
              </Grid>
              <Grid item xs={12} md={3}>
                <Card>
                  <CardContent>
                    <Box display="flex" alignItems="center" mb={1}>
                      <BusinessIcon color="info" sx={{ mr: 1 }} />
                      <Typography variant="h6">Kâr Marjı</Typography>
                    </Box>
                    <Typography variant="h4" color="info.main">
                      {formatPercentage(report.profitMargin)}
                    </Typography>
                  </CardContent>
                </Card>
              </Grid>
            </Grid>

            {/* Service Type Breakdown */}
            <Paper sx={{ p: 3, mb: 3 }}>
              <Typography variant="h6" gutterBottom>
                Hizmet Türüne Göre Dağılım
              </Typography>
              <TableContainer>
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>Hizmet Türü</TableCell>
                      <TableCell align="right">Hizmet Sayısı</TableCell>
                      <TableCell align="right">Gelir</TableCell>
                      <TableCell align="right">Maliyet</TableCell>
                      <TableCell align="right">Kâr</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {report.serviceTypeBreakdown.map((service) => (
                      <TableRow key={service.serviceType}>
                        <TableCell>
                          <Chip
                            label={service.serviceType}
                            color={service.serviceType === 'Transfer' ? 'primary' :
                                   service.serviceType === 'Tour' ? 'secondary' : 'success'}
                            size="small"
                          />
                        </TableCell>
                        <TableCell align="right">{service.serviceCount}</TableCell>
                        <TableCell align="right">{formatCurrency(service.revenue)}</TableCell>
                        <TableCell align="right">{formatCurrency(service.cost)}</TableCell>
                        <TableCell align="right" sx={{ color: 'success.main', fontWeight: 'bold' }}>
                          {formatCurrency(service.revenue - service.cost)}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </Paper>

            {/* Supplier Breakdown */}
            <Paper sx={{ p: 3 }}>
              <Typography variant="h6" gutterBottom>
                Tedarikçi Bazlı Kârlılık
              </Typography>
              <TableContainer>
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>Tedarikçi</TableCell>
                      <TableCell align="right">Hizmet Sayısı</TableCell>
                      <TableCell align="right">Gelir</TableCell>
                      <TableCell align="right">Maliyet</TableCell>
                      <TableCell align="right">Kâr</TableCell>
                      <TableCell align="right">Kâr Marjı</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {report.supplierBreakdown.map((supplier) => (
                      <TableRow key={supplier.supplierName}>
                        <TableCell sx={{ fontWeight: 'bold' }}>
                          {supplier.supplierName}
                        </TableCell>
                        <TableCell align="right">{supplier.serviceCount}</TableCell>
                        <TableCell align="right">{formatCurrency(supplier.revenue)}</TableCell>
                        <TableCell align="right">{formatCurrency(supplier.cost)}</TableCell>
                        <TableCell align="right">
                          <Typography
                            sx={{
                              color: supplier.profit >= 0 ? 'success.main' : 'error.main',
                              fontWeight: 'bold'
                            }}
                          >
                            {formatCurrency(supplier.profit)}
                          </Typography>
                        </TableCell>
                        <TableCell align="right">
                          <Chip
                            label={formatPercentage(supplier.profitMargin)}
                            color={supplier.profitMargin >= 20 ? 'success' :
                                   supplier.profitMargin >= 10 ? 'warning' : 'error'}
                            size="small"
                          />
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </Paper>
          </>
        )}

        {/* Top Suppliers */}
        {topSuppliers && topSuppliers.length > 0 && (
          <Paper sx={{ p: 3, mt: 3 }}>
            <Typography variant="h6" gutterBottom>
              En Karlı Tedarikçiler (Top 10)
            </Typography>
            <TableContainer>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell>Sıra</TableCell>
                    <TableCell>Tedarikçi</TableCell>
                    <TableCell align="right">Gelir</TableCell>
                    <TableCell align="right">Maliyet</TableCell>
                    <TableCell align="right">Net Kâr</TableCell>
                    <TableCell align="right">Kâr Marjı</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {topSuppliers.map((supplier: any, index: number) => (
                    <TableRow key={supplier.supplierId}>
                      <TableCell>
                        <Chip
                          label={`#${index + 1}`}
                          color={index < 3 ? 'primary' : 'default'}
                          size="small"
                        />
                      </TableCell>
                      <TableCell sx={{ fontWeight: 'bold' }}>
                        {supplier.supplierName}
                      </TableCell>
                      <TableCell align="right">{formatCurrency(supplier.revenue)}</TableCell>
                      <TableCell align="right">{formatCurrency(supplier.cost)}</TableCell>
                      <TableCell align="right" sx={{ color: 'success.main', fontWeight: 'bold' }}>
                        {formatCurrency(supplier.profit)}
                      </TableCell>
                      <TableCell align="right">
                        <Chip
                          label={formatPercentage(supplier.profitMargin)}
                          color={supplier.profitMargin >= 25 ? 'success' :
                                 supplier.profitMargin >= 15 ? 'warning' : 'error'}
                          size="small"
                        />
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          </Paper>
        )}

        {!report && !isLoading && (
          <Alert severity="info" sx={{ mt: 3 }}>
            Kârlılık analizi için tarih aralığı seçin ve "Rapor Oluştur" butonuna tıklayın.
          </Alert>
        )}
      </Box>
    </LocalizationProvider>
  );
};

export default ProfitabilityDashboard;
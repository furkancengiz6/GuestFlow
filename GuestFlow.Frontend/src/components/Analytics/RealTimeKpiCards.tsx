// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

import {
  Box,
  Grid,
  Card,
  CardContent,
  Typography,
  Chip,
  LinearProgress,
} from '@mui/material'
import {
  AttachMoney as MoneyIcon,
  TrendingUp as TrendingUpIcon,
  TrendingDown as TrendingDownIcon,
  Assessment as AssessmentIcon,
  Percent as PercentIcon,
} from '@mui/icons-material'
import { useRealTimeKpis } from '../../hooks/useAnalytics'
import { formatCurrency } from '../../utils/formatters'
import ContentState from '../Feedback/ContentState'

const RealTimeKpiCards = () => {
  const { data: kpis, isLoading, error } = useRealTimeKpis()

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={4} />
  }

  if (error || !kpis) {
    return (
      <ContentState
        state="error"
        title="KPI'lar yüklenemedi"
        description="Lütfen daha sonra tekrar deneyin."
      />
    )
  }

  const getGrowthColor = (rate: number) => {
    if (rate > 0) return 'success'
    if (rate < 0) return 'error'
    return 'default'
  }

  const getGrowthIcon = (rate: number) => {
    if (rate > 0) return <TrendingUpIcon fontSize="small" />
    if (rate < 0) return <TrendingDownIcon fontSize="small" />
    return undefined
  }

  return (
    <Grid container spacing={3}>
      {/* Bugünkü Gelir */}
      <Grid item xs={12} sm={6} md={3}>
        <Card data-testid="kpi-card-today-revenue">
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
              <Typography variant="body2" color="text.secondary">
                Bugünkü Gelir
              </Typography>
              <MoneyIcon color="primary" />
            </Box>
            <Typography variant="h4" component="div" data-testid="kpi-revenue-today">
              {formatCurrency(kpis.todayRevenue)}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {kpis.todayServiceCount} hizmet
            </Typography>
          </CardContent>
        </Card>
      </Grid>

      {/* Bu Ayın Geliri */}
      <Grid item xs={12} sm={6} md={3}>
        <Card data-testid="kpi-card-month-revenue">
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
              <Typography variant="body2" color="text.secondary">
                Bu Ayın Geliri
              </Typography>
              <AssessmentIcon color="primary" />
            </Box>
            <Typography variant="h4" component="div" data-testid="kpi-revenue-month">
              {formatCurrency(kpis.thisMonthRevenue)}
            </Typography>
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 1 }}>
              <Chip
                icon={getGrowthIcon(kpis.revenueGrowthRate)}
                label={`${kpis.revenueGrowthRate >= 0 ? '+' : ''}${kpis.revenueGrowthRate.toFixed(1)}%`}
                size="small"
                color={getGrowthColor(kpis.revenueGrowthRate)}
                variant="outlined"
                data-testid="kpi-growth-rate"
              />
              <Typography variant="caption" color="text.secondary">
                Geçen aya göre
              </Typography>
            </Box>
          </CardContent>
        </Card>
      </Grid>

      {/* Net Kâr */}
      <Grid item xs={12} sm={6} md={3}>
        <Card data-testid="kpi-card-net-profit">
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
              <Typography variant="body2" color="text.secondary">
                Bu Ayın Net Kârı
              </Typography>
              <MoneyIcon color="success" />
            </Box>
            <Typography variant="h4" component="div" color="success.main" data-testid="kpi-net-profit">
              {formatCurrency(kpis.thisMonthNetProfit)}
            </Typography>
            <Box sx={{ mt: 1 }}>
              <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 0.5 }}>
                <Typography variant="caption" color="text.secondary">
                  Kâr Marjı
                </Typography>
                <Typography variant="caption" fontWeight="bold">
                  {kpis.profitMargin.toFixed(1)}%
                </Typography>
              </Box>
              <LinearProgress
                variant="determinate"
                value={Math.min(kpis.profitMargin, 100)}
                color={kpis.profitMargin > 20 ? 'success' : kpis.profitMargin > 10 ? 'warning' : 'error'}
                sx={{ height: 6, borderRadius: 3 }}
              />
            </Box>
          </CardContent>
        </Card>
      </Grid>

      {/* Ortalama Hizmet Başına Gelir */}
      <Grid item xs={12} sm={6} md={3}>
        <Card>
          <CardContent>
            <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between', mb: 1 }}>
              <Typography variant="body2" color="text.secondary">
                Ortalama Hizmet Başına
              </Typography>
              <PercentIcon color="info" />
            </Box>
            <Typography variant="h4" component="div">
              {formatCurrency(kpis.averageRevenuePerService)}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {kpis.thisMonthServiceCount} toplam hizmet
            </Typography>
          </CardContent>
        </Card>
      </Grid>

      {/* En Karlı Hizmetler */}
      {kpis.mostProfitableServices.length > 0 && (
        <Grid item xs={12}>
          <Card data-testid="most-profitable-services">
            <CardContent>
              <Typography variant="h6" gutterBottom>
                En Karlı Hizmetler
              </Typography>
              <Grid container spacing={2}>
                {kpis.mostProfitableServices.map((service, index) => (
                  <Grid item xs={12} sm={6} md={4} key={index}>
                    <Box
                      sx={{
                        p: 2,
                        border: '1px solid',
                        borderColor: 'divider',
                        borderRadius: 2,
                      }}
                    >
                      <Typography variant="subtitle1" fontWeight="bold">
                        {service.serviceType}
                      </Typography>
                      <Box sx={{ mt: 1 }}>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                          <Typography variant="caption" color="text.secondary">
                            Net Kâr:
                          </Typography>
                          <Typography variant="caption" fontWeight="bold" color="success.main">
                            {formatCurrency(service.netProfit)}
                          </Typography>
                        </Box>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 0.5 }}>
                          <Typography variant="caption" color="text.secondary">
                            Kâr Marjı:
                          </Typography>
                          <Typography variant="caption" fontWeight="bold">
                            {service.profitMargin.toFixed(1)}%
                          </Typography>
                        </Box>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                          <Typography variant="caption" color="text.secondary">
                            Hizmet Sayısı:
                          </Typography>
                          <Typography variant="caption">{service.serviceCount}</Typography>
                        </Box>
                      </Box>
                    </Box>
                  </Grid>
                ))}
              </Grid>
            </CardContent>
          </Card>
        </Grid>
      )}
    </Grid>
  )
}

export default RealTimeKpiCards

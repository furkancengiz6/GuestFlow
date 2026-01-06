import { useState } from 'react'
import { Card, CardContent, Box, ToggleButton, ToggleButtonGroup, Typography } from '@mui/material'
import { LineChart, LineChartData, LineChartSeries } from './LineChart'
import { formatCurrency } from '../../utils/formatters'

export interface RevenueChartData extends LineChartData {
  revenue: number
  bookingCount?: number
  expense?: number
  profit?: number
}

interface RevenueChartProps {
  data: RevenueChartData[]
  loading?: boolean
  period?: 'daily' | 'weekly' | 'monthly'
  onPeriodChange?: (period: 'daily' | 'weekly' | 'monthly') => void
  showBookingCount?: boolean
  showExpense?: boolean
  showProfit?: boolean
  title?: string
}

/**
 * Revenue chart component with period selector and trend analysis
 */
export const RevenueChart = ({
  data,
  loading = false,
  period = 'daily',
  onPeriodChange,
  showBookingCount = true,
  showExpense = false,
  showProfit = false,
  title = 'Gelir Trendi',
}: RevenueChartProps) => {
  const [selectedPeriod, setSelectedPeriod] = useState(period)

  const handlePeriodChange = (_event: React.MouseEvent<HTMLElement>, newPeriod: 'daily' | 'weekly' | 'monthly' | null) => {
    if (newPeriod) {
      setSelectedPeriod(newPeriod)
      onPeriodChange?.(newPeriod)
    }
  }

  const series: LineChartSeries[] = [
    {
      dataKey: 'revenue',
      name: 'Gelir',
      color: '#1976d2',
    },
  ]

  if (showBookingCount) {
    series.push({
      dataKey: 'bookingCount',
      name: 'Rezervasyon Sayısı',
      color: '#ed6c02',
    })
  }

  if (showExpense) {
    series.push({
      dataKey: 'expense',
      name: 'Gider',
      color: '#d32f2f',
    })
  }

  if (showProfit) {
    series.push({
      dataKey: 'profit',
      name: 'Kar',
      color: '#2e7d32',
    })
  }

  // Calculate trend
  const calculateTrend = () => {
    if (!data || data.length < 2) return null

    const firstHalf = data.slice(0, Math.floor(data.length / 2))
    const secondHalf = data.slice(Math.floor(data.length / 2))

    const firstAvg = firstHalf.reduce((sum, d) => sum + d.revenue, 0) / firstHalf.length
    const secondAvg = secondHalf.reduce((sum, d) => sum + d.revenue, 0) / secondHalf.length

    const trend = ((secondAvg - firstAvg) / firstAvg) * 100
    return trend
  }

  const trend = calculateTrend()

  return (
    <Card>
      <CardContent>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
          <Box>
            <Typography variant="h6">{title}</Typography>
            {trend !== null && (
              <Typography
                variant="body2"
                color={trend > 0 ? 'success.main' : trend < 0 ? 'error.main' : 'text.secondary'}
                sx={{ mt: 0.5 }}
              >
                {trend > 0 ? '↑' : trend < 0 ? '↓' : '→'} {Math.abs(trend).toFixed(1)}% trend
              </Typography>
            )}
          </Box>
          {onPeriodChange && (
            <ToggleButtonGroup
              value={selectedPeriod}
              exclusive
              onChange={handlePeriodChange}
              size="small"
            >
              <ToggleButton value="daily">Günlük</ToggleButton>
              <ToggleButton value="weekly">Haftalık</ToggleButton>
              <ToggleButton value="monthly">Aylık</ToggleButton>
            </ToggleButtonGroup>
          )}
        </Box>
        <LineChart
          data={data}
          series={series}
          height={300}
          loading={loading}
          tooltipFormatter={(value) => formatCurrency(Number(value))}
          emptyMessage="Gelir verisi bulunamadı"
        />
      </CardContent>
    </Card>
  )
}

export default RevenueChart


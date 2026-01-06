import {
  LineChart as RechartsLineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts'
import { Box, Typography, Paper, useTheme } from '@mui/material'
import { SkeletonLoader } from '../Feedback/SkeletonLoader'

export interface LineChartData {
  name: string
  [key: string]: string | number
}

export interface LineChartSeries {
  dataKey: string
  name: string
  color?: string
  strokeWidth?: number
}

interface LineChartProps {
  data: LineChartData[]
  series: LineChartSeries[]
  title?: string
  height?: number
  loading?: boolean
  xAxisKey?: string
  yAxisLabel?: string
  tooltipFormatter?: (value: any) => string
  emptyMessage?: string
}

/**
 * Reusable line chart component
 */
export const LineChart = ({
  data,
  series,
  title,
  height = 300,
  loading = false,
  xAxisKey = 'name',
  yAxisLabel,
  tooltipFormatter,
  emptyMessage = 'Gösterilecek veri yok',
}: LineChartProps) => {
  const theme = useTheme()

  if (loading) {
    return (
      <Box sx={{ p: 2 }}>
        {title && <Typography variant="h6" gutterBottom>{title}</Typography>}
        <SkeletonLoader variant="list" rows={4} />
      </Box>
    )
  }

  if (!data || data.length === 0) {
    return (
      <Paper sx={{ p: 3, textAlign: 'center' }}>
        {title && <Typography variant="h6" gutterBottom>{title}</Typography>}
        <Typography color="text.secondary">{emptyMessage}</Typography>
      </Paper>
    )
  }

  return (
    <Box>
      {title && (
        <Typography variant="h6" gutterBottom>
          {title}
        </Typography>
      )}
      <ResponsiveContainer width="100%" height={height}>
        <RechartsLineChart data={data} margin={{ top: 5, right: 30, left: 20, bottom: 5 }}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey={xAxisKey} />
          <YAxis label={yAxisLabel ? { value: yAxisLabel, angle: -90, position: 'insideLeft' } : undefined} />
          <Tooltip formatter={tooltipFormatter} />
          <Legend />
          {series.map((serie, index) => (
            <Line
              key={serie.dataKey}
              type="monotone"
              dataKey={serie.dataKey}
              name={serie.name}
              stroke={serie.color || theme.palette.primary.main}
              strokeWidth={serie.strokeWidth || 2}
              dot={{ r: 4 }}
              activeDot={{ r: 6 }}
            />
          ))}
        </RechartsLineChart>
      </ResponsiveContainer>
    </Box>
  )
}

export default LineChart


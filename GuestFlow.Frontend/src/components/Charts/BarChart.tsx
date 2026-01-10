import {
  BarChart as RechartsBarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts'
import { Box, Typography, Paper, useTheme } from '@mui/material'
import { SkeletonLoader } from '../Feedback/SkeletonLoader'

export interface BarChartData {
  name: string
  [key: string]: string | number
}

export interface BarChartSeries {
  dataKey: string
  name: string
  color?: string
}

interface BarChartProps {
  data: BarChartData[]
  series: BarChartSeries[]
  title?: string
  height?: number
  loading?: boolean
  xAxisKey?: string
  yAxisLabel?: string
  tooltipFormatter?: (value: any) => string
  emptyMessage?: string
  horizontal?: boolean
}

/**
 * Reusable bar chart component
 */
export const BarChart = ({
  data,
  series,
  title,
  height = 300,
  loading = false,
  xAxisKey = 'name',
  yAxisLabel,
  tooltipFormatter,
  emptyMessage = 'Gösterilecek veri yok',
  horizontal = false,
}: BarChartProps) => {
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
        <RechartsBarChart
          data={data}
          layout={horizontal ? 'vertical' : 'horizontal'}
          margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
        >
          <CartesianGrid strokeDasharray="3 3" />
          {horizontal ? (
            <>
              <XAxis type="number" />
              <YAxis dataKey={xAxisKey} type="category" width={100} />
            </>
          ) : (
            <>
              <XAxis dataKey={xAxisKey} />
              <YAxis label={yAxisLabel ? { value: yAxisLabel, angle: -90, position: 'insideLeft' } : undefined} />
            </>
          )}
          <Tooltip formatter={tooltipFormatter} />
          <Legend />
          {series.map((serie, _index) => (
            <Bar
              key={serie.dataKey}
              dataKey={serie.dataKey}
              name={serie.name}
              fill={serie.color || theme.palette.primary.main}
              radius={[4, 4, 0, 0]}
            />
          ))}
        </RechartsBarChart>
      </ResponsiveContainer>
    </Box>
  )
}

export default BarChart


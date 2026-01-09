import {
  PieChart as RechartsPieChart,
  Pie,
  Cell,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts'
import { Box, Typography, Paper, useTheme } from '@mui/material'
import { SkeletonLoader } from '../Feedback/SkeletonLoader'

export interface PieChartData {
  name: string
  value: number
  color?: string
}

interface PieChartProps {
  data: PieChartData[]
  title?: string
  height?: number
  loading?: boolean
  emptyMessage?: string
  showLabel?: boolean
  labelFormatter?: (entry: PieChartData, percent: number) => string
  tooltipFormatter?: (value?: number, name?: string) => any
  innerRadius?: number
  outerRadius?: number
}

/**
 * Reusable pie chart component
 */
export const PieChart = ({
  data,
  title,
  height = 300,
  loading = false,
  emptyMessage = 'Gösterilecek veri yok',
  showLabel = true,
  labelFormatter,
  tooltipFormatter,
  innerRadius = 0,
  outerRadius = 80,
}: PieChartProps) => {
  const theme = useTheme()

  const defaultColors = [
    theme.palette.primary.main,
    theme.palette.secondary.main,
    theme.palette.success.main,
    theme.palette.error.main,
    theme.palette.warning.main,
    theme.palette.info.main,
  ]

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

  const chartData = data.map((item, index) => ({
    ...item,
    color: item.color || defaultColors[index % defaultColors.length],
  }))

  return (
    <Box>
      {title && (
        <Typography variant="h6" gutterBottom>
          {title}
        </Typography>
      )}
      <ResponsiveContainer width="100%" height={height}>
        <RechartsPieChart>
          <Pie
            data={chartData}
            cx="50%"
            cy="50%"
            labelLine={showLabel}
            label={
              showLabel
                ? labelFormatter
                  ? (props: any) => {
                      const name = (props && props.name) || ''
                      const percent = (props && props.percent ? (props.percent * 100) : 0)
                      const entry = chartData.find((d) => d.name === name) || { name, value: Number(props?.value) || 0 }
                      return labelFormatter(entry as PieChartData, percent)
                    }
                  : (props: any) => {
                      const name = (props && props.name) || ''
                      const percent = props && props.percent ? (props.percent * 100) : 0
                      return `${name} ${percent.toFixed(0)}%`
                    }
                : false
            }
            outerRadius={outerRadius}
            innerRadius={innerRadius}
            fill="#8884d8"
            dataKey="value"
          >
            {chartData.map((entry, index) => (
              <Cell key={`cell-${index}`} fill={entry.color || defaultColors[index % defaultColors.length]} />
            ))}
          </Pie>
          <Tooltip formatter={(value?: number, name?: string) => (tooltipFormatter ? tooltipFormatter(value, name) : value)} />
          <Legend />
        </RechartsPieChart>
      </ResponsiveContainer>
    </Box>
  )
}

export default PieChart


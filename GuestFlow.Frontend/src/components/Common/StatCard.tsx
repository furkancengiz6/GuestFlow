import { Card, CardContent, Typography, Box, useTheme } from '@mui/material'
import { ReactNode } from 'react'
import TrendingUpIcon from '@mui/icons-material/TrendingUp'
import TrendingDownIcon from '@mui/icons-material/TrendingDown'

interface StatCardProps {
  title: string
  value: string | number
  icon?: ReactNode
  trend?: {
    value: number
    label?: string
  }
  color?: 'primary' | 'secondary' | 'success' | 'error' | 'warning' | 'info'
  onClick?: () => void
}

/**
 * Statistics card component for dashboard
 */
export const StatCard = ({
  title,
  value,
  icon,
  trend,
  color = 'primary',
  onClick,
}: StatCardProps) => {
  const theme = useTheme()

  return (
    <Card
      sx={{
        height: '100%',
        cursor: onClick ? 'pointer' : 'default',
        transition: 'all 0.2s ease-in-out',
        '&:hover': onClick
          ? {
              elevation: 4,
              transform: 'translateY(-2px)',
            }
          : {},
      }}
      onClick={onClick}
    >
      <CardContent>
        <Box sx={{ display: 'flex', alignItems: 'flex-start', justifyContent: 'space-between' }}>
          <Box sx={{ flex: 1 }}>
            <Typography color="text.secondary" variant="body2" gutterBottom>
              {title}
            </Typography>
            <Typography variant="h4" component="div" sx={{ fontWeight: 600, mb: 1 }}>
              {value}
            </Typography>
            {trend && (
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                {trend.value > 0 ? (
                  <TrendingUpIcon sx={{ fontSize: 16, color: 'success.main' }} />
                ) : (
                  <TrendingDownIcon sx={{ fontSize: 16, color: 'error.main' }} />
                )}
                <Typography
                  variant="body2"
                  sx={{
                    color: trend.value > 0 ? 'success.main' : 'error.main',
                    fontWeight: 500,
                  }}
                >
                  {Math.abs(trend.value)}% {trend.label}
                </Typography>
              </Box>
            )}
          </Box>
          {icon && (
            <Box
              sx={{
                p: 1.5,
                borderRadius: 1,
                bgcolor: `${theme.palette[color].main}15`,
                color: `${color}.main`,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
              }}
            >
              {icon}
            </Box>
          )}
        </Box>
      </CardContent>
    </Card>
  )
}

export default StatCard


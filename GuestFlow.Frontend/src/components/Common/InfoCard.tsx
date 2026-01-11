import { Card, CardContent, CardHeader, CardActions, Typography } from '@mui/material'
import { ReactNode } from 'react'

interface InfoCardProps {
  title: string
  subtitle?: string
  content: ReactNode
  actions?: ReactNode
  icon?: ReactNode
  variant?: 'elevation' | 'outlined'
  elevation?: number
  onClick?: () => void
  headerAction?: ReactNode
}

/**
 * Reusable info card component for dashboard and list views
 */
export const InfoCard = ({
  title,
  subtitle,
  content,
  actions,
  icon,
  variant = 'elevation',
  elevation = 1,
  onClick,
  headerAction,
}: InfoCardProps) => {
  return (
    <Card
      variant={variant}
      elevation={variant === 'elevation' ? elevation : undefined}
      sx={{
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        cursor: onClick ? 'pointer' : 'default',
        transition: 'all 0.2s ease-in-out',
        '&:hover': onClick
          ? {
              elevation: elevation + 2,
              transform: 'translateY(-2px)',
            }
          : {},
      }}
      onClick={onClick}
    >
      <CardHeader
        avatar={icon}
        title={
          <Typography variant="h6" component="div">
            {title}
          </Typography>
        }
        subheader={subtitle}
        action={headerAction}
      />
      <CardContent sx={{ flexGrow: 1 }}>{content}</CardContent>
      {actions && <CardActions>{actions}</CardActions>}
    </Card>
  )
}

export default InfoCard


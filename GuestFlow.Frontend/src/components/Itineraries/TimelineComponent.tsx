import { Box, Paper, Typography, Chip } from '@mui/material'
import { TimelineItem as TimelineItemType } from '../../types/itinerary'
import { formatDate, formatCurrency } from '../../utils/formatters'

interface TimelineComponentProps {
  items: TimelineItemType[]
}

const TimelineComponent = ({ items }: TimelineComponentProps) => {
  const getColor = (itemType: string): 'primary' | 'success' | 'info' | 'warning' | 'default' => {
    switch (itemType.toLowerCase()) {
      case 'transfer':
        return 'primary'
      case 'citytour':
        return 'success'
      case 'yachttour':
        return 'info'
      case 'restaurantreservation':
        return 'warning'
      default:
        return 'default'
    }
  }

  if (items.length === 0) {
    return (
      <Paper sx={{ p: 3 }}>
        <Typography variant="body2" color="text.secondary" align="center">
          Timeline'da aktivite bulunamadı
        </Typography>
      </Paper>
    )
  }

  return (
    <Paper sx={{ p: 2 }}>
      <Typography variant="h6" gutterBottom>
        Zaman Çizelgesi
      </Typography>
      <Box sx={{ position: 'relative', pl: 3 }}>
        {items.map((item, index) => (
          <Box key={item.id} sx={{ position: 'relative', pb: 3 }}>
            {/* Timeline line */}
            {index < items.length - 1 && (
              <Box
                sx={{
                  position: 'absolute',
                  left: '-12px',
                  top: '24px',
                  bottom: '-12px',
                  width: '2px',
                  bgcolor: 'divider',
                }}
              />
            )}
            {/* Timeline dot */}
            <Box
              sx={{
                position: 'absolute',
                left: '-16px',
                top: '4px',
                width: '12px',
                height: '12px',
                borderRadius: '50%',
                bgcolor: `${getColor(item.itemType)}.main`,
                border: '2px solid',
                borderColor: 'background.paper',
                zIndex: 1,
              }}
            />
            {/* Content */}
            <Box>
              <Box display="flex" alignItems="center" gap={1} mb={1}>
                <Chip
                  label={item.itemTypeTurkish}
                  size="small"
                  color={getColor(item.itemType)}
                />
                <Typography variant="caption" color="text.secondary">
                  {formatDate(item.scheduledDateTime)}
                </Typography>
              </Box>
              <Typography variant="subtitle2" fontWeight="medium">
                {item.serviceName || item.description}
              </Typography>
              {item.description && (
                <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                  {item.description}
                </Typography>
              )}
              {item.location && (
                <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                  📍 {item.location}
                </Typography>
              )}
              {item.pickupLocation && (
                <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                  🚗 Alış: {item.pickupLocation}
                </Typography>
              )}
              {item.dropoffLocation && (
                <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                  🎯 Bırakış: {item.dropoffLocation}
                </Typography>
              )}
              {item.price && (
                <Typography variant="body2" color="primary" sx={{ mt: 0.5, fontWeight: 'medium' }}>
                  💰 {formatCurrency(item.price, item.currency || 'TRY')}
                </Typography>
              )}
              {item.duration && (
                <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
                  ⏱️ {item.duration}
                </Typography>
              )}
              {item.notes && (
                <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5, fontStyle: 'italic' }}>
                  📝 {item.notes}
                </Typography>
              )}
              {item.status && (
                <Chip
                  label={item.status}
                  size="small"
                  sx={{ mt: 1 }}
                  color={item.status === 'Completed' ? 'success' : 'default'}
                />
              )}
            </Box>
          </Box>
        ))}
      </Box>
    </Paper>
  )
}

export default TimelineComponent


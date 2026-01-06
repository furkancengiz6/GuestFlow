import { useParams, useNavigate } from 'react-router-dom'
import { Box, Paper, Typography, Button, Chip, Divider, Grid } from '@mui/material'
import { ArrowBack as ArrowBackIcon } from '@mui/icons-material'
import { useQuery } from '@tanstack/react-query'
import { itineraryService } from '../../services/itineraryService'
import { formatDate, formatCurrency } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'
import TimelineComponent from '../../components/Itineraries/TimelineComponent'

const ItineraryTimelinePage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const itineraryId = id ? parseInt(id, 10) : 0

  const { data: timeline, isLoading, error } = useQuery({
    queryKey: ['itinerary-timeline', itineraryId],
    queryFn: () => itineraryService.getItineraryTimeline(itineraryId),
    enabled: !!itineraryId,
  })

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={6} />
  }

  if (error || !timeline) {
    return (
      <ContentState
        state="error"
        title="Hata"
        description="Timeline yüklenirken bir hata oluştu."
      />
    )
  }

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'confirmed':
        return 'success'
      case 'inprogress':
        return 'info'
      case 'completed':
        return 'default'
      case 'cancelled':
        return 'error'
      default:
        return 'default'
    }
  }

  const getStatusLabel = (status: string) => {
    switch (status.toLowerCase()) {
      case 'draft':
        return 'Taslak'
      case 'confirmed':
        return 'Onaylandı'
      case 'inprogress':
        return 'Devam Ediyor'
      case 'completed':
        return 'Tamamlandı'
      case 'cancelled':
        return 'İptal Edildi'
      default:
        return status
    }
  }

  return (
    <Box>
      <Box display="flex" alignItems="center" gap={2} mb={3}>
        <Button
          variant="outlined"
          startIcon={<ArrowBackIcon />}
          onClick={() => navigate('/itineraries')}
        >
          Geri
        </Button>
        <Typography variant="h4">İtinerary Timeline</Typography>
      </Box>

      <Paper sx={{ p: 3, mb: 3 }}>
        <Grid container spacing={2}>
          <Grid item xs={12} md={6}>
            <Typography variant="body2" color="text.secondary">
              İtinerary No
            </Typography>
            <Typography variant="h6">{timeline.itineraryNumber}</Typography>
          </Grid>
          <Grid item xs={12} md={6}>
            <Typography variant="body2" color="text.secondary">
              Misafir
            </Typography>
            <Typography variant="h6">{timeline.guestName}</Typography>
          </Grid>
          <Grid item xs={12} md={4}>
            <Typography variant="body2" color="text.secondary">
              Başlangıç Tarihi
            </Typography>
            <Typography variant="body1">{formatDate(timeline.startDate)}</Typography>
          </Grid>
          <Grid item xs={12} md={4}>
            <Typography variant="body2" color="text.secondary">
              Bitiş Tarihi
            </Typography>
            <Typography variant="body1">{formatDate(timeline.endDate)}</Typography>
          </Grid>
          <Grid item xs={12} md={4}>
            <Typography variant="body2" color="text.secondary">
              Durum
            </Typography>
            <Chip
              label={getStatusLabel(timeline.status)}
              color={getStatusColor(timeline.status)}
              sx={{ mt: 0.5 }}
            />
          </Grid>
          <Grid item xs={12}>
            <Divider sx={{ my: 2 }} />
            <Typography variant="body2" color="text.secondary">
              Toplam Tutar
            </Typography>
            <Typography variant="h5" color="primary">
              {formatCurrency(timeline.totalCost, timeline.currency)}
            </Typography>
          </Grid>
        </Grid>
      </Paper>

      <TimelineComponent items={timeline.items} />
    </Box>
  )
}

export default ItineraryTimelinePage


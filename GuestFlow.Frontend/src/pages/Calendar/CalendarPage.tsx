import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import {
  Box,
  Paper,
  Typography,
  Button,
  Tabs,
  Tab,
  Grid,
  Card,
  CardContent,
  Chip,
  IconButton,
  Tooltip,
} from '@mui/material'
import {
  DirectionsCar as TransferIcon,
  Tour as TourIcon,
  Download as DownloadIcon,
} from '@mui/icons-material'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { transferService } from '../../services/transferService'
import { tourService } from '../../services/tourService'
import { formatDate, formatCurrency } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'
import { useNotification } from '../../hooks/useNotification'
import { downloadICS, createTransferEvent, createTourEvent } from '../../utils/calendarExport'

const CalendarPage = () => {
  const navigate = useNavigate()
  const [tabValue, setTabValue] = useState(0)
  const notification = useNotification()
  const queryClient = useQueryClient()

  const { data: transfers, isLoading: transfersLoading, error: transfersError } = useQuery({
    queryKey: ['transfers-calendar'],
    queryFn: () => transferService.getTransfers(1, 100, {}),
  })

  const { data: cityTours, isLoading: cityToursLoading, error: cityToursError } = useQuery({
    queryKey: ['city-tours-calendar'],
    queryFn: () => tourService.getCityTours(1, 100, {}),
  })

  const { data: yachtTours, isLoading: yachtToursLoading, error: yachtToursError } = useQuery({
    queryKey: ['yacht-tours-calendar'],
    queryFn: () => tourService.getYachtTours(1, 100, {}),
  })

  const handleDownloadCalendar = async (type: 'transfer' | 'citytour' | 'yachttour', id: number) => {
    try {
      let event
      
      if (type === 'transfer') {
        const transfer = transfers?.data?.find((t) => t.id === id)
        if (!transfer) {
          notification.showError('Transfer bulunamadı')
          return
        }
        event = createTransferEvent(transfer)
      } else if (type === 'citytour') {
        const tour = cityTours?.data?.find((t) => t.id === id)
        if (!tour) {
          notification.showError('Şehir turu bulunamadı')
          return
        }
        event = createTourEvent(tour, 'city')
      } else {
        const tour = yachtTours?.data?.find((t) => t.id === id)
        if (!tour) {
          notification.showError('Yat turu bulunamadı')
          return
        }
        event = createTourEvent(tour, 'yacht')
      }
      
      downloadICS([event], `${type}-${id}.ics`)
      notification.showSuccess('Takvim dosyası indirildi.')
    } catch (error: any) {
      notification.showError(error?.message || 'Takvim dosyası indirilirken bir hata oluştu.')
    }
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Confirmed':
      case 'Completed':
        return 'success'
      case 'Cancelled':
        return 'error'
      case 'Pending':
        return 'warning'
      default:
        return 'default'
    }
  }

  const isLoading = transfersLoading || cityToursLoading || yachtToursLoading
  const hasError = transfersError || cityToursError || yachtToursError

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={6} />
  }

  if (hasError) {
    return (
      <ContentState
        state="error"
        title="Veriler yüklenemedi"
        description={transfersError?.message || cityToursError?.message || yachtToursError?.message || "Lütfen daha sonra tekrar deneyin."}
        actionLabel="Tekrar dene"
        onAction={() => {
          queryClient.refetchQueries({ queryKey: ['transfers'] })
          queryClient.refetchQueries({ queryKey: ['cityTours'] })
          queryClient.refetchQueries({ queryKey: ['yachtTours'] })
        }}
      />
    )
  }

  return (
    <Box p={3}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" sx={{ fontWeight: 600 }}>
          Takvim Görünümü
        </Typography>
      </Box>

      <Paper sx={{ mb: 3 }}>
        <Tabs value={tabValue} onChange={(_, newValue) => setTabValue(newValue)}>
          <Tab label="Transferler" icon={<TransferIcon />} iconPosition="start" />
          <Tab label="Şehir Turları" icon={<TourIcon />} iconPosition="start" />
          <Tab label="Yat Turları" icon={<TourIcon />} iconPosition="start" />
        </Tabs>
      </Paper>

      {tabValue === 0 && (
        <Box>
          {!transfers?.data || transfers.data.length === 0 ? (
            <ContentState
              state="empty"
              title="Transfer bulunamadı"
              description="Henüz kayıtlı transfer bulunmamaktadır."
            />
          ) : (
            <Grid container spacing={2}>
              {transfers.data.map((transfer) => (
                <Grid item xs={12} sm={6} md={4} key={transfer.id}>
                  <Card>
                    <CardContent>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start', mb: 2 }}>
                        <Typography variant="h6" sx={{ fontWeight: 600 }}>
                          Transfer #{transfer.id}
                        </Typography>
                        <Chip
                          label={transfer.status}
                          color={getStatusColor(transfer.status || 'pending') as any}
                          size="small"
                        />
                      </Box>
                      <Typography variant="body2" color="text.secondary" gutterBottom>
                        <strong>Tarih:</strong> {formatDate(transfer.transferDate)}
                      </Typography>
                      <Typography variant="body2" color="text.secondary" gutterBottom>
                        <strong>Alış:</strong> {transfer.pickupAddress}
                      </Typography>
                      <Typography variant="body2" color="text.secondary" gutterBottom>
                        <strong>Bırakış:</strong> {transfer.dropoffAddress}
                      </Typography>
                      {transfer.guest?.fullName && (
                        <Typography variant="body2" color="text.secondary" gutterBottom>
                          <strong>Misafir:</strong> {transfer.guest.fullName}
                        </Typography>
                      )}
                      <Box sx={{ display: 'flex', gap: 1, mt: 2 }}>
                        <Button
                          size="small"
                          variant="outlined"
                          onClick={() => navigate(`/transfers/${transfer.id}`)}
                        >
                          Detay
                        </Button>
                        <Tooltip title="Takvim Dosyası İndir">
                          <IconButton
                            size="small"
                            color="primary"
                            onClick={() => handleDownloadCalendar('transfer', transfer.id)}
                          >
                            <DownloadIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      </Box>
                    </CardContent>
                  </Card>
                </Grid>
              ))}
            </Grid>
          )}
        </Box>
      )}

      {tabValue === 1 && (
        <Box>
          {!cityTours?.data || cityTours.data.length === 0 ? (
            <ContentState
              state="empty"
              title="Şehir turu bulunamadı"
              description="Henüz kayıtlı şehir turu bulunmamaktadır."
            />
          ) : (
            <Grid container spacing={2}>
              {cityTours.data.map((tour) => (
                <Grid item xs={12} sm={6} md={4} key={tour.id}>
                  <Card>
                    <CardContent>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start', mb: 2 }}>
                        <Typography variant="h6" sx={{ fontWeight: 600 }}>
                          Şehir Turu #{tour.id}
                        </Typography>
                        <Chip
                          label="Aktif"
                          color="success"
                          size="small"
                        />
                      </Box>
                      <Typography variant="body2" color="text.secondary" gutterBottom>
                        <strong>Tarih:</strong> {formatDate(tour.tourDate)}
                      </Typography>
                      <Typography variant="body2" color="text.secondary" gutterBottom>
                        <strong>Dil:</strong> {tour.language}
                      </Typography>
                      <Typography variant="body2" color="text.secondary" gutterBottom>
                        <strong>Süre:</strong> {tour.durationHours} saat
                      </Typography>
                      <Typography variant="body2" color="text.secondary" gutterBottom>
                        <strong>Fiyat:</strong> {formatCurrency(tour.price, 'TRY')}
                      </Typography>
                      <Box sx={{ display: 'flex', gap: 1, mt: 2 }}>
                        <Button
                          size="small"
                          variant="outlined"
                          onClick={() => navigate(`/tours/city/${tour.id}`)}
                        >
                          Detay
                        </Button>
                        <Tooltip title="Takvim Dosyası İndir">
                          <IconButton
                            size="small"
                            color="primary"
                            onClick={() => handleDownloadCalendar('citytour', tour.id)}
                          >
                            <DownloadIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      </Box>
                    </CardContent>
                  </Card>
                </Grid>
              ))}
            </Grid>
          )}
        </Box>
      )}

      {tabValue === 2 && (
        <Box>
          {!yachtTours?.data || yachtTours.data.length === 0 ? (
            <ContentState
              state="empty"
              title="Yat turu bulunamadı"
              description="Henüz kayıtlı yat turu bulunmamaktadır."
            />
          ) : (
            <Grid container spacing={2}>
              {yachtTours.data.map((tour) => (
                <Grid item xs={12} sm={6} md={4} key={tour.id}>
                  <Card>
                    <CardContent>
                      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start', mb: 2 }}>
                        <Typography variant="h6" sx={{ fontWeight: 600 }}>
                          Yat Turu #{tour.id}
                        </Typography>
                        <Chip
                          label="Aktif"
                          color="success"
                          size="small"
                        />
                      </Box>
                      <Typography variant="body2" color="text.secondary" gutterBottom>
                        <strong>Tarih:</strong> {formatDate(tour.tourDate)}
                      </Typography>
                      <Typography variant="body2" color="text.secondary" gutterBottom>
                        <strong>Yat Adı:</strong> {tour.yachtName}
                      </Typography>
                      <Typography variant="body2" color="text.secondary" gutterBottom>
                        <strong>Kişi Sayısı:</strong> {tour.numberOfPeople}
                      </Typography>
                      <Typography variant="body2" color="text.secondary" gutterBottom>
                        <strong>Fiyat:</strong> {formatCurrency(tour.price, 'TRY')}
                      </Typography>
                      {tour.specialRequest && (
                        <Typography variant="body2" color="text.secondary" gutterBottom>
                          <strong>Özel İstek:</strong> {tour.specialRequest}
                        </Typography>
                      )}
                      <Box sx={{ display: 'flex', gap: 1, mt: 2 }}>
                        <Button
                          size="small"
                          variant="outlined"
                          onClick={() => navigate(`/tours/yacht/${tour.id}`)}
                        >
                          Detay
                        </Button>
                        <Tooltip title="Takvim Dosyası İndir">
                          <IconButton
                            size="small"
                            color="primary"
                            onClick={() => handleDownloadCalendar('yachttour', tour.id)}
                          >
                            <DownloadIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      </Box>
                    </CardContent>
                  </Card>
                </Grid>
              ))}
            </Grid>
          )}
        </Box>
      )}
    </Box>
  )
}

export default CalendarPage


import React, { useState } from 'react'
import {
  Box,
  Typography,
  Button,
  Card,
  CardContent,
  Grid,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Alert,
  Accordion,
  AccordionSummary,
  AccordionDetails,
  List,
  ListItem,
  ListItemText,
  Chip,
} from '@mui/material'
import { ExpandMore as ExpandMoreIcon, Person as PersonIcon } from '@mui/icons-material'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns'
import { useQuery } from '@tanstack/react-query'
import { tr } from 'date-fns/locale'
import { format } from 'date-fns'

import { roomService, RoomContext, RoomContextRequest } from '../../services/roomService'
import { hotelService } from '../../services/hotelService'
import { formatCurrency } from '../../utils/formatters'
import RoomAssignmentForm from '../../components/Guests/RoomAssignmentForm'

const RoomAssignmentsPage: React.FC = () => {
  const [searchRequest, setSearchRequest] = useState<RoomContextRequest>({
    roomNumber: '',
    startDate: new Date().toISOString().split('T')[0],
    endDate: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString().split('T')[0], // 1 week from now
    hotelId: undefined,
  })

  const [roomContext, setRoomContext] = useState<RoomContext | null>(null)
  const [openAssignmentForm, setOpenAssignmentForm] = useState(false)
  const [selectedGuestId, setSelectedGuestId] = useState<number | undefined>()

  // Fetch hotels for dropdown
  const { data: hotels } = useQuery({
    queryKey: ['hotels-dropdown'],
    queryFn: () => hotelService.getHotels(),
  })

  // Search room context
  const searchRoomContext = async () => {
    if (!searchRequest.roomNumber.trim()) {
      alert('Oda numarası zorunludur.')
      return
    }

    try {
      const result = await roomService.getRoomContext(searchRequest)
      setRoomContext(result)
    } catch (error) {
      console.error('Room context search failed:', error)
      alert('Oda arama sırasında hata oluştu.')
    }
  }

  const handleCreateAssignment = (guestId?: number) => {
    setSelectedGuestId(guestId)
    setOpenAssignmentForm(true)
  }

  const getStatusColor = (status: string): 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning' => {
    switch (status.toLowerCase()) {
      case 'completed':
      case 'paid':
        return 'success'
      case 'pending':
      case 'unpaid':
        return 'warning'
      case 'cancelled':
        return 'error'
      case 'inprogress':
        return 'primary'
      default:
        return 'default'
    }
  }

  return (
    <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={tr}>
      <Box>
        <Typography variant="h4" component="h1" gutterBottom>
          Oda Yönetimi
        </Typography>

        <Alert severity="info" sx={{ mb: 3 }}>
          Oda numarası, misafirin kaldığı odayı belirli bir tarih aralığında gösterir.
          Misafirler aynı odada farklı tarihlerde kalabilir.
        </Alert>

        {/* Search Form */}
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Oda Arama
            </Typography>
            <Grid container spacing={2} alignItems="center">
              <Grid item xs={12} md={3}>
                <TextField
                  fullWidth
                  label="Oda Numarası"
                  value={searchRequest.roomNumber}
                  onChange={(e) => setSearchRequest(prev => ({ ...prev, roomNumber: e.target.value }))}
                  placeholder="101, 205, VIP-1"
                  required
                />
              </Grid>
              <Grid item xs={12} md={2}>
                <DatePicker
                  label="Başlangıç Tarihi"
                  value={searchRequest.startDate ? new Date(searchRequest.startDate) : null}
                  onChange={(date) => setSearchRequest(prev => ({
                    ...prev,
                    startDate: date ? date.toISOString().split('T')[0] : ''
                  }))}
                  slotProps={{
                    textField: { fullWidth: true }
                  }}
                />
              </Grid>
              <Grid item xs={12} md={2}>
                <DatePicker
                  label="Bitiş Tarihi"
                  value={searchRequest.endDate ? new Date(searchRequest.endDate) : null}
                  onChange={(date) => setSearchRequest(prev => ({
                    ...prev,
                    endDate: date ? date.toISOString().split('T')[0] : ''
                  }))}
                  slotProps={{
                    textField: { fullWidth: true }
                  }}
                />
              </Grid>
              <Grid item xs={12} md={3}>
                <FormControl fullWidth>
                  <InputLabel>Otel (Opsiyonel)</InputLabel>
                  <Select
                    value={searchRequest.hotelId || ''}
                    onChange={(e) => setSearchRequest(prev => ({
                      ...prev,
                      hotelId: e.target.value ? parseInt(String(e.target.value)) : undefined
                    }))}
                    label="Otel (Opsiyonel)"
                  >
                    <MenuItem value="">Tümü</MenuItem>
                    {hotels?.data?.map((hotel: any) => (
                      <MenuItem key={hotel.id} value={String(hotel.id)}>
                        {hotel.hotelName}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} md={2}>
                <Button
                  fullWidth
                  variant="contained"
                  onClick={searchRoomContext}
                  disabled={!searchRequest.roomNumber.trim()}
                >
                  Ara
                </Button>
              </Grid>
            </Grid>
          </CardContent>
        </Card>

        {/* Results */}
        {roomContext && (
          <Box>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
              <Typography variant="h5">
                Oda {roomContext.roomNumber} - {roomContext.hotelName || 'Tüm Oteller'}
              </Typography>
              <Button
                variant="outlined"
                onClick={() => handleCreateAssignment()}
              >
                Oda Ataması Ekle
              </Button>
            </Box>

            {/* Guests */}
            <Accordion defaultExpanded>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <PersonIcon />
                  <Typography variant="h6">
                    Misafirler ({roomContext.guests.length})
                  </Typography>
                </Box>
              </AccordionSummary>
              <AccordionDetails>
                {roomContext.guests.length === 0 ? (
                  <Alert severity="info">Bu tarih aralığında odada misafir bulunamadı.</Alert>
                ) : (
                  <List>
                    {roomContext.guests.map((guest, index) => (
                      <ListItem key={index} divider>
                        <ListItemText
                          primary={`${guest.guestName} (${guest.guestCode})`}
                          secondary={
                            <Box>
                              <Typography variant="body2">
                                {format(new Date(guest.assignmentStart), 'dd/MM/yyyy', { locale: tr })} -
                                {guest.assignmentEnd
                                  ? format(new Date(guest.assignmentEnd), 'dd/MM/yyyy', { locale: tr })
                                  : 'Devam ediyor'
                                }
                              </Typography>
                              {guest.notes && (
                                <Typography variant="body2" color="text.secondary">
                                  {guest.notes}
                                </Typography>
                              )}
                            </Box>
                          }
                        />
                        <Button
                          size="small"
                          variant="outlined"
                          onClick={() => handleCreateAssignment(guest.guestId)}
                        >
                          Atama Ekle
                        </Button>
                      </ListItem>
                    ))}
                  </List>
                )}
              </AccordionDetails>
            </Accordion>

            {/* Services */}
            {[
              { title: 'Transferler', data: roomContext.transfers, icon: '🚗' },
              { title: 'Şehir Turları', data: roomContext.cityTours, icon: '🏛️' },
              { title: 'Yat Turları', data: roomContext.yachtTours, icon: '⛵' },
            ].map(({ title, data, icon }) => (
              <Accordion key={title}>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Typography variant="h6">
                    {icon} {title} ({data.length})
                  </Typography>
                </AccordionSummary>
                <AccordionDetails>
                  {data.length === 0 ? (
                    <Alert severity="info">Bu tarih aralığında oda ile ilişkili {title.toLowerCase()} bulunamadı.</Alert>
                  ) : (
                    <List>
                      {data.map((service, index) => (
                        <ListItem key={index} divider>
                          <ListItemText
                            primary={`${service.description} - ${service.guestName}`}
                            secondary={
                              <Box sx={{ display: 'flex', gap: 1, alignItems: 'center', mt: 1 }}>
                                <Typography variant="body2">
                                  {format(new Date(service.serviceDate), 'dd/MM/yyyy', { locale: tr })}
                                </Typography>
                                <Chip
                                  label={formatCurrency(service.amount, service.currency)}
                                  size="small"
                                  color="primary"
                                />
                                <Chip
                                  label={service.status}
                                  size="small"
                                  color={getStatusColor(service.status)}
                                />
                              </Box>
                            }
                          />
                        </ListItem>
                      ))}
                    </List>
                  )}
                </AccordionDetails>
              </Accordion>
            ))}

            {/* Financial Summary */}
            <Accordion>
              <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                <Typography variant="h6">
                  💰 Finansal Özet
                </Typography>
              </AccordionSummary>
              <AccordionDetails>
                <Grid container spacing={2}>
                  <Grid item xs={12} md={3}>
                    <Card>
                      <CardContent>
                        <Typography variant="h6" color="primary">
                          {roomContext.financialSummary.totalInvoices}
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          Fatura Sayısı
                        </Typography>
                      </CardContent>
                    </Card>
                  </Grid>
                  <Grid item xs={12} md={3}>
                    <Card>
                      <CardContent>
                        <Typography variant="h6" color="primary">
                          {formatCurrency(roomContext.financialSummary.totalInvoicedAmount, roomContext.financialSummary.currency)}
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          Faturalanan Tutar
                        </Typography>
                      </CardContent>
                    </Card>
                  </Grid>
                  <Grid item xs={12} md={3}>
                    <Card>
                      <CardContent>
                        <Typography variant="h6" color="success.main">
                          {roomContext.financialSummary.totalPayments}
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          Ödeme Sayısı
                        </Typography>
                      </CardContent>
                    </Card>
                  </Grid>
                  <Grid item xs={12} md={3}>
                    <Card>
                      <CardContent>
                        <Typography variant="h6" color="success.main">
                          {formatCurrency(roomContext.financialSummary.totalPaidAmount, roomContext.financialSummary.currency)}
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                          Ödenen Tutar
                        </Typography>
                      </CardContent>
                    </Card>
                  </Grid>
                </Grid>
              </AccordionDetails>
            </Accordion>
          </Box>
        )}

        {/* Room Assignment Form Dialog */}
        <RoomAssignmentForm
          open={openAssignmentForm}
          onClose={() => {
            setOpenAssignmentForm(false)
            setSelectedGuestId(undefined)
          }}
          onSubmit={async (data) => {
            // This would normally call the API
            console.log('Room assignment data:', data)
            alert('Oda ataması özelliği henüz tam olarak uygulanmadı.')
            setOpenAssignmentForm(false)
            setSelectedGuestId(undefined)
          }}
          guestId={selectedGuestId}
        />
      </Box>
    </LocalizationProvider>
  )
}

export default RoomAssignmentsPage

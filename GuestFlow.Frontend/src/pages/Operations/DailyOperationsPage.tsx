// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

import { useState } from 'react'
import {
  Box,
  Grid,
  Card,
  CardContent,
  Typography,
  Chip,
  Button,
  Tabs,
  Tab,
  Alert,
  IconButton,
  Tooltip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
} from '@mui/material'
import {
  Refresh as RefreshIcon,
  Warning as WarningIcon,
  CheckCircle as CheckCircleIcon,
  Schedule as ScheduleIcon,
  Person as PersonIcon,
  Payment as PaymentIcon,
  Assignment as AssignmentIcon,
} from '@mui/icons-material'
import { useDailyOperations } from '../../hooks/useDailyOperations'
import { useConfirmTransfer, useCancelTransfer, useAssignDriver, useRecordPayment } from '../../hooks/useOperations'
import { useSignalR } from '../../hooks/useSignalR'
import { formatCurrency } from '../../utils/formatters'
import { formatDate, formatDateTime } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'
import { RiskFlag, RiskFlagSeverity, ServiceOperation } from '../../types/dailyOperations'

const DailyOperationsPage = () => {
  const [selectedDate, setSelectedDate] = useState<string | undefined>(undefined)
  const [activeTab, setActiveTab] = useState(0)
  const { data, isLoading, error, refetch } = useDailyOperations(selectedDate)

  // Dialog states
  const [assignDriverDialog, setAssignDriverDialog] = useState({ open: false, serviceId: 0, serviceType: '' })
  const [paymentDialog, setPaymentDialog] = useState({ open: false, serviceId: 0, serviceType: '', guestId: 0, amount: 0, currency: 'TRY' })
  const [selectedPersonnelId, setSelectedPersonnelId] = useState(0)
  const [paymentAmount, setPaymentAmount] = useState('')
  const [paymentMethod, setPaymentMethod] = useState('Cash')

  // Hooks
  const confirmTransfer = useConfirmTransfer()
  const cancelTransfer = useCancelTransfer()
  const assignDriver = useAssignDriver()
  const recordPayment = useRecordPayment()

  // SignalR for real-time updates
  useSignalR({
    onDailyOperationsUpdate: (update) => {
      console.log('Daily operations update received:', update)
      // Refetch data when update is received
      refetch()
    },
    autoConnect: true,
  })

  const handleRefresh = () => {
    refetch()
  }

  const getSeverityColor = (severity: RiskFlagSeverity) => {
    switch (severity) {
      case RiskFlagSeverity.Critical:
        return 'error'
      case RiskFlagSeverity.High:
        return 'warning'
      case RiskFlagSeverity.Medium:
        return 'info'
      default:
        return 'default'
    }
  }

  const getServiceTypeColor = (type: string) => {
    switch (type) {
      case 'Transfer':
        return 'primary'
      case 'CityTour':
        return 'success'
      case 'YachtTour':
        return 'info'
      default:
        return 'default'
    }
  }

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={6} />
  }

  if (error || !data) {
    return (
      <ContentState
        state="error"
        title="Günlük operasyon verileri yüklenemedi"
        description="Lütfen daha sonra tekrar deneyin."
      />
    )
  }

  return (
    <Box sx={{ p: 3 }}>
      {/* Header */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box>
          <Typography variant="h4" gutterBottom>
            Günlük Operasyon Ekranı
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {formatDate(data.date)} - Bugünkü ve yaklaşan servisler, risk bayrakları
          </Typography>
        </Box>
        <Box>
          <Tooltip title="Yenile">
            <IconButton onClick={handleRefresh} color="primary">
              <RefreshIcon />
            </IconButton>
          </Tooltip>
        </Box>
      </Box>

      {/* Quick Stats */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid item xs={12} sm={6} md={2}>
          <Card>
            <CardContent>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Bugünkü Servisler
              </Typography>
              <Typography variant="h5">{data.quickStats.todayServiceCount}</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={2}>
          <Card>
            <CardContent>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Yaklaşan Servisler
              </Typography>
              <Typography variant="h5">{data.quickStats.upcomingServiceCount}</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={2}>
          <Card sx={{ bgcolor: 'warning.light' }}>
            <CardContent>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Acil Servisler
              </Typography>
              <Typography variant="h5" color="warning.dark">
                {data.quickStats.urgentServiceCount}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={2}>
          <Card sx={{ bgcolor: 'error.light' }}>
            <CardContent>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Atanmayan Şoför
              </Typography>
              <Typography variant="h5" color="error.dark">
                {data.quickStats.unassignedDriverCount}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={2}>
          <Card sx={{ bgcolor: 'info.light' }}>
            <CardContent>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Ödemesi Alınmamış
              </Typography>
              <Typography variant="h5" color="info.dark">
                {data.quickStats.unpaidServiceCount}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} sm={6} md={2}>
          <Card sx={{ bgcolor: 'error.light' }}>
            <CardContent>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Geciken Ödeme
              </Typography>
              <Typography variant="h5" color="error.dark">
                {data.quickStats.overduePaymentCount}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Risk Flags */}
      {data.riskFlags.length > 0 && (
        <Box sx={{ mb: 3 }}>
          <Typography variant="h6" gutterBottom>
            Risk Bayrakları
          </Typography>
          <Grid container spacing={2}>
            {data.riskFlags.map((flag, index) => (
              <Grid item xs={12} sm={6} md={4} key={index}>
                <Alert
                  severity={getSeverityColor(flag.severity) as any}
                  icon={<WarningIcon />}
                  action={
                    <Button size="small" color="inherit">
                      Detay
                    </Button>
                  }
                >
                  <Typography variant="subtitle2">{flag.title}</Typography>
                  <Typography variant="body2">{flag.description}</Typography>
                </Alert>
              </Grid>
            ))}
          </Grid>
        </Box>
      )}

      {/* Services Tabs */}
      <Card>
        <Box sx={{ borderBottom: 1, borderColor: 'divider' }}>
          <Tabs value={activeTab} onChange={(_, newValue) => setActiveTab(newValue)}>
            <Tab label={`Bugünkü Servisler (${data.todayServices.length})`} />
            <Tab label={`Yaklaşan Servisler (${data.upcomingServices.length})`} />
          </Tabs>
        </Box>
        <CardContent>
          {activeTab === 0 && (
            <ServiceList
              services={data.todayServices}
              getServiceTypeColor={getServiceTypeColor}
              onAssignDriver={(serviceId, serviceType) =>
                setAssignDriverDialog({ open: true, serviceId, serviceType })
              }
              onRecordPayment={(serviceId, serviceType, guestId, amount, currency) =>
                setPaymentDialog({ open: true, serviceId, serviceType, guestId, amount, currency })
              }
              onConfirmTransfer={(transferId) => confirmTransfer.mutate(transferId)}
              onCancelTransfer={(transferId) => cancelTransfer.mutate({ transferId })}
              confirmPending={confirmTransfer.isPending}
              cancelPending={cancelTransfer.isPending}
            />
          )}
          {activeTab === 1 && (
            <ServiceList
              services={data.upcomingServices}
              getServiceTypeColor={getServiceTypeColor}
              onAssignDriver={(serviceId, serviceType) =>
                setAssignDriverDialog({ open: true, serviceId, serviceType })
              }
              onRecordPayment={(serviceId, serviceType, guestId, amount, currency) =>
                setPaymentDialog({ open: true, serviceId, serviceType, guestId, amount, currency })
              }
              onConfirmTransfer={(transferId) => confirmTransfer.mutate(transferId)}
              onCancelTransfer={(transferId) => cancelTransfer.mutate({ transferId })}
              confirmPending={confirmTransfer.isPending}
              cancelPending={cancelTransfer.isPending}
            />
          )}
        </CardContent>
      </Card>

      {/* Assign Driver Dialog */}
      <Dialog
        open={assignDriverDialog.open}
        onClose={() => setAssignDriverDialog({ open: false, serviceId: 0, serviceType: '' })}
      >
        <DialogTitle>Şoför Ata</DialogTitle>
        <DialogContent>
          <TextField
            label="Personel ID"
            type="number"
            fullWidth
            margin="normal"
            value={selectedPersonnelId || ''}
            onChange={(e) => setSelectedPersonnelId(parseInt(e.target.value) || 0)}
          />
          <Typography variant="body2" color="text.secondary" sx={{ mt: 1 }}>
            Not: Personel listesi için Personnel sayfasına bakın
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setAssignDriverDialog({ open: false, serviceId: 0, serviceType: '' })}>
            İptal
          </Button>
          <Button
            onClick={() => {
              if (selectedPersonnelId > 0) {
                assignDriver.mutate({
                  transferId: assignDriverDialog.serviceId,
                  personnelId: selectedPersonnelId,
                })
                setAssignDriverDialog({ open: false, serviceId: 0, serviceType: '' })
                setSelectedPersonnelId(0)
              }
            }}
            variant="contained"
            disabled={selectedPersonnelId <= 0 || assignDriver.isPending}
          >
            Ata
          </Button>
        </DialogActions>
      </Dialog>

      {/* Payment Dialog */}
      <Dialog open={paymentDialog.open} onClose={() => setPaymentDialog({ open: false, serviceId: 0, serviceType: '', guestId: 0, amount: 0, currency: 'TRY' })}>
        <DialogTitle>Ödeme Kaydet</DialogTitle>
        <DialogContent>
          <TextField
            label="Tutar"
            type="number"
            fullWidth
            margin="normal"
            value={paymentAmount || paymentDialog.amount}
            onChange={(e) => setPaymentAmount(e.target.value)}
          />
          <FormControl fullWidth margin="normal">
            <InputLabel>Ödeme Yöntemi</InputLabel>
            <Select value={paymentMethod} onChange={(e) => setPaymentMethod(e.target.value)}>
              <MenuItem value="Cash">Nakit</MenuItem>
              <MenuItem value="CreditCard">Kredi Kartı</MenuItem>
              <MenuItem value="BankTransfer">Banka Transferi</MenuItem>
              <MenuItem value="RoomCharge">Oda Hesabı</MenuItem>
              <MenuItem value="Other">Diğer</MenuItem>
            </Select>
          </FormControl>
        </DialogContent>
        <DialogActions>
          <Button
            onClick={() => setPaymentDialog({ open: false, serviceId: 0, serviceType: '', guestId: 0, amount: 0, currency: 'TRY' })}
          >
            İptal
          </Button>
          <Button
            onClick={() => {
              const amount = parseFloat(paymentAmount || paymentDialog.amount.toString())
              if (amount > 0) {
                recordPayment.mutate({
                  serviceType: paymentDialog.serviceType as 'Transfer' | 'CityTour' | 'YachtTour',
                  serviceId: paymentDialog.serviceId,
                  request: {
                    guestId: paymentDialog.guestId,
                    amount,
                    currency: paymentDialog.currency,
                    paymentMethod: paymentMethod,
                  },
                })
                setPaymentDialog({ open: false, serviceId: 0, serviceType: '', guestId: 0, amount: 0, currency: 'TRY' })
                setPaymentAmount('')
                setPaymentMethod('Cash')
              }
            }}
            variant="contained"
            disabled={!paymentAmount || parseFloat(paymentAmount) <= 0 || recordPayment.isPending}
          >
            Kaydet
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

interface ServiceListProps {
  services: ServiceOperation[]
  getServiceTypeColor: (type: string) => 'primary' | 'success' | 'info' | 'default'
  onAssignDriver: (serviceId: number, serviceType: string) => void
  onRecordPayment: (serviceId: number, serviceType: string, guestId: number, amount: number, currency: string) => void
  onConfirmTransfer: (transferId: number) => void
  onCancelTransfer: (transferId: number) => void
  confirmPending: boolean
  cancelPending: boolean
}

const ServiceList = ({
  services,
  getServiceTypeColor,
  onAssignDriver,
  onRecordPayment,
  onConfirmTransfer,
  onCancelTransfer,
  confirmPending,
  cancelPending,
}: ServiceListProps) => {
  if (services.length === 0) {
    return (
      <Box sx={{ textAlign: 'center', py: 4 }}>
        <Typography variant="body1" color="text.secondary">
          Servis bulunamadı
        </Typography>
      </Box>
    )
  }

  return (
    <Grid container spacing={2}>
      {services.map((service) => (
        <Grid item xs={12} md={6} key={`${service.serviceType}-${service.serviceId}`}>
          <Card
            sx={{
              borderLeft: service.isUrgent ? 4 : 1,
              borderLeftColor: service.isUrgent ? 'error.main' : 'divider',
            }}
          >
            <CardContent>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'start', mb: 1 }}>
                <Box>
                  <Chip
                    label={service.serviceType}
                    color={getServiceTypeColor(service.serviceType)}
                    size="small"
                    sx={{ mb: 1 }}
                  />
                  {service.isUrgent && (
                    <Chip label="Acil" color="error" size="small" sx={{ ml: 1 }} />
                  )}
                  {service.isPaid && (
                    <Chip
                      icon={<CheckCircleIcon />}
                      label="Ödendi"
                      color="success"
                      size="small"
                      sx={{ ml: 1 }}
                    />
                  )}
                </Box>
                <Typography variant="h6" color="primary">
                  {formatCurrency(service.amount, service.currency)}
                </Typography>
              </Box>

              <Typography variant="subtitle1" gutterBottom>
                {service.guestName}
              </Typography>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Oda: {service.roomNumber} | Misafir Kodu: {service.guestCode}
              </Typography>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                <ScheduleIcon sx={{ fontSize: 16, verticalAlign: 'middle', mr: 0.5 }} />
                {formatDateTime(service.serviceTime)}
              </Typography>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                {service.location}
              </Typography>
              {service.assignedPersonnelName && (
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  <PersonIcon sx={{ fontSize: 16, verticalAlign: 'middle', mr: 0.5 }} />
                  {service.assignedPersonnelName}
                </Typography>
              )}
              {!service.assignedPersonnelName && service.serviceType === 'Transfer' && (
                <Alert severity="warning" sx={{ mt: 1 }}>
                  Şoför atanmamış
                </Alert>
              )}
              {service.notes && (
                <Typography variant="body2" color="text.secondary" sx={{ mt: 1, fontStyle: 'italic' }}>
                  Not: {service.notes}
                </Typography>
              )}

              <Box sx={{ display: 'flex', gap: 1, mt: 2, flexWrap: 'wrap' }}>
                <Button
                  size="small"
                  variant="outlined"
                  startIcon={<AssignmentIcon />}
                  onClick={() => {
                    const path =
                      service.serviceType === 'Transfer'
                        ? `/transfers/${service.serviceId}`
                        : service.serviceType === 'CityTour'
                          ? `/tours/city/${service.serviceId}`
                          : `/tours/yacht/${service.serviceId}`
                    window.location.href = path
                  }}
                >
                  Detay
                </Button>
                {service.serviceType === 'Transfer' && !service.assignedPersonnelName && (
                  <Button
                    size="small"
                    variant="contained"
                    color="primary"
                    startIcon={<PersonIcon />}
                    onClick={() => onAssignDriver(service.serviceId, service.serviceType)}
                  >
                    Şoför Ata
                  </Button>
                )}
                {service.serviceType === 'Transfer' && service.status !== 'Confirmed' && service.status !== 'Completed' && (
                  <Button
                    size="small"
                    variant="contained"
                    color="info"
                    onClick={() => onConfirmTransfer(service.serviceId)}
                    disabled={confirmPending}
                  >
                    Onayla
                  </Button>
                )}
                {service.serviceType === 'Transfer' && service.status !== 'Cancelled' && service.status !== 'Completed' && (
                  <Button
                    size="small"
                    variant="outlined"
                    color="error"
                    onClick={() => onCancelTransfer(service.serviceId)}
                    disabled={cancelPending}
                  >
                    İptal
                  </Button>
                )}
                {!service.isPaid && (
                  <Button
                    size="small"
                    variant="contained"
                    color="success"
                    startIcon={<PaymentIcon />}
                    onClick={() => onRecordPayment(service.serviceId, service.serviceType, service.guestId, service.amount, service.currency)}
                  >
                    Ödeme Al
                  </Button>
                )}
              </Box>
            </CardContent>
          </Card>
        </Grid>
      ))}
    </Grid>
  )
}

export default DailyOperationsPage

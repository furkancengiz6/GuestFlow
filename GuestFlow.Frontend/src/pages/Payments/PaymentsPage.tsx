import React, { useState } from 'react'
import {
  Box,
  Typography,
  Button,
  Card,
  CardContent,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  Alert,
  Grid,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Skeleton,
} from '@mui/material'
import { useLiveUpdates } from '../../hooks/useLiveUpdates'
import { Add as AddIcon } from '@mui/icons-material'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns'
import { format } from 'date-fns'
import { tr } from 'date-fns/locale'

import { paymentService, CreatePaymentRequest } from '../../services/paymentService'
import { guestService } from '../../services/guestService'
import { PaymentMethod, PaymentStatus, PaymentStatusLabels } from '../../types/enums'
import { formatCurrency } from '../../utils/formatters'
import PaymentForm from '../../components/Payments/PaymentForm'
import ContentState from '../../components/Feedback/ContentState'

const PaymentsPage: React.FC = () => {
  // Enable real-time updates for payment changes
  useLiveUpdates(['payment'])

  const queryClient = useQueryClient()
  const [openForm, setOpenForm] = useState(false)
  const [selectedService, setSelectedService] = useState<{
    serviceType: 'transfer' | 'citytour' | 'yachttour'
    serviceId: number
    guestId: number
    amount: number
    currency: string
  } | undefined>()

  // Filters
  const [filters, setFilters] = useState({
    guestId: '',
    startDate: null as Date | null,
    endDate: null as Date | null,
  })

  // Fetch payments with filters
  const {
    data: paymentsData,
    isLoading,
    error,
  } = useQuery({
    queryKey: ['payments', filters],
    queryFn: () => paymentService.getPayments({
      guestId: filters.guestId ? parseInt(filters.guestId) : undefined,
      startDate: filters.startDate?.toISOString().split('T')[0],
      endDate: filters.endDate?.toISOString().split('T')[0],
    }),
  })

  const payments = Array.isArray(paymentsData) ? paymentsData : (paymentsData as any)?.data || []

  // Fetch dropdown data
  const { data: guests = [] } = useQuery({
    queryKey: ['guests-dropdown'],
    queryFn: () => guestService.getGuests(),
  })

  const guestList = Array.isArray(guests) ? guests : guests.data || []

  // Create payment mutation
  const createPaymentMutation = useMutation({
    mutationFn: (data: CreatePaymentRequest) => paymentService.createPayment(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['payments'] })
      queryClient.invalidateQueries({ queryKey: ['transfers'] })
      queryClient.invalidateQueries({ queryKey: ['citytours'] })
      queryClient.invalidateQueries({ queryKey: ['yachttours'] })
      setOpenForm(false)
      setSelectedService(undefined)
    },
    onError: (error) => {
      console.error('Payment creation failed:', error)
      alert('Ödeme oluşturulurken hata oluştu.')
    },
  })

  const handleCreatePayment = async (data: any) => {
    const createData: CreatePaymentRequest = {
      ...data,
      paymentDate: data.paymentDate.toISOString()
    }
    createPaymentMutation.mutate(createData)
  }

  const getPaymentMethodLabel = (method: PaymentMethod): string => {
    switch (method) {
      case PaymentMethod.Cash:
        return 'Nakit'
      case PaymentMethod.CreditCard:
        return 'Kredi Kartı'
      case PaymentMethod.RoomCharge:
        return 'Odaya Charge'
      case PaymentMethod.BankTransfer:
        return 'Banka Havalesi'
      case PaymentMethod.Other:
        return 'Diğer'
      default:
        return method
    }
  }

  const getPaymentMethodColor = (method: PaymentMethod): 'primary' | 'secondary' | 'success' | 'warning' | 'error' => {
    switch (method) {
      case PaymentMethod.Cash:
        return 'success'
      case PaymentMethod.CreditCard:
        return 'primary'
      case PaymentMethod.RoomCharge:
        return 'warning'
      case PaymentMethod.BankTransfer:
        return 'secondary'
      case PaymentMethod.Other:
        return 'error'
      default:
        return 'primary'
    }
  }

  const getPaymentStatusColor = (status: PaymentStatus): 'primary' | 'secondary' | 'success' | 'warning' | 'error' | 'info' => {
    switch (status) {
      case PaymentStatus.Pending:
        return 'warning'
      case PaymentStatus.Completed:
        return 'success'
      case PaymentStatus.Failed:
        return 'error'
      case PaymentStatus.Refunded:
        return 'info'
      case PaymentStatus.Cancelled:
        return 'secondary'
      default:
        return 'primary'
    }
  }

  if (error) {
    return (
      <ContentState
        state="error"
        title="Ödemeler yüklenemedi"
        description="Lütfen daha sonra tekrar deneyin."
      />
    )
  }

  return (
    <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={tr}>
      <Box className="fade-in" p={0}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Typography variant="h4" component="h1" className="premium-gradient-text" sx={{ fontWeight: 800 }}>
            Ödeme Yönetimi
          </Typography>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => setOpenForm(true)}
            className="premium-gradient"
            sx={{ borderRadius: 2, boxShadow: '0 4px 14px 0 rgba(0,118,255,0.39)' }}
          >
            Yeni Ödeme
          </Button>
        </Box>

        {/* Filters */}
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Typography variant="h6" gutterBottom>
              Filtreler
            </Typography>
            <Grid container spacing={2}>
              <Grid item xs={12} md={4}>
                <FormControl fullWidth>
                  <InputLabel>Misafir</InputLabel>
                  <Select
                    value={filters.guestId}
                    onChange={(e) => setFilters(prev => ({ ...prev, guestId: e.target.value }))}
                    label="Misafir"
                  >
                    <MenuItem value="">Tümü</MenuItem>
                    {guestList.map((guest: any) => (
                      <MenuItem key={guest.id} value={guest.id.toString()}>
                        {guest.fullName} ({guest.guestCode})
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} md={4}>
                <DatePicker
                  label="Başlangıç Tarihi"
                  value={filters.startDate}
                  onChange={(date) => setFilters(prev => ({ ...prev, startDate: date }))}
                  slotProps={{
                    textField: { fullWidth: true }
                  }}
                />
              </Grid>
              <Grid item xs={12} md={4}>
                <DatePicker
                  label="Bitiş Tarihi"
                  value={filters.endDate}
                  onChange={(date) => setFilters(prev => ({ ...prev, endDate: date }))}
                  slotProps={{
                    textField: { fullWidth: true }
                  }}
                />
              </Grid>
            </Grid>
          </CardContent>
        </Card>

        {/* Payments Table */}
        <Card className="glass-panel">
          <CardContent>
            <Typography variant="h6" gutterBottom sx={{ fontWeight: 700 }}>
              Ödeme Listesi
            </Typography>

            {isLoading ? (
              <Box>
                {[...Array(5)].map((_, index) => (
                  <Skeleton key={index} height={60} sx={{ mb: 1 }} />
                ))}
              </Box>
            ) : payments.length === 0 ? (
              <Alert severity="info">
                Seçilen filtrelere göre ödeme bulunamadı.
              </Alert>
            ) : (
              <TableContainer component={Paper}>
                <Table>
                  <TableHead>
                    <TableRow>
                      <TableCell>Tarih</TableCell>
                      <TableCell>Misafir</TableCell>
                      <TableCell>Tutar</TableCell>
                      <TableCell>Yöntem</TableCell>
                      <TableCell>Durum</TableCell>
                      <TableCell>İşlem ID</TableCell>
                      <TableCell>Personel</TableCell>
                      <TableCell>İlişkili Hizmet</TableCell>
                      <TableCell>Notlar</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {payments.map((payment) => (
                      <TableRow key={payment.id}>
                        <TableCell>
                          {format(new Date(payment.paymentDate), 'dd/MM/yyyy HH:mm', { locale: tr })}
                        </TableCell>
                        <TableCell>
                          {guestList.find((g: any) => g.id === payment.guestId)?.fullName || `Misafir ${payment.guestId}`}
                        </TableCell>
                        <TableCell>
                          {formatCurrency(payment.amount, payment.currency)}
                        </TableCell>
                        <TableCell>
                          <Chip
                            label={getPaymentMethodLabel(payment.paymentMethod)}
                            color={getPaymentMethodColor(payment.paymentMethod)}
                            size="small"
                          />
                        </TableCell>
                        <TableCell>
                          <Chip
                            label={PaymentStatusLabels[payment.status]}
                            color={getPaymentStatusColor(payment.status)}
                            size="small"
                            variant="outlined"
                          />
                        </TableCell>
                        <TableCell>
                          <Typography variant="caption" sx={{ fontFamily: 'monospace' }}>
                            {payment.transactionId || '-'}
                          </Typography>
                        </TableCell>
                        <TableCell>
                          {payment.collectedByPersonnelName}
                        </TableCell>
                        <TableCell>
                          {payment.transferId && `Transfer #${payment.transferId}`}
                          {payment.cityTourId && `Şehir Turu #${payment.cityTourId}`}
                          {payment.yachtTourId && `Yat Turu #${payment.yachtTourId}`}
                          {!payment.transferId && !payment.cityTourId && !payment.yachtTourId && 'Genel Ödeme'}
                        </TableCell>
                        <TableCell>
                          {payment.notes || '-'}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            )}
          </CardContent>
        </Card>

        {/* Payment Form Dialog */}
        <PaymentForm
          open={openForm}
          onClose={() => {
            setOpenForm(false)
            setSelectedService(undefined)
          }}
          onSubmit={handleCreatePayment}
          preselectedService={selectedService}
          isLoading={createPaymentMutation.isPending}
        />
      </Box>
    </LocalizationProvider>
  )
}

export default PaymentsPage

import { useParams, useNavigate } from 'react-router-dom'
import {
  Box,
  Card,
  CardContent,
  Typography,
  Grid,
  Chip,
  Button,
  Divider,
  IconButton,
  Tooltip,
} from '@mui/material'
import {
  ArrowBack as ArrowBackIcon,
  Person as PersonIcon,
  Receipt as ReceiptIcon,
  AttachMoney as AttachMoneyIcon,
  Download as DownloadIcon,
  CalendarMonth as CalendarMonthIcon,
} from '@mui/icons-material'
import { useState } from 'react'
import { useQuery, useMutation } from '@tanstack/react-query'
import { invoiceService } from '../../services/invoiceService'
import { formatDate, formatCurrency } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'
import PrintButton from '../../components/Common/PrintButton'
import PDFViewer from '../../components/Common/PDFViewer'
import { useNotification } from '../../hooks/useNotification'
import PictureAsPdfIcon from '@mui/icons-material/PictureAsPdf'
import api from '../../services/api'

const InvoiceDetailPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const notification = useNotification()
  const invoiceId = id ? parseInt(id, 10) : 0
  const [pdfViewerOpen, setPdfViewerOpen] = useState(false)

  const { data: invoice, isLoading, error } = useQuery({
    queryKey: ['invoice-detail', invoiceId],
    queryFn: () => invoiceService.getInvoiceDetail(invoiceId),
    enabled: !!invoiceId && !isNaN(invoiceId),
  })

  const generatePdfMutation = useMutation({
    mutationFn: async () => {
      const response = await api.post(`/invoices/${invoiceId}/generate-pdf`)
      return response.data.data.pdfUrl
    },
    onSuccess: (pdfUrl) => {
      notification.showSuccess('PDF başarıyla oluşturuldu.')
      // Refresh invoice data to get updated pdfUrl
      window.location.reload()
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'PDF oluşturulurken bir hata oluştu.')
    },
  })

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={8} />
  }

  if (error || !invoice) {
    return (
      <ContentState
        state="error"
        title="Fatura detayı yüklenemedi"
        description="Lütfen daha sonra tekrar deneyin."
        actionLabel="Geri dön"
        onAction={() => navigate('/invoices')}
      />
    )
  }

  const getServiceTypeLabel = (type: string) => {
    switch (type.toLowerCase()) {
      case 'transfer':
        return 'Transfer'
      case 'citytour':
        return 'Şehir Turu'
      case 'yachttour':
        return 'Yat Turu'
      default:
        return type
    }
  }

  const getServiceTypeColor = (type: string) => {
    switch (type.toLowerCase()) {
      case 'transfer':
        return 'primary'
      case 'citytour':
        return 'secondary'
      case 'yachttour':
        return 'info'
      default:
        return 'default'
    }
  }

  return (
    <Box p={3}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <IconButton onClick={() => navigate('/invoices')} color="primary">
            <ArrowBackIcon />
          </IconButton>
          <Typography variant="h4" sx={{ fontWeight: 600 }}>
            Fatura Detayı
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <PrintButton />
          {invoice.hasPdf && invoice.pdfUrl && (
            <Button
              variant="contained"
              startIcon={<PictureAsPdfIcon />}
              onClick={() => setPdfViewerOpen(true)}
            >
              PDF Görüntüle
            </Button>
          )}
          {!invoice.hasPdf && (
            <Button
              variant="outlined"
              startIcon={<PictureAsPdfIcon />}
              onClick={() => generatePdfMutation.mutate()}
              disabled={generatePdfMutation.isPending}
            >
              PDF Oluştur
            </Button>
          )}
        </Box>
      </Box>

      {/* Fatura Bilgileri */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 2 }}>
            <Box>
              <Typography variant="h5" gutterBottom>
                Fatura #{invoice.invoiceNumber}
              </Typography>
              {invoice.service && (
                <Chip
                  label={getServiceTypeLabel(invoice.service.serviceType)}
                  color={getServiceTypeColor(invoice.service.serviceType) as any}
                  size="small"
                  sx={{ mt: 1 }}
                />
              )}
            </Box>
            <Box sx={{ textAlign: 'right' }}>
              <Typography variant="body2" color="text.secondary">
                Oluşturulma Tarihi
              </Typography>
              <Typography variant="body1">{formatDate(invoice.createdDate)}</Typography>
            </Box>
          </Box>
          <Divider sx={{ my: 2 }} />
          <Grid container spacing={3}>
            <Grid item xs={12} md={6}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <CalendarMonthIcon color="action" />
                <Typography variant="body2" color="text.secondary">
                  Fatura Tarihi
                </Typography>
              </Box>
              <Typography variant="body1" fontWeight="medium">
                {formatDate(invoice.issueDate)}
              </Typography>
            </Grid>
            <Grid item xs={12} md={6}>
              <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 1 }}>
                <AttachMoneyIcon color="action" />
                <Typography variant="body2" color="text.secondary">
                  Toplam Tutar
                </Typography>
              </Box>
              <Typography variant="h6" fontWeight="medium" color="primary">
                {formatCurrency(invoice.totalAmount, invoice.currency)}
              </Typography>
            </Grid>
            {invoice.service && (
              <>
                {invoice.service.serviceDate && (
                  <Grid item xs={12} md={6}>
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                      Hizmet Tarihi
                    </Typography>
                    <Typography variant="body1">{formatDate(invoice.service.serviceDate)}</Typography>
                  </Grid>
                )}
                {invoice.service.serviceAmount && (
                  <Grid item xs={12} md={6}>
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                      Hizmet Tutarı
                    </Typography>
                    <Typography variant="body1">{formatCurrency(invoice.service.serviceAmount, invoice.currency)}</Typography>
                  </Grid>
                )}
                {invoice.service.serviceName && (
                  <Grid item xs={12}>
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                      Hizmet Adı
                    </Typography>
                    <Typography variant="body1">{invoice.service.serviceName}</Typography>
                  </Grid>
                )}
                {invoice.service.additionalInfo && (
                  <Grid item xs={12}>
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                      Ek Bilgi
                    </Typography>
                    <Typography variant="body1">{invoice.service.additionalInfo}</Typography>
                  </Grid>
                )}
              </>
            )}
            {invoice.notes && (
              <Grid item xs={12}>
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  Notlar
                </Typography>
                <Typography variant="body1">{invoice.notes}</Typography>
              </Grid>
            )}
          </Grid>
        </CardContent>
      </Card>

      <Grid container spacing={3}>
        {/* Misafir Bilgileri */}
        {invoice.guest && (
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                  <PersonIcon color="primary" />
                  <Typography variant="h6">Misafir</Typography>
                </Box>
                <Divider sx={{ mb: 2 }} />
                <Typography variant="body2" color="text.secondary">
                  Ad Soyad
                </Typography>
                <Typography variant="body1" fontWeight="medium" gutterBottom>
                  {invoice.guest.fullName}
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Misafir Kodu
                </Typography>
                <Typography variant="body1" gutterBottom>
                  {invoice.guest.guestCode}
                </Typography>
                {invoice.guest.email && (
                  <>
                    <Typography variant="body2" color="text.secondary">
                      E-posta
                    </Typography>
                    <Typography variant="body1" gutterBottom>
                      {invoice.guest.email}
                    </Typography>
                  </>
                )}
                {invoice.guest.phoneNumber && (
                  <>
                    <Typography variant="body2" color="text.secondary">
                      Telefon
                    </Typography>
                    <Typography variant="body1" gutterBottom>
                      {invoice.guest.phoneNumber}
                    </Typography>
                  </>
                )}
                <Typography variant="body2" color="text.secondary">
                  Uyruk
                </Typography>
                <Typography variant="body1" gutterBottom>
                  {invoice.guest.nationality}
                </Typography>
                {invoice.guest.isSpecialGuest && (
                  <Chip label="Özel Misafir" color="primary" size="small" sx={{ mt: 1 }} />
                )}
                <Button
                  variant="outlined"
                  size="small"
                  sx={{ mt: 2 }}
                  onClick={() => navigate(`/guests/${invoice.guest!.id}`)}
                >
                  Misafir Detayı
                </Button>
              </CardContent>
            </Card>
          </Grid>
        )}

        {/* Personel Bilgileri */}
        {invoice.personnel && (
          <Grid item xs={12} md={6}>
            <Card>
              <CardContent>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                  <PersonIcon color="primary" />
                  <Typography variant="h6">Personel</Typography>
                </Box>
                <Divider sx={{ mb: 2 }} />
                <Typography variant="body2" color="text.secondary">
                  Ad Soyad
                </Typography>
                <Typography variant="body1" fontWeight="medium" gutterBottom>
                  {invoice.personnel.fullName}
                </Typography>
                {invoice.personnel.email && (
                  <>
                    <Typography variant="body2" color="text.secondary">
                      E-posta
                    </Typography>
                    <Typography variant="body1" gutterBottom>
                      {invoice.personnel.email}
                    </Typography>
                  </>
                )}
                <Typography variant="body2" color="text.secondary">
                  Rol
                </Typography>
                <Typography variant="body1">{invoice.personnel.userType}</Typography>
              </CardContent>
            </Card>
          </Grid>
        )}
      </Grid>

      {/* PDF Viewer Dialog */}
      {pdfViewerOpen && invoice.pdfUrl && (
        <PDFViewer
          url={invoice.pdfUrl}
          fileName={`fatura_${invoice.invoiceNumber}.pdf`}
          onClose={() => setPdfViewerOpen(false)}
          fullScreen={true}
        />
      )}
    </Box>
  )
}

export default InvoiceDetailPage


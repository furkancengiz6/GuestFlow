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
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Table,
  TableHead,
  TableRow,
  TableCell,
  TableBody,
  CircularProgress,
} from '@mui/material'
import {
  ArrowBack as ArrowBackIcon,
  Person as PersonIcon,
  AttachMoney as AttachMoneyIcon,
  CalendarMonth as CalendarMonthIcon,
  Email as EmailIcon,
} from '@mui/icons-material'
import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { invoiceService } from '../../services/invoiceService'
import { journalService, JournalPreviewResponse } from '../../services/journalService'
import { formatDate, formatCurrency } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'
import PrintButton from '../../components/Common/PrintButton'
import PDFViewer from '../../components/Common/PDFViewer'
import { useNotification } from '../../hooks/useNotification'
import { useLiveUpdates } from '../../hooks/useLiveUpdates'
import PictureAsPdfIcon from '@mui/icons-material/PictureAsPdf'
import api from '../../services/api'
import { useAuthStore } from '../../stores/authStore'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns'
import { tr } from 'date-fns/locale'

const InvoiceDetailPage = () => {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()
  const notification = useNotification()
  const queryClient = useQueryClient()
  const user = useAuthStore((s) => s.user)
  const parsedId = id ? parseInt(id, 10) : NaN
  const invoiceId = !isNaN(parsedId) && parsedId > 0 ? parsedId : null
  const [pdfViewerOpen, setPdfViewerOpen] = useState(false)
  const [journalPreviewOpen, setJournalPreviewOpen] = useState(false)
  const [journalPreview, setJournalPreview] = useState<JournalPreviewResponse | null>(null)
  const [journalPostingDate, setJournalPostingDate] = useState<Date | null>(new Date())
  const [journalPosted, setJournalPosted] = useState(false)

  // Enable real-time updates for invoice changes
  useLiveUpdates(['invoice'])

  const { data: invoice, isLoading, error } = useQuery({
    queryKey: ['invoice-detail', invoiceId],
    queryFn: () => {
      if (!invoiceId) throw new Error('Geçersiz fatura ID')
      return invoiceService.getInvoiceDetail(invoiceId)
    },
    enabled: invoiceId !== null && invoiceId > 0,
  })

  const generatePdfMutation = useMutation({
    mutationFn: () => {
      if (!invoiceId) throw new Error('Geçersiz fatura ID')
      return invoiceService.generateInvoicePdf(invoiceId)
    },
    onSuccess: () => {
      notification.showSuccess('PDF başarıyla oluşturuldu.')
      // Refresh invoice data to get updated pdfUrl
      queryClient.refetchQueries({ queryKey: ['invoice-detail', invoiceId] })
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'PDF oluşturulurken bir hata oluştu.')
    },
  })

  const sendEmailMutation = useMutation({
    mutationFn: () => {
      if (!invoiceId) throw new Error('Geçersiz fatura ID')
      return invoiceService.sendInvoiceEmail(invoiceId)
    },
    onSuccess: () => {
      notification.showSuccess('Fatura e-postası gönderildi.')
    },
    onError: (error: any) => {
      notification.showError(`E-posta gönderme hatası: ${error.response?.data?.message || 'Bilinmeyen hata'}`)
    }
  })

  const cancelInvoiceMutation = useMutation({
    mutationFn: () => {
      if (!invoiceId) throw new Error('Geçersiz fatura ID')
      return invoiceService.cancelInvoice(invoiceId)
    },
    onSuccess: () => {
      notification.showSuccess('Fatura iptal edildi.')
      queryClient.refetchQueries({ queryKey: ['invoice-detail', invoiceId] })
      queryClient.invalidateQueries({ queryKey: ['invoices'] })
    },
    onError: (error: any) => {
      notification.showError(`Fatura iptal hatası: ${error.response?.data?.message || 'Bilinmeyen hata'}`)
    }
  })

  const journalPreviewMutation = useMutation({
    mutationFn: async () => {
      if (!invoiceId) throw new Error('Geçersiz fatura ID')
      return journalService.preview(invoiceId)
    },
    onSuccess: (data) => {
      setJournalPreview(data)
      setJournalPosted(false)
      setJournalPostingDate(new Date())
      setJournalPreviewOpen(true)
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Muhasebe önizlemesi alınamadı.')
    },
  })

  const journalPostMutation = useMutation({
    mutationFn: async () => {
      if (!invoiceId) throw new Error('Geçersiz fatura ID')
      if (!journalPreview) throw new Error('Önizleme bulunamadı')
      if (!journalPostingDate) throw new Error('Posting date seçiniz')

      const postingDate = journalPostingDate.toISOString().split('T')[0]
      return journalService.post({
        invoiceId,
        postingDate,
        lines: journalPreview.lines,
      })
    },
    onSuccess: () => {
      setJournalPosted(true)
      notification.showSuccess('Journal başarıyla post edildi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Journal post edilemedi.')
    },
  })

  if (!invoiceId) {
    return (
      <ContentState
        state="error"
        title="Geçersiz fatura ID"
        description="Fatura ID'si geçersiz veya eksik."
        actionLabel="Geri dön"
        onAction={() => navigate('/invoices')}
      />
    )
  }

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
          <Button
            variant="outlined"
            color="info"
            startIcon={<EmailIcon />}
            onClick={() => sendEmailMutation.mutate()}
            disabled={sendEmailMutation.isPending}
          >
            {sendEmailMutation.isPending ? 'Gönderiliyor...' : 'E-posta Gönder'}
          </Button>
          {(user?.role === 'Admin' || user?.role === 'Staff') && (
            <Button
              variant="outlined"
              onClick={() => journalPreviewMutation.mutate()}
              disabled={journalPreviewMutation.isPending}
            >
              {journalPreviewMutation.isPending ? 'Önizleniyor...' : 'Journal Preview'}
            </Button>
          )}
          {(invoice as any).status === 'Draft' && (
            <Button
              variant="outlined"
              color="error"
              onClick={() => {
                if (window.confirm('Bu faturayı iptal etmek istediğinizden emin misiniz?')) {
                  cancelInvoiceMutation.mutate()
                }
              }}
              disabled={cancelInvoiceMutation.isPending}
            >
              {cancelInvoiceMutation.isPending ? 'İptal Ediliyor...' : 'İptal Et'}
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
            {invoice.paymentStatus && (
              <Grid item xs={12}>
                <Typography variant="body2" color="text.secondary" gutterBottom>
                  Ödeme Durumu
                </Typography>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
                  <Chip
                    label={invoice.paymentStatus === 'Paid' ? 'Ödendi' :
                           invoice.paymentStatus === 'PartiallyPaid' ? 'Kısmi Ödeme' : 'Ödenmedi'}
                    color={invoice.paymentStatus === 'Paid' ? 'success' :
                           invoice.paymentStatus === 'PartiallyPaid' ? 'warning' : 'error'}
                    size="small"
                  />
                  {invoice.paidAmount !== undefined && invoice.remainingAmount !== undefined && (
                    <Box sx={{ display: 'flex', gap: 2, ml: 2 }}>
                      <Typography variant="body2" color="text.secondary">
                        Ödenen: <span style={{ fontWeight: 'bold', color: '#2e7d32' }}>{formatCurrency(invoice.paidAmount, invoice.currency)}</span>
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Kalan: <span style={{ fontWeight: 'bold', color: '#d32f2f' }}>{formatCurrency(invoice.remainingAmount, invoice.currency)}</span>
                      </Typography>
                    </Box>
                  )}
                </Box>
                {invoice.paidAmountByCurrency && Object.keys(invoice.paidAmountByCurrency).length > 0 && (
                  <Box sx={{ mt: 1 }}>
                    <Typography variant="body2" color="text.secondary" gutterBottom>
                      Para Birimine Göre Ödemeler:
                    </Typography>
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                      {Object.entries(invoice.paidAmountByCurrency).map(([currency, amount]) => (
                        <Chip
                          key={currency}
                          label={`${currency}: ${formatCurrency(amount)}`}
                          variant="outlined"
                          size="small"
                        />
                      ))}
                    </Box>
                  </Box>
                )}
              </Grid>
            )}
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

      <Dialog
        open={journalPreviewOpen}
        onClose={() => setJournalPreviewOpen(false)}
        fullWidth
        maxWidth="md"
      >
        <DialogTitle>Journal Preview</DialogTitle>
        <DialogContent>
          {!journalPreview && (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
              <CircularProgress />
            </Box>
          )}

          {journalPreview && (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <Typography variant="subtitle1">{journalPreview.description}</Typography>
              <Typography variant="body2" color="text.secondary">
                Toplam Borç: {formatCurrency(journalPreview.totalDebit, journalPreview.currency)} — Toplam Alacak:{' '}
                {formatCurrency(journalPreview.totalCredit, journalPreview.currency)}
              </Typography>

              <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={tr}>
                <DatePicker
                  label="Posting Date"
                  value={journalPostingDate}
                  onChange={(newValue) => setJournalPostingDate(newValue)}
                  slotProps={{
                    textField: { size: 'small', fullWidth: true },
                  }}
                />
              </LocalizationProvider>

              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Hesap</TableCell>
                    <TableCell>Açıklama</TableCell>
                    <TableCell align="right">Borç</TableCell>
                    <TableCell align="right">Alacak</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {journalPreview.lines.map((l, idx) => (
                    <TableRow key={idx}>
                      <TableCell>{l.accountCode}</TableCell>
                      <TableCell>{l.description || '-'}</TableCell>
                      <TableCell align="right">
                        {l.debit ? formatCurrency(l.debit, journalPreview.currency) : '-'}
                      </TableCell>
                      <TableCell align="right">
                        {l.credit ? formatCurrency(l.credit, journalPreview.currency) : '-'}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setJournalPreviewOpen(false)}>Kapat</Button>
          <Button
            variant="contained"
            onClick={() => journalPostMutation.mutate()}
            disabled={
              journalPostMutation.isPending ||
              journalPosted ||
              !journalPreview ||
              journalPreview.totalDebit !== journalPreview.totalCredit
            }
          >
            {journalPosted ? 'Posted' : journalPostMutation.isPending ? 'Posting...' : 'Post Journal'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

export default InvoiceDetailPage


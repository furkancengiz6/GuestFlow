import { useState } from 'react'
import {
  Box,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  Typography,
  Chip,
  Button,
  IconButton,
  Tooltip,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  InputAdornment,
  Grid,
  Card,
  CardContent,
  Tabs,
  Tab,
} from '@mui/material'
import {
  Search as SearchIcon,
  Clear as ClearIcon,
  Visibility as VisibilityIcon,
} from '@mui/icons-material'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { emailService, EmailFilters, EmailHistory } from '../../services/emailService'
import { formatDate } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'

const EmailsPage = () => {
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
  const [tabValue, setTabValue] = useState(0)
  const [searchTerm, setSearchTerm] = useState('')
  const [viewDialogOpen, setViewDialogOpen] = useState(false)
  const [selectedEmail, setSelectedEmail] = useState<EmailHistory | null>(null)

  const queryClient = useQueryClient()

  const filters: EmailFilters = {
    ...(searchTerm && { searchTerm }),
  }

  const { data, isLoading, error } = useQuery({
    queryKey: ['emails', page + 1, rowsPerPage, filters],
    queryFn: () => emailService.getEmailHistory(page + 1, rowsPerPage, filters),
  })

  const { data: templates } = useQuery({
    queryKey: ['email-templates'],
    queryFn: () => emailService.getEmailTemplates(),
  })

  const { data: statistics } = useQuery({
    queryKey: ['email-statistics'],
    queryFn: () => emailService.getStatistics(),
  })

  const handleChangePage = (_event: unknown, newPage: number) => {
    setPage(newPage)
  }

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10))
    setPage(0)
  }

  const handleViewEmail = (email: EmailHistory) => {
    setSelectedEmail(email)
    setViewDialogOpen(true)
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'Sent':
        return 'success'
      case 'Failed':
        return 'error'
      case 'Pending':
        return 'warning'
      default:
        return 'default'
    }
  }

  const getStatusLabel = (status: string) => {
    switch (status) {
      case 'Sent':
        return 'Gönderildi'
      case 'Failed':
        return 'Başarısız'
      case 'Pending':
        return 'Beklemede'
      default:
        return status
    }
  }

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={6} />
  }

  if (error) {
    return (
      <ContentState
        state="error"
        title="E-posta geçmişi yüklenemedi"
        description="Lütfen daha sonra tekrar deneyin."
        actionLabel="Tekrar dene"
        onAction={() => queryClient.refetchQueries({ queryKey: ['emails'] })}
      />
    )
  }

  const hasData = data && data.data && data.data.length > 0

  return (
    <Box p={3}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" sx={{ fontWeight: 600 }}>
          E-posta Yönetimi
        </Typography>
      </Box>

      {statistics && (
        <Grid container spacing={2} sx={{ mb: 3 }}>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography variant="body2" color="text.secondary">
                  Toplam Gönderilen
                </Typography>
                <Typography variant="h5" sx={{ fontWeight: 600 }}>
                  {statistics.totalSent}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography variant="body2" color="text.secondary">
                  Başarısız
                </Typography>
                <Typography variant="h5" sx={{ fontWeight: 600, color: 'error.main' }}>
                  {statistics.totalFailed}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography variant="body2" color="text.secondary">
                  Açılma Oranı
                </Typography>
                <Typography variant="h5" sx={{ fontWeight: 600, color: 'info.main' }}>
                  %{statistics.openRate.toFixed(1)}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography variant="body2" color="text.secondary">
                  Tıklanma Oranı
                </Typography>
                <Typography variant="h5" sx={{ fontWeight: 600, color: 'success.main' }}>
                  %{statistics.clickRate.toFixed(1)}
                </Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      <Paper sx={{ mb: 3 }}>
        <Tabs value={tabValue} onChange={(_, newValue) => setTabValue(newValue)}>
          <Tab label="E-posta Geçmişi" />
          <Tab label="Şablonlar" />
        </Tabs>
      </Paper>

      {tabValue === 0 && (
        <>
          <Paper sx={{ p: 2, mb: 3 }}>
            <TextField
              placeholder="Ara (alıcı, konu, mesaj)..."
              value={searchTerm}
              onChange={(e) => {
                setSearchTerm(e.target.value)
                setPage(0)
              }}
              size="small"
              fullWidth
              InputProps={{
                startAdornment: (
                  <InputAdornment position="start">
                    <SearchIcon />
                  </InputAdornment>
                ),
                endAdornment: searchTerm && (
                  <InputAdornment position="end">
                    <IconButton
                      size="small"
                      onClick={() => {
                        setSearchTerm('')
                        setPage(0)
                      }}
                    >
                      <ClearIcon fontSize="small" />
                    </IconButton>
                  </InputAdornment>
                ),
              }}
            />
          </Paper>

          {!hasData ? (
            <ContentState
              state="empty"
              title="E-posta geçmişi bulunamadı"
              description="Henüz e-posta geçmişi bulunmamaktadır."
            />
          ) : (
            <TableContainer component={Paper}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell><strong>Alıcı</strong></TableCell>
                    <TableCell><strong>Konu</strong></TableCell>
                    <TableCell><strong>Durum</strong></TableCell>
                    <TableCell><strong>Gönderim Tarihi</strong></TableCell>
                    <TableCell><strong>Açılma</strong></TableCell>
                    <TableCell><strong>Tıklama</strong></TableCell>
                    <TableCell><strong>İşlemler</strong></TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {data?.data.map((email) => (
                    <TableRow key={email.id} hover>
                      <TableCell>{email.to}</TableCell>
                      <TableCell sx={{ maxWidth: 200, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                        {email.subject}
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={getStatusLabel(email.status)}
                          color={getStatusColor(email.status) as any}
                          size="small"
                        />
                      </TableCell>
                      <TableCell>{formatDate(email.sentDate)}</TableCell>
                      <TableCell>{email.openedDate ? formatDate(email.openedDate) : '-'}</TableCell>
                      <TableCell>{email.clickCount}</TableCell>
                      <TableCell>
                        <Tooltip title="Görüntüle">
                          <IconButton
                            size="small"
                            color="primary"
                            onClick={() => handleViewEmail(email)}
                          >
                            <VisibilityIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
              <TablePagination
                component="div"
                count={data?.totalCount || 0}
                page={page}
                onPageChange={handleChangePage}
                rowsPerPage={rowsPerPage}
                onRowsPerPageChange={handleChangeRowsPerPage}
                rowsPerPageOptions={[5, 10, 25, 50]}
                labelRowsPerPage="Sayfa başına:"
                labelDisplayedRows={({ from, to, count }) => `${from}-${to} / ${count}`}
              />
            </TableContainer>
          )}
        </>
      )}

      {tabValue === 1 && (
        <TableContainer component={Paper}>
          <Table>
            <TableHead>
              <TableRow>
                <TableCell><strong>Şablon Adı</strong></TableCell>
                <TableCell><strong>Konu</strong></TableCell>
                <TableCell><strong>Durum</strong></TableCell>
                <TableCell><strong>İşlemler</strong></TableCell>
              </TableRow>
            </TableHead>
            <TableBody>
              {templates && templates.length > 0 ? (
                templates.map((template) => (
                  <TableRow key={template.id} hover>
                    <TableCell>{template.name}</TableCell>
                    <TableCell>{template.subject}</TableCell>
                    <TableCell>
                      <Chip
                        label={template.isActive ? 'Aktif' : 'Pasif'}
                        color={template.isActive ? 'success' : 'default'}
                        size="small"
                      />
                    </TableCell>
                    <TableCell>
                      <Tooltip title="Görüntüle">
                        <IconButton
                          size="small"
                          color="primary"
                          onClick={() => {
                            setSelectedEmail({
                              id: template.id,
                              to: '',
                              subject: template.subject,
                              body: template.body,
                              status: template.isActive ? 'Sent' : 'Pending',
                              sentDate: '',
                              clickCount: 0,
                            } as EmailHistory)
                            setViewDialogOpen(true)
                          }}
                        >
                          <VisibilityIcon fontSize="small" />
                        </IconButton>
                      </Tooltip>
                    </TableCell>
                  </TableRow>
                ))
              ) : (
                <TableRow>
                  <TableCell colSpan={4} align="center">
                    <Typography variant="body2" color="text.secondary">
                      Şablon bulunamadı
                    </Typography>
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </TableContainer>
      )}

      <Dialog open={viewDialogOpen} onClose={() => setViewDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>
          {selectedEmail?.subject || 'E-posta Detayı'}
        </DialogTitle>
        <DialogContent>
          {selectedEmail && (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              <TextField
                label="Alıcı"
                value={selectedEmail.to || '-'}
                fullWidth
                disabled
              />
              <TextField
                label="Konu"
                value={selectedEmail.subject}
                fullWidth
                disabled
              />
              <TextField
                label="İçerik"
                value={selectedEmail.body}
                multiline
                rows={10}
                fullWidth
                disabled
              />
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setViewDialogOpen(false)}>Kapat</Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

export default EmailsPage


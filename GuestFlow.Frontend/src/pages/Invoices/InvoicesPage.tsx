import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useLiveUpdates } from '../../hooks/useLiveUpdates'
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
  IconButton,
  Link,
  Button,
  TextField,
  InputAdornment,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Grid,
  Collapse,
} from '@mui/material'
import {
  PictureAsPdf as PdfIcon,
  Search as SearchIcon,
  FilterList as FilterListIcon,
  Clear as ClearIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon,
} from '@mui/icons-material'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns'
import { tr } from 'date-fns/locale'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { invoiceService, InvoiceFilters } from '../../services/invoiceService'
import { dropdownService } from '../../services/dropdownService'
import { formatDate, formatCurrency } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'
import { exportService, InvoiceExportFilters } from '../../services/exportService'
import { useNotification } from '../../hooks/useNotification'
import FileDownloadIcon from '@mui/icons-material/FileDownload'

const InvoicesPage = () => {
  const navigate = useNavigate()
  const notification = useNotification()
  const queryClient = useQueryClient()

  // Enable real-time updates for invoice changes
  useLiveUpdates(['invoice'])

  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
  const [filtersOpen, setFiltersOpen] = useState(false)
  
  // Filter states
  const [searchTerm, setSearchTerm] = useState('')
  const [startDate, setStartDate] = useState<Date | null>(null)
  const [endDate, setEndDate] = useState<Date | null>(null)
  const [guestId, setGuestId] = useState<number | undefined>(undefined)
  const [personnelId, setPersonnelId] = useState<number | undefined>(undefined)
  const [currency, setCurrency] = useState('')
  const [hasPdf, setHasPdf] = useState<boolean | undefined>(undefined)
  const [serviceType, setServiceType] = useState('')
  const [minAmount, setMinAmount] = useState<number | undefined>(undefined)
  const [maxAmount, setMaxAmount] = useState<number | undefined>(undefined)
  const [sortBy, setSortBy] = useState('')
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('desc')

  // Build filters object
  const filters: InvoiceFilters = {
    ...(searchTerm && { searchTerm }),
    ...(startDate && { startDate: startDate.toISOString().split('T')[0] }),
    ...(endDate && { endDate: endDate.toISOString().split('T')[0] }),
    ...(guestId && { guestId }),
    ...(personnelId && { personnelId }),
    ...(currency && { currency }),
    ...(hasPdf !== undefined && { hasPdf }),
    ...(serviceType && { serviceType }),
    ...(minAmount && { minAmount }),
    ...(maxAmount && { maxAmount }),
    ...(sortBy && { sortBy }),
    ...(sortOrder && { sortOrder }),
  }

  // Fetch dropdown data
  const { data: guests } = useQuery({
    queryKey: ['guests-dropdown'],
    queryFn: () => dropdownService.getGuests(),
  })

  const { data: personnel } = useQuery({
    queryKey: ['personnel-dropdown'],
    queryFn: () => dropdownService.getPersonnel(),
  })

  const { data, isLoading, error } = useQuery({
    queryKey: ['invoices', page + 1, rowsPerPage, filters],
    queryFn: () => invoiceService.getInvoices(page + 1, rowsPerPage, filters),
  })

  const handleChangePage = (_event: unknown, newPage: number) => {
    setPage(newPage)
  }

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10))
    setPage(0)
  }

  const handleClearFilters = () => {
    setSearchTerm('')
    setStartDate(null)
    setEndDate(null)
    setGuestId(undefined)
    setPersonnelId(undefined)
    setCurrency('')
    setHasPdf(undefined)
    setServiceType('')
    setMinAmount(undefined)
    setMaxAmount(undefined)
    setSortBy('')
    setSortOrder('desc')
    setPage(0)
  }

  const hasActiveFilters = searchTerm || startDate || endDate || guestId || personnelId || currency || hasPdf !== undefined || serviceType || minAmount || maxAmount || sortBy

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={6} />
  }

  if (error) {
    return (
      <ContentState
        state="error"
        title="Faturalar yüklenemedi"
        description="Lütfen daha sonra tekrar deneyin."
        actionLabel="Tekrar dene"
        onAction={() => queryClient.refetchQueries({ queryKey: ['invoices'] })}
      />
    )
  }

  const hasData = data && data.data && data.data.length > 0

  return (
    <Box p={3}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" sx={{ fontWeight: 600 }}>
          Faturalar
        </Typography>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button
            variant="outlined"
            startIcon={<FileDownloadIcon />}
            onClick={async () => {
              try {
                const exportFilters: InvoiceExportFilters = {
                  ...(startDate && { startDate: startDate.toISOString().split('T')[0] }),
                  ...(endDate && { endDate: endDate.toISOString().split('T')[0] }),
                  ...(guestId && { guestId }),
                  ...(personnelId && { personnelId }),
                  ...(currency && { currency }),
                  ...(hasPdf !== undefined && { hasPdf }),
                  ...(serviceType && { serviceType }),
                  ...(minAmount && { minAmount }),
                  ...(maxAmount && { maxAmount }),
                  ...(searchTerm && { searchTerm }),
                }
                await exportService.exportInvoicesToExcel(exportFilters)
                notification.showSuccess('Excel dosyası indiriliyor...')
              } catch (error: any) {
                notification.showError(error?.response?.data?.message || 'Dışa aktarma başarısız oldu.')
              }
            }}
          >
            Excel
          </Button>
          <Button
            variant="outlined"
            startIcon={<FileDownloadIcon />}
            onClick={async () => {
              try {
                const exportFilters: InvoiceExportFilters = {
                  ...(startDate && { startDate: startDate.toISOString().split('T')[0] }),
                  ...(endDate && { endDate: endDate.toISOString().split('T')[0] }),
                  ...(guestId && { guestId }),
                  ...(personnelId && { personnelId }),
                  ...(currency && { currency }),
                  ...(hasPdf !== undefined && { hasPdf }),
                  ...(serviceType && { serviceType }),
                  ...(minAmount && { minAmount }),
                  ...(maxAmount && { maxAmount }),
                  ...(searchTerm && { searchTerm }),
                }
                await exportService.exportInvoicesToCsv(exportFilters)
                notification.showSuccess('CSV dosyası indiriliyor...')
              } catch (error: any) {
                notification.showError(error?.response?.data?.message || 'Dışa aktarma başarısız oldu.')
              }
            }}
          >
            CSV
          </Button>
        </Box>
      </Box>

      {/* Search and Filter Section */}
      <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={tr}>
        <Paper sx={{ p: 2, mb: 3 }}>
          <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', mb: 2 }}>
            <TextField
              placeholder="Ara (fatura no, misafir adı)..."
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
            <Button
              variant={filtersOpen ? 'contained' : 'outlined'}
              startIcon={<FilterListIcon />}
              endIcon={filtersOpen ? <ExpandLessIcon /> : <ExpandMoreIcon />}
              onClick={() => setFiltersOpen(!filtersOpen)}
            >
              Filtreler
            </Button>
            {hasActiveFilters && (
              <Button
                variant="outlined"
                color="error"
                startIcon={<ClearIcon />}
                onClick={handleClearFilters}
              >
                Temizle
              </Button>
            )}
          </Box>

          <Collapse in={filtersOpen}>
            <Grid container spacing={2}>
              <Grid item xs={12} sm={6} md={3}>
                <DatePicker
                  label="Başlangıç Tarihi"
                  value={startDate}
                  onChange={(newValue) => {
                    setStartDate(newValue)
                    setPage(0)
                  }}
                  slotProps={{
                    textField: { size: 'small', fullWidth: true },
                  }}
                />
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <DatePicker
                  label="Bitiş Tarihi"
                  value={endDate}
                  onChange={(newValue) => {
                    setEndDate(newValue)
                    setPage(0)
                  }}
                  slotProps={{
                    textField: { size: 'small', fullWidth: true },
                  }}
                />
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <FormControl fullWidth size="small">
                  <InputLabel>Misafir</InputLabel>
                  <Select
                    value={guestId || ''}
                    label="Misafir"
                    onChange={(e) => {
                      setGuestId(e.target.value ? Number(e.target.value) : undefined)
                      setPage(0)
                    }}
                  >
                    <MenuItem value="">Tümü</MenuItem>
                    {guests?.map((guest) => (
                      <MenuItem key={guest.id} value={guest.id}>
                        {guest.fullName} ({guest.guestCode})
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <FormControl fullWidth size="small">
                  <InputLabel>Personel</InputLabel>
                  <Select
                    value={personnelId || ''}
                    label="Personel"
                    onChange={(e) => {
                      setPersonnelId(e.target.value ? Number(e.target.value) : undefined)
                      setPage(0)
                    }}
                  >
                    <MenuItem value="">Tümü</MenuItem>
                    {personnel?.map((p) => (
                      <MenuItem key={p.id} value={p.id}>
                        {p.fullName}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <FormControl fullWidth size="small">
                  <InputLabel>Para Birimi</InputLabel>
                  <Select
                    value={currency}
                    label="Para Birimi"
                    onChange={(e) => {
                      setCurrency(e.target.value)
                      setPage(0)
                    }}
                  >
                    <MenuItem value="">Tümü</MenuItem>
                    <MenuItem value="TRY">TRY - Türk Lirası</MenuItem>
                    <MenuItem value="USD">USD - US Dollar</MenuItem>
                    <MenuItem value="EUR">EUR - Euro</MenuItem>
                    <MenuItem value="GBP">GBP - British Pound</MenuItem>
                    <MenuItem value="RUB">RUB - Russian Ruble</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <FormControl fullWidth size="small">
                  <InputLabel>PDF Durumu</InputLabel>
                  <Select
                    value={hasPdf === undefined ? '' : hasPdf ? 'true' : 'false'}
                    label="PDF Durumu"
                    onChange={(e) => {
                      const value = e.target.value
                      setHasPdf(value === '' ? undefined : value === 'true')
                      setPage(0)
                    }}
                  >
                    <MenuItem value="">Tümü</MenuItem>
                    <MenuItem value="true">PDF Var</MenuItem>
                    <MenuItem value="false">PDF Yok</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <FormControl fullWidth size="small">
                  <InputLabel>Hizmet Tipi</InputLabel>
                  <Select
                    value={serviceType}
                    label="Hizmet Tipi"
                    onChange={(e) => {
                      setServiceType(e.target.value)
                      setPage(0)
                    }}
                  >
                    <MenuItem value="">Tümü</MenuItem>
                    <MenuItem value="Transfer">Transfer</MenuItem>
                    <MenuItem value="CityTour">Şehir Turu</MenuItem>
                    <MenuItem value="YachtTour">Yat Turu</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <TextField
                  label="Min Tutar"
                  type="number"
                  size="small"
                  fullWidth
                  value={minAmount || ''}
                  onChange={(e) => {
                    setMinAmount(e.target.value ? Number(e.target.value) : undefined)
                    setPage(0)
                  }}
                  inputProps={{ step: '0.01', min: '0' }}
                />
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <TextField
                  label="Max Tutar"
                  type="number"
                  size="small"
                  fullWidth
                  value={maxAmount || ''}
                  onChange={(e) => {
                    setMaxAmount(e.target.value ? Number(e.target.value) : undefined)
                    setPage(0)
                  }}
                  inputProps={{ step: '0.01', min: '0' }}
                />
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <FormControl fullWidth size="small">
                  <InputLabel>Sırala</InputLabel>
                  <Select
                    value={sortBy}
                    label="Sırala"
                    onChange={(e) => {
                      setSortBy(e.target.value)
                      setPage(0)
                    }}
                  >
                    <MenuItem value="">Varsayılan</MenuItem>
                    <MenuItem value="IssueDate">Fatura Tarihi</MenuItem>
                    <MenuItem value="TotalAmount">Tutar</MenuItem>
                    <MenuItem value="InvoiceNumber">Fatura No</MenuItem>
                    <MenuItem value="CreatedDate">Oluşturulma Tarihi</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              {sortBy && (
                <Grid item xs={12} sm={6} md={3}>
                  <FormControl fullWidth size="small">
                    <InputLabel>Sıralama Yönü</InputLabel>
                    <Select
                      value={sortOrder}
                      label="Sıralama Yönü"
                      onChange={(e) => {
                        setSortOrder(e.target.value as 'asc' | 'desc')
                        setPage(0)
                      }}
                    >
                      <MenuItem value="asc">Artan (A-Z)</MenuItem>
                      <MenuItem value="desc">Azalan (Z-A)</MenuItem>
                    </Select>
                  </FormControl>
                </Grid>
              )}
            </Grid>
          </Collapse>
        </Paper>
      </LocalizationProvider>

      {!hasData ? (
        <ContentState
          state="empty"
          title="Fatura bulunamadı"
          description="Henüz kayıtlı fatura bulunmamaktadır."
        />
      ) : (
        <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell><strong>Fatura No</strong></TableCell>
              <TableCell><strong>Tarih</strong></TableCell>
              <TableCell><strong>Tutar</strong></TableCell>
              <TableCell><strong>Para Birimi</strong></TableCell>
              <TableCell><strong>Notlar</strong></TableCell>
              <TableCell><strong>PDF</strong></TableCell>
              <TableCell><strong>İşlemler</strong></TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {data?.data.map((invoice) => (
              <TableRow key={invoice.id} hover>
                <TableCell>
                  <Button
                    variant="text"
                    onClick={() => navigate(`/invoices/${invoice.id}`)}
                    sx={{ textTransform: 'none', fontWeight: 500 }}
                  >
                    {invoice.invoiceNumber}
                  </Button>
                </TableCell>
                <TableCell>{formatDate(invoice.issueDate)}</TableCell>
                <TableCell>{formatCurrency(invoice.totalAmount, invoice.currency)}</TableCell>
                <TableCell>{invoice.currency}</TableCell>
                <TableCell>{invoice.notes || '-'}</TableCell>
                <TableCell>
                  {invoice.pdfUrl ? (
                    <Link href={invoice.pdfUrl} target="_blank" rel="noopener noreferrer">
                      <IconButton size="small" color="primary">
                        <PdfIcon />
                      </IconButton>
                    </Link>
                  ) : (
                    '-'
                  )}
                </TableCell>
                <TableCell>
                  <Button
                    variant="outlined"
                    size="small"
                    onClick={() => navigate(`/invoices/${invoice.id}`)}
                  >
                    Detay
                  </Button>
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
    </Box>
  )
}

export default InvoicesPage


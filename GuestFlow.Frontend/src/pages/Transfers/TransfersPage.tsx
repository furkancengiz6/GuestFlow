import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
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
  DialogContentText,
  DialogActions,
  TextField,
  InputAdornment,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Grid,
  Collapse,
  FormControlLabel,
  Switch,
} from '@mui/material'
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Visibility as VisibilityIcon,
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
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { transferService, CreateTransferRequest, UpdateTransferRequest, TransferFilters } from '../../services/transferService'
import { dropdownService } from '../../services/dropdownService'
import { useNotification } from '../../hooks/useNotification'
import { formatDate, formatCurrency } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'
import TransferForm from '../../components/Transfers/TransferForm'
import { Transfer } from '../../types/transfer'
import { exportService, TransferExportFilters } from '../../services/exportService'
import FileDownloadIcon from '@mui/icons-material/FileDownload'

const getStatusColor = (status: string) => {
  switch (status.toLowerCase()) {
    case 'completed':
      return 'success'
    case 'pending':
      return 'warning'
    case 'cancelled':
      return 'error'
    default:
      return 'default'
  }
}

const getStatusLabel = (status: string) => {
  switch (status.toLowerCase()) {
    case 'completed':
      return 'Tamamlandı'
    case 'pending':
      return 'Beklemede'
    case 'cancelled':
      return 'İptal Edildi'
    default:
      return status
  }
}

const TransfersPage = () => {
  const navigate = useNavigate()
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
  const [formOpen, setFormOpen] = useState(false)
  const [editingTransfer, setEditingTransfer] = useState<Transfer | null>(null)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [transferToDelete, setTransferToDelete] = useState<Transfer | null>(null)
  const [filtersOpen, setFiltersOpen] = useState(false)
  
  // Filter states
  const [searchTerm, setSearchTerm] = useState('')
  const [startDate, setStartDate] = useState<Date | null>(null)
  const [endDate, setEndDate] = useState<Date | null>(null)
  const [status, setStatus] = useState('')
  const [guestId, setGuestId] = useState<number | undefined>(undefined)
  const [personnelId, setPersonnelId] = useState<number | undefined>(undefined)
  const [vehicleId, setVehicleId] = useState<number | undefined>(undefined)
  const [airportId, setAirportId] = useState<number | undefined>(undefined)
  const [isFromAirport, setIsFromAirport] = useState<boolean | undefined>(undefined)
  const [sortBy, setSortBy] = useState('')
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc')

  const queryClient = useQueryClient()

  // Build filters object
  const filters: TransferFilters = {
    ...(searchTerm && { searchTerm }),
    ...(startDate && { startDate: startDate.toISOString().split('T')[0] }),
    ...(endDate && { endDate: endDate.toISOString().split('T')[0] }),
    ...(status && { status }),
    ...(guestId && { guestId }),
    ...(personnelId && { personnelId }),
    ...(vehicleId && { vehicleId }),
    ...(airportId && { airportId }),
    ...(isFromAirport !== undefined && { isFromAirport }),
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

  const { data: vehicles } = useQuery({
    queryKey: ['vehicles-dropdown'],
    queryFn: () => dropdownService.getVehicles(),
  })

  const { data: airports } = useQuery({
    queryKey: ['airports-dropdown'],
    queryFn: () => dropdownService.getAirports(),
  })

  const { data, isLoading, error } = useQuery({
    queryKey: ['transfers', page + 1, rowsPerPage, filters],
    queryFn: () => transferService.getTransfers(page + 1, rowsPerPage, filters),
  })

  const createMutation = useMutation({
    mutationFn: (data: CreateTransferRequest) => transferService.createTransfer(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transfers'] })
      setFormOpen(false)
      notification.showSuccess('Transfer başarıyla eklendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Transfer eklenirken bir hata oluştu.')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateTransferRequest }) =>
      transferService.updateTransfer(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transfers'] })
      setFormOpen(false)
      setEditingTransfer(null)
      notification.showSuccess('Transfer başarıyla güncellendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Transfer güncellenirken bir hata oluştu.')
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (id: number) => transferService.deleteTransfer(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['transfers'] })
      setDeleteDialogOpen(false)
      setTransferToDelete(null)
      notification.showSuccess('Transfer başarıyla silindi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Transfer silinirken bir hata oluştu.')
    },
  })

  const handleChangePage = (_event: unknown, newPage: number) => {
    setPage(newPage)
  }

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10))
    setPage(0)
  }

  const handleOpenForm = (transfer?: Transfer) => {
    if (transfer) {
      setEditingTransfer(transfer)
    } else {
      setEditingTransfer(null)
    }
    setFormOpen(true)
  }

  const handleCloseForm = () => {
    setFormOpen(false)
    setEditingTransfer(null)
  }

  const handleFormSubmit = async (data: CreateTransferRequest | UpdateTransferRequest) => {
    if (editingTransfer) {
      await updateMutation.mutateAsync({ id: editingTransfer.id, data })
    } else {
      await createMutation.mutateAsync(data)
    }
  }

  const handleDeleteClick = (transfer: Transfer) => {
    setTransferToDelete(transfer)
    setDeleteDialogOpen(true)
  }

  const handleDeleteConfirm = () => {
    if (transferToDelete) {
      deleteMutation.mutate(transferToDelete.id)
    }
  }

  const handleClearFilters = () => {
    setSearchTerm('')
    setStartDate(null)
    setEndDate(null)
    setStatus('')
    setGuestId(undefined)
    setPersonnelId(undefined)
    setVehicleId(undefined)
    setAirportId(undefined)
    setIsFromAirport(undefined)
    setSortBy('')
    setSortOrder('asc')
    setPage(0)
  }

  const hasActiveFilters = searchTerm || startDate || endDate || status || guestId || personnelId || vehicleId || airportId || isFromAirport !== undefined || sortBy

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={6} />
  }

  if (error) {
    return (
      <ContentState
        state="error"
        title="Transferler yüklenemedi"
        description="Lütfen daha sonra tekrar deneyin."
        actionLabel="Tekrar dene"
        onAction={() => window.location.reload()}
      />
    )
  }

  const hasData = data && data.data && data.data.length > 0

  return (
    <Box p={3}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" sx={{ fontWeight: 600 }}>
          Transferler
        </Typography>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button
            variant="outlined"
            startIcon={<FileDownloadIcon />}
            onClick={async () => {
              try {
                const exportFilters: TransferExportFilters = {
                  ...(startDate && { startDate: startDate.toISOString().split('T')[0] }),
                  ...(endDate && { endDate: endDate.toISOString().split('T')[0] }),
                  ...(status && { status }),
                  ...(guestId && { guestId }),
                  ...(personnelId && { personnelId }),
                  ...(vehicleId && { vehicleId }),
                  ...(airportId && { airportId }),
                  ...(isFromAirport !== undefined && { isFromAirport }),
                  ...(searchTerm && { searchTerm }),
                }
                await exportService.exportTransfersToExcel(exportFilters)
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
                const exportFilters: TransferExportFilters = {
                  ...(startDate && { startDate: startDate.toISOString().split('T')[0] }),
                  ...(endDate && { endDate: endDate.toISOString().split('T')[0] }),
                  ...(status && { status }),
                  ...(guestId && { guestId }),
                  ...(personnelId && { personnelId }),
                  ...(vehicleId && { vehicleId }),
                  ...(airportId && { airportId }),
                  ...(isFromAirport !== undefined && { isFromAirport }),
                  ...(searchTerm && { searchTerm }),
                }
                await exportService.exportTransfersToCsv(exportFilters)
                notification.showSuccess('CSV dosyası indiriliyor...')
              } catch (error: any) {
                notification.showError(error?.response?.data?.message || 'Dışa aktarma başarısız oldu.')
              }
            }}
          >
            CSV
          </Button>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => handleOpenForm()}
          >
            Yeni Transfer
          </Button>
        </Box>
      </Box>

      {/* Search and Filter Section */}
      <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={tr}>
        <Paper sx={{ p: 2, mb: 3 }}>
          <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', mb: 2 }}>
            <TextField
              placeholder="Ara (adres, not)..."
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
                  <InputLabel>Durum</InputLabel>
                  <Select
                    value={status}
                    label="Durum"
                    onChange={(e) => {
                      setStatus(e.target.value)
                      setPage(0)
                    }}
                  >
                    <MenuItem value="">Tümü</MenuItem>
                    <MenuItem value="Pending">Beklemede</MenuItem>
                    <MenuItem value="InProgress">Devam Ediyor</MenuItem>
                    <MenuItem value="Completed">Tamamlandı</MenuItem>
                    <MenuItem value="Cancelled">İptal Edildi</MenuItem>
                  </Select>
                </FormControl>
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
                  <InputLabel>Araç</InputLabel>
                  <Select
                    value={vehicleId || ''}
                    label="Araç"
                    onChange={(e) => {
                      setVehicleId(e.target.value ? Number(e.target.value) : undefined)
                      setPage(0)
                    }}
                  >
                    <MenuItem value="">Tümü</MenuItem>
                    {vehicles?.map((vehicle) => (
                      <MenuItem key={vehicle.id} value={vehicle.id}>
                        {vehicle.plateNumber} ({vehicle.vehicleType})
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <FormControl fullWidth size="small">
                  <InputLabel>Havaalanı</InputLabel>
                  <Select
                    value={airportId || ''}
                    label="Havaalanı"
                    onChange={(e) => {
                      setAirportId(e.target.value ? Number(e.target.value) : undefined)
                      setPage(0)
                    }}
                  >
                    <MenuItem value="">Tümü</MenuItem>
                    {airports?.map((airport) => (
                      <MenuItem key={airport.id} value={airport.id}>
                        {airport.airportName} {airport.cityName && `- ${airport.cityName}`}
                      </MenuItem>
                    ))}
                  </Select>
                </FormControl>
              </Grid>
              <Grid item xs={12} sm={6} md={3}>
                <FormControl fullWidth size="small">
                  <InputLabel>Havaalanından mı?</InputLabel>
                  <Select
                    value={isFromAirport === undefined ? '' : isFromAirport ? 'true' : 'false'}
                    label="Havaalanından mı?"
                    onChange={(e) => {
                      const value = e.target.value
                      setIsFromAirport(value === '' ? undefined : value === 'true')
                      setPage(0)
                    }}
                  >
                    <MenuItem value="">Tümü</MenuItem>
                    <MenuItem value="true">Evet</MenuItem>
                    <MenuItem value="false">Hayır</MenuItem>
                  </Select>
                </FormControl>
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
                    <MenuItem value="TransferDate">Transfer Tarihi</MenuItem>
                    <MenuItem value="Price">Fiyat</MenuItem>
                    <MenuItem value="Status">Durum</MenuItem>
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
          title="Transfer bulunamadı"
          description="Kayıtlı transfer olmadığında burada listelenecek."
        />
      ) : (
        <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell><strong>Tarih</strong></TableCell>
              <TableCell><strong>Alış Adresi</strong></TableCell>
              <TableCell><strong>Bırakış Adresi</strong></TableCell>
              <TableCell><strong>Fiyat</strong></TableCell>
              <TableCell><strong>Durum</strong></TableCell>
              <TableCell><strong>Havaalanı</strong></TableCell>
              <TableCell><strong>İşlemler</strong></TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {data?.data.map((transfer) => (
              <TableRow key={transfer.id} hover>
                <TableCell>
                  <Button
                    variant="text"
                    onClick={() => navigate(`/transfers/${transfer.id}`)}
                    sx={{ textTransform: 'none', fontWeight: 500 }}
                  >
                    {formatDate(transfer.transferDate)}
                  </Button>
                </TableCell>
                <TableCell>{transfer.pickupAddress}</TableCell>
                <TableCell>{transfer.dropoffAddress}</TableCell>
                <TableCell>{formatCurrency(transfer.price, 'TRY')}</TableCell>
                <TableCell>
                  <Chip
                    label={getStatusLabel(transfer.status)}
                    color={getStatusColor(transfer.status) as any}
                    size="small"
                  />
                </TableCell>
                <TableCell>
                  {transfer.isFromAirport ? (
                    <Chip label="Evet" color="info" size="small" />
                  ) : (
                    <Chip label="Hayır" size="small" />
                  )}
                </TableCell>
                <TableCell>
                  <Box sx={{ display: 'flex', gap: 1 }}>
                    <Tooltip title="Detay">
                      <IconButton
                        size="small"
                        color="primary"
                        onClick={() => navigate(`/transfers/${transfer.id}`)}
                      >
                        <VisibilityIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Düzenle">
                      <IconButton
                        size="small"
                        color="primary"
                        onClick={() => handleOpenForm(transfer)}
                      >
                        <EditIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Sil">
                      <IconButton
                        size="small"
                        color="error"
                        onClick={() => handleDeleteClick(transfer)}
                      >
                        <DeleteIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  </Box>
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

      <TransferForm
        open={formOpen}
        onClose={handleCloseForm}
        onSubmit={handleFormSubmit}
        transfer={editingTransfer}
        isLoading={createMutation.isPending || updateMutation.isPending}
      />

      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>Transfer Sil</DialogTitle>
        <DialogContent>
          <DialogContentText>
            {transferToDelete && (
              <>
                Bu transferi silmek istediğinize emin misiniz? Bu işlem geri alınamaz.
              </>
            )}
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)} disabled={deleteMutation.isPending}>
            İptal
          </Button>
          <Button
            onClick={handleDeleteConfirm}
            color="error"
            variant="contained"
            disabled={deleteMutation.isPending}
          >
            {deleteMutation.isPending ? 'Siliniyor...' : 'Sil'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

export default TransfersPage


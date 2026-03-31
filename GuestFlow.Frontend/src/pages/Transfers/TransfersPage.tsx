import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useLiveUpdates } from '../../hooks/useLiveUpdates'
import {
  Box,
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
  Checkbox,
  Switch,
  FormControlLabel,
  LinearProgress,
  Card,
  CardContent,
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
import { transferService, CreateTransferRequest, UpdateTransferRequest, TransferFilters, BulkTransferOperation, BulkOperationResult } from '../../services/transferService'
import { dropdownService } from '../../services/dropdownService'
import { useNotification } from '../../hooks/useNotification'
import { formatDate } from '../../utils/formatters'
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

  // Enable real-time updates for transfer changes
  useLiveUpdates(['transfer'])

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
  const [sortBy, setSortBy] = useState('')
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc')

  // Bulk operations states
  const [selectedTransfers, setSelectedTransfers] = useState<number[]>([])
  const [bulkDialogOpen, setBulkDialogOpen] = useState(false)
  const [bulkOperation, setBulkOperation] = useState<string>('')
  const [bulkReason, setBulkReason] = useState('')

  // Advanced filter states
  const [priority, setPriority] = useState('')
  const [transportMode, setTransportMode] = useState('')
  const [isVip, setIsVip] = useState<boolean | undefined>(undefined)
  const [groupSizeMin, setGroupSizeMin] = useState<number | undefined>(undefined)
  const [groupSizeMax, setGroupSizeMax] = useState<number | undefined>(undefined)
  const [priceMin, setPriceMin] = useState<number | undefined>(undefined)
  const [priceMax, setPriceMax] = useState<number | undefined>(undefined)

  const queryClient = useQueryClient()
  const notification = useNotification()

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
    ...(priority && { priority }),
    ...(transportMode && { transportMode }),
    ...(isVip !== undefined && { isVip }),
    ...(groupSizeMin !== undefined && { groupSizeMin }),
    ...(groupSizeMax !== undefined && { groupSizeMax }),
    ...(priceMin !== undefined && { priceMin }),
    ...(priceMax !== undefined && { priceMax }),
    ...(sortBy && { sortBy, sortOrder }),
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
    onSuccess: (response) => {
      queryClient.invalidateQueries({ queryKey: ['transfers'] })
      setFormOpen(false)
      notification.showSuccess('Transfer başarıyla eklendi.')

      // Otomatik fatura indirme
      if (response.invoicePdfUrl) {
        try {
          // PDF URL'ini yeni sekmede aç
          window.open(response.invoicePdfUrl, '_blank')
          notification.showSuccess('Fatura PDF\'i indiriliyor...')
        } catch (error) {
          console.error('Fatura indirme hatası:', error)
        }
      }
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

  // Bulk operations mutations
  const bulkUpdateMutation = useMutation({
    mutationFn: (operation: BulkTransferOperation) => transferService.bulkUpdateTransfers(operation),
    onSuccess: (result: BulkOperationResult) => {
      queryClient.invalidateQueries({ queryKey: ['transfers'] })
      setBulkDialogOpen(false)
      setSelectedTransfers([])
      setBulkOperation('')
      setBulkReason('')

      if (result.successCount > 0) {
        notification.showSuccess(`${result.successCount} transfer başarıyla güncellendi.`)
      }
      if (result.failureCount > 0) {
        notification.showWarning(`${result.failureCount} transfer güncellenirken hata oluştu.`)
      }
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Toplu işlem sırasında bir hata oluştu.')
    },
  })

  const bulkDeleteMutation = useMutation({
    mutationFn: ({ transferIds, reason }: { transferIds: number[], reason?: string }) =>
      transferService.bulkDeleteTransfers(transferIds, reason),
    onSuccess: (result: BulkOperationResult) => {
      queryClient.invalidateQueries({ queryKey: ['transfers'] })
      setSelectedTransfers([])

      if (result.successCount > 0) {
        notification.showSuccess(`${result.successCount} transfer başarıyla silindi.`)
      }
      if (result.failureCount > 0) {
        notification.showWarning(`${result.failureCount} transfer silinirken hata oluştu.`)
      }
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Toplu silme sırasında bir hata oluştu.')
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
    setPriority('')
    setTransportMode('')
    setIsVip(undefined)
    setGroupSizeMin(undefined)
    setGroupSizeMax(undefined)
    setPriceMin(undefined)
    setPriceMax(undefined)
    setSortBy('')
    setSortOrder('asc')
    setPage(0)
  }

  // Bulk operations handlers
  const handleSelectTransfer = (transferId: number, checked: boolean) => {
    if (checked) {
      setSelectedTransfers(prev => [...prev, transferId])
    } else {
      setSelectedTransfers(prev => prev.filter(id => id !== transferId))
    }
  }

  const handleSelectAll = (checked: boolean) => {
    if (checked) {
      setSelectedTransfers(data?.data?.map(t => t.id) || [])
    } else {
      setSelectedTransfers([])
    }
  }

  const handleBulkOperation = () => {
    if (selectedTransfers.length === 0) {
      notification.showWarning('Lütfen en az bir transfer seçin.')
      return
    }

    setBulkDialogOpen(true)
  }

  const handleBulkConfirm = () => {
    if (selectedTransfers.length === 0) return

    const operation: BulkTransferOperation = {
      operation: bulkOperation as any,
      transferIds: selectedTransfers,
      reason: bulkReason,
    }

    if (bulkOperation === 'delete') {
      bulkDeleteMutation.mutate({ transferIds: selectedTransfers, reason: bulkReason })
    } else {
      bulkUpdateMutation.mutate(operation)
    }
  }

  const hasActiveFilters = searchTerm || startDate || endDate || status || guestId || personnelId || vehicleId || airportId || sortBy

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
        onAction={() => queryClient.refetchQueries({ queryKey: ['transfers'] })}
      />
    )
  }

  const hasData = data && data.data && data.data.length > 0
  return (
    <Box className="fade-in" p={3}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" component="h1" className="premium-gradient-text" sx={{ fontWeight: 800 }}>
          Transfer Yönetimi
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
            className="premium-gradient"
            sx={{ borderRadius: 2, boxShadow: '0 4px 14px 0 rgba(0,118,255,0.39)' }}
          >
            Yeni Transfer
          </Button>
        </Box>
      </Box>

      {/* Search and Filter Section */}
      <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={tr}>
        <Card className="glass-panel" sx={{ p: 2, mb: 3 }}>
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
            {selectedTransfers.length > 0 && (
              <Button
                variant="contained"
                color="primary"
                onClick={handleBulkOperation}
                disabled={bulkUpdateMutation.isPending || bulkDeleteMutation.isPending}
              >
                Toplu İşlemler ({selectedTransfers.length})
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
                    {Array.isArray(guests) && guests.map((guest) => (
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
                    {Array.isArray(personnel) && personnel.map((p) => (
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
                    {Array.isArray(vehicles) && vehicles.map((vehicle) => (
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
                    {Array.isArray(airports) && airports.map((airport) => (
                      <MenuItem key={airport.id} value={airport.id}>
                        {airport.airportName} {airport.cityName && `- ${airport.cityName}`}
                      </MenuItem>
                    ))}
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

              {/* Advanced Filters */}
              <Grid item xs={12}>
                <Typography variant="subtitle2" sx={{ mt: 2, mb: 1, fontWeight: 'bold' }}>
                  Gelişmiş Filtreler
                </Typography>
              </Grid>

              <Grid item xs={12} sm={6} md={3}>
                <FormControl fullWidth size="small">
                  <InputLabel>Öncelik</InputLabel>
                  <Select
                    value={priority}
                    label="Öncelik"
                    onChange={(e) => {
                      setPriority(e.target.value)
                      setPage(0)
                    }}
                  >
                    <MenuItem value="">Tümü</MenuItem>
                    <MenuItem value="Normal">Normal</MenuItem>
                    <MenuItem value="SameDay">Aynı Gün</MenuItem>
                    <MenuItem value="Urgent">Acil</MenuItem>
                    <MenuItem value="Emergency">Acil Durum</MenuItem>
                  </Select>
                </FormControl>
              </Grid>

              <Grid item xs={12} sm={6} md={3}>
                <FormControl fullWidth size="small">
                  <InputLabel>Taşıma Modu</InputLabel>
                  <Select
                    value={transportMode}
                    label="Taşıma Modu"
                    onChange={(e) => {
                      setTransportMode(e.target.value)
                      setPage(0)
                    }}
                  >
                    <MenuItem value="">Tümü</MenuItem>
                    <MenuItem value="Sedan">Sedan</MenuItem>
                    <MenuItem value="Van">Van</MenuItem>
                    <MenuItem value="Minibus">Minibüs</MenuItem>
                    <MenuItem value="Bus">Otobüs</MenuItem>
                    <MenuItem value="Limousine">Limuzin</MenuItem>
                    <MenuItem value="SUV">SUV</MenuItem>
                  </Select>
                </FormControl>
              </Grid>

              <Grid item xs={12} sm={6} md={3}>
                <FormControlLabel
                  control={
                    <Switch
                      checked={isVip === true}
                      onChange={(e) => {
                        setIsVip(e.target.checked ? true : undefined)
                        setPage(0)
                      }}
                      color="primary"
                    />
                  }
                  label="VIP Transferler"
                />
              </Grid>

              <Grid item xs={12} sm={6} md={3}>
                <TextField
                  fullWidth
                  size="small"
                  label="Min Grup Boyutu"
                  type="number"
                  value={groupSizeMin || ''}
                  onChange={(e) => {
                    setGroupSizeMin(e.target.value ? Number(e.target.value) : undefined)
                    setPage(0)
                  }}
                  inputProps={{ min: 1 }}
                />
              </Grid>

              <Grid item xs={12} sm={6} md={3}>
                <TextField
                  fullWidth
                  size="small"
                  label="Max Grup Boyutu"
                  type="number"
                  value={groupSizeMax || ''}
                  onChange={(e) => {
                    setGroupSizeMax(e.target.value ? Number(e.target.value) : undefined)
                    setPage(0)
                  }}
                  inputProps={{ min: 1 }}
                />
              </Grid>

              <Grid item xs={12} sm={6} md={3}>
                <TextField
                  fullWidth
                  size="small"
                  label="Min Fiyat"
                  type="number"
                  value={priceMin || ''}
                  onChange={(e) => {
                    setPriceMin(e.target.value ? Number(e.target.value) : undefined)
                    setPage(0)
                  }}
                  inputProps={{ min: 0, step: 0.01 }}
                />
              </Grid>

              <Grid item xs={12} sm={6} md={3}>
                <TextField
                  fullWidth
                  size="small"
                  label="Max Fiyat"
                  type="number"
                  value={priceMax || ''}
                  onChange={(e) => {
                    setPriceMax(e.target.value ? Number(e.target.value) : undefined)
                    setPage(0)
                  }}
                  inputProps={{ min: 0, step: 0.01 }}
                />
              </Grid>
            </Grid>
          </Collapse>
        </Card>
      </LocalizationProvider>

      {!hasData ? (
        <ContentState
          state="empty"
          title="Transfer bulunamadı"
          description="Henüz kayıtlı transfer bulunmamaktadır."
        />
      ) : (
        <Card className="glass-panel">
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell padding="checkbox">
                    <Checkbox
                      indeterminate={selectedTransfers.length > 0 && selectedTransfers.length < (data?.data?.length || 0)}
                      checked={selectedTransfers.length === (data?.data?.length || 0) && selectedTransfers.length > 0}
                      onChange={(e) => handleSelectAll(e.target.checked)}
                    />
                  </TableCell>
                  <TableCell><strong>Öncelik</strong></TableCell>
                  <TableCell><strong>Tarih/Saat</strong></TableCell>
                  <TableCell><strong>Misafir</strong></TableCell>
                  <TableCell><strong>Rota</strong></TableCell>
                  <TableCell><strong>Grup</strong></TableCell>
                  <TableCell><strong>Şoför</strong></TableCell>
                  <TableCell><strong>Durum</strong></TableCell>
                  <TableCell><strong>İşlemler</strong></TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {data?.data.map((transfer) => (
                  <TableRow key={transfer.id} hover>
                    <TableCell padding="checkbox">
                      <Checkbox
                        checked={selectedTransfers.includes(transfer.id)}
                        onChange={(e) => handleSelectTransfer(transfer.id, e.target.checked)}
                      />
                    </TableCell>
                    <TableCell>
                      <Chip
                        label="ACİL"
                        color="error"
                        size="small"
                        sx={{ fontWeight: 'bold' }}
                      />
                    </TableCell>
                    <TableCell>
                      <Button
                        variant="text"
                        onClick={() => navigate(`/transfers/${transfer.id}`)}
                        sx={{ textTransform: 'none', fontWeight: 500, display: 'block' }}
                      >
                        {formatDate(transfer.transferDate)}
                        <Typography variant="caption" color="text.secondary" display="block">
                          {transfer.pickupTime || 'Saat belirtilmemiş'}
                        </Typography>
                      </Button>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2" fontWeight="medium">
                        {transfer.guest?.fullName || 'Misafir'}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {transfer.guest?.guestCode}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">
                        {transfer.pickupAddress?.substring(0, 20)}...
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        → {transfer.dropoffAddress?.substring(0, 15)}...
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2" fontWeight="medium">
                        {(transfer as any).groupSize || 1} kişi
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {(transfer as any).childCount || 0} çocuk
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Typography variant="body2">
                        {(transfer as any).driverName || 'Atanmamış'}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {transfer.vehicleId ? `Araç #${transfer.vehicleId}` : 'Araç yok'}
                      </Typography>
                    </TableCell>
                    <TableCell>
                      <Chip
                        label={getStatusLabel(transfer.status || 'pending')}
                        color={getStatusColor(transfer.status || 'pending') as any}
                        size="small"
                      />
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
            {isLoading && (
              <Box sx={{ width: '100%', mt: 2 }}>
                <LinearProgress />
                <Typography variant="body2" color="text.secondary" sx={{ mt: 1, textAlign: 'center' }}>
                  Transferler yükleniyor...
                </Typography>
              </Box>
            )}

            <TablePagination
              component="div"
              count={data?.totalCount || 0}
              page={page}
              onPageChange={handleChangePage}
              rowsPerPage={rowsPerPage}
              onRowsPerPageChange={handleChangeRowsPerPage}
              rowsPerPageOptions={[10, 25, 50, 100]}
              labelRowsPerPage="Sayfa başına:"
              labelDisplayedRows={({ from, to, count }) => {
                if (count === 0) return '0 kayıt';
                if (count === -1) return `${from}-${to} kayıt (toplam bilinmiyor)`;
                return `${from}-${to} / ${count.toLocaleString()} kayıt`;
              }}
              showFirstButton
              showLastButton
            />

            {/* Performance info for large datasets */}
            {data && data.totalCount > 1000 && (
              <Box sx={{ mt: 2, p: 2, bgcolor: 'info.main', color: 'info.contrastText', borderRadius: 1 }}>
                <Typography variant="body2">
                  💡 {data.totalCount.toLocaleString()} kayıt arasından {((page + 1) * rowsPerPage).toLocaleString()} kayıt gösteriliyor.
                  Daha hızlı erişim için filtreleri kullanın.
                </Typography>
              </Box>
            )}
          </TableContainer>
        </Card>
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

      {/* Bulk Operations Dialog */}
      <Dialog open={bulkDialogOpen} onClose={() => setBulkDialogOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle>Toplu İşlemler</DialogTitle>
        <DialogContent>
          <DialogContentText sx={{ mb: 3 }}>
            {selectedTransfers.length} adet transfer için toplu işlem gerçekleştirmek istediğinize emin misiniz?
          </DialogContentText>

          <FormControl fullWidth sx={{ mb: 3 }}>
            <InputLabel>İşlem Türü</InputLabel>
            <Select
              value={bulkOperation}
              onChange={(e) => setBulkOperation(e.target.value)}
              label="İşlem Türü"
            >
              <MenuItem value="status_change">Durum Değiştir</MenuItem>
              <MenuItem value="assign_driver">Şoför Ata</MenuItem>
              <MenuItem value="assign_vehicle">Araç Ata</MenuItem>
              <MenuItem value="cancel">İptal Et</MenuItem>
              <MenuItem value="delete">Sil</MenuItem>
            </Select>
          </FormControl>

          {(bulkOperation === 'status_change' || bulkOperation === 'cancel' || bulkOperation === 'delete') && (
            <TextField
              fullWidth
              label="Açıklama / Sebep"
              value={bulkReason}
              onChange={(e) => setBulkReason(e.target.value)}
              multiline
              rows={3}
              placeholder="İşlem sebebi (opsiyonel)"
              sx={{ mb: 2 }}
            />
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setBulkDialogOpen(false)} disabled={bulkUpdateMutation.isPending || bulkDeleteMutation.isPending}>
            İptal
          </Button>
          <Button
            onClick={handleBulkConfirm}
            variant="contained"
            color={bulkOperation === 'delete' ? 'error' : 'primary'}
            disabled={!bulkOperation || bulkUpdateMutation.isPending || bulkDeleteMutation.isPending}
          >
            {bulkUpdateMutation.isPending || bulkDeleteMutation.isPending ? 'İşleniyor...' : 'Uygula'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

export default TransfersPage


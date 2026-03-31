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
  Tabs,
  Tab,
  Button,
  Chip,
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
} from '@mui/material'
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
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
import { tourService, CreateCityTourRequest, UpdateCityTourRequest, CreateYachtTourRequest, UpdateYachtTourRequest, CityTourFilters, YachtTourFilters } from '../../services/tourService'
import { dropdownService } from '../../services/dropdownService'
import { useNotification } from '../../hooks/useNotification'
import { formatDate } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'
import CityTourForm from '../../components/Tours/CityTourForm'
import YachtTourForm from '../../components/Tours/YachtTourForm'
import { CityTour, YachtTour } from '../../types/tour'

const ToursPage = () => {
  const navigate = useNavigate()

  // Enable real-time updates for tour changes
  useLiveUpdates(['citytour', 'yachttour'])

  const [tabValue, setTabValue] = useState(0)
  const [cityTourPage, setCityTourPage] = useState(0)
  const [yachtTourPage, setYachtTourPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
  const [cityTourFormOpen, setCityTourFormOpen] = useState(false)
  const [yachtTourFormOpen, setYachtTourFormOpen] = useState(false)
  const [editingCityTour, setEditingCityTour] = useState<CityTour | null>(null)
  const [editingYachtTour, setEditingYachtTour] = useState<YachtTour | null>(null)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [tourToDelete, setTourToDelete] = useState<{ type: 'city' | 'yacht'; id: number } | null>(null)

  // Filter states - City Tours
  const [cityTourFiltersOpen, setCityTourFiltersOpen] = useState(false)
  const [cityTourSearchTerm, setCityTourSearchTerm] = useState('')
  const [cityTourStartDate, setCityTourStartDate] = useState<Date | null>(null)
  const [cityTourEndDate, setCityTourEndDate] = useState<Date | null>(null)
  const [cityTourCityId, setCityTourCityId] = useState<number | undefined>(undefined)
  const [cityTourGuestId, setCityTourGuestId] = useState<number | undefined>(undefined)
  const [cityTourPersonnelId, setCityTourPersonnelId] = useState<number | undefined>(undefined)
  const [cityTourSortBy, setCityTourSortBy] = useState('')
  const [cityTourSortOrder, setCityTourSortOrder] = useState<'asc' | 'desc'>('desc')

  // Filter states - Yacht Tours
  const [yachtTourFiltersOpen, setYachtTourFiltersOpen] = useState(false)
  const [yachtTourSearchTerm, setYachtTourSearchTerm] = useState('')
  const [yachtTourStartDate, setYachtTourStartDate] = useState<Date | null>(null)
  const [yachtTourEndDate, setYachtTourEndDate] = useState<Date | null>(null)
  const [yachtTourCityId, setYachtTourCityId] = useState<number | undefined>(undefined)
  const [yachtTourGuestId, setYachtTourGuestId] = useState<number | undefined>(undefined)
  const [yachtTourPersonnelId, setYachtTourPersonnelId] = useState<number | undefined>(undefined)
  const [yachtTourSortBy, setYachtTourSortBy] = useState('')
  const [yachtTourSortOrder, setYachtTourSortOrder] = useState<'asc' | 'desc'>('desc')

  const queryClient = useQueryClient()
  const notification = useNotification()

  // Build filters
  const cityTourFilters: CityTourFilters = {
    ...(cityTourSearchTerm && { searchTerm: cityTourSearchTerm }),
    ...(cityTourStartDate && { startDate: cityTourStartDate.toISOString().split('T')[0] }),
    ...(cityTourEndDate && { endDate: cityTourEndDate.toISOString().split('T')[0] }),
    ...(cityTourCityId && { cityId: cityTourCityId }),
    ...(cityTourGuestId && { guestId: cityTourGuestId }),
    ...(cityTourPersonnelId && { personnelId: cityTourPersonnelId }),
    ...(cityTourSortBy && { sortBy: cityTourSortBy }),
    ...(cityTourSortOrder && { sortOrder: cityTourSortOrder }),
  }

  const yachtTourFilters: YachtTourFilters = {
    ...(yachtTourSearchTerm && { searchTerm: yachtTourSearchTerm }),
    ...(yachtTourStartDate && { startDate: yachtTourStartDate.toISOString().split('T')[0] }),
    ...(yachtTourEndDate && { endDate: yachtTourEndDate.toISOString().split('T')[0] }),
    ...(yachtTourCityId && { cityId: yachtTourCityId }),
    ...(yachtTourGuestId && { guestId: yachtTourGuestId }),
    ...(yachtTourPersonnelId && { personnelId: yachtTourPersonnelId }),
    ...(yachtTourSortBy && { sortBy: yachtTourSortBy }),
    ...(yachtTourSortOrder && { sortOrder: yachtTourSortOrder }),
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

  const { data: cities } = useQuery({
    queryKey: ['cities-dropdown'],
    queryFn: () => dropdownService.getCities(),
  })

  const { data: cityTours, isLoading: cityToursLoading, error: cityToursError } = useQuery({
    queryKey: ['cityTours', cityTourPage + 1, rowsPerPage, cityTourFilters],
    queryFn: () => tourService.getCityTours(cityTourPage + 1, rowsPerPage, cityTourFilters),
  })

  const { data: yachtTours, isLoading: yachtToursLoading, error: yachtToursError } = useQuery({
    queryKey: ['yachtTours', yachtTourPage + 1, rowsPerPage, yachtTourFilters],
    queryFn: () => tourService.getYachtTours(yachtTourPage + 1, rowsPerPage, yachtTourFilters),
  })

  const createCityTourMutation = useMutation({
    mutationFn: (data: CreateCityTourRequest) => tourService.createCityTour(data),
    onSuccess: (response) => {
      queryClient.invalidateQueries({ queryKey: ['cityTours'] })
      setCityTourFormOpen(false)
      notification.showSuccess('Şehir turu başarıyla eklendi.')

      // Otomatik fatura indirme
      if (response.invoicePdfUrl) {
        try {
          window.open(response.invoicePdfUrl, '_blank')
          notification.showSuccess('Fatura PDF\'i indiriliyor...')
        } catch (error) {
          console.error('Fatura indirme hatası:', error)
        }
      }
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Şehir turu eklenirken bir hata oluştu.')
    },
  })

  const updateCityTourMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateCityTourRequest }) =>
      tourService.updateCityTour(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cityTours'] })
      setCityTourFormOpen(false)
      setEditingCityTour(null)
      notification.showSuccess('Şehir turu başarıyla güncellendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Şehir turu güncellenirken bir hata oluştu.')
    },
  })

  const deleteCityTourMutation = useMutation({
    mutationFn: (id: number) => tourService.deleteCityTour(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['cityTours'] })
      setDeleteDialogOpen(false)
      setTourToDelete(null)
      notification.showSuccess('Şehir turu başarıyla silindi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Şehir turu silinirken bir hata oluştu.')
    },
  })

  const createYachtTourMutation = useMutation({
    mutationFn: (data: CreateYachtTourRequest) => tourService.createYachtTour(data),
    onSuccess: (response) => {
      queryClient.invalidateQueries({ queryKey: ['yachtTours'] })
      setYachtTourFormOpen(false)
      notification.showSuccess('Yat turu başarıyla eklendi.')

      // Otomatik fatura indirme
      if (response.invoicePdfUrl) {
        try {
          window.open(response.invoicePdfUrl, '_blank')
          notification.showSuccess('Fatura PDF\'i indiriliyor...')
        } catch (error) {
          console.error('Fatura indirme hatası:', error)
        }
      }
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Yat turu eklenirken bir hata oluştu.')
    },
  })

  const updateYachtTourMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateYachtTourRequest }) =>
      tourService.updateYachtTour(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['yachtTours'] })
      setYachtTourFormOpen(false)
      setEditingYachtTour(null)
      notification.showSuccess('Yat turu başarıyla güncellendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Yat turu güncellenirken bir hata oluştu.')
    },
  })

  const deleteYachtTourMutation = useMutation({
    mutationFn: (id: number) => tourService.deleteYachtTour(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['yachtTours'] })
      setDeleteDialogOpen(false)
      setTourToDelete(null)
      notification.showSuccess('Yat turu başarıyla silindi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Yat turu silinirken bir hata oluştu.')
    },
  })

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue)
  }

  const handleCityTourPageChange = (_event: unknown, newPage: number) => {
    setCityTourPage(newPage)
  }

  const handleYachtTourPageChange = (_event: unknown, newPage: number) => {
    setYachtTourPage(newPage)
  }

  const handleRowsPerPageChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10))
    setCityTourPage(0)
    setYachtTourPage(0)
  }

  const handleOpenCityTourForm = (tour?: CityTour) => {
    if (tour) {
      setEditingCityTour(tour)
    } else {
      setEditingCityTour(null)
    }
    setCityTourFormOpen(true)
  }

  const handleCloseCityTourForm = () => {
    setCityTourFormOpen(false)
    setEditingCityTour(null)
  }

  const handleCityTourFormSubmit = async (data: CreateCityTourRequest | UpdateCityTourRequest) => {
    if (editingCityTour) {
      await updateCityTourMutation.mutateAsync({ id: editingCityTour.id, data })
    } else {
      await createCityTourMutation.mutateAsync(data)
    }
  }

  const handleOpenYachtTourForm = (tour?: YachtTour) => {
    if (tour) {
      setEditingYachtTour(tour)
    } else {
      setEditingYachtTour(null)
    }
    setYachtTourFormOpen(true)
  }

  const handleCloseYachtTourForm = () => {
    setYachtTourFormOpen(false)
    setEditingYachtTour(null)
  }

  const handleYachtTourFormSubmit = async (data: CreateYachtTourRequest | UpdateYachtTourRequest) => {
    if (editingYachtTour) {
      await updateYachtTourMutation.mutateAsync({ id: editingYachtTour.id, data })
    } else {
      await createYachtTourMutation.mutateAsync(data)
    }
  }

  const handleDeleteClick = (type: 'city' | 'yacht', id: number) => {
    setTourToDelete({ type, id })
    setDeleteDialogOpen(true)
  }

  const handleDeleteConfirm = () => {
    if (tourToDelete) {
      if (tourToDelete.type === 'city') {
        deleteCityTourMutation.mutate(tourToDelete.id)
      } else {
        deleteYachtTourMutation.mutate(tourToDelete.id)
      }
    }
  }

  const handleClearCityTourFilters = () => {
    setCityTourSearchTerm('')
    setCityTourStartDate(null)
    setCityTourEndDate(null)
    setCityTourCityId(undefined)
    setCityTourGuestId(undefined)
    setCityTourPersonnelId(undefined)
    setCityTourSortBy('')
    setCityTourSortOrder('desc')
    setCityTourPage(0)
  }

  const handleClearYachtTourFilters = () => {
    setYachtTourSearchTerm('')
    setYachtTourStartDate(null)
    setYachtTourEndDate(null)
    setYachtTourCityId(undefined)
    setYachtTourGuestId(undefined)
    setYachtTourPersonnelId(undefined)
    setYachtTourSortBy('')
    setYachtTourSortOrder('desc')
    setYachtTourPage(0)
  }

  const hasActiveCityTourFilters = cityTourSearchTerm || cityTourStartDate || cityTourEndDate || cityTourCityId || cityTourGuestId || cityTourPersonnelId || cityTourSortBy
  const hasActiveYachtTourFilters = yachtTourSearchTerm || yachtTourStartDate || yachtTourEndDate || yachtTourCityId || yachtTourGuestId || yachtTourPersonnelId || yachtTourSortBy

  const isLoading = cityToursLoading || yachtToursLoading

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={6} />
  }

  if (cityToursError || yachtToursError) {
    return (
      <ContentState
        state="error"
        title="Turlar yüklenemedi"
        description={cityToursError?.message || yachtToursError?.message || "Lütfen daha sonra tekrar deneyin."}
        actionLabel="Tekrar dene"
        onAction={() => {
          queryClient.refetchQueries({ queryKey: ['cityTours'] })
          queryClient.refetchQueries({ queryKey: ['yachtTours'] })
        }}
      />
    )
  }

  return (
    <Box className="fade-in" p={3}>
      <Typography variant="h4" component="h1" className="premium-gradient-text" sx={{ mb: 3, fontWeight: 800 }}>
        Tur Yönetimi
      </Typography>

      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Paper sx={{ flex: 1, mr: 2 }}>
          <Tabs value={tabValue} onChange={handleTabChange}>
            <Tab label="Şehir Turları" />
            <Tab label="Yat Turları" />
          </Tabs>
        </Paper>
        {tabValue === 0 ? (
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => handleOpenCityTourForm()}
          >
            Yeni Şehir Turu
          </Button>
        ) : (
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => handleOpenYachtTourForm()}
          >
            Yeni Yat Turu
          </Button>
        )}
      </Box>

      {tabValue === 0 && (
        <>
          {/* City Tour Filters */}
          <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={tr}>
            <Paper sx={{ p: 2, mb: 3 }}>
              <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', mb: 2 }}>
                <TextField
                  placeholder="Ara (dil, not)..."
                  value={cityTourSearchTerm}
                  onChange={(e) => {
                    setCityTourSearchTerm(e.target.value)
                    setCityTourPage(0)
                  }}
                  size="small"
                  fullWidth
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <SearchIcon />
                      </InputAdornment>
                    ),
                    endAdornment: cityTourSearchTerm && (
                      <InputAdornment position="end">
                        <IconButton
                          size="small"
                          onClick={() => {
                            setCityTourSearchTerm('')
                            setCityTourPage(0)
                          }}
                        >
                          <ClearIcon fontSize="small" />
                        </IconButton>
                      </InputAdornment>
                    ),
                  }}
                />
                <Button
                  variant={cityTourFiltersOpen ? 'contained' : 'outlined'}
                  startIcon={<FilterListIcon />}
                  endIcon={cityTourFiltersOpen ? <ExpandLessIcon /> : <ExpandMoreIcon />}
                  onClick={() => setCityTourFiltersOpen(!cityTourFiltersOpen)}
                >
                  Filtreler
                </Button>
                {hasActiveCityTourFilters && (
                  <Button
                    variant="outlined"
                    color="error"
                    startIcon={<ClearIcon />}
                    onClick={handleClearCityTourFilters}
                  >
                    Temizle
                  </Button>
                )}
              </Box>

              <Collapse in={cityTourFiltersOpen}>
                <Grid container spacing={2}>
                  <Grid item xs={12} sm={6} md={3}>
                    <DatePicker
                      label="Başlangıç Tarihi"
                      value={cityTourStartDate}
                      onChange={(newValue) => {
                        setCityTourStartDate(newValue)
                        setCityTourPage(0)
                      }}
                      slotProps={{
                        textField: { size: 'small', fullWidth: true },
                      }}
                    />
                  </Grid>
                  <Grid item xs={12} sm={6} md={3}>
                    <DatePicker
                      label="Bitiş Tarihi"
                      value={cityTourEndDate}
                      onChange={(newValue) => {
                        setCityTourEndDate(newValue)
                        setCityTourPage(0)
                      }}
                      slotProps={{
                        textField: { size: 'small', fullWidth: true },
                      }}
                    />
                  </Grid>
                  <Grid item xs={12} sm={6} md={3}>
                    <FormControl fullWidth size="small">
                      <InputLabel>Şehir</InputLabel>
                      <Select
                        value={cityTourCityId || ''}
                        label="Şehir"
                        onChange={(e) => {
                          setCityTourCityId(e.target.value ? Number(e.target.value) : undefined)
                          setCityTourPage(0)
                        }}
                      >
                        <MenuItem value="">Tümü</MenuItem>
                        {cities?.map((city) => (
                          <MenuItem key={city.id} value={city.id}>
                            {city.cityName}
                          </MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  </Grid>
                  <Grid item xs={12} sm={6} md={3}>
                    <FormControl fullWidth size="small">
                      <InputLabel>Misafir</InputLabel>
                      <Select
                        value={cityTourGuestId || ''}
                        label="Misafir"
                        onChange={(e) => {
                          setCityTourGuestId(e.target.value ? Number(e.target.value) : undefined)
                          setCityTourPage(0)
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
                        value={cityTourPersonnelId || ''}
                        label="Personel"
                        onChange={(e) => {
                          setCityTourPersonnelId(e.target.value ? Number(e.target.value) : undefined)
                          setCityTourPage(0)
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
                      <InputLabel>Sırala</InputLabel>
                      <Select
                        value={cityTourSortBy}
                        label="Sırala"
                        onChange={(e) => {
                          setCityTourSortBy(e.target.value)
                          setCityTourPage(0)
                        }}
                      >
                        <MenuItem value="">Varsayılan</MenuItem>
                        <MenuItem value="TourDate">Tur Tarihi</MenuItem>
                        <MenuItem value="Price">Fiyat</MenuItem>
                        <MenuItem value="DurationHours">Süre</MenuItem>
                        <MenuItem value="CreatedDate">Oluşturulma Tarihi</MenuItem>
                      </Select>
                    </FormControl>
                  </Grid>
                  {cityTourSortBy && (
                    <Grid item xs={12} sm={6} md={3}>
                      <FormControl fullWidth size="small">
                        <InputLabel>Sıralama Yönü</InputLabel>
                        <Select
                          value={cityTourSortOrder}
                          label="Sıralama Yönü"
                          onChange={(e) => {
                            setCityTourSortOrder(e.target.value as 'asc' | 'desc')
                            setCityTourPage(0)
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

          {cityToursLoading ? (
            <ContentState state="loading" skeletonLines={6} />
          ) : cityToursError ? (
            <ContentState
              state="error"
              title="Şehir turları yüklenemedi"
              description="Lütfen daha sonra tekrar deneyin."
              actionLabel="Tekrar dene"
              onAction={() => {
                queryClient.refetchQueries({ queryKey: ['cityTours'] })
                queryClient.refetchQueries({ queryKey: ['yachtTours'] })
              }}
            />
          ) : !cityTours?.data || cityTours.data.length === 0 ? (
            <ContentState
              state="empty"
              title="Şehir turu bulunamadı"
              description="Henüz kayıtlı şehir turu bulunmamaktadır."
            />
          ) : (
            <TableContainer component={Paper}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell><strong>Tarih</strong></TableCell>
                    <TableCell><strong>Misafir/Grup</strong></TableCell>
                    <TableCell><strong>Kapasite</strong></TableCell>
                    <TableCell><strong>Hava</strong></TableCell>
                    <TableCell><strong>Rehber</strong></TableCell>
                    <TableCell><strong>Araç</strong></TableCell>
                    <TableCell><strong>Onay</strong></TableCell>
                    <TableCell><strong>İşlemler</strong></TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {cityTours?.data.map((tour) => (
                    <TableRow key={tour.id} hover>
                      <TableCell>
                        <Button
                          variant="text"
                          onClick={() => navigate(`/tours/city/${tour.id}`)}
                          sx={{ textTransform: 'none', fontWeight: 500 }}
                        >
                          {formatDate(tour.tourDate)}
                        </Button>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" fontWeight="medium">
                          {(tour as any).ownerGuest?.fullName || 'Misafir'}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          {((tour as any).adultCount || 0) + ((tour as any).childCount || 0) + ((tour as any).infantCount || 0)} kişi toplam
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                          <Typography variant="body2">
                            {((tour as any).adultCount || 0) + ((tour as any).childCount || 0)}/
                            {(tour as any).maximumParticipantCount || 20}
                          </Typography>
                          {((tour as any).adultCount || 0) + ((tour as any).childCount || 0) > (tour as any).maximumParticipantCount && (
                            <Chip label="DOLU" color="error" size="small" />
                          )}
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={(tour as any).weatherDependent ? 'Bağımlı' : 'Normal'}
                          color={(tour as any).weatherDependent ? 'warning' : 'success'}
                          size="small"
                        />
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {(tour as any).guideName || 'Atanmamış'}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          {(tour as any).guideLanguages || '-'}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {tour.vehicleId ? `Araç #${tour.vehicleId}` : 'Atanmamış'}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        {(tour as any).tourConfirmationTime ? (
                          <Chip label="Onaylandı" color="success" size="small" />
                        ) : (
                          <Chip label="Bekliyor" color="warning" size="small" />
                        )}
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', gap: 1 }}>
                          <Button
                            variant="outlined"
                            size="small"
                            onClick={() => navigate(`/tours/city/${tour.id}`)}
                          >
                            Detay
                          </Button>
                          <Tooltip title="Düzenle">
                            <IconButton
                              size="small"
                              color="primary"
                              onClick={() => handleOpenCityTourForm(tour)}
                            >
                              <EditIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Sil">
                            <IconButton
                              size="small"
                              color="error"
                              onClick={() => handleDeleteClick('city', tour.id)}
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
                count={cityTours?.totalCount || 0}
                page={cityTourPage}
                onPageChange={handleCityTourPageChange}
                rowsPerPage={rowsPerPage}
                onRowsPerPageChange={handleRowsPerPageChange}
                rowsPerPageOptions={[5, 10, 25, 50]}
                labelRowsPerPage="Sayfa başına:"
                labelDisplayedRows={({ from, to, count }) => `${from}-${to} / ${count}`}
              />
            </TableContainer>
          )}
        </>
      )}

      {tabValue === 1 && (
        <>
          {/* Yacht Tour Filters */}
          <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={tr}>
            <Paper sx={{ p: 2, mb: 3 }}>
              <Box sx={{ display: 'flex', gap: 2, alignItems: 'center', mb: 2 }}>
                <TextField
                  placeholder="Ara (yat adı, özel istek)..."
                  value={yachtTourSearchTerm}
                  onChange={(e) => {
                    setYachtTourSearchTerm(e.target.value)
                    setYachtTourPage(0)
                  }}
                  size="small"
                  fullWidth
                  InputProps={{
                    startAdornment: (
                      <InputAdornment position="start">
                        <SearchIcon />
                      </InputAdornment>
                    ),
                    endAdornment: yachtTourSearchTerm && (
                      <InputAdornment position="end">
                        <IconButton
                          size="small"
                          onClick={() => {
                            setYachtTourSearchTerm('')
                            setYachtTourPage(0)
                          }}
                        >
                          <ClearIcon fontSize="small" />
                        </IconButton>
                      </InputAdornment>
                    ),
                  }}
                />
                <Button
                  variant={yachtTourFiltersOpen ? 'contained' : 'outlined'}
                  startIcon={<FilterListIcon />}
                  endIcon={yachtTourFiltersOpen ? <ExpandLessIcon /> : <ExpandMoreIcon />}
                  onClick={() => setYachtTourFiltersOpen(!yachtTourFiltersOpen)}
                >
                  Filtreler
                </Button>
                {hasActiveYachtTourFilters && (
                  <Button
                    variant="outlined"
                    color="error"
                    startIcon={<ClearIcon />}
                    onClick={handleClearYachtTourFilters}
                  >
                    Temizle
                  </Button>
                )}
              </Box>

              <Collapse in={yachtTourFiltersOpen}>
                <Grid container spacing={2}>
                  <Grid item xs={12} sm={6} md={3}>
                    <DatePicker
                      label="Başlangıç Tarihi"
                      value={yachtTourStartDate}
                      onChange={(newValue) => {
                        setYachtTourStartDate(newValue)
                        setYachtTourPage(0)
                      }}
                      slotProps={{
                        textField: { size: 'small', fullWidth: true },
                      }}
                    />
                  </Grid>
                  <Grid item xs={12} sm={6} md={3}>
                    <DatePicker
                      label="Bitiş Tarihi"
                      value={yachtTourEndDate}
                      onChange={(newValue) => {
                        setYachtTourEndDate(newValue)
                        setYachtTourPage(0)
                      }}
                      slotProps={{
                        textField: { size: 'small', fullWidth: true },
                      }}
                    />
                  </Grid>
                  <Grid item xs={12} sm={6} md={3}>
                    <FormControl fullWidth size="small">
                      <InputLabel>Şehir</InputLabel>
                      <Select
                        value={yachtTourCityId || ''}
                        label="Şehir"
                        onChange={(e) => {
                          setYachtTourCityId(e.target.value ? Number(e.target.value) : undefined)
                          setYachtTourPage(0)
                        }}
                      >
                        <MenuItem value="">Tümü</MenuItem>
                        {cities?.map((city) => (
                          <MenuItem key={city.id} value={city.id}>
                            {city.cityName}
                          </MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                  </Grid>
                  <Grid item xs={12} sm={6} md={3}>
                    <FormControl fullWidth size="small">
                      <InputLabel>Misafir</InputLabel>
                      <Select
                        value={yachtTourGuestId || ''}
                        label="Misafir"
                        onChange={(e) => {
                          setYachtTourGuestId(e.target.value ? Number(e.target.value) : undefined)
                          setYachtTourPage(0)
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
                        value={yachtTourPersonnelId || ''}
                        label="Personel"
                        onChange={(e) => {
                          setYachtTourPersonnelId(e.target.value ? Number(e.target.value) : undefined)
                          setYachtTourPage(0)
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
                      <InputLabel>Sırala</InputLabel>
                      <Select
                        value={yachtTourSortBy}
                        label="Sırala"
                        onChange={(e) => {
                          setYachtTourSortBy(e.target.value)
                          setYachtTourPage(0)
                        }}
                      >
                        <MenuItem value="">Varsayılan</MenuItem>
                        <MenuItem value="TourDate">Tur Tarihi</MenuItem>
                        <MenuItem value="Price">Fiyat</MenuItem>
                        <MenuItem value="NumberOfPeople">Kişi Sayısı</MenuItem>
                        <MenuItem value="CreatedDate">Oluşturulma Tarihi</MenuItem>
                      </Select>
                    </FormControl>
                  </Grid>
                  {yachtTourSortBy && (
                    <Grid item xs={12} sm={6} md={3}>
                      <FormControl fullWidth size="small">
                        <InputLabel>Sıralama Yönü</InputLabel>
                        <Select
                          value={yachtTourSortOrder}
                          label="Sıralama Yönü"
                          onChange={(e) => {
                            setYachtTourSortOrder(e.target.value as 'asc' | 'desc')
                            setYachtTourPage(0)
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

          {yachtToursLoading ? (
            <ContentState state="loading" skeletonLines={6} />
          ) : yachtToursError ? (
            <ContentState
              state="error"
              title="Yat turları yüklenemedi"
              description="Lütfen daha sonra tekrar deneyin."
              actionLabel="Tekrar dene"
              onAction={() => {
                queryClient.refetchQueries({ queryKey: ['cityTours'] })
                queryClient.refetchQueries({ queryKey: ['yachtTours'] })
              }}
            />
          ) : !yachtTours?.data || yachtTours.data.length === 0 ? (
            <ContentState
              state="empty"
              title="Yat turu bulunamadı"
              description="Henüz kayıtlı yat turu bulunmamaktadır."
            />
          ) : (
            <TableContainer component={Paper}>
              <Table>
                <TableHead>
                  <TableRow>
                    <TableCell><strong>Tarih</strong></TableCell>
                    <TableCell><strong>Misafir</strong></TableCell>
                    <TableCell><strong>Yat/Kaptan</strong></TableCell>
                    <TableCell><strong>Güvenlik</strong></TableCell>
                    <TableCell><strong>Hava</strong></TableCell>
                    <TableCell><strong>Yakıt</strong></TableCell>
                    <TableCell><strong>İşlemler</strong></TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {yachtTours?.data.map((tour) => (
                    <TableRow key={tour.id} hover>
                      <TableCell>
                        <Button
                          variant="text"
                          onClick={() => navigate(`/tours/yacht/${tour.id}`)}
                          sx={{ textTransform: 'none', fontWeight: 500 }}
                        >
                          {formatDate(tour.tourDate)}
                        </Button>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2" fontWeight="medium">
                          {(tour as any).ownerGuest?.fullName || 'Misafir'}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          {tour.numberOfPeople} kişi
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {tour.yachtName || 'Atanmamış'}
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                          Kaptan: {(tour as any).captainId ? `ID ${(tour as any).captainId}` : 'Atanmamış'}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5 }}>
                          <Chip
                            label={(tour as any).lifeJacketsProvided ? 'C.Yelek ✓' : 'C.Yelek ✗'}
                            color={(tour as any).lifeJacketsProvided ? 'success' : 'error'}
                            size="small"
                            sx={{ fontSize: '0.7rem', height: 20 }}
                          />
                          <Chip
                            label={(tour as any).safetyEquipmentCheck ? 'Ekipman ✓' : 'Ekipman ✗'}
                            color={(tour as any).safetyEquipmentCheck ? 'success' : 'error'}
                            size="small"
                            sx={{ fontSize: '0.7rem', height: 20 }}
                          />
                        </Box>
                      </TableCell>
                      <TableCell>
                        <Chip
                          label={(tour as any).weatherCheckTime ? 'Kontrol ✓' : 'Bekliyor'}
                          color={(tour as any).weatherCheckTime ? 'success' : 'warning'}
                          size="small"
                        />
                      </TableCell>
                      <TableCell>
                        <Typography variant="body2">
                          {(tour as any).fuelLevelCheck || 'Kontrol Yok'}
                        </Typography>
                      </TableCell>
                      <TableCell>
                        <Box sx={{ display: 'flex', gap: 1 }}>
                          <Button
                            variant="outlined"
                            size="small"
                            onClick={() => navigate(`/tours/yacht/${tour.id}`)}
                          >
                            Detay
                          </Button>
                          <Tooltip title="Düzenle">
                            <IconButton
                              size="small"
                              color="primary"
                              onClick={() => handleOpenYachtTourForm(tour)}
                            >
                              <EditIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                          <Tooltip title="Sil">
                            <IconButton
                              size="small"
                              color="error"
                              onClick={() => handleDeleteClick('yacht', tour.id)}
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
                count={yachtTours?.totalCount || 0}
                page={yachtTourPage}
                onPageChange={handleYachtTourPageChange}
                rowsPerPage={rowsPerPage}
                onRowsPerPageChange={handleRowsPerPageChange}
                rowsPerPageOptions={[5, 10, 25, 50]}
                labelRowsPerPage="Sayfa başına:"
                labelDisplayedRows={({ from, to, count }) => `${from}-${to} / ${count}`}
              />
            </TableContainer>
          )}
        </>
      )}

      <CityTourForm
        open={cityTourFormOpen}
        onClose={handleCloseCityTourForm}
        onSubmit={handleCityTourFormSubmit}
        cityTour={editingCityTour}
        isLoading={createCityTourMutation.isPending || updateCityTourMutation.isPending}
      />

      <YachtTourForm
        open={yachtTourFormOpen}
        onClose={handleCloseYachtTourForm}
        onSubmit={handleYachtTourFormSubmit}
        yachtTour={editingYachtTour}
        isLoading={createYachtTourMutation.isPending || updateYachtTourMutation.isPending}
      />

      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>Tur Sil</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Bu turu silmek istediğinize emin misiniz? Bu işlem geri alınamaz.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)} disabled={deleteCityTourMutation.isPending || deleteYachtTourMutation.isPending}>
            İptal
          </Button>
          <Button
            onClick={handleDeleteConfirm}
            color="error"
            variant="contained"
            disabled={deleteCityTourMutation.isPending || deleteYachtTourMutation.isPending}
          >
            {(deleteCityTourMutation.isPending || deleteYachtTourMutation.isPending) ? 'Siliniyor...' : 'Sil'}
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

export default ToursPage


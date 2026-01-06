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
  Chip,
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
  Clear as ClearIcon,
  FilterList as FilterListIcon,
  ExpandMore as ExpandMoreIcon,
  ExpandLess as ExpandLessIcon,
} from '@mui/icons-material'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { servicePackageService, CreateServicePackageRequest, UpdateServicePackageRequest, ServicePackageFilters, ServicePackage } from '../../services/servicePackageService'
import { formatDate, formatCurrency } from '../../utils/formatters'
import { PackageType, PackageTypeLabels } from '../../types/enums'
import ContentState from '../../components/Feedback/ContentState'
import ServicePackageForm from '../../components/ServicePackages/ServicePackageForm'
import { useNotification } from '../../hooks/useNotification'

const ServicePackagesPage = () => {
  const [page, setPage] = useState(0)
  const [rowsPerPage, setRowsPerPage] = useState(10)
  const [formOpen, setFormOpen] = useState(false)
  const [editingPackage, setEditingPackage] = useState<ServicePackage | null>(null)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [packageToDelete, setPackageToDelete] = useState<ServicePackage | null>(null)
  const [filtersOpen, setFiltersOpen] = useState(false)
  
  // Filter states
  const [searchTerm, setSearchTerm] = useState('')
  const [packageType, setPackageType] = useState<PackageType | ''>('')
  const [isActive, setIsActive] = useState<boolean | ''>('')
  const [sortBy, setSortBy] = useState('')
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc')

  const queryClient = useQueryClient()
  const notification = useNotification()

  // Build filters object
  const filters: ServicePackageFilters = {
    ...(searchTerm && { searchTerm }),
    ...(packageType && { packageType: packageType as PackageType }),
    ...(isActive !== '' && { isActive: isActive === true }),
    ...(sortBy && { sortBy }),
    ...(sortOrder && { sortOrder }),
  }

  const { data, isLoading, error } = useQuery({
    queryKey: ['service-packages', page + 1, rowsPerPage, filters],
    queryFn: () => servicePackageService.getServicePackages(page + 1, rowsPerPage, filters),
  })

  const createMutation = useMutation({
    mutationFn: (data: CreateServicePackageRequest) => servicePackageService.createServicePackage(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['service-packages'] })
      setFormOpen(false)
      notification.showSuccess('Servis paketi başarıyla eklendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Servis paketi eklenirken bir hata oluştu.')
    },
  })

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: number; data: UpdateServicePackageRequest }) =>
      servicePackageService.updateServicePackage(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['service-packages'] })
      setFormOpen(false)
      setEditingPackage(null)
      notification.showSuccess('Servis paketi başarıyla güncellendi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Servis paketi güncellenirken bir hata oluştu.')
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (id: number) => servicePackageService.deleteServicePackage(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['service-packages'] })
      setDeleteDialogOpen(false)
      setPackageToDelete(null)
      notification.showSuccess('Servis paketi başarıyla silindi.')
    },
    onError: (error: any) => {
      notification.showError(error?.response?.data?.message || 'Servis paketi silinirken bir hata oluştu.')
    },
  })

  const handleChangePage = (_event: unknown, newPage: number) => {
    setPage(newPage)
  }

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setRowsPerPage(parseInt(event.target.value, 10))
    setPage(0)
  }

  const handleOpenForm = (pkg?: ServicePackage) => {
    if (pkg) {
      setEditingPackage(pkg)
    } else {
      setEditingPackage(null)
    }
    setFormOpen(true)
  }

  const handleCloseForm = () => {
    setFormOpen(false)
    setEditingPackage(null)
  }

  const handleFormSubmit = async (data: CreateServicePackageRequest | UpdateServicePackageRequest) => {
    if (editingPackage) {
      await updateMutation.mutateAsync({ id: editingPackage.id, data: data as UpdateServicePackageRequest })
    } else {
      await createMutation.mutateAsync(data as CreateServicePackageRequest)
    }
  }

  const handleDeleteClick = (pkg: ServicePackage) => {
    setPackageToDelete(pkg)
    setDeleteDialogOpen(true)
  }

  const handleDeleteConfirm = () => {
    if (packageToDelete) {
      deleteMutation.mutate(packageToDelete.id)
    }
  }

  const handleClearFilters = () => {
    setSearchTerm('')
    setPackageType('')
    setIsActive('')
    setSortBy('')
    setSortOrder('asc')
  }

  const getPackageTypeLabel = (type: PackageType | number) => {
    return PackageTypeLabels[type as PackageType] || 'Bilinmeyen'
  }

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={6} />
  }

  if (error) {
    return (
      <ContentState
        state="error"
        title="Hata"
        description="Servis paketleri yüklenirken bir hata oluştu."
      />
    )
  }

  return (
    <Box>
      <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
        <Typography variant="h4">Servis Paketleri</Typography>
        <Box>
          <Button
            variant="outlined"
            startIcon={<FilterListIcon />}
            onClick={() => setFiltersOpen(!filtersOpen)}
            sx={{ mr: 1 }}
          >
            Filtreler
            {filtersOpen ? <ExpandLessIcon /> : <ExpandMoreIcon />}
          </Button>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => handleOpenForm()}
          >
            Yeni Paket
          </Button>
        </Box>
      </Box>

      {/* Filters */}
      <Collapse in={filtersOpen}>
        <Paper sx={{ p: 2, mb: 2 }}>
          <Grid container spacing={2} alignItems="center">
            <Grid item xs={12} md={4}>
              <TextField
                fullWidth
                size="small"
                label="Ara"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <SearchIcon />
                    </InputAdornment>
                  ),
                  endAdornment: searchTerm && (
                    <InputAdornment position="end">
                      <IconButton size="small" onClick={() => setSearchTerm('')}>
                        <ClearIcon />
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
              />
            </Grid>
            <Grid item xs={12} md={3}>
              <FormControl fullWidth size="small">
                <InputLabel>Paket Tipi</InputLabel>
                <Select
                  value={packageType}
                  label="Paket Tipi"
                  onChange={(e) => setPackageType(e.target.value as PackageType | '')}
                >
                  <MenuItem value="">Tümü</MenuItem>
                  {Object.entries(PackageTypeLabels).map(([key, label]) => (
                    <MenuItem key={key} value={key}>
                      {label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={2}>
              <FormControl fullWidth size="small">
                <InputLabel>Durum</InputLabel>
                <Select
                  value={isActive}
                  label="Durum"
                  onChange={(e) => setIsActive(e.target.value as boolean | '')}
                >
                  <MenuItem value="">Tümü</MenuItem>
                  <MenuItem value="true">Aktif</MenuItem>
                  <MenuItem value="false">Pasif</MenuItem>
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={12} md={3}>
              <Box display="flex" gap={1}>
                <Button
                  variant="outlined"
                  size="small"
                  onClick={handleClearFilters}
                  startIcon={<ClearIcon />}
                >
                  Temizle
                </Button>
              </Box>
            </Grid>
          </Grid>
        </Paper>
      </Collapse>

      {/* Table */}
      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Paket Adı</TableCell>
              <TableCell>Tip</TableCell>
              <TableCell>Başlangıç Tarihi</TableCell>
              <TableCell>Bitiş Tarihi</TableCell>
              <TableCell>Toplam Fiyat</TableCell>
              <TableCell>İndirim</TableCell>
              <TableCell>Final Fiyat</TableCell>
              <TableCell>Durum</TableCell>
              <TableCell>Oluşturulma</TableCell>
              <TableCell align="right">İşlemler</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {data?.data.length === 0 ? (
              <TableRow>
                <TableCell colSpan={10} align="center">
                  <Typography variant="body2" color="text.secondary" sx={{ py: 3 }}>
                    Servis paketi bulunamadı
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              data?.data.map((pkg) => (
                <TableRow key={pkg.id} hover>
                  <TableCell>{pkg.packageName}</TableCell>
                  <TableCell>
                    <Chip
                      label={getPackageTypeLabel(pkg.packageType)}
                      size="small"
                      color="primary"
                    />
                  </TableCell>
                  <TableCell>{pkg.startDate ? formatDate(pkg.startDate) : '-'}</TableCell>
                  <TableCell>{pkg.endDate ? formatDate(pkg.endDate) : '-'}</TableCell>
                  <TableCell>{formatCurrency(pkg.totalPrice, pkg.currency)}</TableCell>
                  <TableCell>
                    {pkg.discountPercentage ? `${pkg.discountPercentage}%` : '-'}
                  </TableCell>
                  <TableCell>{formatCurrency(pkg.finalPrice, pkg.currency)}</TableCell>
                  <TableCell>
                    <Chip
                      label={pkg.isActive ? 'Aktif' : 'Pasif'}
                      size="small"
                      color={pkg.isActive ? 'success' : 'default'}
                    />
                  </TableCell>
                  <TableCell>{formatDate(pkg.createdDate)}</TableCell>
                  <TableCell align="right">
                    <Tooltip title="Düzenle">
                      <IconButton
                        size="small"
                        onClick={() => handleOpenForm(pkg)}
                        color="primary"
                      >
                        <EditIcon />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Sil">
                      <IconButton
                        size="small"
                        onClick={() => handleDeleteClick(pkg)}
                        color="error"
                      >
                        <DeleteIcon />
                      </IconButton>
                    </Tooltip>
                  </TableCell>
                </TableRow>
              ))
            )}
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
        />
      </TableContainer>

      {/* Form Dialog */}
      <ServicePackageForm
        open={formOpen}
        onClose={handleCloseForm}
        onSubmit={handleFormSubmit}
        servicePackage={editingPackage}
        isLoading={createMutation.isPending || updateMutation.isPending}
      />

      {/* Delete Confirmation Dialog */}
      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>Servis Paketi Sil</DialogTitle>
        <DialogContent>
          <DialogContentText>
            "{packageToDelete?.packageName}" adlı servis paketini silmek istediğinizden emin misiniz? Bu işlem geri alınamaz.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>İptal</Button>
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

export default ServicePackagesPage


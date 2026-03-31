// Copyright (c) 2026 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

import { useState, useEffect, useCallback } from 'react'
import {
  Box,
  Grid,
  Card,
  CardContent,
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
  MenuItem,
  Stack,
  alpha,
  useTheme
} from '@mui/material'
import {
  Refresh as RefreshIcon,
  CheckCircle as CleanIcon,
  Error as DirtyIcon,
  AssignmentInd as AssignIcon,
  History as HistoryIcon,
  CleaningServices as CleaningIcon,
  Search as SearchIcon,
  CheckCircle as CheckCircleIcon
} from '@mui/icons-material'
import housekeepingService, { RoomStatus, RoomCleaningStatus, RoomOccupancyStatus } from '../../services/housekeepingService'
import { formatDateTime } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'

const HousekeepingPage = () => {
  const theme = useTheme()
  const [rooms, setRooms] = useState<RoomStatus[]>([])
  const [loading, setLoading] = useState(true)
  const [searchTerm, setSearchTerm] = useState('')
  const [filterStatus, setFilterStatus] = useState<string>('All')

  // Dialog states
  const [updateDialog, setUpdateDialog] = useState<{ open: boolean, room?: RoomStatus }>({ open: false })
  const [selectedStatus, setSelectedStatus] = useState<RoomCleaningStatus>(RoomCleaningStatus.Dirty)
  const [selectedOccupancy, setSelectedOccupancy] = useState<RoomOccupancyStatus>(RoomOccupancyStatus.Vacant)
  const [notes, setNotes] = useState('')

  const fetchRooms = useCallback(async () => {
    setLoading(true)
    try {
      const response = await housekeepingService.getRoomStatuses()
      if (response.success) {
        setRooms(response.data || [])
      }
    } catch (err) {
      console.error('Oda durumları yüklenirken bir hata oluştu.', err)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchRooms()
  }, [fetchRooms])

  const handleUpdateStatus = async () => {
    if (!updateDialog.room) return

    try {
      const response = await housekeepingService.updateRoomStatus(updateDialog.room.id, {
        cleaningStatus: selectedStatus,
        occupancyStatus: selectedOccupancy,
        notes: notes
      })

      if (response.success) {
        setUpdateDialog({ open: false })
        fetchRooms()
      }
    } catch (err) {
      console.error('Update failed', err)
    }
  }

  const handleMarkCleaned = async (id: number) => {
    try {
      const response = await housekeepingService.markAsCleaned(id)
      if (response.success) {
        fetchRooms()
      }
    } catch (err) {
      console.error('Mark as cleaned failed', err)
    }
  }

  const getCleaningStatusColor = (status: RoomCleaningStatus): "success" | "info" | "warning" | "error" | "inherit" => {
    switch (status) {
      case RoomCleaningStatus.Clean: return 'success'
      case RoomCleaningStatus.Inspected: return 'info'
      case RoomCleaningStatus.Cleaning: return 'warning'
      case RoomCleaningStatus.Dirty: return 'error'
      case RoomCleaningStatus.OutOfOrder: return 'inherit'
      default: return 'inherit'
    }
  }

  const getOccupancyStatusColor = (status: RoomOccupancyStatus) => {
    switch (status) {
      case RoomOccupancyStatus.Occupied: return 'primary'
      case RoomOccupancyStatus.Vacant: return 'success'
      case RoomOccupancyStatus.ExpectedArrival: return 'info'
      case RoomOccupancyStatus.ExpectedDeparture: return 'warning'
      default: return 'default'
    }
  }

  const filteredRooms = rooms.filter(room => {
    const matchesSearch = room.roomNumber.toLowerCase().includes(searchTerm.toLowerCase())
    const matchesFilter = filterStatus === 'All' || room.cleaningStatus === filterStatus
    return matchesSearch && matchesFilter
  })

  if (loading && rooms.length === 0) {
    return <ContentState state="loading" />
  }

  return (
    <Box sx={{ p: 4 }}>
      {/* Header Area */}
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 4 }}>
        <Box>
          <Typography variant="h4" sx={{ fontWeight: 800, color: 'primary.main', mb: 1, display: 'flex', alignItems: 'center', gap: 2 }}>
            <CleaningIcon fontSize="large" />
            Kat Hizmetleri Paneli
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Gerçek zamanlı oda temizlik ve doluluk durumlarını yönetin.
          </Typography>
        </Box>
        <Stack direction="row" spacing={2}>
          <Tooltip title="Yenile">
            <IconButton onClick={fetchRooms} sx={{ bgcolor: alpha(theme.palette.primary.main, 0.1) }}>
              <RefreshIcon color="primary" />
            </IconButton>
          </Tooltip>
        </Stack>
      </Box>

      {/* Filters & Search */}
      <Card sx={{ mb: 4, borderRadius: 4, boxShadow: '0 4px 20px rgba(0,0,0,0.05)' }}>
        <CardContent sx={{ p: 3 }}>
          <Grid container spacing={2} alignItems="center">
            <Grid item xs={12} md={4}>
              <TextField
                fullWidth
                placeholder="Oda numarasıyla ara..."
                variant="outlined"
                size="small"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                InputProps={{
                  startAdornment: <SearchIcon sx={{ color: 'text.secondary', mr: 1 }} />
                }}
              />
            </Grid>
            <Grid item xs={12} md={3}>
              <TextField
                select
                fullWidth
                size="small"
                label="Temizlik Durumu"
                value={filterStatus}
                onChange={(e) => setFilterStatus(e.target.value)}
              >
                <MenuItem value="All">Tümü</MenuItem>
                <MenuItem value={RoomCleaningStatus.Clean}>Temiz</MenuItem>
                <MenuItem value={RoomCleaningStatus.Dirty}>Kirli</MenuItem>
                <MenuItem value={RoomCleaningStatus.Cleaning}>Temizleniyor</MenuItem>
                <MenuItem value={RoomCleaningStatus.OutOfOrder}>Arızalı</MenuItem>
              </TextField>
            </Grid>
            <Grid item xs={12} md={5}>
              <Stack direction="row" spacing={1} justifyContent="flex-end">
                <Chip icon={<DirtyIcon />} label={`Kirli: ${rooms.filter(r => r.cleaningStatus === RoomCleaningStatus.Dirty).length}`} color="error" variant="outlined" />
                <Chip icon={<CleaningIcon />} label={`Temizleniyor: ${rooms.filter(r => r.cleaningStatus === RoomCleaningStatus.Cleaning).length}`} color="warning" variant="outlined" />
                <Chip icon={<CleanIcon />} label={`Temiz: ${rooms.filter(r => r.cleaningStatus === RoomCleaningStatus.Clean).length}`} color="success" variant="outlined" />
              </Stack>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Rooms Grid */}
      <Grid container spacing={3}>
        {filteredRooms.map((room) => (
          <Grid item xs={12} sm={6} md={4} lg={3} key={room.id}>
            <Card 
              sx={{ 
                borderRadius: 4, 
                transition: 'all 0.3s ease',
                '&:hover': { transform: 'translateY(-5px)', boxShadow: '0 8px 30px rgba(0,0,0,0.1)' },
                border: '1px solid',
                borderColor: room.cleaningStatus === RoomCleaningStatus.Dirty ? alpha(theme.palette.error.main, 0.2) : 'divider'
              }}
            >
              <CardContent sx={{ p: 3 }}>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', mb: 2 }}>
                  <Typography variant="h5" sx={{ fontWeight: 800 }}>
                    {room.roomNumber}
                  </Typography>
                  <Tooltip title={room.occupancyStatusDisplay}>
                    <Chip 
                      size="small" 
                      label={room.occupancyStatusDisplay} 
                      color={getOccupancyStatusColor(room.occupancyStatus)}
                      sx={{ fontWeight: 600 }}
                    />
                  </Tooltip>
                </Box>

                <Stack spacing={1.5} sx={{ mb: 3 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <CleaningIcon fontSize="small" color={getCleaningStatusColor(room.cleaningStatus)} />
                    <Typography variant="body2" sx={{ fontWeight: 600 }}>
                      {room.cleaningStatusDisplay}
                    </Typography>
                  </Box>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <AssignIcon fontSize="small" color="action" />
                    <Typography variant="body2" color="text.secondary">
                      {room.assignedHousekeeperName || 'Atanmamış'}
                    </Typography>
                  </Box>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <HistoryIcon fontSize="small" color="action" />
                    <Typography variant="caption" color="text.secondary">
                      Son Temizlik: {formatDateTime(room.lastCleaned)}
                    </Typography>
                  </Box>
                </Stack>

                <Box sx={{ display: 'flex', gap: 1 }}>
                  <Button 
                    fullWidth 
                    variant="contained" 
                    size="small"
                    startIcon={<CheckCircleIcon />}
                    onClick={() => handleMarkCleaned(room.id)}
                    disabled={room.cleaningStatus === RoomCleaningStatus.Clean}
                    sx={{ borderRadius: 2 }}
                  >
                    Temizlendi
                  </Button>
                  <IconButton 
                    size="small" 
                    sx={{ bgcolor: alpha(theme.palette.primary.main, 0.05), borderRadius: 2 }}
                    onClick={() => {
                      setUpdateDialog({ open: true, room })
                      setSelectedStatus(room.cleaningStatus)
                      setSelectedOccupancy(room.occupancyStatus)
                      setNotes(room.notes || '')
                    }}
                  >
                    <AssignIcon fontSize="small" color="primary" />
                  </IconButton>
                </Box>
              </CardContent>
            </Card>
          </Grid>
        ))}
      </Grid>

      {/* Update Dialog */}
      <Dialog 
        open={updateDialog.open} 
        onClose={() => setUpdateDialog({ open: false })}
        PaperProps={{ sx: { borderRadius: 4, width: '100%', maxWidth: 450 } }}
      >
        <DialogTitle sx={{ fontWeight: 800 }}>
          Oda Durumunu Güncelle: {updateDialog.room?.roomNumber}
        </DialogTitle>
        <DialogContent>
          <Stack spacing={3} sx={{ mt: 1 }}>
            <TextField
              select
              fullWidth
              label="Temizlik Durumu"
              value={selectedStatus}
              onChange={(e) => setSelectedStatus(e.target.value as RoomCleaningStatus)}
            >
              {Object.values(RoomCleaningStatus).map((status) => (
                <MenuItem key={status} value={status}>{status}</MenuItem>
              ))}
            </TextField>
            <TextField
              select
              fullWidth
              label="Doluluk Durumu"
              value={selectedOccupancy}
              onChange={(e) => setSelectedOccupancy(e.target.value as RoomOccupancyStatus)}
            >
              {Object.values(RoomOccupancyStatus).map((status) => (
                <MenuItem key={status} value={status}>{status}</MenuItem>
              ))}
            </TextField>
            <TextField
              fullWidth
              multiline
              rows={3}
              label="Notlar"
              value={notes}
              onChange={(e) => setNotes(e.target.value)}
            />
          </Stack>
        </DialogContent>
        <DialogActions sx={{ p: 3, pt: 0 }}>
          <Button onClick={() => setUpdateDialog({ open: false })} color="inherit">İptal</Button>
          <Button onClick={handleUpdateStatus} variant="contained" sx={{ borderRadius: 2, px: 4 }}>
            Güncelle
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

export default HousekeepingPage

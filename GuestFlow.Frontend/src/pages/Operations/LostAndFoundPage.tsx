// Copyright (c) 2026 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

import { useState, useEffect, useCallback } from 'react'
import {
  Box,
  Card,
  CardContent,
  Typography,
  Chip,
  Button,
  Grid,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Stack,
  alpha,
  useTheme,
  IconButton,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Tooltip
} from '@mui/material'
import {
  Inventory as LostFoundIcon,
  Add as AddIcon,
  CheckCircle as ReturnedIcon,
  LocationOn as StorageIcon,
  Search as SearchIcon,
  Refresh as RefreshIcon
} from '@mui/icons-material'
import housekeepingService, { LostAndFoundItem } from '../../services/housekeepingService'
import { formatDateTime } from '../../utils/formatters'
import ContentState from '../../components/Feedback/ContentState'

const LostAndFoundPage = () => {
  const theme = useTheme()
  const [items, setItems] = useState<LostAndFoundItem[]>([])
  const [loading, setLoading] = useState(true)
  const [searchTerm, setSearchTerm] = useState('')

  // Dialog states
  const [createDialog, setCreateDialog] = useState(false)
  const [returnDialog, setReturnDialog] = useState<{ open: boolean, itemId?: number }>({ open: false })
  const [newItem, setNewItem] = useState({
    itemDescription: '',
    roomNumber: '',
    storageLocation: '',
    itemCategory: 'Personal',
    hotelId: 1
  })
  const [guestId, setGuestId] = useState(0)

  const fetchItems = useCallback(async () => {
    setLoading(true)
    try {
      const response = await housekeepingService.getLostAndFoundItems()
      if (response.success) {
        setItems(response.data || [])
      }
    } catch (err) {
      console.error('Kayıp eşyalar yüklenirken bir hata oluştu.', err)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchItems()
  }, [fetchItems])

  const handleCreateItem = async () => {
    try {
      const response = await housekeepingService.createLostAndFoundItem(newItem)
      if (response.success) {
        setCreateDialog(false)
        fetchItems()
      }
    } catch (err) {
      console.error('Create item failed', err)
    }
  }

  const handleReturnItem = async () => {
    if (!returnDialog.itemId) return

    try {
      const response = await housekeepingService.returnLostItem(returnDialog.itemId, guestId)
      if (response.success) {
        setReturnDialog({ open: false })
        fetchItems()
      }
    } catch (err) {
      console.error('Return failed', err)
    }
  }

  const filteredItems = items.filter(item => 
    item.itemDescription.toLowerCase().includes(searchTerm.toLowerCase()) ||
    item.roomNumber.toLowerCase().includes(searchTerm.toLowerCase())
  )

  if (loading && items.length === 0) {
    return <ContentState state="loading" />
  }

  return (
    <Box sx={{ p: 4 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 4 }}>
        <Box>
          <Typography variant="h4" sx={{ fontWeight: 800, color: 'primary.main', mb: 1, display: 'flex', alignItems: 'center', gap: 2 }}>
            <LostFoundIcon fontSize="large" />
            Kayıp ve Bulunan Eşyalar
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Misafirlerin unuttuğu eşyaları takip edin ve iade süreçlerini yönetin.
          </Typography>
        </Box>
        <Stack direction="row" spacing={2}>
          <Button 
            variant="contained" 
            startIcon={<AddIcon />} 
            onClick={() => setCreateDialog(true)}
            sx={{ borderRadius: 3, px: 3 }}
          >
            Yeni Eşya Kaydı
          </Button>
          <Tooltip title="Yenile">
            <IconButton onClick={fetchItems} sx={{ bgcolor: alpha(theme.palette.primary.main, 0.1) }}>
              <RefreshIcon color="primary" />
            </IconButton>
          </Tooltip>
        </Stack>
      </Box>

      {/* Search & Filter */}
      <Card sx={{ mb: 4, borderRadius: 4, boxShadow: '0 4px 20px rgba(0,0,0,0.05)' }}>
        <CardContent>
          <Grid container spacing={2}>
            <Grid item xs={12} md={6}>
              <TextField 
                fullWidth 
                placeholder="Eşya açıklaması veya oda no ile ara..." 
                size="small"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                InputProps={{
                  startAdornment: <SearchIcon sx={{ color: 'text.secondary', mr: 1 }} />
                }}
              />
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Items Table */}
      <TableContainer component={Paper} sx={{ borderRadius: 4, boxShadow: '0 4px 20px rgba(0,0,0,0.05)', overflow: 'hidden' }}>
        <Table>
          <TableHead sx={{ bgcolor: alpha(theme.palette.primary.main, 0.02) }}>
            <TableRow>
              <TableCell sx={{ fontWeight: 800 }}>Eşya</TableCell>
              <TableCell sx={{ fontWeight: 800 }}>Oda</TableCell>
              <TableCell sx={{ fontWeight: 800 }}>Kategori</TableCell>
              <TableCell sx={{ fontWeight: 800 }}>Konum</TableCell>
              <TableCell sx={{ fontWeight: 800 }}>Durum</TableCell>
              <TableCell sx={{ fontWeight: 800 }} align="right">İşlemler</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {filteredItems.map((item) => (
              <TableRow key={item.id} hover>
                <TableCell>
                  <Typography variant="body2" sx={{ fontWeight: 600 }}>{item.itemDescription}</Typography>
                  <Typography variant="caption" color="text.secondary">{formatDateTime(item.foundDate)}</Typography>
                </TableCell>
                <TableCell sx={{ fontWeight: 700 }}>{item.roomNumber}</TableCell>
                <TableCell>{item.itemCategory}</TableCell>
                <TableCell>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                    <StorageIcon fontSize="inherit" color="action" />
                    {item.storageLocation}
                  </Box>
                </TableCell>
                <TableCell>
                  {item.isReturned ? (
                    <Chip size="small" label="İade Edildi" color="success" />
                  ) : (
                    <Chip size="small" label="Muhafazada" color="warning" />
                  )}
                </TableCell>
                <TableCell align="right">
                  {!item.isReturned && (
                    <Button 
                      size="small" 
                      variant="outlined" 
                      startIcon={<ReturnedIcon />}
                      onClick={() => setReturnDialog({ open: true, itemId: item.id })}
                    >
                      İade Et
                    </Button>
                  )}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Create Dialog */}
      <Dialog open={createDialog} onClose={() => setCreateDialog(false)} PaperProps={{ sx: { borderRadius: 4, width: '100%', maxWidth: 450 } }}>
        <DialogTitle sx={{ fontWeight: 800 }}>Yeni Kayıp Eşya</DialogTitle>
        <DialogContent>
          <Stack spacing={3} sx={{ mt: 1 }}>
            <TextField 
              fullWidth 
              label="Eşya Açıklaması" 
              value={newItem.itemDescription}
              onChange={(e) => setNewItem({ ...newItem, itemDescription: e.target.value })}
            />
            <TextField 
              fullWidth 
              label="Oda No" 
              value={newItem.roomNumber}
              onChange={(e) => setNewItem({ ...newItem, roomNumber: e.target.value })}
            />
            <TextField 
              fullWidth 
              label="Saklama Konumu" 
              value={newItem.storageLocation}
              onChange={(e) => setNewItem({ ...newItem, storageLocation: e.target.value })}
              placeholder="Örn: Resepsiyon Kasa A1"
            />
          </Stack>
        </DialogContent>
        <DialogActions sx={{ p: 3 }}>
          <Button onClick={() => setCreateDialog(false)} color="inherit">İptal</Button>
          <Button onClick={handleCreateItem} variant="contained" sx={{ borderRadius: 2 }}>Kaydet</Button>
        </DialogActions>
      </Dialog>

      {/* Return Dialog */}
      <Dialog open={returnDialog.open} onClose={() => setReturnDialog({ open: false })} PaperProps={{ sx: { borderRadius: 4, width: '100%', maxWidth: 400 } }}>
        <DialogTitle sx={{ fontWeight: 800 }}>İade İşlemi</DialogTitle>
        <DialogContent>
          <Typography variant="body2" sx={{ mb: 2 }}>
            Eşyayı misafire iade ettiğinizden eminsiniz? Lütfen misafir ID'sini (varsa) girin.
          </Typography>
          <TextField 
            fullWidth 
            type="number" 
            label="Misafir ID (İsteğe bağlı)" 
            value={guestId || ''}
            onChange={(e) => setGuestId(parseInt(e.target.value) || 0)}
          />
        </DialogContent>
        <DialogActions sx={{ p: 3 }}>
          <Button onClick={() => setReturnDialog({ open: false })} color="inherit">İptal</Button>
          <Button onClick={handleReturnItem} variant="contained" color="success" sx={{ borderRadius: 2 }}>İade Edildi</Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

export default LostAndFoundPage

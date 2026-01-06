import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Box,
  Typography,
  Tabs,
  Tab,
  Stack,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  TextField,
  Chip,
  Alert,
} from '@mui/material'
import {
  Delete as DeleteIcon,
  Edit as EditIcon,
  CheckCircle as CheckCircleIcon,
  Cancel as CancelIcon,
} from '@mui/icons-material'
import { useState } from 'react'

interface BulkOperationsDialogProps {
  open: boolean
  onClose: () => void
  selectedCount: number
  onBulkDelete?: () => void
  onBulkEdit?: (data: any) => void
  onBulkStatusChange?: (status: string) => void
  entityType?: string // 'guest', 'transfer', 'invoice', etc.
}

interface TabPanelProps {
  children?: React.ReactNode
  index: number
  value: number
}

const TabPanel = ({ children, value, index }: TabPanelProps) => {
  return (
    <div role="tabpanel" hidden={value !== index}>
      {value === index && <Box sx={{ pt: 3 }}>{children}</Box>}
    </div>
  )
}

/**
 * Bulk operations dialog component
 */
export const BulkOperationsDialog = ({
  open,
  onClose,
  selectedCount,
  onBulkDelete,
  onBulkEdit,
  onBulkStatusChange,
  entityType = 'item',
}: BulkOperationsDialogProps) => {
  const [tabValue, setTabValue] = useState(0)
  const [status, setStatus] = useState('')
  const [editData, setEditData] = useState<any>({})

  const handleTabChange = (_event: React.SyntheticEvent, newValue: number) => {
    setTabValue(newValue)
  }

  const handleDelete = () => {
    if (onBulkDelete) {
      onBulkDelete()
      onClose()
    }
  }

  const handleStatusChange = () => {
    if (onBulkStatusChange && status) {
      onBulkStatusChange(status)
      onClose()
    }
  }

  const handleEdit = () => {
    if (onBulkEdit) {
      onBulkEdit(editData)
      onClose()
    }
  }

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>
        <Stack direction="row" spacing={2} alignItems="center">
          <Typography variant="h6">Toplu İşlemler</Typography>
          <Chip label={`${selectedCount} öğe seçili`} color="primary" size="small" />
        </Stack>
      </DialogTitle>
      <DialogContent>
        <Alert severity="info" sx={{ mb: 2 }}>
          Seçili {selectedCount} {entityType} üzerinde toplu işlem yapabilirsiniz.
        </Alert>

        <Tabs value={tabValue} onChange={handleTabChange}>
          {onBulkStatusChange && (
            <Tab label="Durum Değiştir" icon={<CheckCircleIcon />} iconPosition="start" />
          )}
          {onBulkEdit && (
            <Tab label="Toplu Düzenle" icon={<EditIcon />} iconPosition="start" />
          )}
          {onBulkDelete && (
            <Tab label="Toplu Sil" icon={<DeleteIcon />} iconPosition="start" />
          )}
        </Tabs>

        {/* Status Change Tab */}
        {onBulkStatusChange && (
          <TabPanel value={tabValue} index={0}>
            <Stack spacing={2}>
              <FormControl fullWidth>
                <InputLabel>Yeni Durum</InputLabel>
                <Select
                  value={status}
                  onChange={(e) => setStatus(e.target.value)}
                  label="Yeni Durum"
                >
                  <MenuItem value="Pending">Beklemede</MenuItem>
                  <MenuItem value="Confirmed">Onaylandı</MenuItem>
                  <MenuItem value="InProgress">Devam Ediyor</MenuItem>
                  <MenuItem value="Completed">Tamamlandı</MenuItem>
                  <MenuItem value="Cancelled">İptal Edildi</MenuItem>
                </Select>
              </FormControl>
              <Button
                variant="contained"
                onClick={handleStatusChange}
                disabled={!status}
                fullWidth
              >
                Durumu Değiştir
              </Button>
            </Stack>
          </TabPanel>
        )}

        {/* Bulk Edit Tab */}
        {onBulkEdit && (
          <TabPanel value={tabValue} index={onBulkStatusChange ? 1 : 0}>
            <Stack spacing={2}>
              <TextField
                label="Not"
                multiline
                rows={3}
                value={editData.note || ''}
                onChange={(e) => setEditData({ ...editData, note: e.target.value })}
                fullWidth
              />
              <Button variant="contained" onClick={handleEdit} fullWidth>
                Toplu Düzenle
              </Button>
            </Stack>
          </TabPanel>
        )}

        {/* Bulk Delete Tab */}
        {onBulkDelete && (
          <TabPanel
            value={tabValue}
            index={[onBulkStatusChange, onBulkEdit].filter(Boolean).length}
          >
            <Stack spacing={2}>
              <Alert severity="warning">
                Seçili {selectedCount} {entityType} kalıcı olarak silinecektir. Bu işlem
                geri alınamaz.
              </Alert>
              <Button
                variant="contained"
                color="error"
                onClick={handleDelete}
                fullWidth
                startIcon={<DeleteIcon />}
              >
                Tümünü Sil
              </Button>
            </Stack>
          </TabPanel>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} startIcon={<CancelIcon />}>
          İptal
        </Button>
      </DialogActions>
    </Dialog>
  )
}

export default BulkOperationsDialog


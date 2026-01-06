import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Box,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Checkbox,
  FormControlLabel,
  Chip,
  Alert,
  CircularProgress,
  Stack,
} from '@mui/material'
import {
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  Save as SaveIcon,
  Cancel as CancelIcon,
} from '@mui/icons-material'
import { ImportGuestDto, ImportPreviewResponse } from '../../services/importService'
import { useState, useMemo } from 'react'

interface ImportPreviewDialogProps {
  open: boolean
  previewData: ImportPreviewResponse | null
  onClose: () => void
  onSave: (guests: ImportGuestDto[], skipDuplicates: boolean) => void
  isSaving?: boolean
}

/**
 * Import preview dialog component
 */
export const ImportPreviewDialog = ({
  open,
  previewData,
  onClose,
  onSave,
  isSaving = false,
}: ImportPreviewDialogProps) => {
  const [selectedGuests, setSelectedGuests] = useState<Set<number>>(new Set())
  const [skipDuplicates, setSkipDuplicates] = useState(true)

  // Initialize selected guests with all valid guests
  useMemo(() => {
    if (previewData && previewData.data) {
      const validIndices = previewData.data
        .map((guest, index) => (guest.isValid ? index : -1))
        .filter((index) => index !== -1)
      setSelectedGuests(new Set(validIndices))
    }
  }, [previewData])

  const handleToggleGuest = (index: number) => {
    const newSelected = new Set(selectedGuests)
    if (newSelected.has(index)) {
      newSelected.delete(index)
    } else {
      newSelected.add(index)
    }
    setSelectedGuests(newSelected)
  }

  const handleSelectAll = () => {
    if (previewData && previewData.data) {
      const validIndices = previewData.data
        .map((guest, index) => (guest.isValid ? index : -1))
        .filter((index) => index !== -1)
      setSelectedGuests(new Set(validIndices))
    }
  }

  const handleDeselectAll = () => {
    setSelectedGuests(new Set())
  }

  const handleSave = () => {
    if (!previewData || !previewData.data) return

    const guestsToSave = previewData.data.filter((_, index) => selectedGuests.has(index))
    onSave(guestsToSave, skipDuplicates)
  }

  const selectedCount = selectedGuests.size
  const validCount = previewData?.validRows || 0

  return (
    <Dialog open={open} onClose={onClose} maxWidth="lg" fullWidth>
      <DialogTitle>
        <Stack direction="row" spacing={2} alignItems="center">
          <Typography variant="h6">İçe Aktarma Önizleme</Typography>
          {previewData && (
            <Chip
              label={`${previewData.validRows} geçerli / ${previewData.invalidRows} geçersiz`}
              color={previewData.validRows > 0 ? 'success' : 'error'}
              size="small"
            />
          )}
        </Stack>
      </DialogTitle>
      <DialogContent>
        {!previewData ? (
          <Box display="flex" justifyContent="center" alignItems="center" minHeight={200}>
            <CircularProgress />
          </Box>
        ) : (
          <Stack spacing={2}>
            {/* Summary */}
            <Alert severity="info">
              <Typography variant="body2">
                Toplam {previewData.totalRows} satır okundu. {previewData.validRows} geçerli,{' '}
                {previewData.invalidRows} geçersiz kayıt bulundu.
              </Typography>
            </Alert>

            {/* Options */}
            <Box>
              <FormControlLabel
                control={
                  <Checkbox
                    checked={skipDuplicates}
                    onChange={(e) => setSkipDuplicates(e.target.checked)}
                  />
                }
                label="Tekrarlanan kayıtları atla"
              />
            </Box>

            {/* Actions */}
            <Box display="flex" gap={1}>
              <Button size="small" onClick={handleSelectAll}>
                Tümünü Seç
              </Button>
              <Button size="small" onClick={handleDeselectAll}>
                Seçimi Temizle
              </Button>
              <Typography variant="body2" sx={{ ml: 'auto', alignSelf: 'center' }}>
                {selectedCount} / {validCount} seçili
              </Typography>
            </Box>

            {/* Preview Table */}
            <TableContainer component={Paper} sx={{ maxHeight: 400 }}>
              <Table stickyHeader size="small">
                <TableHead>
                  <TableRow>
                    <TableCell padding="checkbox" width={50}>
                      <Checkbox
                        indeterminate={selectedCount > 0 && selectedCount < validCount}
                        checked={selectedCount === validCount && validCount > 0}
                        onChange={(e) => {
                          if (e.target.checked) {
                            handleSelectAll()
                          } else {
                            handleDeselectAll()
                          }
                        }}
                      />
                    </TableCell>
                    <TableCell>Durum</TableCell>
                    <TableCell>Ad Soyad</TableCell>
                    <TableCell>E-posta</TableCell>
                    <TableCell>Telefon</TableCell>
                    <TableCell>Uyruk</TableCell>
                    <TableCell>Özel Misafir</TableCell>
                    <TableCell>Hatalar</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {previewData.data.map((guest, index) => (
                    <TableRow key={index} hover>
                      <TableCell padding="checkbox">
                        <Checkbox
                          checked={selectedGuests.has(index)}
                          onChange={() => handleToggleGuest(index)}
                          disabled={!guest.isValid}
                        />
                      </TableCell>
                      <TableCell>
                        {guest.isValid ? (
                          <Chip
                            icon={<CheckCircleIcon />}
                            label="Geçerli"
                            color="success"
                            size="small"
                          />
                        ) : (
                          <Chip
                            icon={<ErrorIcon />}
                            label="Geçersiz"
                            color="error"
                            size="small"
                          />
                        )}
                      </TableCell>
                      <TableCell>{guest.fullName}</TableCell>
                      <TableCell>{guest.email || '-'}</TableCell>
                      <TableCell>{guest.phoneNumber || '-'}</TableCell>
                      <TableCell>{guest.nationality}</TableCell>
                      <TableCell>{guest.isSpecialGuest ? 'Evet' : 'Hayır'}</TableCell>
                      <TableCell>
                        {guest.errors && guest.errors.length > 0 ? (
                          <Box>
                            {guest.errors.map((error, i) => (
                              <Typography key={i} variant="caption" color="error" display="block">
                                {error}
                              </Typography>
                            ))}
                          </Box>
                        ) : (
                          '-'
                        )}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>

            {/* Errors Summary */}
            {previewData.errors && previewData.errors.length > 0 && (
              <Alert severity="warning">
                <Typography variant="subtitle2" gutterBottom>
                  Genel Hatalar:
                </Typography>
                {previewData.errors.map((error, index) => (
                  <Typography key={index} variant="body2">
                    • {error}
                  </Typography>
                ))}
              </Alert>
            )}
          </Stack>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} startIcon={<CancelIcon />} disabled={isSaving}>
          İptal
        </Button>
        <Button
          onClick={handleSave}
          variant="contained"
          startIcon={isSaving ? <CircularProgress size={16} /> : <SaveIcon />}
          disabled={selectedCount === 0 || isSaving}
        >
          {isSaving ? 'Kaydediliyor...' : `Seçilenleri Kaydet (${selectedCount})`}
        </Button>
      </DialogActions>
    </Dialog>
  )
}

export default ImportPreviewDialog


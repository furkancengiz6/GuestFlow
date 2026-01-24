// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

import React, { useState } from 'react'
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  FormControlLabel,
  Grid,
  IconButton,
  InputLabel,
  MenuItem,
  Select,
  Switch,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
  Paper,
  Tooltip,
  Alert,
} from '@mui/material'
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  PlayArrow as PlayArrowIcon,
  PowerSettingsNew as PowerSettingsNewIcon,
  FilterList as FilterListIcon,
} from '@mui/icons-material'
import { useNotificationRules } from '../../hooks/useNotificationRules'
import {
  useCreateNotificationRule,
  useUpdateNotificationRule,
  useDeleteNotificationRule,
  useToggleNotificationRule,
  useExecuteNotificationRule,
} from '../../hooks/useNotificationRules'
import type {
  NotificationRule,
  UpsertNotificationRule,
} from '../../types/notificationRule'
import {
  RULE_TYPES,
  RULE_CATEGORIES,
  NOTIFICATION_CHANNELS,
  RECIPIENT_TYPES,
} from '../../types/notificationRule'
import { ConfirmationDialog } from '../Common/ConfirmationDialog'

const NotificationRules: React.FC = () => {
  const [filterActive, setFilterActive] = useState<boolean | undefined>(undefined)
  const [dialogOpen, setDialogOpen] = useState(false)
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false)
  const [executeDialogOpen, setExecuteDialogOpen] = useState(false)
  const [selectedRule, setSelectedRule] = useState<NotificationRule | null>(null)
  const [formData, setFormData] = useState<UpsertNotificationRule>({
    name: '',
    description: '',
    category: RULE_CATEGORIES.PAYMENT,
    ruleType: RULE_TYPES.OVERDUE_PAYMENT,
    conditions: '{}',
    notificationChannel: NOTIFICATION_CHANNELS.EMAIL,
    templateName: '',
    recipientType: RECIPIENT_TYPES.GUEST,
    recipientId: undefined,
    isActive: true,
    priority: 5,
    checkIntervalMinutes: 60,
    parameters: '{}',
  })

  const { data: rules, isLoading } = useNotificationRules(filterActive)
  const createMutation = useCreateNotificationRule()
  const updateMutation = useUpdateNotificationRule()
  const deleteMutation = useDeleteNotificationRule()
  const toggleMutation = useToggleNotificationRule()
  const executeMutation = useExecuteNotificationRule()

  const handleOpenDialog = (rule?: NotificationRule) => {
    if (rule) {
      setSelectedRule(rule)
      setFormData({
        name: rule.name,
        description: rule.description || '',
        category: rule.category,
        ruleType: rule.ruleType,
        conditions: rule.conditions,
        notificationChannel: rule.notificationChannel,
        templateName: rule.templateName || '',
        recipientType: rule.recipientType,
        recipientId: rule.recipientId,
        isActive: rule.isActive,
        priority: rule.priority,
        checkIntervalMinutes: rule.checkIntervalMinutes,
        parameters: rule.parameters || '{}',
      })
    } else {
      setSelectedRule(null)
      setFormData({
        name: '',
        description: '',
        category: RULE_CATEGORIES.PAYMENT,
        ruleType: RULE_TYPES.OVERDUE_PAYMENT,
        conditions: '{}',
        notificationChannel: NOTIFICATION_CHANNELS.EMAIL,
        templateName: '',
        recipientType: RECIPIENT_TYPES.GUEST,
        recipientId: undefined,
        isActive: true,
        priority: 5,
        checkIntervalMinutes: 60,
        parameters: '{}',
      })
    }
    setDialogOpen(true)
  }

  const handleCloseDialog = () => {
    setDialogOpen(false)
    setSelectedRule(null)
  }

  const handleSubmit = () => {
    if (selectedRule) {
      updateMutation.mutate({ id: selectedRule.id, rule: formData })
    } else {
      createMutation.mutate(formData)
    }
    handleCloseDialog()
  }

  const handleDelete = (rule: NotificationRule) => {
    setSelectedRule(rule)
    setDeleteDialogOpen(true)
  }

  const confirmDelete = () => {
    if (selectedRule) {
      deleteMutation.mutate(selectedRule.id)
      setDeleteDialogOpen(false)
      setSelectedRule(null)
    }
  }

  const handleToggle = (rule: NotificationRule) => {
    toggleMutation.mutate({ id: rule.id, isActive: !rule.isActive })
  }

  const handleExecute = (rule: NotificationRule) => {
    setSelectedRule(rule)
    setExecuteDialogOpen(true)
    executeMutation.mutate(rule.id)
  }

  const getRuleTypeLabel = (ruleType: string) => {
    const labels: Record<string, string> = {
      [RULE_TYPES.OVERDUE_PAYMENT]: 'Geciken Ödeme',
      [RULE_TYPES.UPCOMING_SERVICE]: 'Yaklaşan Servis',
      [RULE_TYPES.UNASSIGNED_DRIVER]: 'Atanmayan Şoför',
      [RULE_TYPES.LOW_INVENTORY]: 'Düşük Stok',
    }
    return labels[ruleType] || ruleType
  }

  const getCategoryLabel = (category: string) => {
    const labels: Record<string, string> = {
      [RULE_CATEGORIES.PAYMENT]: 'Ödeme',
      [RULE_CATEGORIES.SERVICE]: 'Servis',
      [RULE_CATEGORIES.ASSIGNMENT]: 'Atama',
      [RULE_CATEGORIES.INVENTORY]: 'Stok',
    }
    return labels[category] || category
  }

  if (isLoading) {
    return <Typography>Yükleniyor...</Typography>
  }

  return (
    <Box sx={{ p: 3 }}>
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          mb: 3,
        }}
      >
        <Typography variant="h4">Bildirim Kuralları</Typography>
        <Box sx={{ display: 'flex', gap: 2 }}>
          <FormControl size="small" sx={{ minWidth: 150 }}>
            <InputLabel>Filtre</InputLabel>
            <Select
              value={filterActive === undefined ? 'all' : filterActive ? 'active' : 'inactive'}
              label="Filtre"
              onChange={(e) => {
                const value = e.target.value
                setFilterActive(
                  value === 'all' ? undefined : value === 'active'
                )
              }}
            >
              <MenuItem value="all">Tümü</MenuItem>
              <MenuItem value="active">Aktif</MenuItem>
              <MenuItem value="inactive">Pasif</MenuItem>
            </Select>
          </FormControl>
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => handleOpenDialog()}
          >
            Yeni Kural
          </Button>
        </Box>
      </Box>

      <TableContainer component={Paper}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Ad</TableCell>
              <TableCell>Kategori</TableCell>
              <TableCell>Kural Tipi</TableCell>
              <TableCell>Kanal</TableCell>
              <TableCell>Öncelik</TableCell>
              <TableCell>Kontrol Aralığı</TableCell>
              <TableCell>Tetiklenme</TableCell>
              <TableCell>Durum</TableCell>
              <TableCell align="right">İşlemler</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {rules?.map((rule) => (
              <TableRow key={rule.id}>
                <TableCell>
                  <Typography variant="body2" fontWeight="medium">
                    {rule.name}
                  </Typography>
                  {rule.description && (
                    <Typography variant="caption" color="text.secondary">
                      {rule.description}
                    </Typography>
                  )}
                </TableCell>
                <TableCell>
                  <Chip
                    label={getCategoryLabel(rule.category)}
                    size="small"
                    color="primary"
                    variant="outlined"
                  />
                </TableCell>
                <TableCell>{getRuleTypeLabel(rule.ruleType)}</TableCell>
                <TableCell>{rule.notificationChannel}</TableCell>
                <TableCell>{rule.priority}</TableCell>
                <TableCell>{rule.checkIntervalMinutes} dk</TableCell>
                <TableCell>
                  <Typography variant="body2">
                    {rule.triggerCount} kez
                  </Typography>
                  {rule.lastTriggeredAt && (
                    <Typography variant="caption" color="text.secondary">
                      {new Date(rule.lastTriggeredAt).toLocaleDateString('tr-TR')}
                    </Typography>
                  )}
                </TableCell>
                <TableCell>
                  <Chip
                    label={rule.isActive ? 'Aktif' : 'Pasif'}
                    color={rule.isActive ? 'success' : 'default'}
                    size="small"
                  />
                </TableCell>
                <TableCell align="right">
                  <Box sx={{ display: 'flex', gap: 1, justifyContent: 'flex-end' }}>
                    <Tooltip title="Test Et">
                      <IconButton
                        size="small"
                        onClick={() => handleExecute(rule)}
                        color="primary"
                      >
                        <PlayArrowIcon />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title={rule.isActive ? 'Pasif Yap' : 'Aktif Yap'}>
                      <IconButton
                        size="small"
                        onClick={() => handleToggle(rule)}
                        color={rule.isActive ? 'warning' : 'success'}
                      >
                        <PowerSettingsNewIcon />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Düzenle">
                      <IconButton
                        size="small"
                        onClick={() => handleOpenDialog(rule)}
                        color="primary"
                      >
                        <EditIcon />
                      </IconButton>
                    </Tooltip>
                    <Tooltip title="Sil">
                      <IconButton
                        size="small"
                        onClick={() => handleDelete(rule)}
                        color="error"
                      >
                        <DeleteIcon />
                      </IconButton>
                    </Tooltip>
                  </Box>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Create/Edit Dialog */}
      <Dialog open={dialogOpen} onClose={handleCloseDialog} maxWidth="md" fullWidth>
        <DialogTitle>
          {selectedRule ? 'Kural Düzenle' : 'Yeni Kural Oluştur'}
        </DialogTitle>
        <DialogContent>
          <Grid container spacing={2} sx={{ mt: 1 }}>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Kural Adı"
                value={formData.name}
                onChange={(e) =>
                  setFormData({ ...formData, name: e.target.value })
                }
                required
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Açıklama"
                value={formData.description}
                onChange={(e) =>
                  setFormData({ ...formData, description: e.target.value })
                }
                multiline
                rows={2}
              />
            </Grid>
            <Grid item xs={6}>
              <FormControl fullWidth>
                <InputLabel>Kategori</InputLabel>
                <Select
                  value={formData.category}
                  label="Kategori"
                  onChange={(e) =>
                    setFormData({ ...formData, category: e.target.value })
                  }
                >
                  {Object.values(RULE_CATEGORIES).map((cat) => (
                    <MenuItem key={cat} value={cat}>
                      {getCategoryLabel(cat)}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={6}>
              <FormControl fullWidth>
                <InputLabel>Kural Tipi</InputLabel>
                <Select
                  value={formData.ruleType}
                  label="Kural Tipi"
                  onChange={(e) =>
                    setFormData({ ...formData, ruleType: e.target.value })
                  }
                >
                  {Object.values(RULE_TYPES).map((type) => (
                    <MenuItem key={type} value={type}>
                      {getRuleTypeLabel(type)}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={6}>
              <FormControl fullWidth>
                <InputLabel>Bildirim Kanalı</InputLabel>
                <Select
                  value={formData.notificationChannel}
                  label="Bildirim Kanalı"
                  onChange={(e) =>
                    setFormData({
                      ...formData,
                      notificationChannel: e.target.value,
                    })
                  }
                >
                  {Object.values(NOTIFICATION_CHANNELS).map((channel) => (
                    <MenuItem key={channel} value={channel}>
                      {channel}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={6}>
              <FormControl fullWidth>
                <InputLabel>Alıcı Tipi</InputLabel>
                <Select
                  value={formData.recipientType}
                  label="Alıcı Tipi"
                  onChange={(e) =>
                    setFormData({ ...formData, recipientType: e.target.value })
                  }
                >
                  {Object.values(RECIPIENT_TYPES).map((type) => (
                    <MenuItem key={type} value={type}>
                      {type}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid item xs={6}>
              <TextField
                fullWidth
                label="Öncelik (1-10)"
                type="number"
                value={formData.priority}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    priority: parseInt(e.target.value) || 5,
                  })
                }
                inputProps={{ min: 1, max: 10 }}
              />
            </Grid>
            <Grid item xs={6}>
              <TextField
                fullWidth
                label="Kontrol Aralığı (dakika)"
                type="number"
                value={formData.checkIntervalMinutes}
                onChange={(e) =>
                  setFormData({
                    ...formData,
                    checkIntervalMinutes: parseInt(e.target.value) || 60,
                  })
                }
                inputProps={{ min: 1 }}
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Koşullar (JSON)"
                value={formData.conditions}
                onChange={(e) =>
                  setFormData({ ...formData, conditions: e.target.value })
                }
                multiline
                rows={3}
                helperText="JSON formatında koşul tanımları"
              />
            </Grid>
            <Grid item xs={12}>
              <TextField
                fullWidth
                label="Parametreler (JSON)"
                value={formData.parameters}
                onChange={(e) =>
                  setFormData({ ...formData, parameters: e.target.value })
                }
                multiline
                rows={2}
                helperText="JSON formatında ek parametreler"
              />
            </Grid>
            <Grid item xs={12}>
              <FormControlLabel
                control={
                  <Switch
                    checked={formData.isActive}
                    onChange={(e) =>
                      setFormData({ ...formData, isActive: e.target.checked })
                    }
                  />
                }
                label="Aktif"
              />
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleCloseDialog}>İptal</Button>
          <Button
            onClick={handleSubmit}
            variant="contained"
            disabled={!formData.name || createMutation.isPending || updateMutation.isPending}
          >
            {selectedRule ? 'Güncelle' : 'Oluştur'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Confirmation Dialog */}
      <ConfirmationDialog
        open={deleteDialogOpen}
        onCancel={() => {
          setDeleteDialogOpen(false)
          setSelectedRule(null)
        }}
        onConfirm={confirmDelete}
        title="Kuralı Sil"
        message={`"${selectedRule?.name}" kuralını silmek istediğinize emin misiniz?`}
      />

      {/* Execute Result Dialog */}
      <Dialog
        open={executeDialogOpen}
        onClose={() => {
          setExecuteDialogOpen(false)
          setSelectedRule(null)
        }}
      >
        <DialogTitle>Kural Test Sonucu</DialogTitle>
        <DialogContent>
          {executeMutation.isPending ? (
            <Typography>Test ediliyor...</Typography>
          ) : executeMutation.data ? (
            <Box>
              <Alert
                severity={executeMutation.data.triggered ? 'success' : 'info'}
                sx={{ mb: 2 }}
              >
                {executeMutation.data.triggered
                  ? 'Kural tetiklendi!'
                  : 'Kural tetiklenmedi (koşul sağlanmadı)'}
              </Alert>
              <Typography variant="body2">
                <strong>Eşleşen Kayıt:</strong>{' '}
                {executeMutation.data.matchedEntitiesCount}
              </Typography>
              <Typography variant="body2">
                <strong>Gönderilen Bildirim:</strong>{' '}
                {executeMutation.data.notificationsSent}
              </Typography>
              {executeMutation.data.errorMessage && (
                <Alert severity="error" sx={{ mt: 2 }}>
                  {executeMutation.data.errorMessage}
                </Alert>
              )}
            </Box>
          ) : executeMutation.isError ? (
            <Alert severity="error">
              Test sırasında bir hata oluştu
            </Alert>
          ) : null}
        </DialogContent>
        <DialogActions>
          <Button
            onClick={() => {
              setExecuteDialogOpen(false)
              setSelectedRule(null)
            }}
          >
            Kapat
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

export default NotificationRules

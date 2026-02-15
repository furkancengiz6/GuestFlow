// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

import {
  Box,
  Typography,
  Card,
  CardContent,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
  Alert,
  AlertTitle,
  Grid,
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Select,
  MenuItem,
  FormControl,
  InputLabel,
} from '@mui/material'
import {
  Email as EmailIcon,
  Sms as SmsIcon,
  WhatsApp as WhatsAppIcon,
  Notifications as InAppIcon,
  Send as SendIcon,
  SmartToy as SmartNotificationIcon,
  ArrowDownward as InboundIcon,
  ArrowUpward as OutboundIcon,
} from '@mui/icons-material'
import { useState } from 'react'
import { useGuestCommunicationHistory, useSendMessage, useSendSmartNotification } from '../../hooks/useCommunication'
import { formatDate } from '../../utils/formatters'
import ContentState from '../Feedback/ContentState'
import type { CommunicationItem, SendMessageRequest, SmartNotificationType } from '../../types/communication'

interface CommunicationHistoryProps {
  guestId: number
}

const CommunicationHistory = ({ guestId }: CommunicationHistoryProps) => {
  const { data: history, isLoading, error } = useGuestCommunicationHistory(guestId)
  const sendMessageMutation = useSendMessage()
  const sendSmartNotificationMutation = useSendSmartNotification()

  const [sendMessageOpen, setSendMessageOpen] = useState(false)
  const [smartNotificationOpen, setSmartNotificationOpen] = useState(false)
  const [messageData, setMessageData] = useState<SendMessageRequest>({
    channel: 'Email',
    subject: '',
    content: '',
  })
  const [selectedNotificationType, setSelectedNotificationType] = useState<SmartNotificationType>('PreArrival')

  const getChannelIcon = (channel: string): React.ReactElement | null => {
    switch (channel) {
      case 'Email':
        return <EmailIcon fontSize="small" />
      case 'SMS':
        return <SmsIcon fontSize="small" />
      case 'WhatsApp':
        return <WhatsAppIcon fontSize="small" />
      case 'InApp':
        return <InAppIcon fontSize="small" />
      default:
        return null
    }
  }

  const getChannelColor = (channel: string): 'primary' | 'success' | 'info' | 'default' => {
    switch (channel) {
      case 'Email':
        return 'primary'
      case 'SMS':
        return 'success'
      case 'WhatsApp':
        return 'success'
      case 'InApp':
        return 'info'
      default:
        return 'default'
    }
  }

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'sent':
      case 'delivered':
        return 'success'
      case 'failed':
        return 'error'
      case 'pending':
        return 'warning'
      default:
        return 'default'
    }
  }

  const handleSendMessage = () => {
    sendMessageMutation.mutate(
      { guestId, data: messageData },
      {
        onSuccess: () => {
          setSendMessageOpen(false)
          setMessageData({ channel: 'Email', subject: '', content: '' })
        },
      }
    )
  }

  const handleSendSmartNotification = () => {
    sendSmartNotificationMutation.mutate(
      { guestId, notificationType: selectedNotificationType },
      {
        onSuccess: () => {
          setSmartNotificationOpen(false)
        },
      }
    )
  }

  if (isLoading) {
    return <ContentState state="loading" skeletonLines={10} />
  }

  if (error || !history) {
    return (
      <Alert severity="error">
        <AlertTitle>İletişim geçmişi yüklenemedi</AlertTitle>
        İletişim geçmişi getirilirken bir hata oluştu.
      </Alert>
    )
  }

  return (
    <Box>
      {/* Özet Bilgiler */}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid item xs={12} md={3}>
          <Card variant="outlined">
            <CardContent>
              <Typography variant="body2" color="text.secondary">
                Toplam İletişim
              </Typography>
              <Typography variant="h5">{history.summary.totalCommunications}</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={3}>
          <Card variant="outlined">
            <CardContent>
              <Typography variant="body2" color="text.secondary">
                E-posta
              </Typography>
              <Typography variant="h5">{history.summary.emailCount}</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={3}>
          <Card variant="outlined">
            <CardContent>
              <Typography variant="body2" color="text.secondary">
                SMS
              </Typography>
              <Typography variant="h5">{history.summary.smsCount}</Typography>
            </CardContent>
          </Card>
        </Grid>
        <Grid item xs={12} md={3}>
          <Card variant="outlined">
            <CardContent>
              <Typography variant="body2" color="text.secondary">
                Son İletişim
              </Typography>
              <Typography variant="h6">
                {history.summary.lastCommunicationDate
                  ? formatDate(history.summary.lastCommunicationDate)
                  : 'Yok'}
              </Typography>
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Aksiyon Butonları */}
      <Box sx={{ mb: 2, display: 'flex', gap: 2 }}>
        <Button
          variant="contained"
          startIcon={<SendIcon />}
          onClick={() => setSendMessageOpen(true)}
        >
          Mesaj Gönder
        </Button>
        <Button
          variant="outlined"
          startIcon={<SmartNotificationIcon />}
          onClick={() => setSmartNotificationOpen(true)}
        >
          Smart Notification
        </Button>
      </Box>

      {/* İletişim Geçmişi Tablosu */}
      <TableContainer component={Card}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Tarih</TableCell>
              <TableCell>Kanal</TableCell>
              <TableCell>Yön</TableCell>
              <TableCell>Konu</TableCell>
              <TableCell>Durum</TableCell>
              <TableCell>Kaynak</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {history.communications.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} align="center">
                  <Alert severity="info">Henüz iletişim kaydı bulunmamaktadır.</Alert>
                </TableCell>
              </TableRow>
            ) : (
              history.communications.map((comm: CommunicationItem) => (
                <TableRow key={comm.id}>
                  <TableCell>{formatDate(comm.sentDate)}</TableCell>
                  <TableCell>
                    {(() => {
                      const icon = getChannelIcon(comm.channel)
                      return icon ? (
                        <Chip
                          icon={icon}
                          label={comm.channel}
                          size="small"
                          color={getChannelColor(comm.channel)}
                        />
                      ) : (
                        <Chip
                          label={comm.channel}
                          size="small"
                          color={getChannelColor(comm.channel)}
                        />
                      )
                    })()}
                  </TableCell>
                  <TableCell>
                    <Chip
                      icon={comm.direction === 'Inbound' ? <InboundIcon fontSize="small" /> : <OutboundIcon fontSize="small" />}
                      label={comm.direction === 'Inbound' ? 'Gelen' : 'Giden'}
                      size="small"
                      variant="outlined"
                    />
                  </TableCell>
                  <TableCell>{comm.subject}</TableCell>
                  <TableCell>
                    <Chip
                      label={comm.status}
                      size="small"
                      color={getStatusColor(comm.status) as any}
                    />
                  </TableCell>
                  <TableCell>
                    <Chip label={comm.source} size="small" variant="outlined" />
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {/* Mesaj Gönderme Dialog */}
      <Dialog open={sendMessageOpen} onClose={() => setSendMessageOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Mesaj Gönder</DialogTitle>
        <DialogContent>
          <FormControl fullWidth margin="normal">
            <InputLabel>Kanal</InputLabel>
            <Select
              value={messageData.channel}
              onChange={(e) =>
                setMessageData({ ...messageData, channel: e.target.value as any })
              }
              label="Kanal"
            >
              <MenuItem value="Email">E-posta</MenuItem>
              <MenuItem value="SMS">SMS</MenuItem>
              <MenuItem value="WhatsApp">WhatsApp</MenuItem>
            </Select>
          </FormControl>
          <TextField
            fullWidth
            margin="normal"
            label="Konu"
            value={messageData.subject}
            onChange={(e) => setMessageData({ ...messageData, subject: e.target.value })}
          />
          <TextField
            fullWidth
            margin="normal"
            label="İçerik"
            value={messageData.content}
            onChange={(e) => setMessageData({ ...messageData, content: e.target.value })}
            multiline
            rows={4}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSendMessageOpen(false)}>İptal</Button>
          <Button
            variant="contained"
            onClick={handleSendMessage}
            disabled={sendMessageMutation.isPending || !messageData.content}
          >
            Gönder
          </Button>
        </DialogActions>
      </Dialog>

      {/* Smart Notification Dialog */}
      <Dialog open={smartNotificationOpen} onClose={() => setSmartNotificationOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>Smart Notification Gönder</DialogTitle>
        <DialogContent>
          <FormControl fullWidth margin="normal">
            <InputLabel>Notification Tipi</InputLabel>
            <Select
              value={selectedNotificationType}
              onChange={(e) => setSelectedNotificationType(e.target.value as SmartNotificationType)}
              label="Notification Tipi"
            >
              <MenuItem value="PreArrival">Pre-Arrival (Check-in öncesi)</MenuItem>
              <MenuItem value="Arrival">Arrival (Check-in sonrası)</MenuItem>
              <MenuItem value="DuringStay">During Stay (Hizmet hatırlatmaları)</MenuItem>
              <MenuItem value="PreDeparture">Pre-Departure (Check-out öncesi)</MenuItem>
              <MenuItem value="SpecialOccasion">Special Occasion (Özel günler)</MenuItem>
            </Select>
          </FormControl>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setSmartNotificationOpen(false)}>İptal</Button>
          <Button
            variant="contained"
            onClick={handleSendSmartNotification}
            disabled={sendSmartNotificationMutation.isPending}
          >
            Gönder
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  )
}

export default CommunicationHistory

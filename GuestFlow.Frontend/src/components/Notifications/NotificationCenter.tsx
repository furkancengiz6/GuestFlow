import { useState } from 'react'
import {
  Drawer,
  Box,
  Typography,
  IconButton,
  Badge,
  List,
  ListItem,
  ListItemText,
  ListItemButton,
  ListItemIcon,
  Divider,
  Chip,
  Button,
  Tooltip,
} from '@mui/material'
import {
  Notifications as NotificationsIcon,
  Close as CloseIcon,
  CheckCircle as CheckCircleIcon,
  Delete as DeleteIcon,
  NotificationsNone as NotificationsNoneIcon,
} from '@mui/icons-material'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { notificationService, Notification } from '../../services/notificationService'
import { formatDate } from '../../utils/formatters'
import { useNotification } from '../../hooks/useNotification'
import { useRealtimeNotifications } from '../../hooks/useRealtimeNotifications'
import { SkeletonLoader } from '../Feedback/SkeletonLoader'
import { EnhancedContentState } from '../Feedback/EnhancedContentState'
import { useAuthStore } from '../../stores/authStore'

interface NotificationCenterProps {
  anchor?: 'left' | 'right' | 'top' | 'bottom'
}

/**
 * Notification Center component with real-time updates
 */
export const NotificationCenter = ({ anchor = 'right' }: NotificationCenterProps) => {
  const [open, setOpen] = useState(false)
  const queryClient = useQueryClient()
  const notification = useNotification()
  const { isAuthenticated } = useAuthStore()

  // Enable real-time notifications
  useRealtimeNotifications()

  const { data, isLoading } = useQuery({
    queryKey: ['notifications', 'recent', 10],
    queryFn: () => notificationService.getMyNotifications(1, 10),
    enabled: isAuthenticated, // Only fetch when authenticated
    refetchInterval: 30000, // Refetch every 30 seconds
  })

  const { data: statistics } = useQuery({
    queryKey: ['notification-statistics'],
    queryFn: () => notificationService.getStatistics(),
    enabled: isAuthenticated, // Only fetch when authenticated
  })

  const markAsReadMutation = useMutation({
    mutationFn: (id: number) => notificationService.markAsRead(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] })
      queryClient.invalidateQueries({ queryKey: ['notification-statistics'] })
    },
  })

  const markAllAsReadMutation = useMutation({
    mutationFn: async () => {
      // Mark all notifications as read individually
      if (data?.data) {
        const unreadNotifications = data.data.filter((n) => !n.isRead)
        await Promise.all(unreadNotifications.map((n) => notificationService.markAsRead(n.id)))
      }
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] })
      queryClient.invalidateQueries({ queryKey: ['notification-statistics'] })
      notification.showSuccess('Tüm bildirimler okundu olarak işaretlendi.')
    },
  })

  const deleteMutation = useMutation({
    mutationFn: (id: number) => notificationService.deleteNotification(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['notifications'] })
      queryClient.invalidateQueries({ queryKey: ['notification-statistics'] })
      notification.showSuccess('Bildirim silindi.')
    },
  })

  const handleNotificationClick = (notif: Notification) => {
    if (!notif.isRead) {
      markAsReadMutation.mutate(notif.id)
    }
    // Close drawer or navigate to notification detail
    setOpen(false)
  }

  const getTypeColor = (type: string): 'default' | 'primary' | 'secondary' | 'error' | 'info' | 'success' | 'warning' => {
    switch (type) {
      case 'Info':
        return 'info'
      case 'Success':
        return 'success'
      case 'Warning':
        return 'warning'
      case 'Error':
        return 'error'
      default:
        return 'default'
    }
  }

  const unreadCount = statistics?.unread || 0
  const notifications = data?.data || []

  return (
    <>
      <Tooltip title="Bildirimler">
        <IconButton
          color="inherit"
          onClick={() => setOpen(true)}
          sx={{ position: 'relative' }}
        >
          <Badge badgeContent={unreadCount} color="error">
            <NotificationsIcon />
          </Badge>
        </IconButton>
      </Tooltip>

      <Drawer
        anchor={anchor}
        open={open}
        onClose={() => setOpen(false)}
        PaperProps={{
          sx: { width: { xs: '100%', sm: 400 } },
        }}
      >
        <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
          {/* Header */}
          <Box sx={{ p: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center', borderBottom: 1, borderColor: 'divider' }}>
            <Typography variant="h6" sx={{ fontWeight: 600 }}>
              Bildirimler
            </Typography>
            <Box sx={{ display: 'flex', gap: 1 }}>
              {unreadCount > 0 && (
                <Button
                  size="small"
                  onClick={() => markAllAsReadMutation.mutate()}
                  disabled={markAllAsReadMutation.isPending}
                >
                  Tümünü Okundu İşaretle
                </Button>
              )}
              <IconButton size="small" onClick={() => setOpen(false)}>
                <CloseIcon />
              </IconButton>
            </Box>
          </Box>

          {/* Statistics */}
          {statistics && (
            <Box sx={{ p: 2, borderBottom: 1, borderColor: 'divider' }}>
              <Box sx={{ display: 'flex', gap: 2 }}>
                <Chip
                  label={`${statistics.total} Toplam`}
                  size="small"
                  variant="outlined"
                />
                <Chip
                  label={`${statistics.unread} Okunmamış`}
                  size="small"
                  color="warning"
                />
              </Box>
            </Box>
          )}

          {/* Notifications List */}
          <Box sx={{ flex: 1, overflow: 'auto' }}>
            {isLoading ? (
              <Box sx={{ p: 2 }}>
                <SkeletonLoader variant="list" rows={5} />
              </Box>
            ) : notifications.length === 0 ? (
              <EnhancedContentState
                state="empty"
                title="Bildirim yok"
                description="Henüz bildirim bulunmamaktadır."
                emptyIcon={<NotificationsNoneIcon sx={{ fontSize: 64, color: 'text.secondary' }} />}
              />
            ) : (
              <List>
                {notifications.map((notif, index) => (
                  <Box key={notif.id}>
                    <ListItem
                      disablePadding
                      sx={{
                        backgroundColor: notif.isRead ? 'transparent' : 'action.hover',
                        '&:hover': {
                          backgroundColor: 'action.selected',
                        },
                      }}
                    >
                      <ListItemButton onClick={() => handleNotificationClick(notif)}>
                        <ListItemIcon>
                          {notif.isRead ? (
                            <CheckCircleIcon color="success" fontSize="small" />
                          ) : (
                            <Badge variant="dot" color="error">
                              <NotificationsIcon fontSize="small" />
                            </Badge>
                          )}
                        </ListItemIcon>
                        <ListItemText
                          primary={
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 0.5 }}>
                              <Typography
                                variant="body2"
                                sx={{
                                  fontWeight: notif.isRead ? 'normal' : 'bold',
                                  flex: 1,
                                }}
                              >
                                {notif.title}
                              </Typography>
                              <Chip
                                label={notif.type}
                                size="small"
                                color={getTypeColor(notif.type)}
                              />
                            </Box>
                          }
                          secondary={
                            <Box>
                              <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 0.5 }}>
                                {notif.message}
                              </Typography>
                              <Typography variant="caption" color="text.secondary">
                                {formatDate(notif.createdDate)}
                              </Typography>
                            </Box>
                          }
                        />
                        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 0.5 }}>
                          {!notif.isRead && (
                            <IconButton
                              size="small"
                              onClick={(e) => {
                                e.stopPropagation()
                                markAsReadMutation.mutate(notif.id)
                              }}
                            >
                              <CheckCircleIcon fontSize="small" />
                            </IconButton>
                          )}
                          <IconButton
                            size="small"
                            onClick={(e) => {
                              e.stopPropagation()
                              deleteMutation.mutate(notif.id)
                            }}
                          >
                            <DeleteIcon fontSize="small" />
                          </IconButton>
                        </Box>
                      </ListItemButton>
                    </ListItem>
                    {index < notifications.length - 1 && <Divider />}
                  </Box>
                ))}
              </List>
            )}
          </Box>

          {/* Footer */}
          <Box sx={{ p: 2, borderTop: 1, borderColor: 'divider', textAlign: 'center' }}>
            <Button
              variant="outlined"
              fullWidth
              onClick={() => {
                setOpen(false)
                // Navigate to notifications page
                window.location.href = '/notifications'
              }}
            >
              Tümünü Görüntüle
            </Button>
          </Box>
        </Box>
      </Drawer>
    </>
  )
}

export default NotificationCenter


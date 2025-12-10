import { useEffect } from 'react'
import { Snackbar, Alert, AlertTitle } from '@mui/material'
import { useNotificationStore } from '../../stores/notificationStore'

const NotificationProvider = () => {
  const { notifications, removeNotification } = useNotificationStore()

  const handleClose = (id: string) => {
    removeNotification(id)
  }

  return (
    <>
      {notifications.map((notification) => (
        <Snackbar
          key={notification.id}
          open={true}
          autoHideDuration={notification.duration || 4000}
          onClose={() => handleClose(notification.id)}
          anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
          sx={{ bottom: { xs: 90, sm: 24 } }}
        >
          <Alert
            onClose={() => handleClose(notification.id)}
            severity={notification.severity}
            variant="filled"
            sx={{ width: '100%' }}
          >
            {notification.message}
          </Alert>
        </Snackbar>
      ))}
    </>
  )
}

export default NotificationProvider


import { useNotificationStore } from '../stores/notificationStore'

export const useNotification = () => {
  const addNotification = useNotificationStore((state) => state.addNotification)

  return {
    showSuccess: (message: string, duration?: number) => {
      addNotification(message, 'success', duration)
    },
    showError: (message: string, duration?: number) => {
      addNotification(message, 'error', duration || 6000)
    },
    showWarning: (message: string, duration?: number) => {
      addNotification(message, 'warning', duration)
    },
    showInfo: (message: string, duration?: number) => {
      addNotification(message, 'info', duration)
    },
  }
}


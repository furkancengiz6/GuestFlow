import { useEffect } from 'react'
import { useSignalR } from './useSignalR'
import { useNotification } from './useNotification'
import type { Notification } from '../services/notificationService'
import { useQueryClient } from '@tanstack/react-query'

interface RealtimeNotification extends Notification {
  priority?: 'low' | 'normal' | 'high'
  actionUrl?: string
}

/**
 * Hook for real-time notifications via SignalR
 */
export const useRealtimeNotifications = () => {
  const notification = useNotification()
  const queryClient = useQueryClient()

  const { isConnected } = useSignalR({
    onNotificationReceived: (realtimeNotification: RealtimeNotification) => {
      // Show toast notification
      const severity = realtimeNotification.type?.toLowerCase() || 'info'
      const message = `${realtimeNotification.title || 'Yeni Bildirim'}: ${realtimeNotification.message || ''}`
      
      if (severity === 'success') {
        notification.showSuccess(message)
      } else if (severity === 'error') {
        notification.showError(message)
      } else if (severity === 'warning') {
        notification.showWarning(message)
      } else {
        notification.showInfo(message)
      }

      // Invalidate queries
      queryClient.invalidateQueries({ queryKey: ['notifications'] })
      queryClient.invalidateQueries({ queryKey: ['notification-statistics'] })

      // Play notification sound (optional)
      if (typeof window !== 'undefined' && 'Audio' in window) {
        try {
          const audio = new Audio('/notification-sound.mp3')
          audio.volume = 0.3
          audio.play().catch(() => {
            // Ignore audio play errors
          })
        } catch (error) {
          // Ignore audio errors
        }
      }

      // Show browser notification (if permission granted)
      if (typeof window !== 'undefined' && 'Notification' in window) {
        if (window.Notification.permission === 'granted') {
          new window.Notification(realtimeNotification.title || 'Yeni Bildirim', {
            body: realtimeNotification.message || '',
            icon: '/favicon.ico',
            badge: '/favicon.ico',
            tag: `notification-${realtimeNotification.id}`,
            requireInteraction: realtimeNotification.priority === 'high',
          })
        }
      }
    },
  })

  // Request notification permission only on user interaction
  // Browser requires permission request to be triggered by user gesture
  useEffect(() => {
    if (typeof window === 'undefined' || !('Notification' in window)) {
      return
    }

    // Only request permission if it's default and user has interacted
    // We'll request it when first notification is received or user clicks something
    const handleUserInteraction = () => {
      if (window.Notification.permission === 'default') {
        window.Notification.requestPermission().catch(() => {
          // Ignore permission request errors
        })
        // Remove listener after first interaction
        document.removeEventListener('click', handleUserInteraction)
        document.removeEventListener('keydown', handleUserInteraction)
      }
    }

    // Add listeners for user interaction
    document.addEventListener('click', handleUserInteraction, { once: true })
    document.addEventListener('keydown', handleUserInteraction, { once: true })

    return () => {
      document.removeEventListener('click', handleUserInteraction)
      document.removeEventListener('keydown', handleUserInteraction)
    }
  }, [])

  return {
    isConnected,
  }
}


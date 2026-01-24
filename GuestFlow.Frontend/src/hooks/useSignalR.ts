import { useEffect, useRef, useState } from 'react'
import { signalRService } from '../services/signalRService'
import { useAuthStore } from '../stores/authStore'
import { useQueryClient } from '@tanstack/react-query'

interface UseSignalROptions {
  onNotificationReceived?: (notification: any) => void
  onLiveUpdate?: (update: any) => void
  onDashboardUpdate?: (update: any) => void
  onDailyOperationsUpdate?: (update: any) => void
  autoConnect?: boolean
}

/**
 * Hook for SignalR connection management
 */
export const useSignalR = (options: UseSignalROptions = {}) => {
  const { onNotificationReceived, onLiveUpdate, onDashboardUpdate, onDailyOperationsUpdate, autoConnect = true } = options
  const [isConnected, setIsConnected] = useState(false)
  const [connectionState, setConnectionState] = useState<string>('Disconnected')
  const queryClient = useQueryClient()
  const authStore = useAuthStore()
  const handlersRef = useRef<Map<string, (...args: any[]) => void>>(new Map())
  const initializedRef = useRef(false) // Prevent duplicate initialization in React StrictMode

  useEffect(() => {
    // Only connect if user is authenticated AND has access token AND autoConnect is enabled
    if (!autoConnect || !authStore.isAuthenticated || !authStore.accessToken) {
      // Disconnect if not authenticated
      if (authStore.isAuthenticated === false) {
        signalRService.stop()
        setIsConnected(false)
        setConnectionState('Disconnected')
        initializedRef.current = false // Reset for potential future login
      }
      return
    }

    // Prevent duplicate initialization in React StrictMode (development)
    if (initializedRef.current) {
      return
    }
    initializedRef.current = true

    const connect = async () => {
      try {
        await signalRService.start()
        setIsConnected(signalRService.isConnected())
        setConnectionState(signalRService.getState()?.toString() || 'Disconnected')

        // Register notification handler
        if (onNotificationReceived) {
          // Remove old handler if exists
          const oldHandler = handlersRef.current.get('ReceiveNotification')
          if (oldHandler) {
            signalRService.off('ReceiveNotification', oldHandler)
          }

          const notificationHandler = (notification: any) => {
            onNotificationReceived(notification)
            // Invalidate notifications query
            queryClient.invalidateQueries({ queryKey: ['notifications'] })
            queryClient.invalidateQueries({ queryKey: ['notification-statistics'] })
          }
          signalRService.onNotificationReceived(notificationHandler)
          handlersRef.current.set('ReceiveNotification', notificationHandler)
        }

        // Register live update handler
        if (onLiveUpdate) {
          // Remove old handler if exists
          const oldHandler = handlersRef.current.get('ReceiveLiveUpdate')
          if (oldHandler) {
            signalRService.off('ReceiveLiveUpdate', oldHandler)
          }

          const liveUpdateHandler = (update: any) => {
            onLiveUpdate(update)
            // Invalidate relevant queries based on update type
            if (update.entityType) {
              queryClient.invalidateQueries({ queryKey: [update.entityType.toLowerCase()] })
            }
          }
          signalRService.onLiveUpdate(liveUpdateHandler)
          handlersRef.current.set('ReceiveLiveUpdate', liveUpdateHandler)
        }

        // Register dashboard update handler
        if (onDashboardUpdate) {
          // Remove old handler if exists
          const oldHandler = handlersRef.current.get('ReceiveDashboardUpdate')
          if (oldHandler) {
            signalRService.off('ReceiveDashboardUpdate', oldHandler)
          }

          const dashboardHandler = (update: any) => {
            onDashboardUpdate(update)
            // Invalidate dashboard queries
            queryClient.invalidateQueries({ queryKey: ['dashboard'] })
          }
          signalRService.onDashboardUpdate(dashboardHandler)
          handlersRef.current.set('ReceiveDashboardUpdate', dashboardHandler)
        }

        // Register daily operations update handler
        if (onDailyOperationsUpdate) {
          // Remove old handler if exists
          const oldHandler = handlersRef.current.get('ReceiveDailyOperationsUpdate')
          if (oldHandler) {
            signalRService.off('ReceiveDailyOperationsUpdate', oldHandler)
          }

          const dailyOpsHandler = (update: any) => {
            onDailyOperationsUpdate(update)
            // Invalidate daily operations queries
            queryClient.invalidateQueries({ queryKey: ['dailyOperations'] })
            queryClient.invalidateQueries({ queryKey: ['transfers'] })
            queryClient.invalidateQueries({ queryKey: ['payments'] })
          }
          signalRService.onDailyOperationsUpdate(dailyOpsHandler)
          handlersRef.current.set('ReceiveDailyOperationsUpdate', dailyOpsHandler)
        }
      } catch (error) {
        console.error('SignalR connection error:', error)
        setIsConnected(false)
        setConnectionState('Disconnected')
      }
    }

    connect()

    // Check connection state periodically
    const interval = setInterval(() => {
      setIsConnected(signalRService.isConnected())
      setConnectionState(signalRService.getState()?.toString() || 'Disconnected')
    }, 5000)

    const handlers = handlersRef.current
    return () => {
      clearInterval(interval)
      // Remove handlers
      handlers.forEach((handler, eventName) => {
        signalRService.off(eventName, handler)
      })
      handlers.clear()
    }
  }, [authStore.isAuthenticated, authStore.accessToken, autoConnect, onNotificationReceived, onLiveUpdate, onDashboardUpdate, onDailyOperationsUpdate, queryClient])

  // Disconnect on logout and cleanup on unmount
  useEffect(() => {
    return () => {
      // Cleanup on unmount
      if (initializedRef.current) {
        signalRService.stop()
        setIsConnected(false)
        setConnectionState('Disconnected')
        initializedRef.current = false
      }
    }
  }, [])

  // Handle logout separately
  useEffect(() => {
    if (!authStore.isAuthenticated && initializedRef.current) {
      signalRService.stop()
      setIsConnected(false)
      setConnectionState('Disconnected')
      initializedRef.current = false // Reset for potential future login
    }
  }, [authStore.isAuthenticated])

  const reconnect = async () => {
    try {
      await signalRService.stop()
      await signalRService.start()
      setIsConnected(signalRService.isConnected())
      setConnectionState(signalRService.getState()?.toString() || 'Disconnected')
    } catch (error) {
      console.error('SignalR reconnect error:', error)
    }
  }

  const disconnect = async () => {
    await signalRService.stop()
    setIsConnected(false)
    setConnectionState('Disconnected')
  }

  return {
    isConnected,
    connectionState,
    reconnect,
    disconnect,
  }
}


import { useEffect, useRef, useCallback } from 'react'
import { useAuthStore } from '../stores/authStore'
import { useNotification } from './useNotification'

// Session timeout configuration
const SESSION_TIMEOUT = 30 * 60 * 1000 // 30 minutes
const WARNING_TIME = 5 * 60 * 1000 // 5 minutes before timeout
const CHECK_INTERVAL = 60 * 1000 // Check every minute

interface SessionTimeoutConfig {
  timeout?: number // Session timeout in milliseconds
  warningTime?: number // Warning time before timeout
  onTimeout?: () => void // Callback when session times out
  onWarning?: (remainingTime: number) => void // Callback when warning should be shown
}

/**
 * Hook for session timeout management
 * Automatically logs out user after inactivity
 */
export const useSessionTimeout = (config: SessionTimeoutConfig = {}) => {
  const { logout, isAuthenticated } = useAuthStore()
  const notification = useNotification()
  const timeoutRef = useRef<NodeJS.Timeout | null>(null)
  const warningTimeoutRef = useRef<NodeJS.Timeout | null>(null)
  const lastActivityRef = useRef<number>(Date.now())
  const warningShownRef = useRef(false)

  const {
    timeout = SESSION_TIMEOUT,
    warningTime = WARNING_TIME,
    onTimeout,
    onWarning,
  } = config

  const resetTimeout = useCallback(() => {
    lastActivityRef.current = Date.now()
    warningShownRef.current = false

    // Clear existing timeouts
    if (timeoutRef.current) {
      clearTimeout(timeoutRef.current)
    }
    if (warningTimeoutRef.current) {
      clearTimeout(warningTimeoutRef.current)
    }

    if (!isAuthenticated) return

    // Set warning timeout
    warningTimeoutRef.current = setTimeout(() => {
      const remainingTime = Math.ceil((timeout - warningTime) / 1000 / 60) // minutes
      warningShownRef.current = true

      if (onWarning) {
        onWarning(remainingTime)
      } else {
        notification.showWarning(
          `Oturumunuz ${remainingTime} dakika sonra sona erecek. Lütfen işlemlerinizi tamamlayın.`,
          10000
        )
      }
    }, timeout - warningTime)

    // Set logout timeout
    timeoutRef.current = setTimeout(() => {
      if (onTimeout) {
        onTimeout()
      } else {
        notification.showError('Oturum süreniz doldu. Lütfen tekrar giriş yapınız.')
        logout()
        window.location.href = '/login'
      }
    }, timeout)
  }, [timeout, warningTime, isAuthenticated, logout, notification, onTimeout, onWarning])

  // Track user activity
  useEffect(() => {
    if (!isAuthenticated) {
      // Clear timeouts if not authenticated
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current)
      }
      if (warningTimeoutRef.current) {
        clearTimeout(warningTimeoutRef.current)
      }
      return
    }

    // Events that indicate user activity
    const activityEvents = ['mousedown', 'mousemove', 'keypress', 'scroll', 'touchstart', 'click']

    const handleActivity = () => {
      const now = Date.now()
      // Only reset if significant time has passed (avoid too frequent resets)
      if (now - lastActivityRef.current > 60000) {
        resetTimeout()
      }
    }

    // Add event listeners
    activityEvents.forEach((event) => {
      window.addEventListener(event, handleActivity, { passive: true })
    })

    // Initial timeout setup
    resetTimeout()

    // Periodic check (every minute)
    const checkInterval = setInterval(() => {
      const inactiveTime = Date.now() - lastActivityRef.current
      if (inactiveTime >= timeout) {
        // Session expired
        if (onTimeout) {
          onTimeout()
        } else {
          logout()
          window.location.href = '/login'
        }
      }
    }, CHECK_INTERVAL)

    return () => {
      // Cleanup
      activityEvents.forEach((event) => {
        window.removeEventListener(event, handleActivity)
      })
      if (timeoutRef.current) {
        clearTimeout(timeoutRef.current)
      }
      if (warningTimeoutRef.current) {
        clearTimeout(warningTimeoutRef.current)
      }
      clearInterval(checkInterval)
    }
  }, [isAuthenticated, resetTimeout, timeout, logout, onTimeout])

  const extendSession = useCallback(() => {
    resetTimeout()
  }, [resetTimeout])

  return { extendSession }
}


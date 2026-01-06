import { useEffect, useRef } from 'react'
import { useAuthStore } from '../stores/authStore'
import axios from 'axios'
import { env } from '../config/env'

const API_BASE_URL = env.apiBaseUrl

// Token refresh interval (5 minutes before expiry)
const REFRESH_INTERVAL = 10 * 60 * 1000 // 10 minutes
const REFRESH_BEFORE_EXPIRY = 5 * 60 * 1000 // 5 minutes

/**
 * Hook for automatic token refresh
 * Proactively refreshes token before it expires
 */
export const useTokenRefresh = () => {
  const { accessToken, login, logout } = useAuthStore()
  const refreshIntervalRef = useRef<NodeJS.Timeout | null>(null)
  const isRefreshingRef = useRef(false)

  const refreshToken = async () => {
    if (isRefreshingRef.current) return

    try {
      isRefreshingRef.current = true
      const response = await axios.post(
        `${API_BASE_URL}/auth/refresh-token`,
        {},
        { withCredentials: true }
      )

      const { accessToken: newToken } = response.data.data || response.data

      if (newToken) {
        login(newToken, null)
      } else {
        throw new Error('Token refresh failed')
      }
    } catch (error) {
      console.error('Token refresh error:', error)
      logout()
      window.location.href = '/login'
    } finally {
      isRefreshingRef.current = false
    }
  }

  useEffect(() => {
    if (!accessToken) {
      // Clear interval if no token
      if (refreshIntervalRef.current) {
        clearInterval(refreshIntervalRef.current)
        refreshIntervalRef.current = null
      }
      return
    }

    // Decode JWT to get expiry time
    const getTokenExpiry = (token: string): number | null => {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]))
        return payload.exp ? payload.exp * 1000 : null
      } catch {
        return null
      }
    }

    const tokenExpiry = getTokenExpiry(accessToken)
    if (!tokenExpiry) {
      // If we can't decode token, refresh every 10 minutes
      refreshIntervalRef.current = setInterval(refreshToken, REFRESH_INTERVAL)
      return
    }

    // Calculate time until expiry
    const now = Date.now()
    const timeUntilExpiry = tokenExpiry - now

    // If token expires soon, refresh immediately
    if (timeUntilExpiry < REFRESH_BEFORE_EXPIRY) {
      refreshToken()
    }

    // Schedule refresh before expiry
    const refreshTime = Math.max(timeUntilExpiry - REFRESH_BEFORE_EXPIRY, 60000) // At least 1 minute

    refreshIntervalRef.current = setTimeout(() => {
      refreshToken()
      // Then set up interval for subsequent refreshes
      refreshIntervalRef.current = setInterval(refreshToken, REFRESH_INTERVAL)
    }, refreshTime)

    return () => {
      if (refreshIntervalRef.current) {
        if ('clearInterval' in globalThis && typeof refreshIntervalRef.current === 'number') {
          clearInterval(refreshIntervalRef.current)
        } else {
          clearTimeout(refreshIntervalRef.current as NodeJS.Timeout)
        }
        refreshIntervalRef.current = null
      }
    }
  }, [accessToken, login, logout])

  return { refreshToken }
}


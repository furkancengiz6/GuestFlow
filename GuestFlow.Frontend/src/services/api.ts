import axios, { AxiosInstance, InternalAxiosRequestConfig, AxiosError } from 'axios'
import { useAuthStore } from '../stores/authStore'
import { env } from '../config/env'

// Extend InternalAxiosRequestConfig to include _retryCount
declare module 'axios' {
  interface InternalAxiosRequestConfig {
    _retryCount?: number
  }
}

// API base URL
const API_BASE_URL = env.apiBaseUrl

// Retry configuration
const MAX_RETRIES = 3
const RETRY_DELAY = 1000 // 1 second

/**
 * Check if error should be retried
 */
const shouldRetry = (error: AxiosError): boolean => {
  // Don't retry on 4xx errors (client errors)
  if (error.response) {
    const status = error.response.status
    if (status >= 400 && status < 500) {
      return false
    }
  }

  // Retry on network errors and 5xx errors
  return !error.response || error.response.status >= 500
}

/**
 * Retry request with exponential backoff
 */
const retryRequest = async (
  error: AxiosError,
  retryCount: number = 0
): Promise<unknown> => {
  const config = error.config as InternalAxiosRequestConfig & { _retryCount?: number }

  if (!config || !shouldRetry(error) || retryCount >= MAX_RETRIES) {
    return Promise.reject(error)
  }

  config._retryCount = retryCount + 1

  // Exponential backoff: 1s, 2s, 4s
  const delay = RETRY_DELAY * Math.pow(2, retryCount)

  await new Promise((resolve) => setTimeout(resolve, delay))

  // Retry the request
  return apiClient(config)
}

// Axios instance oluştur
const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 30000,
  withCredentials: true, // refresh token cookie desteği için
})

let refreshPromise: Promise<string | null> | null = null

const getAccessToken = () => {
  const state = useAuthStore.getState()
  return state.accessToken
}

const setTokens = (accessToken: string) => {
  const { login, user } = useAuthStore.getState()
  login(accessToken, user)
}

const clearTokensAndRedirect = () => {
  const { logout } = useAuthStore.getState()
  logout()
  window.location.href = '/login'
}

const refreshTokenOnce = async (): Promise<string | null> => {
  if (refreshPromise) return refreshPromise

  refreshPromise = axios
    .post(`${API_BASE_URL}/auth/refresh-token`, {}, { withCredentials: true })
    .then((res) => {
      const { accessToken } = res.data.data || res.data
      if (!accessToken) throw new Error('refresh_failed')
      setTokens(accessToken)
      return accessToken
    })
    .catch((err) => {
      clearTokensAndRedirect()
      throw err
    })
    .finally(() => {
      refreshPromise = null
    })

  return refreshPromise
}

// Request interceptor - Token ekle
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = getAccessToken()
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Response interceptor - Token refresh ve hata yönetimi
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean }

    // 401 Unauthorized - Token yenileme
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true
      try {
        const newAccess = await refreshTokenOnce()
        if (newAccess && originalRequest.headers) {
          originalRequest.headers.Authorization = `Bearer ${newAccess}`
        }
        return apiClient(originalRequest)
      } catch (refreshError) {
        return Promise.reject(refreshError)
      }
    }

    // Retry logic for network errors and 5xx errors
    if (shouldRetry(error) && (!originalRequest._retryCount || originalRequest._retryCount < MAX_RETRIES)) {
      try {
        return await retryRequest(error, originalRequest._retryCount || 0)
      } catch (retryError) {
        return Promise.reject(retryError)
      }
    }

    // Enhanced error handling
    if (error.response) {
      // Backend validation errors are already formatted
      // Network errors and timeouts are handled by the error handler utility
      const errorMessage = error.response.data?.message || error.message
      
      // Log error for debugging (only in development)
      if (import.meta.env.DEV) {
        console.error('API Error:', {
          url: originalRequest.url,
          method: originalRequest.method,
          status: error.response.status,
          message: errorMessage,
          data: error.response.data,
        })
      }
    } else if (error.request) {
      // Network error - no response received
      console.error('Network Error:', error.message)
    } else {
      // Request setup error
      console.error('Request Error:', error.message)
    }

    return Promise.reject(error)
  }
)

export default apiClient


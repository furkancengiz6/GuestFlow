import axios, { AxiosInstance, InternalAxiosRequestConfig } from 'axios'
import { useAuthStore } from '../stores/authStore'

// API base URL
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5146/api/v1'

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

const getRefreshToken = () => {
  const state = useAuthStore.getState()
  return state.refreshToken
}

const setTokens = (accessToken: string, refreshToken?: string) => {
  const { login, user } = useAuthStore.getState()
  login(accessToken, refreshToken ?? getRefreshToken() ?? '', user)
}

const clearTokensAndRedirect = () => {
  const { logout } = useAuthStore.getState()
  logout()
  window.location.href = '/login'
}

const refreshTokenOnce = async (): Promise<string | null> => {
  if (refreshPromise) return refreshPromise

  const token = getRefreshToken()
  if (!token) {
    clearTokensAndRedirect()
    return null
  }

  refreshPromise = axios
    .post(
      `${API_BASE_URL}/auth/refresh-token`,
      { refreshToken: token },
      { withCredentials: true }
    )
    .then((res) => {
      const { accessToken, refreshToken: newRefresh } = res.data.data || res.data
      if (!accessToken) throw new Error('refresh_failed')
      setTokens(accessToken, newRefresh)
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

    return Promise.reject(error)
  }
)

export default apiClient


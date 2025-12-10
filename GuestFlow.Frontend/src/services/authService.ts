import apiClient from './api'
import { useAuthStore } from '../stores/authStore'

export const authService = {
  async logout() {
    try {
      // refresh token cookie desteği için body boş gönderilebilir
      await apiClient.post('/auth/revoke-token', {})
    } catch (err) {
      // sessizce yut; zaten logout edeceğiz
      console.warn('logout revoke failed', err)
    } finally {
      useAuthStore.getState().logout()
      window.location.href = '/login'
    }
  },
}


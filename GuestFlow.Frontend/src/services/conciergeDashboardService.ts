import apiClient from './api'
import type {
  ConciergeCheckInOut,
  ActiveGuest,
  UnifiedGuestProfile,
  UpcomingServices,
  GuestHistoryDashboard,
} from '../types/conciergeDashboard'

export const conciergeDashboardService = {
  // Bugünkü check-in'leri getir
  getTodayCheckIns: async (): Promise<ConciergeCheckInOut> => {
    const response = await apiClient.get('/ConciergeDashboard/check-ins/today')
    return response.data.data || response.data
  },

  // Bugünkü check-out'ları getir
  getTodayCheckOuts: async (): Promise<ConciergeCheckInOut> => {
    const response = await apiClient.get('/ConciergeDashboard/check-outs/today')
    return response.data.data || response.data
  },

  // Aktif misafirleri getir
  getActiveGuests: async (): Promise<ActiveGuest[]> => {
    const response = await apiClient.get('/ConciergeDashboard/active-guests')
    return response.data.data || response.data
  },

  // Unified guest profile getir
  getUnifiedGuestProfile: async (guestId: number): Promise<UnifiedGuestProfile> => {
    const response = await apiClient.get(`/ConciergeDashboard/guests/${guestId}/unified-profile`)
    return response.data.data || response.data
  },

  // Yaklaşan servisleri getir
  getUpcomingServices: async (): Promise<UpcomingServices> => {
    const response = await apiClient.get('/ConciergeDashboard/upcoming-services')
    return response.data.data || response.data
  },

  // Guest history dashboard getir
  getGuestHistoryDashboard: async (guestId: number): Promise<GuestHistoryDashboard> => {
    const response = await apiClient.get(`/ConciergeDashboard/guests/${guestId}/history`)
    return response.data.data || response.data
  },

  // Bildirim gönderimleri
  sendNotifications: async (type: string, targetDate?: string): Promise<any> => {
    const params = targetDate ? { targetDate } : {}
    const response = await apiClient.post(`/ConciergeDashboard/notifications/${type}`, null, { params })
    return response.data
  },

  sendCustomNotification: async (request: { guestId: number; message: string; channel: string }): Promise<any> => {
    const response = await apiClient.post('/ConciergeDashboard/notifications/custom', request)
    return response.data
  },

  performQuickAction: async (action: string, guestId: number): Promise<any> => {
    const response = await apiClient.post(`/ConciergeDashboard/quick-action/${action}/${guestId}`)
    return response.data
  },
}

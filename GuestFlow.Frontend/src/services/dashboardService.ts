import apiClient from './api'
import type {
  DashboardOverview,
  QuickStats,
  RecentActivity,
  RevenueChartData,
  UpcomingBookings,
  GuestStatistics,
} from '../types/dashboard'

export const dashboardService = {
  // Dashboard genel bakış
  getOverview: async (): Promise<DashboardOverview> => {
    const response = await apiClient.get('/dashboard/overview')
    return response.data.data || response.data
  },

  // Hızlı istatistikler
  getQuickStats: async (): Promise<QuickStats> => {
    const response = await apiClient.get('/dashboard/quick-stats')
    return response.data.data || response.data
  },

  // Son aktiviteler
  getRecentActivities: async (limit: number = 10): Promise<RecentActivity> => {
    const response = await apiClient.get('/dashboard/recent-activities', {
      params: { limit },
    })
    return response.data.data || response.data
  },

  // Gelir grafik verileri
  getRevenueChartData: async (
    period: 'daily' | 'weekly' | 'monthly' = 'daily',
    days?: number
  ): Promise<RevenueChartData> => {
    const response = await apiClient.get('/dashboard/revenue-chart', {
      params: { period, days },
    })
    return response.data.data || response.data
  },

  // Yaklaşan rezervasyonlar
  getUpcomingBookings: async (
    startDate?: string,
    endDate?: string
  ): Promise<UpcomingBookings> => {
    const response = await apiClient.get('/dashboard/upcoming-bookings', {
      params: { startDate, endDate },
    })
    return response.data.data || response.data
  },

  // Misafir istatistikleri
  getGuestStatistics: async (): Promise<GuestStatistics> => {
    const response = await apiClient.get('/dashboard/guest-statistics')
    return response.data.data || response.data
  },
}


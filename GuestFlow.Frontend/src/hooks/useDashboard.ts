import { useQuery } from '@tanstack/react-query'
import { dashboardService } from '../services/dashboardService'
import type {
  DashboardOverview,
  QuickStats,
  RecentActivity,
  RevenueChartData,
  UpcomingBookings,
  GuestStatistics,
} from '../types/dashboard'

export const useDashboardOverview = () => {
  return useQuery<DashboardOverview>({
    queryKey: ['dashboard', 'overview'],
    queryFn: () => dashboardService.getOverview(),
    staleTime: 2 * 60 * 1000, // 2 dakika
  })
}

export const useQuickStats = () => {
  return useQuery<QuickStats>({
    queryKey: ['dashboard', 'quick-stats'],
    queryFn: () => dashboardService.getQuickStats(),
    staleTime: 2 * 60 * 1000,
  })
}

export const useRecentActivities = (limit: number = 10) => {
  return useQuery<RecentActivity>({
    queryKey: ['dashboard', 'recent-activities', limit],
    queryFn: () => dashboardService.getRecentActivities(limit),
    staleTime: 1 * 60 * 1000, // 1 dakika
  })
}

export const useRevenueChartData = (
  period: 'daily' | 'weekly' | 'monthly' = 'daily',
  days?: number
) => {
  return useQuery<RevenueChartData>({
    queryKey: ['dashboard', 'revenue-chart', period, days],
    queryFn: () => dashboardService.getRevenueChartData(period, days),
    staleTime: 5 * 60 * 1000, // 5 dakika
  })
}

export const useUpcomingBookings = (
  startDate?: string,
  endDate?: string
) => {
  return useQuery<UpcomingBookings>({
    queryKey: ['dashboard', 'upcoming-bookings', startDate, endDate],
    queryFn: () => dashboardService.getUpcomingBookings(startDate, endDate),
    staleTime: 1 * 60 * 1000,
  })
}

export const useGuestStatistics = () => {
  return useQuery<GuestStatistics>({
    queryKey: ['dashboard', 'guest-statistics'],
    queryFn: () => dashboardService.getGuestStatistics(),
    staleTime: 2 * 60 * 1000,
  })
}


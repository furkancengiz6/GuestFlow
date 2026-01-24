import { useQuery } from '@tanstack/react-query'
import { conciergeDashboardService } from '../services/conciergeDashboardService'
import type {
  ConciergeCheckInOut,
  ActiveGuest,
  UnifiedGuestProfile,
  UpcomingServices,
  GuestHistoryDashboard,
} from '../types/conciergeDashboard'

export const useTodayCheckIns = () => {
  return useQuery<ConciergeCheckInOut>({
    queryKey: ['concierge', 'check-ins', 'today'],
    queryFn: () => conciergeDashboardService.getTodayCheckIns(),
    refetchInterval: 60000, // Her 1 dakikada bir yenile
  })
}

export const useTodayCheckOuts = () => {
  return useQuery<ConciergeCheckInOut>({
    queryKey: ['concierge', 'check-outs', 'today'],
    queryFn: () => conciergeDashboardService.getTodayCheckOuts(),
    refetchInterval: 60000, // Her 1 dakikada bir yenile
  })
}

export const useActiveGuests = () => {
  return useQuery<ActiveGuest[]>({
    queryKey: ['concierge', 'active-guests'],
    queryFn: () => conciergeDashboardService.getActiveGuests(),
    refetchInterval: 60000, // Her 1 dakikada bir yenile
  })
}

export const useUnifiedGuestProfile = (guestId: number) => {
  return useQuery<UnifiedGuestProfile>({
    queryKey: ['concierge', 'guest-profile', guestId],
    queryFn: () => conciergeDashboardService.getUnifiedGuestProfile(guestId),
    enabled: !!guestId,
  })
}

export const useUpcomingServices = () => {
  return useQuery<UpcomingServices>({
    queryKey: ['concierge', 'upcoming-services'],
    queryFn: () => conciergeDashboardService.getUpcomingServices(),
    refetchInterval: 60000, // Her 1 dakikada bir yenile
  })
}

export const useGuestHistoryDashboard = (guestId: number) => {
  return useQuery<GuestHistoryDashboard>({
    queryKey: ['concierge', 'guest-history', guestId],
    queryFn: () => conciergeDashboardService.getGuestHistoryDashboard(guestId),
    enabled: !!guestId,
  })
}

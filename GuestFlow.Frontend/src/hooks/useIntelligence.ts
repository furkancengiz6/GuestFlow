import { useQuery } from '@tanstack/react-query'
import { intelligenceService } from '../services/intelligenceService'

/**
 * Hook for fetching guest-specific intelligence risks
 */
export const useGuestIntelligenceRisks = (guestId: number) => {
    return useQuery({
        queryKey: ['intelligence', 'risks', guestId],
        queryFn: () => intelligenceService.identifyGuestRisks(guestId),
        enabled: !!guestId,
    })
}

/**
 * Hook for fetching best staff matches for a guest
 */
export const useGuestStaffMatches = (guestId: number, limit: number = 5) => {
    return useQuery({
        queryKey: ['intelligence', 'staff-matches', guestId, limit],
        queryFn: () => intelligenceService.findBestStaffMatches(guestId, limit),
        enabled: !!guestId,
    })
}

/**
 * Hook for fetching best service matches/recommendations for a guest
 */
export const useGuestServiceRecommendations = (guestId: number, serviceType?: string, limit: number = 5) => {
    return useQuery({
        queryKey: ['intelligence', 'service-matches', guestId, serviceType, limit],
        queryFn: () => intelligenceService.findBestServiceMatches(guestId, serviceType, limit),
        enabled: !!guestId,
    })
}

/**
 * Hook for fetching proactive recommendations for a guest
 */
export const useGuestProactiveRecommendations = (guestId: number) => {
    return useQuery({
        queryKey: ['intelligence', 'proactive-recommendations', guestId],
        queryFn: () => intelligenceService.getProactiveRecommendations(guestId),
        enabled: !!guestId,
    })
}

/**
 * Hook for fetching preference patterns for a guest
 */
export const useGuestPreferencePatterns = (guestId: number) => {
    return useQuery({
        queryKey: ['intelligence', 'preference-patterns', guestId],
        queryFn: () => intelligenceService.getGuestPreferencePatterns(guestId),
        enabled: !!guestId,
    })
}

/**
 * Hook for fetching behavioral insights (AI extracted from notes)
 */
export const useGuestBehavioralInsights = (guestId: number, source?: string) => {
    return useQuery({
        queryKey: ['intelligence', 'behavioral-insights', guestId, source],
        queryFn: () => intelligenceService.getRecentBehavioralInsights(guestId, source),
        enabled: !!guestId,
    })
}

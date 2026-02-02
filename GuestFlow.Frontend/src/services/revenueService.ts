import apiClient from './api'
import type { RevenueDashboard } from '../types/revenue'

export const revenueService = {
    /**
     * Revenue dashboard verilerini getirir (ADR, RevPAR, Occupancy Rate)
     * @param startDate - Başlangıç tarihi (opsiyonel, varsayılan: 30 gün önce)
     * @param endDate - Bitiş tarihi (opsiyonel, varsayılan: bugün)
     */
    getRevenueDashboard: async (
        startDate?: string,
        endDate?: string
    ): Promise<RevenueDashboard> => {
        const response = await apiClient.get('/revenue/dashboard', {
            params: { startDate, endDate },
        })
        return response.data.data || response.data
    },
}

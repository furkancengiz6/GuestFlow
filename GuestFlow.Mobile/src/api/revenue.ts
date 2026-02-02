import apiClient from './client';

/**
 * Revenue Dashboard response tipi
 */
export interface RevenueDashboard {
    adr: number;           // Average Daily Rate
    revPar: number;        // Revenue Per Available Room
    occupancyRate: number; // Doluluk Oranı (0-1)
    totalRevenue: number;
    totalRoomsSold: number;
}

/**
 * Revenue API servisi - Mobil uygulama için
 */
export const revenueService = {
    /**
     * Revenue dashboard verilerini getirir
     */
    getRevenueDashboard: async (startDate?: string, endDate?: string): Promise<RevenueDashboard> => {
        const params: Record<string, string> = {};
        if (startDate) params.startDate = startDate;
        if (endDate) params.endDate = endDate;

        const response = await apiClient.get('/revenue/dashboard', { params });
        return response.data.data || response.data;
    },
};

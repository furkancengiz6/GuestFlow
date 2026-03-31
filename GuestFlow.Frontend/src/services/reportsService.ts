import apiClient from './api'

export interface RevenueSummary {
  startDate?: string
  endDate?: string
  totalRevenueByCurrency: Record<string, number>
  transferRevenueByCurrency: Record<string, number>
  cityTourRevenueByCurrency: Record<string, number>
  yachtTourRevenueByCurrency: Record<string, number>
  generalRevenueByCurrency: Record<string, number>
  refundedAmountByCurrency: Record<string, number>
  netRevenueByCurrency: Record<string, number>
  totalBookings: number
  cityTourCount: number
  yachtTourCount: number
  transferCount: number
  totalPaymentCount: number
}

export interface TransferStatistics {
  startDate?: string
  endDate?: string
  totalTransfers: number
  totalRevenue: number
  averagePrice: number
  fromAirportCount: number
  toAirportCount: number
  completedTransfers: number
  pendingTransfers: number
  transfersByStatus: Record<string, number>
}

export interface PersonnelPerformance {
  personnelId: number
  fullName: string
  userType: string
  totalBookings: number
  transferCount: number
  cityTourCount: number
  yachtTourCount: number
  totalRevenue: number
  averageBookingValue: number
}

export interface ReportFilters {
  startDate?: string
  endDate?: string
  serviceType?: string // 'Transfer', 'CityTour', 'YachtTour'
  personnelId?: number
}

export const reportsService = {
  /**
   * Get revenue summary with filters
   */
  getRevenueSummary: async (filters?: ReportFilters): Promise<RevenueSummary> => {
    const params: Record<string, string | number> = {}
    if (filters?.startDate) params.startDate = filters.startDate
    if (filters?.endDate) params.endDate = filters.endDate
    if (filters?.serviceType) params.serviceType = filters.serviceType
    if (filters?.personnelId) params.personnelId = filters.personnelId

    const response = await apiClient.get<{ success: boolean; data: RevenueSummary }>(
      '/Reports/revenue-summary',
      { params }
    )
    return response.data.data
  },

  /**
   * Get transfer statistics with filters
   */
  getTransferStatistics: async (filters?: ReportFilters): Promise<TransferStatistics> => {
    const params: Record<string, string | number> = {}
    if (filters?.startDate) params.startDate = filters.startDate
    if (filters?.endDate) params.endDate = filters.endDate
    if (filters?.personnelId) params.personnelId = filters.personnelId

    const response = await apiClient.get<{ success: boolean; data: TransferStatistics }>(
      '/Reports/transfer-statistics',
      { params }
    )
    return response.data.data
  },

  /**
   * Get personnel performance report with filters
   */
  getPersonnelPerformance: async (filters?: ReportFilters): Promise<PersonnelPerformance[]> => {
    const params: Record<string, string | number> = {}
    if (filters?.startDate) params.startDate = filters.startDate
    if (filters?.endDate) params.endDate = filters.endDate
    if (filters?.serviceType) params.serviceType = filters.serviceType
    if (filters?.personnelId) params.personnelId = filters.personnelId

    const response = await apiClient.get<{ success: boolean; data: PersonnelPerformance[] }>(
      '/Reports/personnel-performance',
      { params }
    )
    return response.data.data
  },

  /**
   * Get AI-driven insights for dashboard
   */
  getDashboardAIInsights: async (): Promise<string> => {
    const response = await apiClient.get<{ success: boolean; data: { insight: string } }>(
      '/Reports/dashboard-ai-insights'
    )
    return response.data.data.insight
  },

  /**
   * Get AI-driven insights for revenue reports
   */
  getRevenueAIInsights: async (filters?: ReportFilters): Promise<string> => {
    const params: Record<string, string | number> = {}
    if (filters?.startDate) params.startDate = filters.startDate
    if (filters?.endDate) params.endDate = filters.endDate

    const response = await apiClient.get<{ success: boolean; data: { insight: string } }>(
      '/Reports/revenue-ai-insights',
      { params }
    )
    return response.data.data.insight
  },
}

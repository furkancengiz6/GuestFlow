import apiClient from './api'

export interface LoginAttempt {
  id: number
  email: string
  ipAddress?: string | null
  isSuccessful: boolean
  failureReason?: string | null
  attemptDate: string
  personnelId?: number | null
  personnelName?: string | null
}

export interface LoginAuditStatistics {
  startDate?: string | null
  endDate?: string | null
  totalAttempts: number
  successfulAttempts: number
  failedAttempts: number
  successRate: number
  uniqueUsers: number
  uniqueIpAddresses: number
  failureReasons: Record<string, number>
  attemptsByHour: Record<string, number>
}

export interface FailedLoginSummary {
  email: string
  personnelId?: number | null
  personnelName?: string | null
  failedAttemptCount: number
  lastFailedAttempt: string
  lastIpAddress?: string | null
  mostCommonFailureReason?: string | null
}

export const loginAuditService = {
  getLoginAttempts: async (params?: {
    startDate?: string
    endDate?: string
    email?: string
    ipAddress?: string
    isSuccessful?: boolean
    personnelId?: number
    pageNumber?: number
    pageSize?: number
  }): Promise<LoginAttempt[]> => {
    const response = await apiClient.get('/LoginAudit/attempts', { params })
    return response.data.data
  },

  getStatistics: async (params?: {
    startDate?: string
    endDate?: string
  }): Promise<LoginAuditStatistics> => {
    const response = await apiClient.get('/LoginAudit/statistics', { params })
    return response.data.data
  },

  getFailedLoginSummary: async (params?: {
    startDate?: string
    endDate?: string
    topCount?: number
  }): Promise<FailedLoginSummary[]> => {
    const response = await apiClient.get('/LoginAudit/failed-summary', { params })
    return response.data.data
  },
}

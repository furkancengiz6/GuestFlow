import apiClient from './api'

export interface SmsHistory {
  id: number
  phoneNumber: string
  message: string
  status: string
  sentDate: string
  guestId?: number
  guestName?: string
  personnelId?: number
  personnelName?: string
  errorMessage?: string
}

export interface PagedSmsHistory {
  data: SmsHistory[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

export interface SendSmsRequest {
  phoneNumber: string
  message: string
  guestId?: number
}

export interface SmsStatistics {
  totalSent: number
  totalFailed: number
  totalPending: number
  successRate: number
}

export interface SmsFilters {
  startDate?: string
  endDate?: string
  status?: string
  guestId?: number
  phoneNumber?: string
  searchTerm?: string
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

export const smsService = {
  getSmsHistory: async (
    pageNumber: number = 1,
    pageSize: number = 10,
    filters?: SmsFilters
  ): Promise<PagedSmsHistory> => {
    const params: any = { pageNumber, pageSize }
    
    if (filters) {
      if (filters.startDate) params.startDate = filters.startDate
      if (filters.endDate) params.endDate = filters.endDate
      if (filters.status) params.status = filters.status
      if (filters.guestId) params.guestId = filters.guestId
      if (filters.phoneNumber) params.phoneNumber = filters.phoneNumber
      if (filters.searchTerm) params.searchTerm = filters.searchTerm
      if (filters.sortBy) params.sortBy = filters.sortBy
      if (filters.sortOrder) params.sortOrder = filters.sortOrder
    }
    
    const response = await apiClient.get('/Sms', { params })
    return response.data.data
  },

  getSmsById: async (id: number): Promise<SmsHistory> => {
    const response = await apiClient.get(`/Sms/${id}`)
    return response.data.data
  },

  sendSms: async (data: SendSmsRequest): Promise<void> => {
    await apiClient.post('/Sms/send', data)
  },

  getSmsByGuestId: async (guestId: number): Promise<SmsHistory[]> => {
    const response = await apiClient.get(`/Sms/by-guest/${guestId}`)
    return response.data.data
  },

  getSmsByStatus: async (status: string): Promise<SmsHistory[]> => {
    const response = await apiClient.get(`/Sms/by-status/${status}`)
    return response.data.data
  },

  getStatistics: async (): Promise<SmsStatistics> => {
    const response = await apiClient.get('/Sms/statistics')
    return response.data.data
  },

  sendTransferReminder: async (transferId: number): Promise<void> => {
    await apiClient.post(`/Sms/transfer-reminder/${transferId}`)
  },

  sendTourReminder: async (tourType: 'city' | 'yacht', tourId: number): Promise<void> => {
    await apiClient.post(`/Sms/tour-reminder/${tourType}/${tourId}`)
  },

  sendReservationConfirmation: async (reservationId: number): Promise<void> => {
    await apiClient.post(`/Sms/reservation-confirmation/${reservationId}`)
  },
}


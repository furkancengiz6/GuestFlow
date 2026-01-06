import apiClient from './api'

export interface DailyRevenue {
  id: number
  revenueDate: string
  revenueAmount: number
  currency: string
  note?: string
  createdDate: string
}

export interface PagedDailyRevenues {
  data: DailyRevenue[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

export interface CreateDailyRevenueRequest {
  revenueDate: string
  revenueAmount: number
  currency: string
  note?: string
}

export interface UpdateDailyRevenueRequest {
  revenueDate: string
  revenueAmount: number
  currency: string
  note?: string
}

export interface DailyRevenueFilters {
  startDate?: string
  endDate?: string
  currency?: string
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

export const dailyRevenueService = {
  getDailyRevenues: async (
    pageNumber: number = 1,
    pageSize: number = 10,
    filters?: DailyRevenueFilters
  ): Promise<PagedDailyRevenues> => {
    const params: any = { pageNumber, pageSize }
    
    if (filters) {
      if (filters.startDate) params.startDate = filters.startDate
      if (filters.endDate) params.endDate = filters.endDate
      if (filters.currency) params.currency = filters.currency
      if (filters.sortBy) params.sortBy = filters.sortBy
      if (filters.sortOrder) params.sortOrder = filters.sortOrder
    }
    
    const response = await apiClient.get('/DailyRevenues', { params })
    return response.data.data
  },

  getDailyRevenueById: async (id: number): Promise<DailyRevenue> => {
    const response = await apiClient.get(`/DailyRevenues/${id}`)
    return response.data.data
  },

  createDailyRevenue: async (data: CreateDailyRevenueRequest): Promise<DailyRevenue> => {
    const response = await apiClient.post('/DailyRevenues', data)
    return response.data.data
  },

  updateDailyRevenue: async (id: number, data: UpdateDailyRevenueRequest): Promise<DailyRevenue> => {
    const response = await apiClient.put(`/DailyRevenues/${id}`, data)
    return response.data.data
  },

  deleteDailyRevenue: async (id: number): Promise<void> => {
    await apiClient.delete(`/DailyRevenues/${id}`)
  },
}


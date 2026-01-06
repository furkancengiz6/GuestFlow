import apiClient from './api'

export interface DailyNote {
  id: number
  noteDate: string
  note: string
  personnelId: number
  personnelName?: string
  createdDate: string
}

export interface PagedDailyNotes {
  data: DailyNote[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

export interface CreateDailyNoteRequest {
  noteDate: string
  note: string
  personnelId: number
}

export interface UpdateDailyNoteRequest {
  noteDate: string
  note: string
  personnelId: number
}

export interface DailyNoteFilters {
  startDate?: string
  endDate?: string
  personnelId?: number
  searchTerm?: string
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

export const dailyNoteService = {
  getDailyNotes: async (
    pageNumber: number = 1,
    pageSize: number = 10,
    filters?: DailyNoteFilters
  ): Promise<PagedDailyNotes> => {
    const params: any = { pageNumber, pageSize }
    
    if (filters) {
      if (filters.startDate) params.startDate = filters.startDate
      if (filters.endDate) params.endDate = filters.endDate
      if (filters.personnelId) params.personnelId = filters.personnelId
      if (filters.searchTerm) params.searchTerm = filters.searchTerm
      if (filters.sortBy) params.sortBy = filters.sortBy
      if (filters.sortOrder) params.sortOrder = filters.sortOrder
    }
    
    const response = await apiClient.get('/DailyNotes', { params })
    return response.data.data
  },

  getDailyNoteById: async (id: number): Promise<DailyNote> => {
    const response = await apiClient.get(`/DailyNotes/${id}`)
    return response.data.data
  },

  createDailyNote: async (data: CreateDailyNoteRequest): Promise<DailyNote> => {
    const response = await apiClient.post('/DailyNotes', data)
    return response.data.data
  },

  updateDailyNote: async (id: number, data: UpdateDailyNoteRequest): Promise<DailyNote> => {
    const response = await apiClient.put(`/DailyNotes/${id}`, data)
    return response.data.data
  },

  deleteDailyNote: async (id: number): Promise<void> => {
    await apiClient.delete(`/DailyNotes/${id}`)
  },
}


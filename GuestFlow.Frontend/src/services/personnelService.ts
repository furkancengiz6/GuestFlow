import api from './api'

export interface Personnel {
  id: number
  fullName: string
  email: string
  userType: string
  createdDate: string
}

export interface PersonnelDetail extends Personnel {
  statistics?: PersonnelStatistics
  activities?: PersonnelActivity[]
}

export interface PersonnelStatistics {
  totalTransfers: number
  totalCityTours: number
  totalYachtTours: number
  totalInvoices: number
  totalRevenue: number
}

export interface PersonnelActivity {
  id: number
  activityType: string
  description: string
  activityDate: string
  relatedEntityId?: number
  relatedEntityType?: string
}

export interface PersonnelFilters {
  searchTerm?: string
  userType?: string
  startDate?: string
  endDate?: string
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

export interface CreatePersonnelRequest {
  fullName: string
  email: string
  password: string
  userType: string
}

export interface UpdatePersonnelRequest {
  fullName?: string
  email?: string
  password?: string
  userType?: string
}

export interface PagedPersonnel {
  data: Personnel[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
  isFirstPage: boolean
  isLastPage: boolean
}

export const personnelService = {
  getPersonnel: async (
    pageNumber: number = 1,
    pageSize: number = 10,
    filters?: PersonnelFilters
  ): Promise<PagedPersonnel> => {
    const params = new URLSearchParams()
    params.append('pageNumber', pageNumber.toString())
    params.append('pageSize', pageSize.toString())

    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          params.append(key, value.toString())
        }
      })
    }

    const response = await api.get(`/personnel?${params.toString()}`)
    return response.data.data
  },

  getPersonnelById: async (id: number): Promise<Personnel> => {
    const response = await api.get(`/personnel/${id}`)
    return response.data.data
  },

  getPersonnelDetail: async (id: number): Promise<PersonnelDetail> => {
    const response = await api.get(`/personnel/${id}/detail`)
    return response.data.data
  },

  createPersonnel: async (data: CreatePersonnelRequest): Promise<Personnel> => {
    const response = await api.post('/personnel', data)
    return response.data.data
  },

  updatePersonnel: async (id: number, data: UpdatePersonnelRequest): Promise<Personnel> => {
    const response = await api.put(`/personnel/${id}`, data)
    return response.data.data
  },

  deletePersonnel: async (id: number): Promise<void> => {
    await api.delete(`/personnel/${id}`)
  },
}


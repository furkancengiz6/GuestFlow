import apiClient from './api'

export interface Airport {
  id: number
  airportName: string
  cityId: number
  cityName?: string
  createdDate: string
}

export interface PagedAirports {
  data: Airport[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

export interface CreateAirportRequest {
  airportName: string
  cityId: number
}

export interface UpdateAirportRequest {
  airportName: string
  cityId: number
}

export interface AirportFilters {
  searchTerm?: string
  cityId?: number
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

export const airportService = {
  getAirports: async (
    pageNumber: number = 1,
    pageSize: number = 10,
    filters?: AirportFilters
  ): Promise<PagedAirports> => {
    const params: any = { pageNumber, pageSize }
    
    if (filters) {
      if (filters.searchTerm) params.searchTerm = filters.searchTerm
      if (filters.cityId) params.cityId = filters.cityId
      if (filters.sortBy) params.sortBy = filters.sortBy
      if (filters.sortOrder) params.sortOrder = filters.sortOrder
    }
    
    const response = await apiClient.get('/Airports', { params })
    return response.data.data
  },

  getAirportById: async (id: number): Promise<Airport> => {
    const response = await apiClient.get(`/Airports/${id}`)
    return response.data.data
  },

  createAirport: async (data: CreateAirportRequest): Promise<Airport> => {
    const response = await apiClient.post('/Airports', data)
    return response.data.data
  },

  updateAirport: async (id: number, data: UpdateAirportRequest): Promise<Airport> => {
    const response = await apiClient.put(`/Airports/${id}`, data)
    return response.data.data
  },

  deleteAirport: async (id: number): Promise<void> => {
    await apiClient.delete(`/Airports/${id}`)
  },
}


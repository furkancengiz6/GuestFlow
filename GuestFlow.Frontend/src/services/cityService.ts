import apiClient from './api'

export interface City {
  id: number
  cityName: string
  createdDate: string
}

export interface PagedCities {
  data: City[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

export interface CreateCityRequest {
  cityName: string
}

export interface UpdateCityRequest {
  cityName: string
}

export interface CityFilters {
  searchTerm?: string
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

export const cityService = {
  getCities: async (
    pageNumber: number = 1,
    pageSize: number = 10,
    filters?: CityFilters
  ): Promise<PagedCities> => {
    const params: any = { pageNumber, pageSize }
    
    if (filters) {
      if (filters.searchTerm) params.searchTerm = filters.searchTerm
      if (filters.sortBy) params.sortBy = filters.sortBy
      if (filters.sortOrder) params.sortOrder = filters.sortOrder
    }
    
    const response = await apiClient.get('/Cities', { params })
    return response.data.data
  },

  getCityById: async (id: number): Promise<City> => {
    const response = await apiClient.get(`/Cities/${id}`)
    return response.data.data
  },

  createCity: async (data: CreateCityRequest): Promise<City> => {
    const response = await apiClient.post('/Cities', data)
    return response.data.data
  },

  updateCity: async (id: number, data: UpdateCityRequest): Promise<City> => {
    const response = await apiClient.put(`/Cities/${id}`, data)
    return response.data.data
  },

  deleteCity: async (id: number): Promise<void> => {
    await apiClient.delete(`/Cities/${id}`)
  },
}


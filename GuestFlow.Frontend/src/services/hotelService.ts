import apiClient from './api'
import { Hotel, PagedHotels, CreateHotelRequest, UpdateHotelRequest, HotelFilters } from '../types/hotel'

// Re-export types for convenience
export type { Hotel, PagedHotels, CreateHotelRequest, UpdateHotelRequest, HotelFilters }

export const hotelService = {
  getHotels: async (
    pageNumber: number = 1,
    pageSize: number = 10,
    filters?: HotelFilters
  ): Promise<PagedHotels> => {
    const params: any = { pageNumber, pageSize }
    
    if (filters) {
      if (filters.searchTerm) params.searchTerm = filters.searchTerm
      if (filters.cityId) params.cityId = filters.cityId
      if (filters.starRating) params.starRating = filters.starRating
      if (filters.sortBy) params.sortBy = filters.sortBy
      if (filters.sortOrder) params.sortOrder = filters.sortOrder
    }
    
    const response = await apiClient.get('/Hotels', { params })
    return response.data.data
  },

  getHotelById: async (id: number): Promise<Hotel> => {
    const response = await apiClient.get(`/Hotels/${id}`)
    return response.data.data
  },

  createHotel: async (data: CreateHotelRequest): Promise<Hotel> => {
    const response = await apiClient.post('/Hotels', data)
    return response.data.data
  },

  updateHotel: async (id: number, data: UpdateHotelRequest): Promise<Hotel> => {
    const response = await apiClient.put(`/Hotels/${id}`, data)
    return response.data.data
  },

  deleteHotel: async (id: number): Promise<void> => {
    await apiClient.delete(`/Hotels/${id}`)
  },
}


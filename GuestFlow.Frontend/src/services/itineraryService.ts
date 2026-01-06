import apiClient from './api'
import {
  Itinerary,
  PagedItineraries,
  ItineraryTimeline,
  CreateItineraryRequest,
  UpdateItineraryRequest,
  ItineraryFilters,
} from '../types/itinerary'

// Re-export types for convenience
export type { Itinerary, PagedItineraries, ItineraryTimeline, CreateItineraryRequest, UpdateItineraryRequest, ItineraryFilters }

export const itineraryService = {
  getItineraries: async (
    pageNumber: number = 1,
    pageSize: number = 10,
    filters?: ItineraryFilters
  ): Promise<PagedItineraries> => {
    const params: any = { pageNumber, pageSize }
    
    if (filters) {
      if (filters.guestId) params.guestId = filters.guestId
      if (filters.personnelId) params.personnelId = filters.personnelId
      if (filters.status) params.status = filters.status
      if (filters.startDate) params.startDate = filters.startDate
      if (filters.endDate) params.endDate = filters.endDate
      if (filters.sortBy) params.sortBy = filters.sortBy
      if (filters.sortOrder) params.sortOrder = filters.sortOrder
    }
    
    const response = await apiClient.get('/Itineraries', { params })
    return response.data.data
  },

  getItineraryById: async (id: number): Promise<Itinerary> => {
    const response = await apiClient.get(`/Itineraries/${id}`)
    return response.data.data
  },

  getItinerariesByGuest: async (guestId: number): Promise<Itinerary[]> => {
    const response = await apiClient.get(`/Itineraries/guest/${guestId}`)
    return response.data.data
  },

  getItineraryTimeline: async (id: number): Promise<ItineraryTimeline> => {
    const response = await apiClient.get(`/Itineraries/${id}/timeline`)
    return response.data.data
  },

  createItinerary: async (data: CreateItineraryRequest): Promise<Itinerary> => {
    const response = await apiClient.post('/Itineraries', data)
    return response.data.data
  },

  updateItinerary: async (id: number, data: UpdateItineraryRequest): Promise<Itinerary> => {
    const response = await apiClient.put(`/Itineraries/${id}`, data)
    return response.data.data
  },

  deleteItinerary: async (id: number): Promise<void> => {
    await apiClient.delete(`/Itineraries/${id}`)
  },

  updateItineraryStatus: async (id: number, status: string): Promise<void> => {
    await apiClient.patch(`/Itineraries/${id}/status`, { status })
  },
}


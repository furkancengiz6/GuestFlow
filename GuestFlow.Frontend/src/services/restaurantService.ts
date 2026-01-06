import apiClient from './api'
import { Restaurant, PagedRestaurants, CreateRestaurantRequest, UpdateRestaurantRequest, RestaurantFilters } from '../types/restaurant'

// Re-export types for convenience
export type { Restaurant, PagedRestaurants, CreateRestaurantRequest, UpdateRestaurantRequest, RestaurantFilters }

export const restaurantService = {
  getRestaurants: async (
    pageNumber: number = 1,
    pageSize: number = 10,
    filters?: RestaurantFilters
  ): Promise<PagedRestaurants> => {
    const params: any = { pageNumber, pageSize }
    
    if (filters) {
      if (filters.searchTerm) params.searchTerm = filters.searchTerm
      if (filters.cityId) params.cityId = filters.cityId
      if (filters.cuisineType) params.cuisineType = filters.cuisineType
      if (filters.reservationRequired !== undefined) params.reservationRequired = filters.reservationRequired
      if (filters.sortBy) params.sortBy = filters.sortBy
      if (filters.sortOrder) params.sortOrder = filters.sortOrder
    }
    
    const response = await apiClient.get('/Restaurants', { params })
    return response.data.data
  },

  getRestaurantById: async (id: number): Promise<Restaurant> => {
    const response = await apiClient.get(`/Restaurants/${id}`)
    return response.data.data
  },

  createRestaurant: async (data: CreateRestaurantRequest): Promise<Restaurant> => {
    const response = await apiClient.post('/Restaurants', data)
    return response.data.data
  },

  updateRestaurant: async (id: number, data: UpdateRestaurantRequest): Promise<Restaurant> => {
    const response = await apiClient.put(`/Restaurants/${id}`, data)
    return response.data.data
  },

  deleteRestaurant: async (id: number): Promise<void> => {
    await apiClient.delete(`/Restaurants/${id}`)
  },
}


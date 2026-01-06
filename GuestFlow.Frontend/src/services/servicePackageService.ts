import apiClient from './api'
import type { ServicePackage, PagedServicePackages, CreateServicePackageRequest, UpdateServicePackageRequest, ServicePackageFilters } from '../types/servicePackage'

// Re-export types for convenience
export type { ServicePackage, PagedServicePackages, CreateServicePackageRequest, UpdateServicePackageRequest, ServicePackageFilters }

export const servicePackageService = {
  getServicePackages: async (
    pageNumber: number = 1,
    pageSize: number = 10,
    filters?: ServicePackageFilters
  ): Promise<PagedServicePackages> => {
    const params: any = { pageNumber, pageSize }
    
    if (filters) {
      if (filters.packageType !== undefined) params.packageType = filters.packageType
      if (filters.isActive !== undefined) params.isActive = filters.isActive
      if (filters.startDate) params.startDate = filters.startDate
      if (filters.endDate) params.endDate = filters.endDate
      if (filters.searchTerm) params.searchTerm = filters.searchTerm
      if (filters.sortBy) params.sortBy = filters.sortBy
      if (filters.sortOrder) params.sortOrder = filters.sortOrder
    }
    
    const response = await apiClient.get('/ServicePackages', { params })
    return response.data.data
  },

  getServicePackageById: async (id: number): Promise<ServicePackage> => {
    const response = await apiClient.get(`/ServicePackages/${id}`)
    return response.data.data
  },

  createServicePackage: async (data: CreateServicePackageRequest): Promise<ServicePackage> => {
    const response = await apiClient.post('/ServicePackages', data)
    return response.data.data
  },

  updateServicePackage: async (id: number, data: UpdateServicePackageRequest): Promise<ServicePackage> => {
    const response = await apiClient.put(`/ServicePackages/${id}`, data)
    return response.data.data
  },

  deleteServicePackage: async (id: number): Promise<void> => {
    await apiClient.delete(`/ServicePackages/${id}`)
  },

  addTransferToPackage: async (packageId: number, transferId: number): Promise<void> => {
    await apiClient.post(`/ServicePackages/${packageId}/transfers/${transferId}`)
  },

  removeTransferFromPackage: async (packageId: number, transferId: number): Promise<void> => {
    await apiClient.delete(`/ServicePackages/${packageId}/transfers/${transferId}`)
  },

  addCityTourToPackage: async (packageId: number, cityTourId: number): Promise<void> => {
    await apiClient.post(`/ServicePackages/${packageId}/city-tours/${cityTourId}`)
  },

  removeCityTourFromPackage: async (packageId: number, cityTourId: number): Promise<void> => {
    await apiClient.delete(`/ServicePackages/${packageId}/city-tours/${cityTourId}`)
  },

  addYachtTourToPackage: async (packageId: number, yachtTourId: number): Promise<void> => {
    await apiClient.post(`/ServicePackages/${packageId}/yacht-tours/${yachtTourId}`)
  },

  removeYachtTourFromPackage: async (packageId: number, yachtTourId: number): Promise<void> => {
    await apiClient.delete(`/ServicePackages/${packageId}/yacht-tours/${yachtTourId}`)
  },

  addRestaurantReservationToPackage: async (packageId: number, reservationId: number): Promise<void> => {
    await apiClient.post(`/ServicePackages/${packageId}/restaurant-reservations/${reservationId}`)
  },

  removeRestaurantReservationFromPackage: async (packageId: number, reservationId: number): Promise<void> => {
    await apiClient.delete(`/ServicePackages/${packageId}/restaurant-reservations/${reservationId}`)
  },

  calculatePackageTotalCost: async (packageId: number): Promise<number> => {
    const response = await apiClient.get(`/ServicePackages/${packageId}/calculate-total`)
    return response.data.data
  },
}


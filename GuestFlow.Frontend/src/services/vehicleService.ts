import apiClient from './api'

export interface Vehicle {
  id: number
  plateNumber: string
  vehicleType: string
  capacity: number
  createdDate: string
}

export interface PagedVehicles {
  data: Vehicle[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

export interface CreateVehicleRequest {
  plateNumber: string
  vehicleType: string
  capacity: number
}

export interface UpdateVehicleRequest {
  plateNumber: string
  vehicleType: string
  capacity: number
}

export interface VehicleFilters {
  searchTerm?: string
  vehicleType?: string
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

export const vehicleService = {
  getVehicles: async (
    pageNumber: number = 1,
    pageSize: number = 10,
    filters?: VehicleFilters
  ): Promise<PagedVehicles> => {
    const params: any = { pageNumber, pageSize }
    
    if (filters) {
      if (filters.searchTerm) params.searchTerm = filters.searchTerm
      if (filters.vehicleType) params.vehicleType = filters.vehicleType
      if (filters.sortBy) params.sortBy = filters.sortBy
      if (filters.sortOrder) params.sortOrder = filters.sortOrder
    }
    
    const response = await apiClient.get('/Vehicles', { params })
    return response.data.data
  },

  getVehicleById: async (id: number): Promise<Vehicle> => {
    const response = await apiClient.get(`/Vehicles/${id}`)
    return response.data.data
  },

  createVehicle: async (data: CreateVehicleRequest): Promise<Vehicle> => {
    const response = await apiClient.post('/Vehicles', data)
    return response.data.data
  },

  updateVehicle: async (id: number, data: UpdateVehicleRequest): Promise<Vehicle> => {
    const response = await apiClient.put(`/Vehicles/${id}`, data)
    return response.data.data
  },

  deleteVehicle: async (id: number): Promise<void> => {
    await apiClient.delete(`/Vehicles/${id}`)
  },
}


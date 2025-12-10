import apiClient from './api'
import { CityTour, YachtTour, PagedCityTours, PagedYachtTours, CityTourDetail, YachtTourDetail } from '../types/tour'

export interface CreateCityTourRequest {
  tourDate: string
  language: string
  durationHours: number
  price: number
  ownerGuestId: number
  personnelId: number
  cityId: number
  createInvoice?: boolean
  discountPercentage?: number
  invoiceDescription?: string
  currency?: string
}

export interface UpdateCityTourRequest {
  tourDate: string
  language: string
  durationHours: number
  price: number
  ownerGuestId: number
  personnelId: number
  cityId: number
}

export interface CreateYachtTourRequest {
  tourDate: string
  numberOfPeople: number
  price: number
  specialRequest?: string
  yachtName: string
  ownerGuestId: number
  personnelId: number
  cityId: number
  createInvoice?: boolean
  discountPercentage?: number
  invoiceDescription?: string
  currency?: string
}

export interface UpdateYachtTourRequest {
  tourDate: string
  numberOfPeople: number
  price: number
  specialRequest?: string
  yachtName: string
  ownerGuestId: number
  personnelId: number
  cityId: number
}

export interface CityTourFilters {
  startDate?: string
  endDate?: string
  cityId?: number
  guestId?: number
  personnelId?: number
  searchTerm?: string
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

export interface YachtTourFilters {
  startDate?: string
  endDate?: string
  cityId?: number
  guestId?: number
  personnelId?: number
  searchTerm?: string
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

export const tourService = {
  getCityTours: async (
    pageNumber: number = 1,
    pageSize: number = 10,
    filters?: CityTourFilters
  ): Promise<PagedCityTours> => {
    const params: any = { pageNumber, pageSize }
    
    if (filters) {
      if (filters.startDate) params.startDate = filters.startDate
      if (filters.endDate) params.endDate = filters.endDate
      if (filters.cityId) params.cityId = filters.cityId
      if (filters.guestId) params.guestId = filters.guestId
      if (filters.personnelId) params.personnelId = filters.personnelId
      if (filters.searchTerm) params.searchTerm = filters.searchTerm
      if (filters.sortBy) params.sortBy = filters.sortBy
      if (filters.sortOrder) params.sortOrder = filters.sortOrder
    }
    
    const response = await apiClient.get('/CityTours', { params })
    return response.data.data
  },

  getCityTourById: async (id: number): Promise<CityTour> => {
    const response = await apiClient.get(`/CityTours/${id}`)
    return response.data.data
  },

  getYachtTours: async (
    pageNumber: number = 1,
    pageSize: number = 10,
    filters?: YachtTourFilters
  ): Promise<PagedYachtTours> => {
    const params: any = { pageNumber, pageSize }
    
    if (filters) {
      if (filters.startDate) params.startDate = filters.startDate
      if (filters.endDate) params.endDate = filters.endDate
      if (filters.cityId) params.cityId = filters.cityId
      if (filters.guestId) params.guestId = filters.guestId
      if (filters.personnelId) params.personnelId = filters.personnelId
      if (filters.searchTerm) params.searchTerm = filters.searchTerm
      if (filters.sortBy) params.sortBy = filters.sortBy
      if (filters.sortOrder) params.sortOrder = filters.sortOrder
    }
    
    const response = await apiClient.get('/YachtTours', { params })
    return response.data.data
  },

  getYachtTourById: async (id: number): Promise<YachtTour> => {
    const response = await apiClient.get(`/YachtTours/${id}`)
    return response.data.data
  },

  getCityTourDetail: async (id: number): Promise<CityTourDetail> => {
    const response = await apiClient.get(`/CityTours/${id}/detail`)
    return response.data.data
  },

  getYachtTourDetail: async (id: number): Promise<YachtTourDetail> => {
    const response = await apiClient.get(`/YachtTours/${id}/detail`)
    return response.data.data
  },

  createCityTour: async (data: CreateCityTourRequest): Promise<CityTour> => {
    const response = await apiClient.post('/CityTours', data)
    return response.data.data
  },

  updateCityTour: async (id: number, data: UpdateCityTourRequest): Promise<CityTour> => {
    const response = await apiClient.put(`/CityTours/${id}`, data)
    return response.data.data
  },

  deleteCityTour: async (id: number): Promise<void> => {
    await apiClient.delete(`/CityTours/${id}`)
  },

  createYachtTour: async (data: CreateYachtTourRequest): Promise<YachtTour> => {
    const response = await apiClient.post('/YachtTours', data)
    return response.data.data
  },

  updateYachtTour: async (id: number, data: UpdateYachtTourRequest): Promise<YachtTour> => {
    const response = await apiClient.put(`/YachtTours/${id}`, data)
    return response.data.data
  },

  deleteYachtTour: async (id: number): Promise<void> => {
    await apiClient.delete(`/YachtTours/${id}`)
  },
}


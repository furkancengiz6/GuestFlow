import apiClient from './api'
import { Guest, PagedGuests, GuestDetail } from '../types/guest'
import { GuestPreferences, UpsertGuestPreferences } from '../types/guestPreferences'

// Re-export types for convenience
export type { Guest, PagedGuests, GuestDetail }
export type { GuestPreferences, UpsertGuestPreferences }

export interface CreateGuestRequest {
  fullName: string
  email?: string
  phoneNumber?: string
  nationality: string
  isSpecialGuest: boolean
}

export interface UpdateGuestRequest {
  fullName: string
  email?: string
  phoneNumber?: string
  nationality: string
  isSpecialGuest: boolean
}

export interface GuestFilters {
  searchTerm?: string
  nationality?: string
  isSpecialGuest?: boolean
  email?: string
  phoneNumber?: string
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

export const guestService = {
  getGuests: async (
    pageNumber: number = 1,
    pageSize: number = 10,
    filters?: GuestFilters
  ): Promise<PagedGuests> => {
    const params: any = { pageNumber, pageSize }

    if (filters) {
      if (filters.searchTerm) params.searchTerm = filters.searchTerm
      if (filters.nationality) params.nationality = filters.nationality
      if (filters.isSpecialGuest !== undefined) params.isSpecialGuest = filters.isSpecialGuest
      if (filters.email) params.email = filters.email
      if (filters.phoneNumber) params.phoneNumber = filters.phoneNumber
      if (filters.sortBy) params.sortBy = filters.sortBy
      if (filters.sortOrder) params.sortOrder = filters.sortOrder
    }

    const response = await apiClient.get('/Guests', { params })
    // Unwrap API envelope: response.data = { success, message, data: { data: [...], totalCount } }
    return response.data.data
  },

  getGuestById: async (id: number): Promise<Guest> => {
    const response = await apiClient.get(`/Guests/${id}`)
    return response.data.data
  },

  getGuestDetail: async (id: number): Promise<GuestDetail> => {
    const response = await apiClient.get(`/Guests/${id}/detail`)
    return response.data.data
  },

  createGuest: async (data: CreateGuestRequest): Promise<Guest> => {
    const response = await apiClient.post('/Guests', data)
    return response.data.data
  },

  updateGuest: async (id: number, data: UpdateGuestRequest): Promise<Guest> => {
    const response = await apiClient.put(`/Guests/${id}`, data)
    return response.data.data
  },

  deleteGuest: async (id: number): Promise<void> => {
    await apiClient.delete(`/Guests/${id}`)
  },

  // Guest Preferences
  getGuestPreferences: async (id: number): Promise<GuestPreferences> => {
    const response = await apiClient.get(`/Guests/${id}/preferences`)
    return response.data.data || response.data
  },

  upsertGuestPreferences: async (id: number, data: UpsertGuestPreferences): Promise<GuestPreferences> => {
    const response = await apiClient.put(`/Guests/${id}/preferences`, data)
    return response.data.data || response.data
  },

  deleteGuestPreferences: async (id: number): Promise<void> => {
    await apiClient.delete(`/Guests/${id}/preferences`)
  },
}


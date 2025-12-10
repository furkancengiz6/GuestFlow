import apiClient from './api'
import { Transfer, PagedTransfers, TransferDetail } from '../types/transfer'

export interface CreateTransferRequest {
  transferDate: string
  pickupAddress: string
  dropoffAddress: string
  price: number
  guestId: number
  personnelId: number
  airportId: number
  vehicleId: number
  note?: string
  status: string
  isFromAirport: boolean
  pickupCityId: number
  dropoffCityId: number
  createInvoice?: boolean
  discountPercentage?: number
  invoiceDescription?: string
  currency?: string
}

export interface UpdateTransferRequest {
  transferDate: string
  pickupAddress: string
  dropoffAddress: string
  price: number
  guestId: number
  personnelId: number
  airportId: number
  vehicleId: number
  note?: string
  status: string
  isFromAirport: boolean
  pickupCityId: number
  dropoffCityId: number
}

export interface TransferFilters {
  startDate?: string
  endDate?: string
  status?: string
  guestId?: number
  personnelId?: number
  vehicleId?: number
  airportId?: number
  isFromAirport?: boolean
  searchTerm?: string
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

export const transferService = {
  getTransfers: async (
    pageNumber: number = 1,
    pageSize: number = 10,
    filters?: TransferFilters
  ): Promise<PagedTransfers> => {
    const params: any = { pageNumber, pageSize }
    
    if (filters) {
      if (filters.startDate) params.startDate = filters.startDate
      if (filters.endDate) params.endDate = filters.endDate
      if (filters.status) params.status = filters.status
      if (filters.guestId) params.guestId = filters.guestId
      if (filters.personnelId) params.personnelId = filters.personnelId
      if (filters.vehicleId) params.vehicleId = filters.vehicleId
      if (filters.airportId) params.airportId = filters.airportId
      if (filters.isFromAirport !== undefined) params.isFromAirport = filters.isFromAirport
      if (filters.searchTerm) params.searchTerm = filters.searchTerm
      if (filters.sortBy) params.sortBy = filters.sortBy
      if (filters.sortOrder) params.sortOrder = filters.sortOrder
    }
    
    const response = await apiClient.get('/Transfers', { params })
    return response.data.data
  },

  getTransferById: async (id: number): Promise<Transfer> => {
    const response = await apiClient.get(`/Transfers/${id}`)
    return response.data.data
  },

  createTransfer: async (data: CreateTransferRequest): Promise<Transfer> => {
    const response = await apiClient.post('/Transfers', data)
    return response.data.data
  },

  updateTransfer: async (id: number, data: UpdateTransferRequest): Promise<Transfer> => {
    const response = await apiClient.put(`/Transfers/${id}`, data)
    return response.data.data
  },

  deleteTransfer: async (id: number): Promise<void> => {
    await apiClient.delete(`/Transfers/${id}`)
  },

  getTransferDetail: async (id: number): Promise<TransferDetail> => {
    const response = await apiClient.get(`/Transfers/${id}/detail`)
    return response.data.data
  },
}


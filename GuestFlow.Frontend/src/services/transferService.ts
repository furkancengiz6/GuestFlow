import apiClient from './api'
import type { Transfer, PagedTransfers, TransferDetail } from '../types/transfer'
import { TransferType } from '../types/enums'

// Re-export types for convenience
export type { Transfer, PagedTransfers, TransferDetail }

export interface AddTransferResponse {
  transferId: number
  invoiceId?: number
  invoicePdfUrl?: string
}

export interface CreateTransferRequest {
  transferDate: string
  pickupTime?: string
  serviceStartTime?: string
  pickupConfirmationTime?: string
  dropoffConfirmationTime?: string
  pickupAddress: string
  dropoffAddress: string
  price: number
  guestId: number
  personnelId?: number
  driverId?: number
  airportId?: number
  vehicleId?: number
  note?: string
  status?: string
  transferType?: TransferType
  pickupCityId?: number
  dropoffCityId?: number
  createInvoice?: boolean
  discountPercentage?: number
  invoiceDescription?: string
  currency?: string
  externalVehiclePlate?: string
  externalDriverName?: string
  externalDriverPhone?: string

  // Guest coordination fields
  contactPersonName?: string
  meetingPointDetails?: string

  // Group management fields
  groupSize?: number
  childCount?: number
  infantCount?: number

  // Communication fields
  guestLanguage?: string
  emergencyContactPhone?: string

  // Service quality fields
  accessibilityRequirements?: string
  specialHandlingNotes?: string

  // Internal coordination fields
  conciergeInternalNotes?: string
  guestVisibleNotes?: string

  // Supplier contact fields
  supplierName?: string
  supplierCost?: number
  supplierCurrency?: string
  supplierPaymentStatus?: string
  supplierPaymentDate?: string
  supplierInvoiceNumber?: string
  supplierContactPhone?: string
  supplierEmergencyContact?: string
}

export interface UpdateTransferRequest {
  transferDate: string
  pickupTime?: string
  serviceStartTime?: string
  pickupConfirmationTime?: string
  dropoffConfirmationTime?: string
  pickupAddress: string
  dropoffAddress: string
  price: number
  guestId: number
  personnelId?: number
  driverId?: number
  airportId?: number
  vehicleId?: number
  note?: string
  status?: string
  transferType?: TransferType
  pickupCityId?: number
  dropoffCityId?: number

  // Guest coordination fields
  contactPersonName?: string
  meetingPointDetails?: string

  // Group management fields
  groupSize?: number
  childCount?: number
  infantCount?: number

  // Communication fields
  guestLanguage?: string
  emergencyContactPhone?: string

  // Service quality fields
  accessibilityRequirements?: string
  specialHandlingNotes?: string

  // Internal coordination fields
  conciergeInternalNotes?: string
  guestVisibleNotes?: string

  discountPercentage?: number
  currency?: string
  externalVehiclePlate?: string
  externalDriverName?: string
  externalDriverPhone?: string
  supplierName?: string
  supplierCost?: number
  supplierCurrency?: string
  supplierPaymentStatus?: string
  supplierPaymentDate?: string
  supplierInvoiceNumber?: string
  supplierContactPhone?: string
  supplierEmergencyContact?: string
}

export interface TransferFilters {
  startDate?: string
  endDate?: string
  status?: string
  guestId?: number
  personnelId?: number
  driverId?: number
  vehicleId?: number
  airportId?: number
  transferType?: string
  searchTerm?: string
  priority?: string
  transportMode?: string
  isVip?: boolean
  groupSizeMin?: number
  groupSizeMax?: number
  priceMin?: number
  priceMax?: number
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
  createdBy?: number
  assignedPersonnel?: number
}

// Bulk operations interface
export interface BulkTransferOperation {
  operation: 'status_change' | 'assign_driver' | 'assign_vehicle' | 'cancel' | 'delete'
  transferIds: number[]
  newStatus?: string
  driverId?: number
  vehicleId?: number
  reason?: string
}

export interface BulkOperationResult {
  successCount: number
  failureCount: number
  errors: { transferId: number; error: string }[]
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
      if (filters.searchTerm) params.searchTerm = filters.searchTerm
      if (filters.sortBy) params.sortBy = filters.sortBy
      if (filters.sortOrder) params.sortOrder = filters.sortOrder
    }
    
    const response = await apiClient.get('/Transfers', { params })
    // Return the paged response object (contains { data: [...], totalCount })
    return response.data
  },

  getTransferById: async (id: number): Promise<Transfer> => {
    const response = await apiClient.get(`/Transfers/${id}`)
    return response.data.data
  },

  createTransfer: async (data: CreateTransferRequest): Promise<AddTransferResponse> => {
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

  // Action methods for TransferDetailPage buttons
  markTransferCompleted: async (id: number): Promise<void> => {
    await apiClient.patch(`/Transfers/${id}/status`, { status: 'Completed' })
  },

  cancelTransfer: async (id: number): Promise<void> => {
    await apiClient.patch(`/Transfers/${id}/status`, { status: 'Cancelled' })
  },

  createTransferInvoice: async (id: number): Promise<void> => {
    await apiClient.post(`/Transfers/${id}/invoice`)
  },

  sendTransferConfirmation: async (id: number): Promise<void> => {
    await apiClient.post(`/Transfers/${id}/send-confirmation`)
  },

  createRoundTrip: async (id: number): Promise<void> => {
    await apiClient.post(`/Transfers/${id}/round-trip`)
  },

  // Bulk operations
  bulkUpdateTransfers: async (operation: BulkTransferOperation): Promise<BulkOperationResult> => {
    const response = await apiClient.post('/Transfers/bulk-update', operation)
    return response.data.data
  },

  bulkDeleteTransfers: async (transferIds: number[], reason?: string): Promise<BulkOperationResult> => {
    const response = await apiClient.post('/Transfers/bulk-delete', { transferIds, reason })
    return response.data.data
  },
}


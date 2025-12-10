import apiClient from './api'
import { Invoice, PagedInvoices, InvoiceDetail } from '../types/invoice'

export interface InvoiceFilters {
  startDate?: string
  endDate?: string
  guestId?: number
  personnelId?: number
  currency?: string
  hasPdf?: boolean
  serviceType?: string
  serviceId?: number
  searchTerm?: string
  minAmount?: number
  maxAmount?: number
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

export const invoiceService = {
  getInvoices: async (
    pageNumber: number = 1,
    pageSize: number = 10,
    filters?: InvoiceFilters
  ): Promise<PagedInvoices> => {
    const params: any = { pageNumber, pageSize }
    
    if (filters) {
      if (filters.startDate) params.startDate = filters.startDate
      if (filters.endDate) params.endDate = filters.endDate
      if (filters.guestId) params.guestId = filters.guestId
      if (filters.personnelId) params.personnelId = filters.personnelId
      if (filters.currency) params.currency = filters.currency
      if (filters.hasPdf !== undefined) params.hasPdf = filters.hasPdf
      if (filters.serviceType) params.serviceType = filters.serviceType
      if (filters.serviceId) params.serviceId = filters.serviceId
      if (filters.searchTerm) params.searchTerm = filters.searchTerm
      if (filters.minAmount) params.minAmount = filters.minAmount
      if (filters.maxAmount) params.maxAmount = filters.maxAmount
      if (filters.sortBy) params.sortBy = filters.sortBy
      if (filters.sortOrder) params.sortOrder = filters.sortOrder
    }
    
    const response = await apiClient.get('/Invoices', { params })
    return response.data.data
  },

  getInvoiceById: async (id: number): Promise<Invoice> => {
    const response = await apiClient.get(`/Invoices/${id}`)
    return response.data.data
  },

  getInvoiceDetail: async (id: number): Promise<InvoiceDetail> => {
    const response = await apiClient.get(`/Invoices/${id}/detail`)
    return response.data.data
  },
}


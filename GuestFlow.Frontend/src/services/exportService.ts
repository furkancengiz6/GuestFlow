import api from './api'

export interface GuestExportFilters {
  searchTerm?: string
  nationality?: string
  isSpecialGuest?: boolean
  email?: string
  phoneNumber?: string
}

export interface TransferExportFilters {
  startDate?: string
  endDate?: string
  status?: string
  guestId?: number
  personnelId?: number
  vehicleId?: number
  airportId?: number
  isFromAirport?: boolean
  searchTerm?: string
}

export interface InvoiceExportFilters {
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
}

export interface RevenueExportFilters {
  startDate?: string
  endDate?: string
}

export interface JournalExportFilters {
  startDate?: string
  endDate?: string
}

const downloadFile = (blob: Blob, filename: string) => {
  const url = window.URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = filename
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  window.URL.revokeObjectURL(url)
}

export const exportService = {
  // Guests
  exportGuestsToExcel: async (filters?: GuestExportFilters) => {
    const params = new URLSearchParams()
    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          params.append(key, value.toString())
        }
      })
    }
    const response = await api.get(`/export/guests/excel?${params.toString()}`, {
      responseType: 'blob',
    })
    const filename = `misafirler_${new Date().toISOString().split('T')[0]}.xlsx`
    downloadFile(response.data, filename)
  },

  exportGuestsToCsv: async (filters?: GuestExportFilters) => {
    const params = new URLSearchParams()
    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          params.append(key, value.toString())
        }
      })
    }
    const response = await api.get(`/export/guests/csv?${params.toString()}`, {
      responseType: 'blob',
    })
    const filename = `misafirler_${new Date().toISOString().split('T')[0]}.csv`
    downloadFile(response.data, filename)
  },

  // Transfers
  exportTransfersToExcel: async (filters?: TransferExportFilters) => {
    const params = new URLSearchParams()
    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          params.append(key, value.toString())
        }
      })
    }
    const response = await api.get(`/export/transfers/excel?${params.toString()}`, {
      responseType: 'blob',
    })
    const filename = `transferler_${new Date().toISOString().split('T')[0]}.xlsx`
    downloadFile(response.data, filename)
  },

  exportTransfersToCsv: async (filters?: TransferExportFilters) => {
    const params = new URLSearchParams()
    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          params.append(key, value.toString())
        }
      })
    }
    const response = await api.get(`/export/transfers/csv?${params.toString()}`, {
      responseType: 'blob',
    })
    const filename = `transferler_${new Date().toISOString().split('T')[0]}.csv`
    downloadFile(response.data, filename)
  },

  // Invoices
  exportInvoicesToExcel: async (filters?: InvoiceExportFilters) => {
    const params = new URLSearchParams()
    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          params.append(key, value.toString())
        }
      })
    }
    const response = await api.get(`/export/invoices/excel?${params.toString()}`, {
      responseType: 'blob',
    })
    const filename = `faturalar_${new Date().toISOString().split('T')[0]}.xlsx`
    downloadFile(response.data, filename)
  },

  exportInvoicesToCsv: async (filters?: InvoiceExportFilters) => {
    const params = new URLSearchParams()
    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          params.append(key, value.toString())
        }
      })
    }
    const response = await api.get(`/export/invoices/csv?${params.toString()}`, {
      responseType: 'blob',
    })
    const filename = `faturalar_${new Date().toISOString().split('T')[0]}.csv`
    downloadFile(response.data, filename)
  },

  // Revenue
  exportRevenueToExcel: async (filters?: RevenueExportFilters) => {
    const params = new URLSearchParams()
    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          params.append(key, value.toString())
        }
      })
    }
    const response = await api.get(`/export/revenue/excel?${params.toString()}`, {
      responseType: 'blob',
    })
    const filename = `gelir_raporu_${new Date().toISOString().split('T')[0]}.xlsx`
    downloadFile(response.data, filename)
  },

  exportRevenueToCsv: async (filters?: RevenueExportFilters) => {
    const params = new URLSearchParams()
    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          params.append(key, value.toString())
        }
      })
    }
    const response = await api.get(`/export/revenue/csv?${params.toString()}`, {
      responseType: 'blob',
    })
    const filename = `gelir_raporu_${new Date().toISOString().split('T')[0]}.csv`
    downloadFile(response.data, filename)
  },

  // Journal
  exportJournalToCsv: async (filters?: JournalExportFilters) => {
    const params = new URLSearchParams()
    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          params.append(key, value.toString())
        }
      })
    }

    const response = await api.get(`/export/journal/csv?${params.toString()}`, {
      responseType: 'blob',
    })
    const filename = `journal_${new Date().toISOString().split('T')[0]}.csv`
    downloadFile(response.data, filename)
  },

  exportJournalToExcel: async (filters?: JournalExportFilters) => {
    const params = new URLSearchParams()
    if (filters) {
      Object.entries(filters).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          params.append(key, value.toString())
        }
      })
    }

    const response = await api.get(`/export/journal/excel?${params.toString()}`, {
      responseType: 'blob',
    })
    const filename = `journal_${new Date().toISOString().split('T')[0]}.xlsx`
    downloadFile(response.data, filename)
  },

  // Guest Ledger
  exportGuestLedgerToCsv: async (params?: { guestId?: number; startDate?: string; endDate?: string }) => {
    const queryParams = new URLSearchParams()
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          queryParams.append(key, value.toString())
        }
      })
    }
    const response = await api.get(`/export/guest-ledger/csv?${queryParams.toString()}`, {
      responseType: 'blob',
    })
    const filename = `guest_ledger_${params?.guestId ?? 'all'}_${new Date().toISOString().split('T')[0]}.csv`
    downloadFile(response.data, filename)
  },

  exportGuestLedgerToExcel: async (params?: { guestId?: number; startDate?: string; endDate?: string }) => {
    const queryParams = new URLSearchParams()
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          queryParams.append(key, value.toString())
        }
      })
    }
    const response = await api.get(`/export/guest-ledger/excel?${queryParams.toString()}`, {
      responseType: 'blob',
    })
    const filename = `guest_ledger_${params?.guestId ?? 'all'}_${new Date().toISOString().split('T')[0]}.xlsx`
    downloadFile(response.data, filename)
  },

  // Supplier Ledger
  exportSupplierLedgerToCsv: async (params?: { supplierId?: number; startDate?: string; endDate?: string }) => {
    const queryParams = new URLSearchParams()
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          queryParams.append(key, value.toString())
        }
      })
    }
    const response = await api.get(`/export/supplier-ledger/csv?${queryParams.toString()}`, {
      responseType: 'blob',
    })
    const filename = `supplier_ledger_${params?.supplierId ?? 'all'}_${new Date().toISOString().split('T')[0]}.csv`
    downloadFile(response.data, filename)
  },

  exportSupplierLedgerToExcel: async (params?: { supplierId?: number; startDate?: string; endDate?: string }) => {
    const queryParams = new URLSearchParams()
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          queryParams.append(key, value.toString())
        }
      })
    }
    const response = await api.get(`/export/supplier-ledger/excel?${queryParams.toString()}`, {
      responseType: 'blob',
    })
    const filename = `supplier_ledger_${params?.supplierId ?? 'all'}_${new Date().toISOString().split('T')[0]}.xlsx`
    downloadFile(response.data, filename)
  },

  // Room Ledger
  exportRoomLedgerToCsv: async (params?: { roomNumber?: string; startDate?: string; endDate?: string }) => {
    const queryParams = new URLSearchParams()
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          queryParams.append(key, value.toString())
        }
      })
    }
    const response = await api.get(`/export/room-ledger/csv?${queryParams.toString()}`, {
      responseType: 'blob',
    })
    const filename = `room_ledger_${params?.roomNumber ?? 'all'}_${new Date().toISOString().split('T')[0]}.csv`
    downloadFile(response.data, filename)
  },

  exportRoomLedgerToExcel: async (params?: { roomNumber?: string; startDate?: string; endDate?: string }) => {
    const queryParams = new URLSearchParams()
    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          queryParams.append(key, value.toString())
        }
      })
    }
    const response = await api.get(`/export/room-ledger/excel?${queryParams.toString()}`, {
      responseType: 'blob',
    })
    const filename = `room_ledger_${params?.roomNumber ?? 'all'}_${new Date().toISOString().split('T')[0]}.xlsx`
    downloadFile(response.data, filename)
  },
}


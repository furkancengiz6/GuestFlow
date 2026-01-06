import apiClient from './api'

export interface FileInfo {
  fileName: string
  fileSize: number
  contentType: string
  category: string
  uploadedDate: string
  uploadedBy?: number
  metadata?: Record<string, any>
}

export interface PagedFiles {
  data: FileInfo[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

export interface FileStatistics {
  totalFiles: number
  totalSize: number
  byCategory: Record<string, number>
  byType: Record<string, number>
}

export interface FileFilters {
  category?: string
  contentType?: string
  startDate?: string
  endDate?: string
  searchTerm?: string
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

export interface ShareLink {
  shareToken: string
  fileName: string
  expiresAt: string
  isActive: boolean
}

export interface CreateShareLinkRequest {
  expiresInDays?: number
}

export const fileService = {
  getFiles: async (
    pageNumber: number = 1,
    pageSize: number = 10,
    filters?: FileFilters
  ): Promise<PagedFiles> => {
    const params: any = { pageNumber, pageSize }
    
    if (filters) {
      if (filters.category) params.category = filters.category
      if (filters.contentType) params.contentType = filters.contentType
      if (filters.startDate) params.startDate = filters.startDate
      if (filters.endDate) params.endDate = filters.endDate
      if (filters.searchTerm) params.searchTerm = filters.searchTerm
      if (filters.sortBy) params.sortBy = filters.sortBy
      if (filters.sortOrder) params.sortOrder = filters.sortOrder
    }
    
    const response = await apiClient.get('/Files', { params })
    return response.data.data
  },

  getFileInfo: async (fileName: string): Promise<FileInfo> => {
    const response = await apiClient.get(`/Files/${fileName}`)
    return response.data.data
  },

  downloadFile: async (fileName: string): Promise<Blob> => {
    const response = await apiClient.get(`/Files/download/${fileName}`, {
      responseType: 'blob',
    })
    return response.data
  },

  uploadFile: async (file: File, category?: string): Promise<FileInfo> => {
    const formData = new FormData()
    formData.append('file', file)
    if (category) formData.append('category', category)
    
    const response = await apiClient.post('/Files/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    })
    return response.data.data
  },

  uploadBulkFiles: async (files: File[], category?: string): Promise<FileInfo[]> => {
    const formData = new FormData()
    files.forEach((file) => {
      formData.append('files', file)
    })
    if (category) formData.append('category', category)
    
    const response = await apiClient.post('/Files/upload/bulk', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    })
    return response.data.data
  },

  deleteFile: async (fileName: string): Promise<void> => {
    await apiClient.delete(`/Files/${fileName}`)
  },

  getCategories: async (): Promise<string[]> => {
    const response = await apiClient.get('/Files/categories')
    return response.data.data
  },

  getStatistics: async (): Promise<FileStatistics> => {
    const response = await apiClient.get('/Files/statistics')
    return response.data.data
  },

  getGuestFiles: async (guestId: number): Promise<FileInfo[]> => {
    const response = await apiClient.get(`/Files/guests/${guestId}`)
    return response.data.data
  },

  getTourFiles: async (tourId: number): Promise<FileInfo[]> => {
    const response = await apiClient.get(`/Files/tours/${tourId}`)
    return response.data.data
  },

  getInvoiceFiles: async (): Promise<FileInfo[]> => {
    const response = await apiClient.get('/Files/invoices')
    return response.data.data
  },

  createShareLink: async (fileName: string, data?: CreateShareLinkRequest): Promise<ShareLink> => {
    const response = await apiClient.post(`/Files/${fileName}/share`, data || {})
    return response.data.data
  },

  getShareLinks: async (): Promise<ShareLink[]> => {
    const response = await apiClient.get('/Files/share')
    return response.data.data
  },

  deleteShareLink: async (shareToken: string): Promise<void> => {
    await apiClient.delete(`/Files/share/${shareToken}`)
  },
}


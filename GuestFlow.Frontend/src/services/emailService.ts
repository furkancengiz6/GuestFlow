import apiClient from './api'

export interface EmailHistory {
  id: number
  to: string
  subject: string
  body: string
  status: string
  sentDate: string
  openedDate?: string
  clickCount: number
  errorMessage?: string
}

export interface PagedEmailHistory {
  data: EmailHistory[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

export interface EmailTemplate {
  id: number
  name: string
  subject: string
  body: string
  isActive: boolean
}

export interface EmailQueue {
  id: number
  to: string
  subject: string
  status: string
  priority: number
  scheduledDate?: string
}

export interface EmailStatistics {
  totalSent: number
  totalFailed: number
  totalPending: number
  openRate: number
  clickRate: number
}

export interface SendEmailRequest {
  to: string
  subject: string
  body: string
  isHtml?: boolean
}

export interface EmailFilters {
  startDate?: string
  endDate?: string
  status?: string
  to?: string
  searchTerm?: string
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

export const emailService = {
  getEmailHistory: async (
    pageNumber: number = 1,
    pageSize: number = 10,
    filters?: EmailFilters
  ): Promise<PagedEmailHistory> => {
    const params: any = { pageNumber, pageSize }
    
    if (filters) {
      if (filters.startDate) params.startDate = filters.startDate
      if (filters.endDate) params.endDate = filters.endDate
      if (filters.status) params.status = filters.status
      if (filters.to) params.to = filters.to
      if (filters.searchTerm) params.searchTerm = filters.searchTerm
      if (filters.sortBy) params.sortBy = filters.sortBy
      if (filters.sortOrder) params.sortOrder = filters.sortOrder
    }
    
    const response = await apiClient.get('/Emails/history', { params })
    return response.data.data
  },

  getEmailById: async (id: number): Promise<EmailHistory> => {
    const response = await apiClient.get(`/Emails/history/${id}`)
    return response.data.data
  },

  getEmailQueue: async (): Promise<EmailQueue[]> => {
    const response = await apiClient.get('/Emails/queue')
    return response.data.data
  },

  getEmailTemplates: async (): Promise<EmailTemplate[]> => {
    const response = await apiClient.get('/Emails/templates')
    return response.data.data
  },

  getEmailTemplateById: async (id: number): Promise<EmailTemplate> => {
    const response = await apiClient.get(`/Emails/templates/${id}`)
    return response.data.data
  },

  createEmailTemplate: async (data: Omit<EmailTemplate, 'id'>): Promise<EmailTemplate> => {
    const response = await apiClient.post('/Emails/templates', data)
    return response.data.data
  },

  updateEmailTemplate: async (id: number, data: Partial<EmailTemplate>): Promise<EmailTemplate> => {
    const response = await apiClient.put(`/Emails/templates/${id}`, data)
    return response.data.data
  },

  deleteEmailTemplate: async (id: number): Promise<void> => {
    await apiClient.delete(`/Emails/templates/${id}`)
  },

  getStatistics: async (): Promise<EmailStatistics> => {
    const response = await apiClient.get('/Emails/statistics')
    return response.data.data
  },
}


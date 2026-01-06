import apiClient from './api'

export interface Notification {
  id: number
  title: string
  message: string
  type: string
  isRead: boolean
  userId: number
  createdDate: string
  readDate?: string
}

export interface PagedNotifications {
  data: Notification[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

export interface NotificationTemplate {
  name: string
  title: string
  message: string
  type: string
}

export interface NotificationStatistics {
  total: number
  unread: number
  read: number
  byType: Record<string, number>
}

export interface CreateNotificationRequest {
  title: string
  message: string
  type: string
  userId?: number
}

export interface NotificationFilters {
  isRead?: boolean
  type?: string
  startDate?: string
  endDate?: string
  searchTerm?: string
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

export const notificationService = {
  getNotifications: async (
    pageNumber: number = 1,
    pageSize: number = 10,
    filters?: NotificationFilters
  ): Promise<PagedNotifications> => {
    const params: any = { pageNumber, pageSize }
    
    if (filters) {
      if (filters.isRead !== undefined) params.isRead = filters.isRead
      if (filters.type) params.type = filters.type
      if (filters.startDate) params.startDate = filters.startDate
      if (filters.endDate) params.endDate = filters.endDate
      if (filters.searchTerm) params.searchTerm = filters.searchTerm
      if (filters.sortBy) params.sortBy = filters.sortBy
      if (filters.sortOrder) params.sortOrder = filters.sortOrder
    }
    
    const response = await apiClient.get('/Notifications', { params })
    return response.data.data
  },

  getMyNotifications: async (
    pageNumber: number = 1,
    pageSize: number = 10,
    filters?: NotificationFilters
  ): Promise<PagedNotifications> => {
    const params: any = {}
    
    // Backend only supports unreadOnly filter
    if (filters?.isRead === false) {
      params.unreadOnly = true
    }
    
    const response = await apiClient.get('/Notifications/my', { params })
    const notifications = Array.isArray(response.data.data) ? response.data.data : []
    
    // Client-side pagination (backend doesn't support pagination for /my endpoint)
    const startIndex = (pageNumber - 1) * pageSize
    const endIndex = startIndex + pageSize
    const paginatedNotifications = notifications.slice(startIndex, endIndex)
    
    // Apply additional filters on client side
    let filteredNotifications = paginatedNotifications
    if (filters) {
      if (filters.type) {
        filteredNotifications = filteredNotifications.filter((n: Notification) => n.type === filters.type)
      }
      if (filters.searchTerm) {
        const searchLower = filters.searchTerm.toLowerCase()
        filteredNotifications = filteredNotifications.filter(
          (n: Notification) =>
            n.title.toLowerCase().includes(searchLower) ||
            n.message.toLowerCase().includes(searchLower)
        )
      }
    }
    
    return {
      data: filteredNotifications,
      totalCount: notifications.length,
      pageNumber,
      pageSize,
      totalPages: Math.ceil(notifications.length / pageSize),
    }
  },

  getNotificationById: async (id: number): Promise<Notification> => {
    const response = await apiClient.get(`/Notifications/${id}`)
    return response.data.data
  },

  createNotification: async (data: CreateNotificationRequest): Promise<Notification> => {
    const response = await apiClient.post('/Notifications', data)
    return response.data.data
  },

  markAsRead: async (id: number): Promise<void> => {
    await apiClient.patch(`/Notifications/${id}/read`)
  },

  deleteNotification: async (id: number): Promise<void> => {
    await apiClient.delete(`/Notifications/${id}`)
  },

  getTemplates: async (): Promise<NotificationTemplate[]> => {
    const response = await apiClient.get('/Notifications/templates')
    return response.data.data
  },

  getTemplateByName: async (templateName: string): Promise<NotificationTemplate> => {
    const response = await apiClient.get(`/Notifications/templates/${templateName}`)
    return response.data.data
  },

  getStatistics: async (): Promise<NotificationStatistics> => {
    const response = await apiClient.get('/Notifications/statistics')
    return response.data.data
  },
}


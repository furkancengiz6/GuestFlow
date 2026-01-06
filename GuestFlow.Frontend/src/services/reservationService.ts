import apiClient from './api'

export interface Reservation {
  id: number
  reservationDate: string
  guestId: number
  guestName?: string
  personnelId: number
  personnelName?: string
  status: string
  note?: string
  createdDate: string
}

export interface ReservationDetail extends Reservation {
  transfers?: any[]
  tours?: any[]
  invoices?: any[]
}

export interface PagedReservations {
  data: Reservation[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

export interface CreateReservationRequest {
  reservationDate: string
  guestId: number
  personnelId: number
  status: string
  note?: string
}

export interface UpdateReservationRequest {
  reservationDate: string
  guestId: number
  personnelId: number
  status: string
  note?: string
}

export interface ReservationFilters {
  startDate?: string
  endDate?: string
  status?: string
  guestId?: number
  personnelId?: number
  searchTerm?: string
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

export const reservationService = {
  getReservations: async (
    pageNumber: number = 1,
    pageSize: number = 10,
    filters?: ReservationFilters
  ): Promise<PagedReservations> => {
    const params: any = { pageNumber, pageSize }
    
    if (filters) {
      if (filters.startDate) params.startDate = filters.startDate
      if (filters.endDate) params.endDate = filters.endDate
      if (filters.status) params.status = filters.status
      if (filters.guestId) params.guestId = filters.guestId
      if (filters.personnelId) params.personnelId = filters.personnelId
      if (filters.searchTerm) params.searchTerm = filters.searchTerm
      if (filters.sortBy) params.sortBy = filters.sortBy
      if (filters.sortOrder) params.sortOrder = filters.sortOrder
    }
    
    const response = await apiClient.get('/Reservations', { params })
    return response.data.data
  },

  getReservationById: async (id: number): Promise<Reservation> => {
    const response = await apiClient.get(`/Reservations/${id}`)
    return response.data.data
  },

  getReservationDetail: async (id: number): Promise<ReservationDetail> => {
    const response = await apiClient.get(`/Reservations/${id}/detail`)
    return response.data.data
  },

  createReservation: async (data: CreateReservationRequest): Promise<Reservation> => {
    const response = await apiClient.post('/Reservations', data)
    return response.data.data
  },

  updateReservation: async (id: number, data: UpdateReservationRequest): Promise<Reservation> => {
    const response = await apiClient.put(`/Reservations/${id}`, data)
    return response.data.data
  },

  deleteReservation: async (id: number): Promise<void> => {
    await apiClient.delete(`/Reservations/${id}`)
  },

  confirmReservation: async (id: number): Promise<void> => {
    await apiClient.post(`/Reservations/${id}/confirm`)
  },

  cancelReservation: async (id: number): Promise<void> => {
    await apiClient.post(`/Reservations/${id}/cancel`)
  },
}


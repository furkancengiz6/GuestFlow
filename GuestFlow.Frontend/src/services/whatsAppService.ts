// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

import apiClient from './api'
import type {
  SendWhatsApp,
  WhatsAppHistory,
  WhatsAppStatistics,
} from '../types/whatsApp'

export const whatsAppService = {
  /**
   * Send WhatsApp message
   */
  sendWhatsApp: async (data: SendWhatsApp): Promise<WhatsAppHistory> => {
    const response = await apiClient.post<{
      success: boolean
      data: WhatsAppHistory
      message?: string
    }>('/api/v1.0/WhatsApp/send', data)
    return response.data.data
  },

  /**
   * Send transfer reminder WhatsApp
   */
  sendTransferReminder: async (
    transferId: number,
    hoursBefore: number = 24
  ): Promise<WhatsAppHistory> => {
    const response = await apiClient.post<{
      success: boolean
      data: WhatsAppHistory
      message?: string
    }>(`/api/v1.0/WhatsApp/transfer/${transferId}/reminder?hoursBefore=${hoursBefore}`)
    return response.data.data
  },

  /**
   * Send tour reminder WhatsApp
   */
  sendTourReminder: async (
    tourType: string,
    tourId: number,
    hoursBefore: number = 24
  ): Promise<WhatsAppHistory> => {
    const response = await apiClient.post<{
      success: boolean
      data: WhatsAppHistory
      message?: string
    }>(`/api/v1.0/WhatsApp/tour/${tourType}/${tourId}/reminder?hoursBefore=${hoursBefore}`)
    return response.data.data
  },

  /**
   * Send reservation confirmation WhatsApp
   */
  sendReservationConfirmation: async (
    reservationId: number
  ): Promise<WhatsAppHistory> => {
    const response = await apiClient.post<{
      success: boolean
      data: WhatsAppHistory
      message?: string
    }>(`/api/v1.0/WhatsApp/reservation/${reservationId}/confirmation`)
    return response.data.data
  },

  /**
   * Get WhatsApp history (paged)
   */
  getWhatsAppHistory: async (params: {
    pageNumber?: number
    pageSize?: number
    guestId?: number
    status?: string
    startDate?: string
    endDate?: string
    sortBy?: string
    sortOrder?: string
  }): Promise<{
    data: WhatsAppHistory[]
    totalCount: number
    pageNumber: number
    pageSize: number
  }> => {
    const response = await apiClient.post<{
      success: boolean
      data: {
        data: WhatsAppHistory[]
        totalCount: number
        pageNumber: number
        pageSize: number
      }
    }>('/api/v1.0/WhatsApp/history', null, { params })
    return response.data.data
  },

  /**
   * Get WhatsApp history by guest ID
   */
  getWhatsAppHistoryByGuest: async (
    guestId: number
  ): Promise<WhatsAppHistory[]> => {
    const response = await apiClient.get<{
      success: boolean
      data: WhatsAppHistory[]
    }>(`/api/v1.0/WhatsApp/guest/${guestId}`)
    return response.data.data
  },

  /**
   * Get WhatsApp statistics
   */
  getWhatsAppStatistics: async (params?: {
    startDate?: string
    endDate?: string
  }): Promise<WhatsAppStatistics> => {
    const response = await apiClient.get<{
      success: boolean
      data: WhatsAppStatistics
    }>('/api/v1.0/WhatsApp/statistics', { params })
    return response.data.data
  },
}

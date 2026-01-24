// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

import apiClient from './api'
import type {
  UnifiedCommunicationHistory,
  SendMessageRequest,
  SmartNotificationType,
} from '../types/communication'

export const communicationService = {
  /**
   * Misafir için tüm iletişim geçmişini getirir
   */
  getGuestCommunicationHistory: async (
    guestId: number,
    startDate?: string,
    endDate?: string
  ): Promise<UnifiedCommunicationHistory> => {
    const params: Record<string, string> = {}
    if (startDate) params.startDate = startDate
    if (endDate) params.endDate = endDate

    const response = await apiClient.get(
      `/Communication/guests/${guestId}/history`,
      { params }
    )
    return response.data.data || response.data
  },

  /**
   * Misafire mesaj gönderir
   */
  sendMessage: async (
    guestId: number,
    data: SendMessageRequest
  ): Promise<boolean> => {
    const response = await apiClient.post(
      `/Communication/guests/${guestId}/send`,
      data
    )
    return response.data.data || response.data
  },

  /**
   * Smart notification gönderir
   */
  sendSmartNotification: async (
    guestId: number,
    notificationType: SmartNotificationType
  ): Promise<boolean> => {
    const response = await apiClient.post(
      `/Communication/guests/${guestId}/smart-notification`,
      null,
      { params: { notificationType } }
    )
    return response.data.data || response.data
  },
}

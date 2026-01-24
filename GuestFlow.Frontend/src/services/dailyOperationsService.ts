// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

import apiClient from './api'
import { DailyOperations } from '../types/dailyOperations'

export const dailyOperationsService = {
  /**
   * Günlük operasyon özetini getirir
   * @param date Tarih (opsiyonel, varsayılan: bugün)
   */
  getDailyOperations: async (date?: string): Promise<DailyOperations> => {
    const params = date ? { date } : {}
    const response = await apiClient.get<{ success: boolean; data: DailyOperations }>(
      '/api/v1.0/dashboard/daily-operations',
      { params }
    )
    return response.data.data
  },
}

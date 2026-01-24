// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

import apiClient from './api'

export interface AssignDriverRequest {
  personnelId: number
}

export interface RecordPaymentRequest {
  guestId: number
  amount: number
  currency?: string
  paymentMethod?: string
  paymentDate?: string
  notes?: string
}

export const operationsService = {
  /**
   * Transfer'i onaylar
   */
  confirmTransfer: async (transferId: number): Promise<void> => {
    await apiClient.post(`/api/v1.0/operations/transfers/${transferId}/confirm`)
  },

  /**
   * Transfer'i iptal eder
   */
  cancelTransfer: async (transferId: number, reason?: string): Promise<void> => {
    await apiClient.post(`/api/v1.0/operations/transfers/${transferId}/cancel`, { reason })
  },

  /**
   * Transfer'e şoför atar
   */
  assignDriver: async (transferId: number, personnelId: number): Promise<void> => {
    await apiClient.post(`/api/v1.0/operations/transfers/${transferId}/assign-driver`, {
      personnelId,
    })
  },

  /**
   * Servis için ödeme kaydeder
   */
  recordPayment: async (
    serviceType: 'Transfer' | 'CityTour' | 'YachtTour',
    serviceId: number,
    request: RecordPaymentRequest
  ): Promise<void> => {
    await apiClient.post(`/api/v1.0/operations/services/${serviceType.toLowerCase()}/${serviceId}/payment`, request)
  },
}

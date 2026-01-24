import apiClient from './api'

export interface PrivacyActionHistory {
  id: number
  guestId: number
  actionType: 'Anonymize' | 'Delete'
  reason: string
  requestedByPersonnelId?: number
  requestedByPersonnelName?: string
  actionDate: string
}

export interface AnonymizeGuestRequest {
  guestId: number
  reason: string
}

export interface DeleteGuestRequest {
  guestId: number
  reason: string
  confirmDeletion: boolean
}

export interface MaskRequest {
  value: string
}

export const privacyService = {
  // Mask PII data
  maskEmail: async (email: string): Promise<string> => {
    const response = await apiClient.post('/api/v1.0/Privacy/mask/email', { value: email })
    return response.data.data
  },

  maskPhone: async (phone: string): Promise<string> => {
    const response = await apiClient.post('/api/v1.0/Privacy/mask/phone', { value: phone })
    return response.data.data
  },

  // Anonymize guest
  anonymizeGuest: async (request: AnonymizeGuestRequest): Promise<boolean> => {
    const response = await apiClient.post('/api/v1.0/Privacy/anonymize-guest', request)
    return response.data.data
  },

  // Delete guest data
  deleteGuest: async (request: DeleteGuestRequest): Promise<boolean> => {
    const response = await apiClient.post('/api/v1.0/Privacy/delete-guest', request)
    return response.data.data
  },

  // Get privacy action history
  getPrivacyActionHistory: async (
    startDate?: string,
    endDate?: string,
    guestId?: number
  ): Promise<PrivacyActionHistory[]> => {
    const params = new URLSearchParams()
    if (startDate) params.append('startDate', startDate)
    if (endDate) params.append('endDate', endDate)
    if (guestId) params.append('guestId', guestId.toString())
    
    const response = await apiClient.get(`/api/v1.0/Privacy/history?${params}`)
    return response.data.data
  },

  // Check if guest is anonymized
  checkAnonymized: async (guestId: number): Promise<boolean> => {
    const response = await apiClient.get(`/api/v1.0/Privacy/check-anonymized/${guestId}`)
    return response.data.data
  },
}

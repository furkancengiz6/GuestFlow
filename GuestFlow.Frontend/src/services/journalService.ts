import apiClient from './api'

export interface JournalLineDto {
  accountCode: string
  debit: number
  credit: number
  description?: string | null
}

export interface JournalPreviewResponse {
  invoiceId: number
  description: string
  currency: string
  lines: JournalLineDto[]
  totalDebit: number
  totalCredit: number
}

export interface JournalPostRequest {
  invoiceId: number
  postingDate: string
  lines: JournalLineDto[]
}

export const journalService = {
  preview: async (invoiceId: number): Promise<JournalPreviewResponse> => {
    const response = await apiClient.get('/Journal/preview', { params: { invoiceId } })
    return response.data.data
  },

  post: async (request: JournalPostRequest): Promise<boolean> => {
    const response = await apiClient.post('/Journal/post', request)
    return response.data.data
  },
}


import apiClient from './api'

type AnyApiResponse = {
  data?: any
  Data?: any
}

const unwrapData = <T>(raw: any): T => {
  // Preferred API shape: { data: { data: T } }
  if (raw?.data?.data !== undefined) return raw.data.data as T
  // Fallback: { data: { Data: T } } or { Data: T }
  if (raw?.data?.Data !== undefined) return raw.data.Data as T
  if (raw?.Data !== undefined) return raw.Data as T
  // Last resort
  return raw as T
}

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
    return unwrapData<JournalPreviewResponse>(response)
  },

  post: async (request: JournalPostRequest): Promise<boolean> => {
    const response = await apiClient.post('/Journal/post', request)
    return unwrapData<boolean>(response)
  },
}


import api from './api'

export interface ImportGuestDto {
  fullName: string
  email?: string
  phoneNumber?: string
  nationality: string
  guestCode?: string
  isSpecialGuest: boolean
  isValid: boolean
  errors?: string[]
  rowNumber?: number
}

export interface ImportPreviewResponse {
  totalRows: number
  validRows: number
  invalidRows: number
  data: ImportGuestDto[]
  errors?: string[]
}

export interface ImportResult {
  successCount: number
  errorCount: number
  skippedCount: number
  errors?: string[]
}

export interface SaveImportedGuestsRequest {
  guests: ImportGuestDto[]
  skipDuplicates: boolean
}

/**
 * Import service for handling file imports
 */
export const importService = {
  /**
   * Preview guests from Excel file
   */
  previewGuestsFromExcel: async (file: File): Promise<ImportPreviewResponse> => {
    const formData = new FormData()
    formData.append('file', file)

    const response = await api.post('/import/guests/excel/preview', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    })

    return response.data.data
  },

  /**
   * Preview guests from CSV file
   */
  previewGuestsFromCsv: async (file: File): Promise<ImportPreviewResponse> => {
    const formData = new FormData()
    formData.append('file', file)

    const response = await api.post('/import/guests/csv/preview', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    })

    return response.data.data
  },

  /**
   * Import guests from Excel file (direct import without preview)
   */
  importGuestsFromExcel: async (
    file: File,
    skipDuplicates: boolean = true
  ): Promise<ImportResult> => {
    const formData = new FormData()
    formData.append('file', file)
    formData.append('skipDuplicates', skipDuplicates.toString())

    const response = await api.post('/import/guests/excel', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    })

    return response.data.data
  },

  /**
   * Import guests from CSV file (direct import without preview)
   */
  importGuestsFromCsv: async (
    file: File,
    skipDuplicates: boolean = true
  ): Promise<ImportResult> => {
    const formData = new FormData()
    formData.append('file', file)
    formData.append('skipDuplicates', skipDuplicates.toString())

    const response = await api.post('/import/guests/csv', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    })

    return response.data.data
  },

  /**
   * Save imported guests after preview
   */
  saveImportedGuests: async (
    request: SaveImportedGuestsRequest
  ): Promise<ImportResult> => {
    const response = await api.post('/import/guests/save', request)
    return response.data.data
  },
}


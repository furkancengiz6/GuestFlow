import { AxiosError } from 'axios'

export interface ApiError {
  message: string
  statusCode?: number
  errors?: Record<string, string[]>
  timestamp?: string
}

/**
 * Extract user-friendly error message from API error
 */
export const getErrorMessage = (error: unknown): string => {
  if (error instanceof AxiosError) {
    const apiError = error.response?.data as ApiError

    // Backend validation errors
    if (apiError?.errors) {
      const firstError = Object.values(apiError.errors)[0]
      if (Array.isArray(firstError) && firstError.length > 0) {
        return firstError[0]
      }
    }

    // Backend error message
    if (apiError?.message) {
      return apiError.message
    }

    // HTTP status code based messages
    switch (error.response?.status) {
      case 400:
        return 'Geçersiz istek. Lütfen bilgilerinizi kontrol ediniz.'
      case 401:
        return 'Oturum süreniz dolmuş. Lütfen tekrar giriş yapınız.'
      case 403:
        return 'Bu işlem için yetkiniz bulunmamaktadır.'
      case 404:
        return 'İstenen kaynak bulunamadı.'
      case 409:
        return 'Bu işlem çakışma yaratıyor. Lütfen tekrar deneyiniz.'
      case 422:
        return 'Gönderilen veriler geçersiz. Lütfen kontrol ediniz.'
      case 500:
        return 'Sunucu hatası oluştu. Lütfen daha sonra tekrar deneyiniz.'
      case 503:
        return 'Servis şu anda kullanılamıyor. Lütfen daha sonra tekrar deneyiniz.'
      default:
        return error.message || 'Bir hata oluştu. Lütfen tekrar deneyiniz.'
    }
  }

  if (error instanceof Error) {
    return error.message
  }

  return 'Beklenmeyen bir hata oluştu. Lütfen tekrar deneyiniz.'
}

/**
 * Check if error is a network error
 */
export const isNetworkError = (error: unknown): boolean => {
  if (error instanceof AxiosError) {
    return !error.response && error.request
  }
  return false
}

/**
 * Check if error is a timeout error
 */
export const isTimeoutError = (error: unknown): boolean => {
  if (error instanceof AxiosError) {
    return error.code === 'ECONNABORTED' || error.message.includes('timeout')
  }
  return false
}

/**
 * Get error status code
 */
export const getErrorStatusCode = (error: unknown): number | undefined => {
  if (error instanceof AxiosError) {
    return error.response?.status
  }
  return undefined
}

/**
 * Format validation errors for display
 */
export const formatValidationErrors = (errors: Record<string, string[]>): string[] => {
  const messages: string[] = []
  Object.keys(errors).forEach((key) => {
    const fieldErrors = errors[key]
    if (Array.isArray(fieldErrors)) {
      fieldErrors.forEach((error) => {
        messages.push(`${key}: ${error}`)
      })
    }
  })
  return messages
}


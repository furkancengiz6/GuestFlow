// Mock i18n for tests
const mockI18n = {
  language: 'tr',
  t: (key: string) => key,
}

// Use mock in test environment, real i18n in production
import i18nLib from '../i18n/config'
const i18n = process.env.NODE_ENV === 'test' ? mockI18n : i18nLib

export const formatCurrency = (amount: number, currency: string = 'TRY') => {
  const locale = i18n.language === 'tr' ? 'tr-TR' : 'en-US'
  return new Intl.NumberFormat(locale, {
    style: 'currency',
    currency,
  }).format(amount)
}

export const formatDate = (dateString: string | Date) => {
  const date = typeof dateString === 'string' ? new Date(dateString) : dateString
  const locale = i18n.language === 'tr' ? 'tr-TR' : 'en-US'
  return new Date(date).toLocaleDateString(locale, {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
  })
}

export const formatDateTime = (dateString: string | Date) => {
  const date = typeof dateString === 'string' ? new Date(dateString) : dateString
  const locale = i18n.language === 'tr' ? 'tr-TR' : 'en-US'
  return new Date(date).toLocaleString(locale, {
    day: '2-digit',
    month: 'short',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}


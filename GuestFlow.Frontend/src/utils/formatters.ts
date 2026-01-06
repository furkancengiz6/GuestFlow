import i18n from '../i18n/config'

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


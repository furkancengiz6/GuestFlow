import { format, parseISO } from 'date-fns'
import { tr, enUS } from 'date-fns/locale'
import i18n from '../i18n/config'

/**
 * Get date-fns locale based on current i18n language
 */
export const getDateLocale = () => {
  return i18n.language === 'tr' ? tr : enUS
}

/**
 * Format date according to current locale
 */
export const formatLocalizedDate = (
  date: string | Date,
  formatString: string = 'dd MMMM yyyy'
): string => {
  const dateObj = typeof date === 'string' ? parseISO(date) : date
  const locale = getDateLocale()
  return format(dateObj, formatString, { locale })
}

/**
 * Format date and time according to current locale
 */
export const formatLocalizedDateTime = (
  date: string | Date,
  dateFormat: string = 'dd MMMM yyyy',
  timeFormat: string = 'HH:mm'
): string => {
  const dateObj = typeof date === 'string' ? parseISO(date) : date
  const locale = getDateLocale()
  const formattedDate = format(dateObj, dateFormat, { locale })
  const formattedTime = format(dateObj, timeFormat, { locale })
  return `${formattedDate} ${formattedTime}`
}

/**
 * Format time according to current locale
 */
export const formatLocalizedTime = (date: string | Date, formatString: string = 'HH:mm'): string => {
  const dateObj = typeof date === 'string' ? parseISO(date) : date
  const locale = getDateLocale()
  return format(dateObj, formatString, { locale })
}

/**
 * Format relative time (e.g., "2 hours ago")
 */
export const formatRelativeTime = (date: string | Date): string => {
  const dateObj = typeof date === 'string' ? parseISO(date) : date
  const now = new Date()
  const diffInSeconds = Math.floor((now.getTime() - dateObj.getTime()) / 1000)

  if (diffInSeconds < 60) {
    return i18n.language === 'tr' ? 'Az önce' : 'Just now'
  }

  const diffInMinutes = Math.floor(diffInSeconds / 60)
  if (diffInMinutes < 60) {
    return i18n.language === 'tr'
      ? `${diffInMinutes} dakika önce`
      : `${diffInMinutes} minutes ago`
  }

  const diffInHours = Math.floor(diffInMinutes / 60)
  if (diffInHours < 24) {
    return i18n.language === 'tr' ? `${diffInHours} saat önce` : `${diffInHours} hours ago`
  }

  const diffInDays = Math.floor(diffInHours / 24)
  if (diffInDays < 7) {
    return i18n.language === 'tr' ? `${diffInDays} gün önce` : `${diffInDays} days ago`
  }

  return formatLocalizedDate(dateObj)
}


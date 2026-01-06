import { useTranslation as useI18nTranslation } from 'react-i18next'
import { useUserPreferencesStore } from '../stores/userPreferencesStore'

/**
 * Custom translation hook with type safety
 * This is a wrapper around react-i18next's useTranslation
 */
export const useTranslation = () => {
  const { t, i18n } = useI18nTranslation()
  const { setLanguage } = useUserPreferencesStore()

  const changeLanguage = (lng: string) => {
    i18n.changeLanguage(lng)
    setLanguage(lng)
  }

  return {
    t,
    i18n,
    currentLanguage: i18n.language,
    changeLanguage,
    isRTL: i18n.dir() === 'rtl',
  }
}


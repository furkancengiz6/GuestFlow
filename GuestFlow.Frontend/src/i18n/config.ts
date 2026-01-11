import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'
import trTranslations from './locales/tr.json'
import enTranslations from './locales/en.json'
import { useUserPreferencesStore } from '../stores/userPreferencesStore'

// Language detection from localStorage or browser
const getInitialLanguage = (): string => {
  const savedLanguage = localStorage.getItem('language')
  if (savedLanguage) return savedLanguage

  const browserLanguage = navigator.language.split('-')[0]
  return ['tr', 'en'].includes(browserLanguage) ? browserLanguage : 'tr'
}

i18n
  .use(initReactI18next)
  .init({
    resources: {
      tr: {
        translation: trTranslations,
      },
      en: {
        translation: enTranslations,
      },
    },
    lng: getInitialLanguage(),
    fallbackLng: 'tr',
    interpolation: {
      escapeValue: false, // React already escapes values
    },
    react: {
      useSuspense: false, // Disable suspense for better compatibility
    },
  })

// Save language to localStorage and user preferences when changed
i18n.on('languageChanged', (lng) => {
  localStorage.setItem('language', lng)
  // Sync with user preferences store
  try {
    useUserPreferencesStore.getState().setLanguage(lng)
  } catch {
    // Ignore if store not available
  }
})

export default i18n


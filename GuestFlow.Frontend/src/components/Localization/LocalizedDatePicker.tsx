import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider'
import { AdapterDateFns } from '@mui/x-date-pickers/AdapterDateFns'
import { tr, enUS } from 'date-fns/locale'
import { useTranslation } from '../../hooks/useTranslation'
import { ReactNode } from 'react'

interface LocalizedDatePickerProps {
  children: ReactNode
}

/**
 * Localized DatePicker provider component
 * Automatically uses the correct locale based on i18n language
 */
export const LocalizedDatePicker = ({ children }: LocalizedDatePickerProps) => {
  const { currentLanguage } = useTranslation()
  const locale = currentLanguage === 'tr' ? tr : enUS

  return (
    <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={locale}>
      {children}
    </LocalizationProvider>
  )
}

export default LocalizedDatePicker


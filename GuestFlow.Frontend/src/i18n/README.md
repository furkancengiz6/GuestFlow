# Internationalization (i18n) Setup

This project uses `react-i18next` for internationalization support.

## Features

- ✅ Turkish (TR) and English (EN) language support
- ✅ Automatic language detection from browser/localStorage
- ✅ Language switcher component in header
- ✅ Localized date/time formatting
- ✅ Localized currency formatting
- ✅ Translation files for all UI text

## Usage

### Using translations in components

```tsx
import { useTranslation } from '../../hooks/useTranslation'

const MyComponent = () => {
  const { t } = useTranslation()

  return (
    <div>
      <h1>{t('common.save')}</h1>
      <p>{t('messages.saveSuccess')}</p>
    </div>
  )
}
```

### Using translations with interpolation

```tsx
const { t } = useTranslation()
t('validation.minLength', { min: 5 }) // "En az 5 karakter olmalıdır"
```

### Changing language programmatically

```tsx
import { useTranslation } from '../../hooks/useTranslation'

const MyComponent = () => {
  const { changeLanguage, currentLanguage } = useTranslation()

  return (
    <button onClick={() => changeLanguage('en')}>
      Current: {currentLanguage}
    </button>
  )
}
```

### Using localized date formatting

```tsx
import { formatLocalizedDate, formatLocalizedDateTime } from '../../utils/dateLocalization'

const MyComponent = () => {
  const date = new Date()
  
  return (
    <div>
      <p>{formatLocalizedDate(date)}</p>
      <p>{formatLocalizedDateTime(date)}</p>
    </div>
  )
}
```

### Using LocalizedDatePicker

```tsx
import { LocalizedDatePicker } from '../../components/Localization/LocalizedDatePicker'
import { DatePicker } from '@mui/x-date-pickers/DatePicker'

const MyComponent = () => {
  return (
    <LocalizedDatePicker>
      <DatePicker label="Select Date" />
    </LocalizedDatePicker>
  )
}
```

## Translation Files

Translation files are located in:
- `src/i18n/locales/tr.json` - Turkish translations
- `src/i18n/locales/en.json` - English translations

## Adding New Translations

1. Add the translation key to both `tr.json` and `en.json`
2. Use the key in your component with `t('namespace.key')`

Example:
```json
// tr.json
{
  "myFeature": {
    "title": "Başlık",
    "description": "Açıklama"
  }
}

// en.json
{
  "myFeature": {
    "title": "Title",
    "description": "Description"
  }
}
```

Usage:
```tsx
t('myFeature.title') // "Başlık" or "Title"
```

## Language Switcher

The language switcher is automatically included in the Header component. Users can switch between Turkish and English by clicking the language icon.

## Date/Time Localization

All date/time formatting functions automatically use the current language:
- `formatDate()` - Uses current locale
- `formatDateTime()` - Uses current locale
- `formatCurrency()` - Uses current locale
- `formatLocalizedDate()` - date-fns with locale
- `formatLocalizedDateTime()` - date-fns with locale
- `formatLocalizedTime()` - date-fns with locale


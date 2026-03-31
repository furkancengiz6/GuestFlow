import { ThemeProvider, createTheme } from '@mui/material/styles'
import { ReactNode, useMemo } from 'react'
import { useUserPreferencesStore } from '../stores/userPreferencesStore'
import { ThemeContext } from './useTheme'

const lightTheme = createTheme({
  palette: {
    mode: 'light',
    primary: {
      main: '#5754E8',
      light: '#7F7DED',
      dark: '#403DBE',
    },
    secondary: {
      main: '#FF6B6B',
      light: '#FF8E8E',
      dark: '#E64A19',
    },
  },
  shape: {
    borderRadius: 12,
  },
})

const darkTheme = createTheme({
  palette: {
    mode: 'dark',
    primary: {
      main: '#90caf9',
      light: '#e3f2fd',
      dark: '#42a5f5',
    },
    secondary: {
      main: '#f48fb1',
      light: '#fce4ec',
      dark: '#ad1457',
    },
  },
  shape: {
    borderRadius: 8,
  },
})

export const ThemeProviderWithToggle = ({ children }: { children: ReactNode }) => {
  const { theme: mode, setTheme } = useUserPreferencesStore()

  const toggleMode = () => {
    const newMode = mode === 'light' ? 'dark' : 'light'
    setTheme(newMode)
  }

  const theme = useMemo(() => (mode === 'light' ? lightTheme : darkTheme), [mode])

  return (
    <ThemeContext.Provider value={{ mode, toggleMode }}>
      <ThemeProvider theme={theme}>{children}</ThemeProvider>
    </ThemeContext.Provider>
  )
}


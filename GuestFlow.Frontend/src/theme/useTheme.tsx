import { createTheme, ThemeProvider, PaletteMode } from '@mui/material/styles'
import { useMemo, useState, useEffect, ReactNode, useContext, createContext } from 'react'

const lightTheme = createTheme({
  palette: {
    mode: 'light',
  },
})

const darkTheme = createTheme({
  palette: {
    mode: 'dark',
  },
})

type ThemeCtx = {
  mode: PaletteMode
  toggleMode: () => void
}

const ThemeContext = createContext<ThemeCtx>({
  mode: 'light',
  toggleMode: () => {},
})

export const ThemeProviderWithToggle = ({ children }: { children: ReactNode }) => {
  const [mode, setMode] = useState<PaletteMode>('light')

  useEffect(() => {
    const saved = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
    setMode(saved as PaletteMode)
  }, [])

  const toggleMode = () => {
    setMode((prev) => (prev === 'light' ? 'dark' : 'light'))
  }

  const theme = useMemo(() => (mode === 'light' ? lightTheme : darkTheme), [mode])

  return (
    <ThemeContext.Provider value={{ mode, toggleMode }}>
      <ThemeProvider theme={theme}>{children}</ThemeProvider>
    </ThemeContext.Provider>
  )
}

export const useTheme = () => useContext(ThemeContext)


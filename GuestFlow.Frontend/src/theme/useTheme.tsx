import { PaletteMode } from '@mui/material'
import { useContext, createContext } from 'react'

type ThemeCtx = {
  mode: PaletteMode
  toggleMode: () => void
}

export const ThemeContext = createContext<ThemeCtx>({
  mode: 'light',
  toggleMode: () => {},
})

export const useTheme = () => useContext(ThemeContext)


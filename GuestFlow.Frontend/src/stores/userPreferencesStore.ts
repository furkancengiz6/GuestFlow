import { create } from 'zustand'
import { persist } from 'zustand/middleware'
import { PaletteMode } from '@mui/material'

interface UserPreferences {
  theme: PaletteMode
  language: string
  notifications: {
    enabled: boolean
    sound: boolean
    email: boolean
  }
  table: {
    pageSize: number
    density: 'comfortable' | 'standard' | 'compact'
  }
  dashboard: {
    defaultPeriod: 'daily' | 'weekly' | 'monthly'
    showCharts: boolean
  }
}

interface UserPreferencesStore extends UserPreferences {
  setTheme: (theme: PaletteMode) => void
  setLanguage: (language: string) => void
  setNotifications: (notifications: Partial<UserPreferences['notifications']>) => void
  setTable: (table: Partial<UserPreferences['table']>) => void
  setDashboard: (dashboard: Partial<UserPreferences['dashboard']>) => void
  reset: () => void
}

const defaultPreferences: UserPreferences = {
  theme: 'light',
  language: 'tr',
  notifications: {
    enabled: true,
    sound: true,
    email: false,
  },
  table: {
    pageSize: 10,
    density: 'standard',
  },
  dashboard: {
    defaultPeriod: 'daily',
    showCharts: true,
  },
}

export const useUserPreferencesStore = create<UserPreferencesStore>()(
  persist(
    (set) => ({
      ...defaultPreferences,
      setTheme: (theme) => set({ theme }),
      setLanguage: (language) => set({ language }),
      setNotifications: (notifications) =>
        set((state) => ({
          notifications: { ...state.notifications, ...notifications },
        })),
      setTable: (table) =>
        set((state) => ({
          table: { ...state.table, ...table },
        })),
      setDashboard: (dashboard) =>
        set((state) => ({
          dashboard: { ...state.dashboard, ...dashboard },
        })),
      reset: () => set(defaultPreferences),
    }),
    {
      name: 'user-preferences',
      version: 1,
    }
  )
)


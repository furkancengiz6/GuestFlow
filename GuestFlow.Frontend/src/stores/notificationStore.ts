import { create } from 'zustand'

export type NotificationSeverity = 'success' | 'error' | 'warning' | 'info'

export interface Notification {
  id: string
  message: string
  severity: NotificationSeverity
  duration?: number
}

interface NotificationState {
  notifications: Notification[]
  addNotification: (message: string, severity?: NotificationSeverity, duration?: number) => void
  removeNotification: (id: string) => void
  clearAll: () => void
}

export const useNotificationStore = create<NotificationState>((set) => ({
  notifications: [],
  addNotification: (message, severity = 'info', duration = 4000) => {
    const id = Date.now().toString() + Math.random().toString(36).substr(2, 9)
    set((state) => ({
      notifications: [...state.notifications, { id, message, severity, duration }],
    }))
  },
  removeNotification: (id) => {
    set((state) => ({
      notifications: state.notifications.filter((n) => n.id !== id),
    }))
  },
  clearAll: () => {
    set({ notifications: [] })
  },
}))


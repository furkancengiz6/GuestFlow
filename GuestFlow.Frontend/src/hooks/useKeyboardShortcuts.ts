import { useEffect, useCallback } from 'react'

export interface KeyboardShortcut {
  key: string
  ctrl?: boolean
  shift?: boolean
  alt?: boolean
  meta?: boolean
  action: () => void
  description?: string
  preventDefault?: boolean
}

/**
 * Hook for managing keyboard shortcuts
 */
export const useKeyboardShortcuts = (shortcuts: KeyboardShortcut[]) => {
  const handleKeyDown = useCallback(
    (event: KeyboardEvent) => {
      shortcuts.forEach((shortcut) => {
        const keyMatches = event.key.toLowerCase() === shortcut.key.toLowerCase()
        const ctrlMatches = shortcut.ctrl ? event.ctrlKey || event.metaKey : !event.ctrlKey && !event.metaKey
        const shiftMatches = shortcut.shift ? event.shiftKey : !event.shiftKey
        const altMatches = shortcut.alt ? event.altKey : !event.altKey

        if (keyMatches && ctrlMatches && shiftMatches && altMatches) {
          if (shortcut.preventDefault !== false) {
            event.preventDefault()
          }
          shortcut.action()
        }
      })
    },
    [shortcuts]
  )

  useEffect(() => {
    window.addEventListener('keydown', handleKeyDown)
    return () => {
      window.removeEventListener('keydown', handleKeyDown)
    }
  }, [handleKeyDown])
}

/**
 * Common keyboard shortcuts for the application
 */
export const commonShortcuts: KeyboardShortcut[] = [
  {
    key: 'k',
    ctrl: true,
    action: () => {
      // Global search - to be implemented
      const searchInput = document.querySelector<HTMLInputElement>('[data-global-search]')
      searchInput?.focus()
    },
    description: 'Global search',
  },
  {
    key: 'n',
    ctrl: true,
    action: () => {
      // New item - context dependent
      const newButton = document.querySelector<HTMLButtonElement>('[data-new-item]')
      newButton?.click()
    },
    description: 'New item',
  },
  {
    key: 's',
    ctrl: true,
    action: () => {
      // Save - context dependent
      const saveButton = document.querySelector<HTMLButtonElement>('[data-save]')
      saveButton?.click()
    },
    description: 'Save',
    preventDefault: false, // Allow browser save dialog
  },
  {
    key: 'Escape',
    action: () => {
      // Close dialogs/modals
      const closeButton = document.querySelector<HTMLButtonElement>('[data-close]')
      closeButton?.click()
    },
    description: 'Close dialog',
  },
  {
    key: '/',
    action: () => {
      // Focus search
      const searchInput = document.querySelector<HTMLInputElement>('[data-search]')
      searchInput?.focus()
    },
    description: 'Focus search',
  },
]


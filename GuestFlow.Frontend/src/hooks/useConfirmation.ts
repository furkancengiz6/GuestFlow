import { useState, useCallback } from 'react'

interface ConfirmationOptions {
  title: string
  message: string
  type?: 'warning' | 'error' | 'info' | 'success'
  confirmText?: string
  cancelText?: string
  destructive?: boolean
}

interface ConfirmationState extends ConfirmationOptions {
  open: boolean
  onConfirm: () => void
  onCancel: () => void
}

/**
 * Hook for managing confirmation dialogs
 */
export const useConfirmation = () => {
  const [state, setState] = useState<ConfirmationState | null>(null)

  const confirm = useCallback(
    (options: ConfirmationOptions): Promise<boolean> => {
      return new Promise((resolve) => {
        setState({
          ...options,
          open: true,
          onConfirm: () => {
            setState(null)
            resolve(true)
          },
          onCancel: () => {
            setState(null)
            resolve(false)
          },
        })
      })
    },
    []
  )

  return {
    confirm,
    confirmationState: state,
  }
}


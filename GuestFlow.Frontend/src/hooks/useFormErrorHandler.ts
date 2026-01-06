import { useCallback } from 'react'
import { UseFormSetError } from 'react-hook-form'
import { extractBackendErrors, setBackendErrors } from '../utils/validation'
import { getErrorMessage } from '../utils/errorHandler'
import { useNotification } from './useNotification'

/**
 * Hook for handling form errors from backend
 */
export const useFormErrorHandler = <T extends Record<string, any>>(
  setError: UseFormSetError<T>
) => {
  const notification = useNotification()

  const handleFormError = useCallback(
    (error: unknown, showNotification: boolean = true) => {
      const backendErrors = extractBackendErrors(error)
      
      // Set field-specific errors
      if (Object.keys(backendErrors).length > 0) {
        setBackendErrors(setError as any, backendErrors)
      }

      // Show notification if requested
      if (showNotification) {
        const errorMessage = getErrorMessage(error)
        notification.showError(errorMessage)
      }
    },
    [setError, notification]
  )

  return { handleFormError }
}


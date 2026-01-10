import { Alert, AlertTitle, Box } from '@mui/material'
import { FieldErrors } from 'react-hook-form'

interface FormErrorDisplayProps {
  errors: FieldErrors<any>
  fieldName: string
  customMessage?: string
}

/**
 * Component to display form field errors in a user-friendly way
 */
export const FormErrorDisplay = ({ errors, fieldName, customMessage }: FormErrorDisplayProps) => {
  const error = errors[fieldName]

  if (!error) return null

  const errorMessage =
    customMessage ||
    (typeof error === 'string' ? error : (error as any)?.message) ||
    'Bu alan için bir hata var'

  return (
    <Box sx={{ mt: 0.5, mb: 1 }}>
      <Alert severity="error" sx={{ py: 0 }}>
        {String(errorMessage)}
      </Alert>
    </Box>
  )
}

/**
 * Component to display global form errors
 */
export const GlobalFormError = ({ error }: { error?: string }) => {
  if (!error) return null

  return (
    <Alert severity="error" sx={{ mb: 2 }}>
      <AlertTitle>Hata</AlertTitle>
      {error}
    </Alert>
  )
}


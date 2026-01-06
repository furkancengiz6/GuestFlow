import React from 'react'
import { Control, Controller, FieldErrors } from 'react-hook-form'
import {
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  FormHelperText,
  Box,
  Typography,
  Button,
} from '@mui/material'

interface Option {
  value: string | number
  label: string
}

interface FormFieldProps {
  name: string
  control: Control<any>
  errors: FieldErrors<any>
  label: string
  type?: 'text' | 'email' | 'number' | 'date' | 'time' | 'textarea' | 'select'
  options?: Option[]
  required?: boolean
  multiline?: boolean
  rows?: number
  disabled?: boolean
  placeholder?: string
  helperText?: string
  fullWidth?: boolean
  size?: 'small' | 'medium'
  showRequiredIndicator?: boolean
  variant?: 'outlined' | 'filled' | 'standard'
}

/**
 * Reusable form field component with standardized validation and styling
 */
export const FormField = ({
  name,
  control,
  errors,
  label,
  type = 'text',
  options,
  required = false,
  multiline = false,
  rows = 3,
  disabled = false,
  placeholder,
  helperText,
  fullWidth = true,
  size = 'medium',
  showRequiredIndicator = true,
  variant = 'outlined',
}: FormFieldProps) => {
  const error = errors[name]
  const errorMessage = error?.message as string

  const displayLabel = showRequiredIndicator && required ? `${label} *` : label

  if (type === 'select' && options) {
    return (
      <FormControl fullWidth={fullWidth} error={!!error} size={size} disabled={disabled} variant={variant}>
        <InputLabel required={required}>{displayLabel}</InputLabel>
        <Controller
          name={name}
          control={control}
          render={({ field }) => (
            <Select {...field} label={displayLabel} required={required}>
              {options.map((option) => (
                <MenuItem key={option.value} value={option.value}>
                  {option.label}
                </MenuItem>
              ))}
            </Select>
          )}
        />
        {(errorMessage || helperText) && (
          <FormHelperText error={!!error}>{errorMessage || helperText}</FormHelperText>
        )}
      </FormControl>
    )
  }

  return (
    <Controller
      name={name}
      control={control}
      render={({ field }) => (
        <TextField
          {...field}
          label={displayLabel}
          type={type}
          fullWidth={fullWidth}
          required={required}
          error={!!error}
          helperText={errorMessage || helperText}
          disabled={disabled}
          placeholder={placeholder}
          multiline={multiline || type === 'textarea'}
          rows={multiline || type === 'textarea' ? rows : undefined}
          size={size}
          variant={variant}
          InputLabelProps={type === 'date' || type === 'time' ? { shrink: true } : undefined}
        />
      )}
    />
  )
}

/**
 * Standardized form container with consistent spacing and validation
 */
export const FormContainer = ({
  children,
  onSubmit,
  submitLabel = 'Kaydet',
  cancelLabel = 'İptal',
  onCancel,
  isSubmitting = false,
  isValid = true,
  submitDisabled = false,
  showRequiredNote = true,
  sx,
}: {
  children: React.ReactNode
  onSubmit: (e: React.FormEvent) => void
  submitLabel?: string
  cancelLabel?: string
  onCancel?: () => void
  isSubmitting?: boolean
  isValid?: boolean
  submitDisabled?: boolean
  showRequiredNote?: boolean
  sx?: any
}) => {
  return (
    <Box
      component="form"
      onSubmit={onSubmit}
      sx={{
        display: 'flex',
        flexDirection: 'column',
        gap: 3,
        ...sx,
      }}
    >
      {children}

      {showRequiredNote && (
        <Typography variant="caption" color="text.secondary" sx={{ fontStyle: 'italic' }}>
          * ile işaretlenmiş alanlar zorunludur
        </Typography>
      )}

      <Box sx={{ display: 'flex', gap: 2, justifyContent: 'flex-end', mt: 2 }}>
        {onCancel && (
          <Button
            type="button"
            variant="outlined"
            onClick={onCancel}
            disabled={isSubmitting}
          >
            {cancelLabel}
          </Button>
        )}
        <Button
          type="submit"
          variant="contained"
          disabled={submitDisabled || !isValid || isSubmitting}
          sx={{ minWidth: 120 }}
        >
          {isSubmitting ? 'Kaydediliyor...' : submitLabel}
        </Button>
      </Box>
    </Box>
  )
}

export default FormField


import { z } from 'zod'

/**
 * Standard validation schemas for common form fields
 */
export const commonValidations = {
  // String validations
  requiredString: (min: number = 1, max: number = 500, fieldName: string = 'Alan') =>
    z
      .string()
      .min(min, `${fieldName} en az ${min} karakter olmalıdır`)
      .max(max, `${fieldName} en fazla ${max} karakter olabilir`),

  optionalString: (max: number = 500, fieldName: string = 'Alan') =>
    z.string().max(max, `${fieldName} en fazla ${max} karakter olabilir`).optional().or(z.literal('')),

  // Email validation
  email: (required: boolean = false) => {
    const emailSchema = z.string().email('Geçerli bir e-posta adresi giriniz')
    return required ? emailSchema : emailSchema.optional().or(z.literal(''))
  },

  // Phone validation
  phone: () => z.string().optional().or(z.literal('')),

  // Number validations
  requiredNumber: (min: number = 0, max: number = Number.MAX_SAFE_INTEGER, fieldName: string = 'Değer') =>
    z.number().min(min, `${fieldName} en az ${min} olmalıdır`).max(max, `${fieldName} en fazla ${max} olabilir`),

  optionalNumber: (min: number = 0, max: number = Number.MAX_SAFE_INTEGER) =>
    z.number().min(min).max(max).optional().nullable(),

  // Date validation
  requiredDate: (fieldName: string = 'Tarih') =>
    z.date({ message: `${fieldName} gereklidir. Geçerli bir tarih seçiniz` }),

  optionalDate: () => z.date().optional().nullable(),

  // ID validation
  requiredId: (fieldName: string = 'Seçim') => z.number().min(1, `${fieldName} yapılmalıdır`),

  optionalId: () => z.number().optional().nullable(),

  // Boolean validation
  boolean: () => z.boolean(),

  // Enum validation
  enum: <T extends z.ZodEnum<any>>(enumSchema: T, fieldName: string = 'Değer') =>
    enumSchema.refine((val) => val !== undefined && val !== null, {
      message: `${fieldName} seçilmelidir`,
    }),
}

/**
 * Helper function to extract backend validation errors
 */
export const extractBackendErrors = (error: any): Record<string, string> => {
  const errors: Record<string, string> = {}

  if (!error?.response?.data) {
    return errors
  }

  const errorData = error.response.data

  // Handle FluentValidation errors (ModelState format)
  if (errorData.errors) {
    Object.keys(errorData.errors).forEach((key) => {
      const fieldErrors = errorData.errors[key]
      if (Array.isArray(fieldErrors) && fieldErrors.length > 0) {
        // Convert field name from PascalCase to camelCase
        const camelKey = key.charAt(0).toLowerCase() + key.slice(1)
        errors[camelKey] = fieldErrors[0]
      }
    })
  }

  // Handle single error message
  if (errorData.message && !errorData.errors) {
    errors._form = errorData.message
  }

  return errors
}

/**
 * Helper function to set backend errors to form
 */
export const setBackendErrors = (
  setError: (name: string, error: { type: string; message: string }) => void,
  errors: Record<string, string>
) => {
  Object.keys(errors).forEach((key) => {
    if (key === '_form') {
      // Global form error - you might want to handle this differently
      setError('root', { type: 'server', message: errors[key] })
    } else {
      setError(key, { type: 'server', message: errors[key] })
    }
  })
}

/**
 * Common validation error messages
 */
export const validationMessages = {
  required: (fieldName: string) => `${fieldName} gereklidir`,
  minLength: (fieldName: string, min: number) => `${fieldName} en az ${min} karakter olmalıdır`,
  maxLength: (fieldName: string, max: number) => `${fieldName} en fazla ${max} karakter olabilir`,
  min: (fieldName: string, min: number) => `${fieldName} en az ${min} olmalıdır`,
  max: (fieldName: string, max: number) => `${fieldName} en fazla ${max} olabilir`,
  email: 'Geçerli bir e-posta adresi giriniz',
  phone: 'Geçerli bir telefon numarası giriniz',
  url: 'Geçerli bir URL giriniz',
  date: 'Geçerli bir tarih seçiniz',
  number: 'Geçerli bir sayı giriniz',
  select: 'Lütfen bir seçim yapınız',
}


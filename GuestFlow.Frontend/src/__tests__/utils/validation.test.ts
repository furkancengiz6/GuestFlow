import { z } from 'zod'
import { commonValidations, extractBackendErrors } from '../../utils/validation'

describe('validation', () => {
  describe('commonValidations', () => {
    it('should validate email correctly', () => {
      const emailSchema = z.object({
        email: commonValidations.email,
      })

      expect(() => emailSchema.parse({ email: 'test@example.com' })).not.toThrow()
      expect(() => emailSchema.parse({ email: 'invalid-email' })).toThrow()
    })

    it('should validate phone number correctly', () => {
      const phoneSchema = z.object({
        phone: commonValidations.phone,
      })

      expect(() => phoneSchema.parse({ phone: '+905551234567' })).not.toThrow()
      expect(() => phoneSchema.parse({ phone: 'invalid' })).toThrow()
    })

    it('should validate required string', () => {
      const requiredSchema = z.object({
        name: commonValidations.requiredString,
      })

      expect(() => requiredSchema.parse({ name: 'Test' })).not.toThrow()
      expect(() => requiredSchema.parse({ name: '' })).toThrow()
    })
  })

  describe('extractBackendErrors', () => {
    it('should extract errors from backend response', () => {
      const backendError = {
        errors: {
          Email: ['Email is required'],
          Password: ['Password must be at least 6 characters'],
        },
      }

      const result = extractBackendErrors(backendError)
      expect(result).toEqual({
        email: 'Email is required',
        password: 'Password must be at least 6 characters',
      })
    })

    it('should handle empty errors object', () => {
      const result = extractBackendErrors({})
      expect(result).toEqual({})
    })

    it('should handle null or undefined', () => {
      expect(extractBackendErrors(null)).toEqual({})
      expect(extractBackendErrors(undefined)).toEqual({})
    })
  })
})


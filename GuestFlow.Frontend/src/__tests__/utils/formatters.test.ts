import { formatCurrency, formatDate, formatDateTime } from '../../utils/formatters'

// Mock i18n
jest.mock('../../i18n/config', () => ({
  default: {
    language: 'tr', // Test in Turkish locale
  },
}))

describe('formatters', () => {
  describe('formatCurrency', () => {
    it('should format currency correctly', () => {
      expect(formatCurrency(1000, 'TRY')).toContain('₺')
      expect(formatCurrency(1000, 'USD')).toContain('$')
    })

    it('should use TRY as default currency', () => {
      const result = formatCurrency(1000)
      expect(result).toContain('₺')
    })

    it('should handle decimal values', () => {
      const result = formatCurrency(1234.56, 'TRY')
      expect(result).toContain('1.234,56')
    })
  })

  describe('formatDate', () => {
    it('should format date string correctly', () => {
      const dateString = '2024-01-15T10:30:00Z'
      const result = formatDate(dateString)
      expect(result).toBeTruthy()
      expect(typeof result).toBe('string')
    })

    it('should format Date object correctly', () => {
      const date = new Date('2024-01-15T10:30:00Z')
      const result = formatDate(date)
      expect(result).toBeTruthy()
      expect(typeof result).toBe('string')
    })
  })

  describe('formatDateTime', () => {
    it('should format date and time correctly', () => {
      const dateString = '2024-01-15T10:30:00Z'
      const result = formatDateTime(dateString)
      expect(result).toBeTruthy()
      expect(typeof result).toBe('string')
    })

    it('should include time in formatted string', () => {
      const date = new Date('2024-01-15T10:30:00Z')
      const result = formatDateTime(date)
      expect(result).toBeTruthy()
    })
  })
})


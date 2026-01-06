/**
 * Data export utilities for Excel and CSV
 */

export interface ExportColumn {
  header: string
  key: string
  formatter?: (value: any) => string
}

/**
 * Convert data to CSV format
 */
export const convertToCSV = <T extends Record<string, any>>(
  data: T[],
  columns: ExportColumn[]
): string => {
  if (!data || data.length === 0) return ''

  // Create header row
  const headers = columns.map((col) => col.header).join(',')
  const rows = [headers]

  // Create data rows
  data.forEach((item) => {
    const values = columns.map((col) => {
      const value = item[col.key]
      const formatted = col.formatter ? col.formatter(value) : value
      // Escape commas and quotes in CSV
      const stringValue = String(formatted || '')
      if (stringValue.includes(',') || stringValue.includes('"') || stringValue.includes('\n')) {
        return `"${stringValue.replace(/"/g, '""')}"`
      }
      return stringValue
    })
    rows.push(values.join(','))
  })

  return rows.join('\n')
}

/**
 * Download data as CSV file
 */
export const downloadCSV = <T extends Record<string, any>>(
  data: T[],
  columns: ExportColumn[],
  filename: string = 'export.csv'
) => {
  const csv = convertToCSV(data, columns)
  const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' })
  const link = document.createElement('a')
  const url = URL.createObjectURL(blob)

  link.setAttribute('href', url)
  link.setAttribute('download', filename)
  link.style.visibility = 'hidden'
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  URL.revokeObjectURL(url)
}

/**
 * Convert data to Excel format (TSV for simplicity, can be enhanced with xlsx library)
 */
export const convertToExcel = <T extends Record<string, any>>(
  data: T[],
  columns: ExportColumn[]
): string => {
  // For now, return TSV format which Excel can open
  if (!data || data.length === 0) return ''

  const headers = columns.map((col) => col.header).join('\t')
  const rows = [headers]

  data.forEach((item) => {
    const values = columns.map((col) => {
      const value = item[col.key]
      return col.formatter ? col.formatter(value) : value || ''
    })
    rows.push(values.join('\t'))
  })

  return rows.join('\n')
}

/**
 * Download data as Excel file (TSV format)
 */
export const downloadExcel = <T extends Record<string, any>>(
  data: T[],
  columns: ExportColumn[],
  filename: string = 'export.xls'
) => {
  const excel = convertToExcel(data, columns)
  const blob = new Blob([excel], { type: 'application/vnd.ms-excel;charset=utf-8;' })
  const link = document.createElement('a')
  const url = URL.createObjectURL(blob)

  link.setAttribute('href', url)
  link.setAttribute('download', filename)
  link.style.visibility = 'hidden'
  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)
  URL.revokeObjectURL(url)
}

/**
 * Format date for export
 */
export const formatDateForExport = (date: string | Date): string => {
  if (!date) return ''
  const d = typeof date === 'string' ? new Date(date) : date
  return d.toLocaleDateString('tr-TR')
}

/**
 * Format currency for export
 */
export const formatCurrencyForExport = (value: number, currency: string = 'TRY'): string => {
  if (value === null || value === undefined) return ''
  return new Intl.NumberFormat('tr-TR', {
    style: 'currency',
    currency,
  }).format(value)
}


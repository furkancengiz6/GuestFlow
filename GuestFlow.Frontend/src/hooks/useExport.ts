import { useCallback } from 'react'
import { downloadCSV, downloadExcel, ExportColumn } from '../utils/exportUtils'
import { useNotification } from './useNotification'

/**
 * Hook for data export functionality
 */
export const useExport = () => {
  const notification = useNotification()

  const exportToCSV = useCallback(
    <T extends Record<string, any>>(
      data: T[],
      columns: ExportColumn[],
      filename: string = 'export.csv'
    ) => {
      try {
        if (!data || data.length === 0) {
          notification.showWarning('Dışa aktarılacak veri bulunamadı.')
          return
        }

        downloadCSV(data, columns, filename)
        notification.showSuccess('Veriler CSV formatında dışa aktarıldı.')
      } catch (error) {
        console.error('CSV export error:', error)
        notification.showError('CSV dışa aktarma sırasında bir hata oluştu.')
      }
    },
    [notification]
  )

  const exportToExcel = useCallback(
    <T extends Record<string, any>>(
      data: T[],
      columns: ExportColumn[],
      filename: string = 'export.xls'
    ) => {
      try {
        if (!data || data.length === 0) {
          notification.showWarning('Dışa aktarılacak veri bulunamadı.')
          return
        }

        downloadExcel(data, columns, filename)
        notification.showSuccess('Veriler Excel formatında dışa aktarıldı.')
      } catch (error) {
        console.error('Excel export error:', error)
        notification.showError('Excel dışa aktarma sırasında bir hata oluştu.')
      }
    },
    [notification]
  )

  return {
    exportToCSV,
    exportToExcel,
  }
}


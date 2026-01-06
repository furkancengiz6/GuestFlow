import { useState, useCallback } from 'react'
import { useMutation } from '@tanstack/react-query'
import {
  importService,
  ImportPreviewResponse,
  ImportGuestDto,
  SaveImportedGuestsRequest,
} from '../services/importService'
import { useNotification } from './useNotification'

/**
 * Hook for data import functionality
 */
export const useImport = () => {
  const notification = useNotification()
  const [previewData, setPreviewData] = useState<ImportPreviewResponse | null>(null)
  const [selectedFile, setSelectedFile] = useState<File | null>(null)

  const previewMutation = useMutation({
    mutationFn: async ({ file, format }: { file: File; format: 'excel' | 'csv' }) => {
      if (format === 'excel') {
        return await importService.previewGuestsFromExcel(file)
      } else {
        return await importService.previewGuestsFromCsv(file)
      }
    },
    onSuccess: (data) => {
      setPreviewData(data)
      notification.showSuccess(
        `Dosya okundu. ${data.validRows} geçerli, ${data.invalidRows} geçersiz kayıt bulundu.`
      )
    },
    onError: (error: any) => {
      console.error('Import preview error:', error)
      notification.showError(
        error.response?.data?.message || 'Dosya okunurken bir hata oluştu.'
      )
    },
  })

  const importMutation = useMutation({
    mutationFn: async ({
      file,
      format,
      skipDuplicates,
    }: {
      file: File
      format: 'excel' | 'csv'
      skipDuplicates: boolean
    }) => {
      if (format === 'excel') {
        return await importService.importGuestsFromExcel(file, skipDuplicates)
      } else {
        return await importService.importGuestsFromCsv(file, skipDuplicates)
      }
    },
    onSuccess: (data) => {
      notification.showSuccess(
        `${data.successCount} kayıt başarıyla içe aktarıldı. ${data.skippedCount} kayıt atlandı.`
      )
      setPreviewData(null)
      setSelectedFile(null)
    },
    onError: (error: any) => {
      console.error('Import error:', error)
      notification.showError(
        error.response?.data?.message || 'İçe aktarma sırasında bir hata oluştu.'
      )
    },
  })

  const saveMutation = useMutation({
    mutationFn: async (request: SaveImportedGuestsRequest) => {
      return await importService.saveImportedGuests(request)
    },
    onSuccess: (data) => {
      notification.showSuccess(
        `${data.successCount} kayıt başarıyla kaydedildi. ${data.skippedCount} kayıt atlandı.`
      )
      setPreviewData(null)
      setSelectedFile(null)
    },
    onError: (error: any) => {
      console.error('Save import error:', error)
      notification.showError(
        error.response?.data?.message || 'Kayıt sırasında bir hata oluştu.'
      )
    },
  })

  const previewFile = useCallback(
    (file: File) => {
      setSelectedFile(file)
      const format = file.name.endsWith('.csv') ? 'csv' : 'excel'
      previewMutation.mutate({ file, format })
    },
    [previewMutation]
  )

  const importFile = useCallback(
    (file: File, skipDuplicates: boolean = true) => {
      const format = file.name.endsWith('.csv') ? 'csv' : 'excel'
      importMutation.mutate({ file, format, skipDuplicates })
    },
    [importMutation]
  )

  const savePreview = useCallback(
    (guests: ImportGuestDto[], skipDuplicates: boolean = true) => {
      saveMutation.mutate({ guests, skipDuplicates })
    },
    [saveMutation]
  )

  const clearPreview = useCallback(() => {
    setPreviewData(null)
    setSelectedFile(null)
  }, [])

  return {
    previewFile,
    importFile,
    savePreview,
    clearPreview,
    previewData,
    selectedFile,
    isPreviewing: previewMutation.isPending,
    isImporting: importMutation.isPending,
    isSaving: saveMutation.isPending,
    previewError: previewMutation.error,
    importError: importMutation.error,
    saveError: saveMutation.error,
  }
}


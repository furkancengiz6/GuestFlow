import { useState, useCallback } from 'react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useNotification } from './useNotification'

interface BulkEditOptions<T> {
  updateFn: (ids: number[], data: Partial<T>) => Promise<any>
  queryKey: string[]
  successMessage?: string
  errorMessage?: string
}

/**
 * Hook for bulk edit operations
 */
export const useBulkEdit = <T = any>(options: BulkEditOptions<T>) => {
  const { updateFn, queryKey, successMessage, errorMessage } = options
  const notification = useNotification()
  const queryClient = useQueryClient()
  const [selectedIds, setSelectedIds] = useState<number[]>([])

  const mutation = useMutation({
    mutationFn: (data: Partial<T>) => updateFn(selectedIds, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey })
      notification.showSuccess(successMessage || 'Toplu düzenleme başarılı')
      setSelectedIds([])
    },
    onError: (error: any) => {
      notification.showError(errorMessage || error?.response?.data?.message || 'Toplu düzenleme başarısız')
    },
  })

  const selectId = useCallback((id: number) => {
    setSelectedIds((prev) => (prev.includes(id) ? prev : [...prev, id]))
  }, [])

  const deselectId = useCallback((id: number) => {
    setSelectedIds((prev) => prev.filter((selectedId) => selectedId !== id))
  }, [])

  const toggleId = useCallback((id: number) => {
    setSelectedIds((prev) =>
      prev.includes(id) ? prev.filter((selectedId) => selectedId !== id) : [...prev, id]
    )
  }, [])

  const selectAll = useCallback((ids: number[]) => {
    setSelectedIds(ids)
  }, [])

  const deselectAll = useCallback(() => {
    setSelectedIds([])
  }, [])

  const isSelected = useCallback(
    (id: number) => {
      return selectedIds.includes(id)
    },
    [selectedIds]
  )

  const edit = useCallback(
    (data: Partial<T>) => {
      if (selectedIds.length === 0) {
        notification.showWarning('Lütfen en az bir öğe seçin')
        return
      }
      mutation.mutate(data)
    },
    [selectedIds, mutation, notification]
  )

  return {
    selectedIds,
    selectId,
    deselectId,
    toggleId,
    selectAll,
    deselectAll,
    isSelected,
    edit,
    isEditing: mutation.isPending,
  }
}


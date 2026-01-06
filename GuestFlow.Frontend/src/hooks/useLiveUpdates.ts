import { useEffect, useCallback } from 'react'
import { useSignalR } from './useSignalR'
import { useQueryClient } from '@tanstack/react-query'
import { useNotification } from './useNotification'

interface LiveUpdate {
  entityType: string
  entityId: number
  action: 'created' | 'updated' | 'deleted'
  data?: any
}

/**
 * Hook for live updates via SignalR
 * Automatically invalidates React Query cache when updates are received
 */
export const useLiveUpdates = (entityTypes?: string[]) => {
  const queryClient = useQueryClient()
  const notification = useNotification()

  const handleLiveUpdate = useCallback(
    (update: LiveUpdate) => {
      // Invalidate queries for the updated entity type
      const entityType = update.entityType.toLowerCase()
      queryClient.invalidateQueries({ queryKey: [entityType] })

      // Also invalidate related queries
      if (entityType === 'guest') {
        queryClient.invalidateQueries({ queryKey: ['dashboard'] })
      } else if (entityType === 'transfer' || entityType === 'citytour' || entityType === 'yachttour') {
        queryClient.invalidateQueries({ queryKey: ['dashboard'] })
        queryClient.invalidateQueries({ queryKey: ['revenue'] })
      } else if (entityType === 'invoice') {
        queryClient.invalidateQueries({ queryKey: ['dashboard'] })
        queryClient.invalidateQueries({ queryKey: ['revenue'] })
      }

      // Show notification for important updates
      if (update.action === 'created' || update.action === 'deleted') {
        const actionText = update.action === 'created' ? 'oluşturuldu' : 'silindi'
        notification.showInfo(`${update.entityType} ${actionText}`)
      }
    },
    [queryClient, notification]
  )

  const handleDashboardUpdate = useCallback(
    (_update: any) => {
      // Invalidate all dashboard queries
      queryClient.invalidateQueries({ queryKey: ['dashboard'] })
      queryClient.invalidateQueries({ queryKey: ['quick-stats'] })
      queryClient.invalidateQueries({ queryKey: ['revenue-chart'] })
    },
    [queryClient]
  )

  const { isConnected } = useSignalR({
    onLiveUpdate: handleLiveUpdate,
    onDashboardUpdate: handleDashboardUpdate,
  })

  // Register entity-specific handlers if provided
  useEffect(() => {
    if (!entityTypes || entityTypes.length === 0) {
      return
    }

    entityTypes.forEach((_entityType) => {
      // Note: This would require extending signalRService to support dynamic handlers
      // For now, the generic handler will catch all updates
    })
  }, [entityTypes, handleLiveUpdate])

  return {
    isConnected,
  }
}


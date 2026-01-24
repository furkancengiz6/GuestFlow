// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { communicationService } from '../services/communicationService'
import type {
  UnifiedCommunicationHistory,
  SendMessageRequest,
  SmartNotificationType,
} from '../types/communication'

export const useGuestCommunicationHistory = (
  guestId: number,
  startDate?: string,
  endDate?: string
) => {
  return useQuery<UnifiedCommunicationHistory>({
    queryKey: ['communication', 'history', guestId, startDate, endDate],
    queryFn: () =>
      communicationService.getGuestCommunicationHistory(
        guestId,
        startDate,
        endDate
      ),
    enabled: !!guestId,
  })
}

export const useSendMessage = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({
      guestId,
      data,
    }: {
      guestId: number
      data: SendMessageRequest
    }) => communicationService.sendMessage(guestId, data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['communication', 'history', variables.guestId],
      })
    },
  })
}

export const useSendSmartNotification = () => {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({
      guestId,
      notificationType,
    }: {
      guestId: number
      notificationType: SmartNotificationType
    }) => communicationService.sendSmartNotification(guestId, notificationType),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ['communication', 'history', variables.guestId],
      })
    },
  })
}

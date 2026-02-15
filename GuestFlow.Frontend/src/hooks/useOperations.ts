// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

import { useMutation, useQueryClient } from '@tanstack/react-query'
import { operationsService, AssignDriverRequest as _AssignDriverRequest, RecordPaymentRequest } from '../services/operationsService'
import { useNotification } from './useNotification'

export const useConfirmTransfer = () => {
  const queryClient = useQueryClient()
  const notification = useNotification()

  return useMutation({
    mutationFn: (transferId: number) => operationsService.confirmTransfer(transferId),
    onSuccess: () => {
      notification.showSuccess('Transfer onaylandı')
      queryClient.invalidateQueries({ queryKey: ['dailyOperations'] })
      queryClient.invalidateQueries({ queryKey: ['transfers'] })
    },
    onError: (error: any) => {
      notification.showError(`Hata: ${error.response?.data?.message || 'Transfer onaylanamadı'}`)
    },
  })
}

export const useCancelTransfer = () => {
  const queryClient = useQueryClient()
  const notification = useNotification()

  return useMutation({
    mutationFn: ({ transferId, reason }: { transferId: number; reason?: string }) =>
      operationsService.cancelTransfer(transferId, reason),
    onSuccess: () => {
      notification.showSuccess('Transfer iptal edildi')
      queryClient.invalidateQueries({ queryKey: ['dailyOperations'] })
      queryClient.invalidateQueries({ queryKey: ['transfers'] })
    },
    onError: (error: any) => {
      notification.showError(`Hata: ${error.response?.data?.message || 'Transfer iptal edilemedi'}`)
    },
  })
}

export const useAssignDriver = () => {
  const queryClient = useQueryClient()
  const notification = useNotification()

  return useMutation({
    mutationFn: ({ transferId, personnelId }: { transferId: number; personnelId: number }) =>
      operationsService.assignDriver(transferId, personnelId),
    onSuccess: () => {
      notification.showSuccess('Şoför atandı')
      queryClient.invalidateQueries({ queryKey: ['dailyOperations'] })
      queryClient.invalidateQueries({ queryKey: ['transfers'] })
    },
    onError: (error: any) => {
      notification.showError(`Hata: ${error.response?.data?.message || 'Şoför atanamadı'}`)
    },
  })
}

export const useRecordPayment = () => {
  const queryClient = useQueryClient()
  const notification = useNotification()

  return useMutation({
    mutationFn: ({
      serviceType,
      serviceId,
      request,
    }: {
      serviceType: 'Transfer' | 'CityTour' | 'YachtTour'
      serviceId: number
      request: RecordPaymentRequest
    }) => operationsService.recordPayment(serviceType, serviceId, request),
    onSuccess: () => {
      notification.showSuccess('Ödeme kaydedildi')
      queryClient.invalidateQueries({ queryKey: ['dailyOperations'] })
      queryClient.invalidateQueries({ queryKey: ['transfers'] })
      queryClient.invalidateQueries({ queryKey: ['payments'] })
    },
    onError: (error: any) => {
      notification.showError(`Hata: ${error.response?.data?.message || 'Ödeme kaydedilemedi'}`)
    },
  })
}

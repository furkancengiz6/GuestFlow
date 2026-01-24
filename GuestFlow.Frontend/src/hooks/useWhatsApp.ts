// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { whatsAppService } from '../services/whatsAppService'
import { useSnackbar } from 'notistack'
import type {
  SendWhatsApp,
  WhatsAppHistory,
  WhatsAppStatistics,
} from '../types/whatsApp'

export const useSendWhatsApp = () => {
  const queryClient = useQueryClient()
  const { enqueueSnackbar } = useSnackbar()

  return useMutation({
    mutationFn: (data: SendWhatsApp) => whatsAppService.sendWhatsApp(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['whatsapp', 'history'] })
      queryClient.invalidateQueries({ queryKey: ['whatsapp', 'statistics'] })
      enqueueSnackbar('WhatsApp mesajı başarıyla gönderildi', {
        variant: 'success',
      })
    },
    onError: (error: any) => {
      enqueueSnackbar(
        error?.response?.data?.message || 'WhatsApp mesajı gönderilemedi',
        { variant: 'error' }
      )
    },
  })
}

export const useWhatsAppHistory = (params: {
  pageNumber?: number
  pageSize?: number
  guestId?: number
  status?: string
  startDate?: string
  endDate?: string
  sortBy?: string
  sortOrder?: string
}) => {
  return useQuery({
    queryKey: ['whatsapp', 'history', params],
    queryFn: () => whatsAppService.getWhatsAppHistory(params),
  })
}

export const useWhatsAppHistoryByGuest = (guestId: number) => {
  return useQuery({
    queryKey: ['whatsapp', 'history', 'guest', guestId],
    queryFn: () => whatsAppService.getWhatsAppHistoryByGuest(guestId),
    enabled: !!guestId,
  })
}

export const useWhatsAppStatistics = (params?: {
  startDate?: string
  endDate?: string
}) => {
  return useQuery({
    queryKey: ['whatsapp', 'statistics', params],
    queryFn: () => whatsAppService.getWhatsAppStatistics(params),
  })
}

export const useSendTransferReminder = () => {
  const queryClient = useQueryClient()
  const { enqueueSnackbar } = useSnackbar()

  return useMutation({
    mutationFn: ({
      transferId,
      hoursBefore,
    }: {
      transferId: number
      hoursBefore?: number
    }) =>
      whatsAppService.sendTransferReminder(
        transferId,
        hoursBefore || 24
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['whatsapp', 'history'] })
      enqueueSnackbar('Transfer hatırlatma WhatsApp mesajı gönderildi', {
        variant: 'success',
      })
    },
    onError: (error: any) => {
      enqueueSnackbar(
        error?.response?.data?.message ||
          'Transfer hatırlatma WhatsApp mesajı gönderilemedi',
        { variant: 'error' }
      )
    },
  })
}

export const useSendTourReminder = () => {
  const queryClient = useQueryClient()
  const { enqueueSnackbar } = useSnackbar()

  return useMutation({
    mutationFn: ({
      tourType,
      tourId,
      hoursBefore,
    }: {
      tourType: string
      tourId: number
      hoursBefore?: number
    }) =>
      whatsAppService.sendTourReminder(
        tourType,
        tourId,
        hoursBefore || 24
      ),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['whatsapp', 'history'] })
      enqueueSnackbar('Tur hatırlatma WhatsApp mesajı gönderildi', {
        variant: 'success',
      })
    },
    onError: (error: any) => {
      enqueueSnackbar(
        error?.response?.data?.message ||
          'Tur hatırlatma WhatsApp mesajı gönderilemedi',
        { variant: 'error' }
      )
    },
  })
}

export const useSendReservationConfirmation = () => {
  const queryClient = useQueryClient()
  const { enqueueSnackbar } = useSnackbar()

  return useMutation({
    mutationFn: (reservationId: number) =>
      whatsAppService.sendReservationConfirmation(reservationId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['whatsapp', 'history'] })
      enqueueSnackbar('Rezervasyon onay WhatsApp mesajı gönderildi', {
        variant: 'success',
      })
    },
    onError: (error: any) => {
      enqueueSnackbar(
        error?.response?.data?.message ||
          'Rezervasyon onay WhatsApp mesajı gönderilemedi',
        { variant: 'error' }
      )
    },
  })
}

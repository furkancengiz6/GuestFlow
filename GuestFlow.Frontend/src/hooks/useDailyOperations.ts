// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

import { useQuery } from '@tanstack/react-query'
import { dailyOperationsService } from '../services/dailyOperationsService'
import { DailyOperations } from '../types/dailyOperations'

export const useDailyOperations = (date?: string) => {
  return useQuery<DailyOperations>({
    queryKey: ['dailyOperations', date],
    queryFn: () => dailyOperationsService.getDailyOperations(date),
    refetchInterval: 30000, // 30 saniyede bir otomatik yenile
    staleTime: 10000, // 10 saniye boyunca cache'den kullan
  })
}

// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

import { useQuery } from '@tanstack/react-query'
import { analyticsService } from '../services/analyticsService'
import type {
  RealTimeKpi,
} from '../types/analytics'

export const useRealTimeKpis = (date?: string) => {
  return useQuery<RealTimeKpi>({
    queryKey: ['analytics', 'kpis', 'realtime', date],
    queryFn: () => analyticsService.getRealTimeKpis(date),
    refetchInterval: 60000, // Her 1 dakikada bir yenile
  })
}

export const useRevenueTrend = (params: {
  period?: string
  startDate?: string
  endDate?: string
  includeComparison?: boolean
}) => {
  return useQuery({
    queryKey: ['analytics', 'revenue', 'trend', params],
    queryFn: () => analyticsService.getRevenueTrend(params),
  })
}

export const useSeasonalComparison = (year?: number) => {
  return useQuery({
    queryKey: ['analytics', 'seasonal', 'comparison', year],
    queryFn: () => analyticsService.getSeasonalComparison(year),
    enabled: false, // Henüz implement edilmedi, manuel çağrılacak
  })
}

export const useYearlyGrowth = (year?: number) => {
  return useQuery({
    queryKey: ['analytics', 'growth', 'yearly', year],
    queryFn: () => analyticsService.getYearlyGrowth(year),
    enabled: false, // Henüz implement edilmedi
  })
}

export const useGuestSegmentation = (params?: {
  startDate?: string
  endDate?: string
}) => {
  return useQuery({
    queryKey: ['analytics', 'segmentation', 'guests', params],
    queryFn: () => analyticsService.getGuestSegmentation(params),
    enabled: false, // Henüz implement edilmedi
  })
}

export const useServiceDistribution = (params?: {
  startDate?: string
  endDate?: string
}) => {
  return useQuery({
    queryKey: ['analytics', 'distribution', 'services', params],
    queryFn: () => analyticsService.getServiceDistribution(params),
    enabled: false, // Henüz implement edilmedi
  })
}

export const useCityPerformance = (params?: {
  startDate?: string
  endDate?: string
}) => {
  return useQuery({
    queryKey: ['analytics', 'performance', 'cities', params],
    queryFn: () => analyticsService.getCityPerformance(params),
    enabled: false, // Henüz implement edilmedi
  })
}

export const useSupplierProfitability = (params?: {
  startDate?: string
  endDate?: string
}) => {
  return useQuery({
    queryKey: ['analytics', 'profitability', 'suppliers', params],
    queryFn: () => analyticsService.getSupplierProfitability(params),
    enabled: false, // Henüz implement edilmedi
  })
}

export const useRevenueForecast = (monthsAhead: number = 1) => {
  return useQuery({
    queryKey: ['analytics', 'forecast', 'revenue', monthsAhead],
    queryFn: () => analyticsService.getRevenueForecast(monthsAhead),
    enabled: false, // Henüz implement edilmedi
  })
}

export const useDemandForecast = (params: {
  serviceType: string
  startDate?: string
  endDate?: string
}) => {
  return useQuery({
    queryKey: ['analytics', 'forecast', 'demand', params],
    queryFn: () => analyticsService.getDemandForecast(params),
    enabled: false, // Henüz implement edilmedi
  })
}

export const useOptimalPriceSuggestions = (params: {
  serviceType: string
  targetDate?: string
}) => {
  return useQuery({
    queryKey: ['analytics', 'pricing', 'optimal', params],
    queryFn: () => analyticsService.getOptimalPriceSuggestions(params),
    enabled: false, // Henüz implement edilmedi
  })
}

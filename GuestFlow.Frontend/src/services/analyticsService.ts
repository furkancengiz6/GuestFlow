// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

import apiClient from './api'
import type {
  RealTimeKpi,
  RevenueTrend,
  SeasonalComparison,
  YearlyGrowth,
  GuestSegmentation,
  ServiceDistribution,
  CityPerformance,
  SupplierProfitability,
  RevenueForecast,
  DemandForecast,
  OptimalPrice,
} from '../types/analytics'

export const analyticsService = {
  /**
   * Get real-time KPIs
   */
  getRealTimeKpis: async (date?: string): Promise<RealTimeKpi> => {
    const response = await apiClient.get<{
      success: boolean
      data: RealTimeKpi
    }>('/api/v1.0/Analytics/kpis/realtime', {
      params: date ? { date } : {},
    })
    return response.data.data
  },

  /**
   * Get revenue trend
   */
  getRevenueTrend: async (params: {
    period?: string
    startDate?: string
    endDate?: string
    includeComparison?: boolean
  }): Promise<RevenueTrend> => {
    const response = await apiClient.get<{
      success: boolean
      data: RevenueTrend
    }>('/api/v1.0/Analytics/revenue/trend', { params })
    return response.data.data
  },

  /**
   * Get seasonal comparison
   */
  getSeasonalComparison: async (year?: number): Promise<SeasonalComparison> => {
    const response = await apiClient.get<{
      success: boolean
      data: SeasonalComparison
    }>('/api/v1.0/Analytics/seasonal/comparison', {
      params: year ? { year } : {},
    })
    return response.data.data
  },

  /**
   * Get yearly growth
   */
  getYearlyGrowth: async (year?: number): Promise<YearlyGrowth> => {
    const response = await apiClient.get<{
      success: boolean
      data: YearlyGrowth
    }>('/api/v1.0/Analytics/growth/yearly', {
      params: year ? { year } : {},
    })
    return response.data.data
  },

  /**
   * Get guest segmentation
   */
  getGuestSegmentation: async (params?: {
    startDate?: string
    endDate?: string
  }): Promise<GuestSegmentation> => {
    const response = await apiClient.get<{
      success: boolean
      data: GuestSegmentation
    }>('/api/v1.0/Analytics/segmentation/guests', { params })
    return response.data.data
  },

  /**
   * Get service distribution
   */
  getServiceDistribution: async (params?: {
    startDate?: string
    endDate?: string
  }): Promise<ServiceDistribution> => {
    const response = await apiClient.get<{
      success: boolean
      data: ServiceDistribution
    }>('/api/v1.0/Analytics/distribution/services', { params })
    return response.data.data
  },

  /**
   * Get city performance
   */
  getCityPerformance: async (params?: {
    startDate?: string
    endDate?: string
  }): Promise<CityPerformance> => {
    const response = await apiClient.get<{
      success: boolean
      data: CityPerformance
    }>('/api/v1.0/Analytics/performance/cities', { params })
    return response.data.data
  },

  /**
   * Get supplier profitability
   */
  getSupplierProfitability: async (params?: {
    startDate?: string
    endDate?: string
  }): Promise<SupplierProfitability> => {
    const response = await apiClient.get<{
      success: boolean
      data: SupplierProfitability
    }>('/api/v1.0/Analytics/profitability/suppliers', { params })
    return response.data.data
  },

  /**
   * Get revenue forecast
   */
  getRevenueForecast: async (monthsAhead: number = 1): Promise<RevenueForecast> => {
    const response = await apiClient.get<{
      success: boolean
      data: RevenueForecast
    }>('/api/v1.0/Analytics/forecast/revenue', {
      params: { monthsAhead },
    })
    return response.data.data
  },

  /**
   * Get demand forecast
   */
  getDemandForecast: async (params: {
    serviceType: string
    startDate?: string
    endDate?: string
  }): Promise<DemandForecast> => {
    const response = await apiClient.get<{
      success: boolean
      data: DemandForecast
    }>('/api/v1.0/Analytics/forecast/demand', { params })
    return response.data.data
  },

  /**
   * Get optimal price suggestions
   */
  getOptimalPriceSuggestions: async (params: {
    serviceType: string
    targetDate?: string
  }): Promise<OptimalPrice> => {
    const response = await apiClient.get<{
      success: boolean
      data: OptimalPrice
    }>('/api/v1.0/Analytics/pricing/optimal', { params })
    return response.data.data
  },
}

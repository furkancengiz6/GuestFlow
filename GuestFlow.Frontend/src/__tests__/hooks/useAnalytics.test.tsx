// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

import { renderHook, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { useRealTimeKpis, useRevenueTrend } from '../../hooks/useAnalytics'
import * as analyticsService from '../../services/analyticsService'

// Mock the service
jest.mock('../../services/analyticsService')

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  })
  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  )
}

describe('useAnalytics hooks', () => {
  beforeEach(() => {
    jest.clearAllMocks()
  })

  describe('useRealTimeKpis', () => {
    it('should fetch real-time KPIs', async () => {
      const mockKpis = {
        todayRevenue: 15000.50,
        thisMonthRevenue: 450000.75,
        thisMonthNetProfit: 135000.25,
        averageRevenuePerService: 2500.00,
        todayServiceCount: 6,
        thisMonthServiceCount: 180,
        mostProfitableServices: [],
        revenueGrowthRate: 15.5,
        profitMargin: 30.0,
      }

        ; (analyticsService.analyticsService.getRealTimeKpis as jest.Mock).mockResolvedValue(mockKpis)

      const { result } = renderHook(() => useRealTimeKpis(), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockKpis)
      expect(analyticsService.analyticsService.getRealTimeKpis).toHaveBeenCalledTimes(1)
    })

    it('should handle errors', async () => {
      const error = new Error('Failed to fetch')
        ; (analyticsService.analyticsService.getRealTimeKpis as jest.Mock).mockRejectedValue(error)

      const { result } = renderHook(() => useRealTimeKpis(), {
        wrapper: createWrapper(),
      })

      await waitFor(() => {
        expect(result.current.isError).toBe(true)
      })

      expect(result.current.error).toEqual(error)
    })
  })

  describe('useRevenueTrend', () => {
    it('should fetch revenue trend', async () => {
      const mockTrend = {
        period: 'daily',
        dataPoints: [
          {
            label: '01.01',
            date: '2025-01-01T00:00:00Z',
            revenue: 10000,
            cost: 6000,
            netProfit: 4000,
            serviceCount: 5,
          },
        ],
        totalRevenue: 10000,
        averageRevenue: 10000,
        growthRate: 12.5,
      }

        ; (analyticsService.analyticsService.getRevenueTrend as jest.Mock).mockResolvedValue(mockTrend)

      const { result } = renderHook(
        () => useRevenueTrend({ period: 'daily', startDate: '2025-01-01', endDate: '2025-01-31' }),
        {
          wrapper: createWrapper(),
        }
      )

      await waitFor(() => {
        expect(result.current.isSuccess).toBe(true)
      })

      expect(result.current.data).toEqual(mockTrend)
      expect(analyticsService.analyticsService.getRevenueTrend).toHaveBeenCalledWith({
        period: 'daily',
        startDate: '2025-01-01',
        endDate: '2025-01-31',
      })
    })
  })
})

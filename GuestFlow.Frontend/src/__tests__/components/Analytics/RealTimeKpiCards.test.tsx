// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

import { render, screen, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import RealTimeKpiCards from '../../../components/Analytics/RealTimeKpiCards'
import * as analyticsHooks from '../../../hooks/useAnalytics'

// Mock the hook
jest.mock('../../../hooks/useAnalytics')

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

describe('RealTimeKpiCards', () => {
  const mockKpis = {
    todayRevenue: 15000.50,
    thisMonthRevenue: 450000.75,
    thisMonthNetProfit: 135000.25,
    averageRevenuePerService: 2500.00,
    todayServiceCount: 6,
    thisMonthServiceCount: 180,
    mostProfitableServices: [
      {
        serviceType: 'Transfer',
        totalRevenue: 200000,
        totalCost: 120000,
        netProfit: 80000,
        profitMargin: 40.0,
        serviceCount: 100,
      },
      {
        serviceType: 'CityTour',
        totalRevenue: 150000,
        totalCost: 90000,
        netProfit: 60000,
        profitMargin: 40.0,
        serviceCount: 50,
      },
    ],
    revenueGrowthRate: 15.5,
    profitMargin: 30.0,
  }

  beforeEach(() => {
    jest.clearAllMocks()
  })

  it('should render loading state', () => {
    ; (analyticsHooks.useRealTimeKpis as jest.Mock).mockReturnValue({
      data: undefined,
      isLoading: true,
      error: null,
    })

    render(<RealTimeKpiCards />, { wrapper: createWrapper() })

    // Loading skeleton should be visible
    const loadingElement = screen.queryByTestId('skeleton-loader')
    expect(loadingElement).toBeTruthy()
  })

  it('should render error state', () => {
    ; (analyticsHooks.useRealTimeKpis as jest.Mock).mockReturnValue({
      data: undefined,
      isLoading: false,
      error: new Error('Failed to load'),
    })

    render(<RealTimeKpiCards />, { wrapper: createWrapper() })

    expect(screen.getByText("KPI'lar yüklenemedi")).toBeInTheDocument()
  })

  it('should render KPI cards with data', async () => {
    ; (analyticsHooks.useRealTimeKpis as jest.Mock).mockReturnValue({
      data: mockKpis,
      isLoading: false,
      error: null,
    })

    render(<RealTimeKpiCards />, { wrapper: createWrapper() })

    // Check if KPI cards are rendered
    await waitFor(() => {
      expect(screen.getByTestId('kpi-card-today-revenue')).toBeInTheDocument()
      expect(screen.getByTestId('kpi-card-month-revenue')).toBeInTheDocument()
      expect(screen.getByTestId('kpi-card-net-profit')).toBeInTheDocument()
    })

    // Check if revenue values are displayed
    expect(screen.getByTestId('kpi-revenue-today')).toBeInTheDocument()
    expect(screen.getByTestId('kpi-revenue-month')).toBeInTheDocument()
    expect(screen.getByTestId('kpi-net-profit')).toBeInTheDocument()
  })

  it('should display growth rate indicator', async () => {
    ; (analyticsHooks.useRealTimeKpis as jest.Mock).mockReturnValue({
      data: mockKpis,
      isLoading: false,
      error: null,
    })

    render(<RealTimeKpiCards />, { wrapper: createWrapper() })

    await waitFor(() => {
      const growthIndicator = screen.getByTestId('kpi-growth-rate')
      expect(growthIndicator).toBeInTheDocument()
      expect(growthIndicator).toHaveTextContent('+15.5%')
    })
  })

  it('should display most profitable services', async () => {
    ; (analyticsHooks.useRealTimeKpis as jest.Mock).mockReturnValue({
      data: mockKpis,
      isLoading: false,
      error: null,
    })

    render(<RealTimeKpiCards />, { wrapper: createWrapper() })

    await waitFor(() => {
      expect(screen.getByTestId('most-profitable-services')).toBeInTheDocument()
      expect(screen.getByText('Transfer')).toBeInTheDocument()
      expect(screen.getByText('CityTour')).toBeInTheDocument()
    })
  })

  it('should not display most profitable services when empty', async () => {
    const kpisWithoutServices = {
      ...mockKpis,
      mostProfitableServices: [],
    }

      ; (analyticsHooks.useRealTimeKpis as jest.Mock).mockReturnValue({
        data: kpisWithoutServices,
        isLoading: false,
        error: null,
      })

    render(<RealTimeKpiCards />, { wrapper: createWrapper() })

    await waitFor(() => {
      expect(screen.queryByTestId('most-profitable-services')).not.toBeInTheDocument()
    })
  })
})

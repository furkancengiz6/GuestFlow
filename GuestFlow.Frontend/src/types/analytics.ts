// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

export interface RealTimeKpi {
  todayRevenue: number
  thisMonthRevenue: number
  thisMonthNetProfit: number
  averageRevenuePerService: number
  todayServiceCount: number
  thisMonthServiceCount: number
  mostProfitableServices: MostProfitableService[]
  revenueGrowthRate: number
  profitMargin: number
}

export interface MostProfitableService {
  serviceType: string
  totalRevenue: number
  totalCost: number
  netProfit: number
  profitMargin: number
  serviceCount: number
}

export interface RevenueTrend {
  period: string
  dataPoints: RevenueTrendPoint[]
  comparisonDataPoints?: RevenueTrendPoint[]
  totalRevenue: number
  averageRevenue: number
  growthRate: number
}

export interface RevenueTrendPoint {
  label: string
  date: string
  revenue: number
  cost: number
  netProfit: number
  serviceCount: number
}

export interface SeasonalComparison {
  year: number
  seasonalData: SeasonalData[]
  peakSeason: string
  lowSeason: string
}

export interface SeasonalData {
  season: string
  revenue: number
  cost: number
  netProfit: number
  serviceCount: number
  averageRevenuePerService: number
}

export interface YearlyGrowth {
  year: number
  revenue: number
  revenueGrowthRate: number
  serviceCountGrowthRate: number
  profitGrowthRate: number
  yearlyData: YearlyGrowthPoint[]
}

export interface YearlyGrowthPoint {
  year: number
  revenue: number
  cost: number
  netProfit: number
  serviceCount: number
  growthRate?: number
}

export interface GuestSegmentation {
  segments: GuestSegment[]
  totalGuests: number
}

export interface GuestSegment {
  segmentType: string
  guestCount: number
  totalRevenue: number
  averageRevenuePerGuest: number
  percentage: number
}

export interface ServiceDistribution {
  services: ServiceDistributionItem[]
  totalServices: number
}

export interface ServiceDistributionItem {
  serviceType: string
  count: number
  revenue: number
  percentage: number
  averageRevenue: number
}

export interface CityPerformance {
  cities: CityPerformanceItem[]
}

export interface CityPerformanceItem {
  cityId: number
  cityName: string
  serviceCount: number
  revenue: number
  cost: number
  netProfit: number
  profitMargin: number
  averageRevenuePerService: number
}

export interface SupplierProfitability {
  suppliers: SupplierProfitabilityItem[]
}

export interface SupplierProfitabilityItem {
  supplierId: number
  supplierName: string
  serviceCount: number
  totalRevenue: number
  totalCost: number
  netProfit: number
  profitMargin: number
  averageCostPerService: number
}

export interface RevenueForecast {
  forecastDate: string
  monthsAhead: number
  forecastPoints: ForecastPoint[]
  predictedTotalRevenue: number
  confidenceLevel: number
  method: string
}

export interface ForecastPoint {
  date: string
  predictedRevenue: number
  minRevenue?: number
  maxRevenue?: number
}

export interface DemandForecast {
  serviceType: string
  forecastPoints: DemandForecastPoint[]
  predictedTotalDemand: number
  confidenceLevel: number
}

export interface DemandForecastPoint {
  date: string
  predictedDemand: number
  minDemand?: number
  maxDemand?: number
}

export interface OptimalPrice {
  serviceType: string
  targetDate: string
  currentPrice: number
  recommendedPrice: number
  minPrice: number
  maxPrice: number
  reason: string
  expectedDemand: number
  expectedRevenue: number
  scenarios: PriceScenario[]
}

export interface PriceScenario {
  price: number
  expectedDemand: number
  expectedRevenue: number
  profitMargin: number
}

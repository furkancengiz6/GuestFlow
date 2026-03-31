/**
 * Copyright (c) 2025 Furkan Cengiz
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

import apiClient from './api'

export interface CommercialDashboardSummary {
    totalRevenue: number
    totalBookings: number
    averageBookingValue: number
    conversionRate: number
    revenueGrowth: number
    occupancyRate: number
}

export interface UpsellOpportunity {
    id: number
    guestId: number
    guestName: string
    serviceType: string
    recommendedService: string
    confidenceScore: number
    estimatedRevenue: number
    reason: string
}

export interface ServiceFrictionReport {
    department: string
    frictionScore: number
    averageResponseTime: number
    pendingRequests: number
    issuesIdentified: string[]
}

export interface LoyaltyIntelligence {
    guestId: number
    guestName: string
    loyaltyTier: string
    churnRisk: number
    lifetimeValue: number
    nextBestAction: string
}

export const commercialDashboardService = {
    getExecutiveSummary: async (): Promise<CommercialDashboardSummary> => {
        const response = await apiClient.get('/commercial-dashboard/summary')
        return response.data.data || response.data
    },

    getUpsellOpportunities: async (): Promise<UpsellOpportunity[]> => {
        const response = await apiClient.get('/commercial-dashboard/upsell-opportunities')
        return response.data.data || response.data
    },

    getFrictionReport: async (): Promise<ServiceFrictionReport[]> => {
        const response = await apiClient.get('/commercial-dashboard/friction-report')
        return response.data.data || response.data
    },

    getLoyaltyInsights: async (): Promise<LoyaltyIntelligence[]> => {
        const response = await apiClient.get('/commercial-dashboard/loyalty-insights')
        return response.data.data || response.data
    },

    getBundledOpportunities: async (): Promise<UpsellOpportunity[]> => {
        const response = await apiClient.get('/commercial-dashboard/ai-bundled-opportunities')
        return response.data.data || response.data
    },

    getSustainableBundles: async (): Promise<UpsellOpportunity[]> => {
        const response = await apiClient.get('/commercial-dashboard/sustainable-bundles')
        return response.data.data || response.data
    },
}

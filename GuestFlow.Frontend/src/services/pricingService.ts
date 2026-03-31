/**
 * Copyright (c) 2025 Furkan Cengiz
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

import apiClient from './api'

export interface PricingRule {
    id: number
    name: string
    ruleType: string
    adjustmentType: string
    adjustmentValue: number
    priority: number
    isActive: boolean
    startDate?: string
    endDate?: string
    conditions?: Record<string, any>
}

export interface PricingIntelligenceResult {
    date: string
    forecastedOccupancy: number
    baseRate: number
    dynamicRate: number
    isStopSell: boolean
    appliedRules: string[]
    ruleDetails: AppliedRuleDetail[]
}

export interface AppliedRuleDetail {
    ruleName: string
    ruleType: string
    adjustmentType: string
    adjustmentValue: number
    resultingRate: number
}

export const pricingService = {
    getRules: async (): Promise<PricingRule[]> => {
        const response = await apiClient.get('/Pricing/rules')
        return response.data.data || response.data
    },

    createRule: async (rule: Partial<PricingRule>): Promise<PricingRule> => {
        const response = await apiClient.post('/Pricing/rules', rule)
        return response.data.data || response.data
    },

    calculateTest: async (params: { roomTypeId: number; baseRate: number; date: string }): Promise<any> => {
        const response = await apiClient.post('/Pricing/calculate-test', params)
        return response.data.data || response.data
    },

    getPricingIntelligence: async (roomTypeId: number, startDate: string, endDate: string): Promise<PricingIntelligenceResult[]> => {
        const response = await apiClient.get(`/Pricing/intelligence?roomTypeId=${roomTypeId}&startDate=${startDate}&endDate=${endDate}`)
        return response.data.data || response.data
    },
}

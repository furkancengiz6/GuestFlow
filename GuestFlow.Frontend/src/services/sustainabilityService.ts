/**
 * Copyright (c) 2025 Furkan Cengiz
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

import apiClient from './api'

export interface SustainabilityActionRequest {
    guestId: number
    actionType: string
    notes?: string
}

export interface AIRewardRecommendation {
    rewardType: string
    description: string
    scoreRequired: number
    confidence: number
}

export interface GuestSustainabilityScore {
    guestId: number
    score: number
    level: string
    recentActions: string[]
}

export const sustainabilityService = {
    recordAction: async (request: SustainabilityActionRequest): Promise<{ success: boolean; totalScore: number }> => {
        const response = await apiClient.post('/Sustainability/record-action', request)
        return response.data.data || response.data
    },

    getRewardRecommendation: async (guestId: number): Promise<AIRewardRecommendation> => {
        const response = await apiClient.get(`/Sustainability/recommend-reward/${guestId}`)
        return response.data.data || response.data
    },

    getGuestScore: async (guestId: number): Promise<GuestSustainabilityScore> => {
        const response = await apiClient.get(`/Sustainability/guest-score/${guestId}`)
        return response.data.data || response.data
    },
}

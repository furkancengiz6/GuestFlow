/**
 * Copyright (c) 2025 Furkan Cengiz
 * Licensed under the MIT License. See LICENSE file in the project root for full license information.
 */

import apiClient from './api'

export interface FeatureFlag {
    name: string
    isEnabled: boolean
    description?: string
    environment?: string
    lastModified?: string
}

export const featureFlagService = {
    getAll: async (): Promise<FeatureFlag[]> => {
        const response = await apiClient.get('/FeatureFlags')
        return response.data.data || response.data
    },

    getByName: async (name: string): Promise<FeatureFlag> => {
        const response = await apiClient.get(`/FeatureFlags/${name}`)
        return response.data.data || response.data
    },

    checkFeature: async (name: string, environment?: string): Promise<boolean> => {
        const params = environment ? { environment } : {}
        const response = await apiClient.get(`/FeatureFlags/check/${name}`, { params })
        return response.data.data || response.data
    },

    upsert: async (flag: FeatureFlag): Promise<FeatureFlag> => {
        const response = await apiClient.post('/FeatureFlags', flag)
        return response.data.data || response.data
    },

    enable: async (name: string): Promise<boolean> => {
        const response = await apiClient.post(`/FeatureFlags/${name}/enable`)
        return response.data.data || response.data
    },

    disable: async (name: string): Promise<boolean> => {
        const response = await apiClient.post(`/FeatureFlags/${name}/disable`)
        return response.data.data || response.data
    },

    delete: async (name: string): Promise<boolean> => {
        const response = await apiClient.delete(`/FeatureFlags/${name}`)
        return response.data.data || response.data
    },
}

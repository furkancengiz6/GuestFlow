// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

import apiClient from './api'
import type { MapView, MapServiceLocation, MapLocation, MapFilter } from '../types/map'

export const mapService = {
  /**
   * Get map view with all service locations
   */
  getMapView: async (filter?: MapFilter): Promise<MapView> => {
    const response = await apiClient.post<{
      success: boolean
      data: MapView
      message?: string
    }>('/api/v1.0/Map/view', filter || {})
    return response.data.data
  },

  /**
   * Get service location by ID and type
   */
  getServiceLocation: async (
    serviceId: number,
    serviceType: string
  ): Promise<MapServiceLocation> => {
    const response = await apiClient.get<{
      success: boolean
      data: MapServiceLocation
      message?: string
    }>(`/api/v1.0/Map/service/${serviceId}/${serviceType}`)
    return response.data.data
  },

  /**
   * Geocode an address to get coordinates
   */
  geocodeAddress: async (
    address: string,
    cityName?: string
  ): Promise<MapLocation> => {
    const params = cityName ? { address, cityName } : { address }
    const response = await apiClient.get<{
      success: boolean
      data: MapLocation
      message?: string
    }>('/api/v1.0/Map/geocode', { params })
    return response.data.data
  },
}

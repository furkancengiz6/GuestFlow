// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

export interface MapServiceLocation {
  serviceId: number
  serviceType: string // Transfer, CityTour, YachtTour
  serviceName: string
  serviceDate: string
  status: string
  pickupLocation?: MapLocation
  dropoffLocation?: MapLocation
  routePoints?: MapLocation[]
  guestId: number
  guestName: string
  roomNumber?: string
  personnelId?: number
  personnelName?: string
  isUrgent: boolean
  isDelayed: boolean
  colorCode?: string // green, yellow, red, gray
  amount?: number
  currency?: string
  notes?: string
}

export interface MapLocation {
  latitude: number
  longitude: number
  address?: string
  cityName?: string
  label?: string // "Pickup", "Dropoff", "Hotel", etc.
}

export interface MapView {
  date: string
  services: MapServiceLocation[]
  bounds?: MapBounds
  statistics: MapStatistics
}

export interface MapBounds {
  north: number
  south: number
  east: number
  west: number
}

export interface MapStatistics {
  totalServices: number
  confirmedServices: number
  inProgressServices: number
  completedServices: number
  urgentServices: number
  delayedServices: number
}

export interface MapFilter {
  startDate?: string
  endDate?: string
  serviceTypes?: string[] // Transfer, CityTour, YachtTour
  statuses?: string[] // Confirmed, InProgress, Completed
  cityId?: number
  personnelId?: number
  showUrgentOnly?: boolean
  showDelayedOnly?: boolean
}

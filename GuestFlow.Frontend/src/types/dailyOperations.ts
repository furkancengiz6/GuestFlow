// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

export interface DailyOperations {
  date: string
  todayServices: ServiceOperation[]
  upcomingServices: ServiceOperation[]
  riskFlags: RiskFlag[]
  quickStats: DailyOperationsQuickStats
}

export interface ServiceOperation {
  serviceId: number
  serviceType: 'Transfer' | 'CityTour' | 'YachtTour'
  serviceTime: string
  guestId: number
  guestName: string
  guestCode: string
  roomNumber: string
  location: string
  cityName?: string
  status: string
  assignedPersonnelId?: number
  assignedPersonnelName?: string
  amount: number
  currency: string
  isUrgent: boolean
  isPaid: boolean
  notes?: string
}

export interface RiskFlag {
  type: RiskFlagType
  severity: RiskFlagSeverity
  title: string
  description: string
  serviceId: number
  serviceType: string
  createdDate: string
}

export enum RiskFlagType {
  OverduePayment = 1,
  UnpaidService = 2,
  UnassignedDriver = 3,
  UrgentUnconfirmed = 4,
  ConflictingReservation = 5,
  MissingGuestInfo = 6,
}

export enum RiskFlagSeverity {
  Low = 1,
  Medium = 2,
  High = 3,
  Critical = 4,
}

export interface DailyOperationsQuickStats {
  todayServiceCount: number
  upcomingServiceCount: number
  urgentServiceCount: number
  unassignedDriverCount: number
  unpaidServiceCount: number
  overduePaymentCount: number
}

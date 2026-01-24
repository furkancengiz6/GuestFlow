export interface ConciergeCheckInOut {
  items: CheckInOutItem[]
  totalCount: number
  date: string
}

export interface CheckInOutItem {
  guestId: number
  guestName: string
  guestCode: string
  roomNumber?: string
  roomType?: string
  checkInDate?: string
  checkOutDate?: string
  numberOfGuests?: number
  specialRequests?: string
  notes?: string
  source: 'GuestFlow' | 'PMS'
  pmsReservationId?: string
  pmsProviderName?: string
  isVIP: boolean
  email?: string
  phoneNumber?: string
}

export interface ActiveGuest {
  guestId: number
  guestName: string
  guestCode: string
  roomNumber?: string
  roomType?: string
  checkInDate?: string
  checkOutDate?: string
  numberOfNights?: number
  email?: string
  phoneNumber?: string
  isVIP: boolean
  source: 'GuestFlow' | 'PMS'
  pmsReservationId?: string
  pmsProviderName?: string
  upcomingServices: UpcomingServiceItem[]
}

export interface UnifiedGuestProfile {
  guestId: number
  guestName: string
  guestCode: string
  guestFlowData?: GuestFlowData
  pmsData?: PMSData[]
  roomNumber?: string
  roomType?: string
  checkInDate?: string
  checkOutDate?: string
  email?: string
  phoneNumber?: string
  isVIP: boolean
}

export interface GuestFlowData {
  guestId: number
  roomNumber?: string
  checkInDate?: string
  checkOutDate?: string
  email?: string
  phoneNumber?: string
  isVIP: boolean
  serviceHistory: ServiceHistory[]
}

export interface PMSData {
  providerName: string
  providerCode: string
  pmsReservationId?: string
  pmsGuestId?: string
  roomNumber?: string
  roomType?: string
  checkInDate?: string
  checkOutDate?: string
  email?: string
  phoneNumber?: string
  isVIP: boolean
  lastSyncedAt?: string
}

export interface ServiceHistory {
  serviceType: string
  serviceDate: string
  description?: string
  amount?: number
  status?: string
}

export interface UpcomingServices {
  items: UpcomingServiceItem[]
}

export interface UpcomingServiceItem {
  serviceId: number
  serviceType: string
  serviceDate: string
  guestName: string
  roomNumber?: string
  cityName?: string
  status?: string
  isUrgent: boolean
}

export interface GuestHistoryDashboard {
  guestId: number
  guestName: string
  guestCode: string
  previousStays: PreviousStay[]
  serviceHistory: ServiceHistory[]
  spendingAnalysis: SpendingAnalysis
  preferenceAnalysis: PreferenceAnalysis
}

export interface PreviousStay {
  pmsReservationId?: string
  pmsProviderName?: string
  roomNumber?: string
  roomType?: string
  checkInDate: string
  checkOutDate: string
  numberOfNights: number
  totalAmount?: number
  currency?: string
  lastSyncedAt?: string
}

export interface SpendingAnalysis {
  totalSpending: number
  currency: string
  pmsSpending?: number
  guestFlowSpending: number
  totalStays: number
  totalServices: number
  averageSpendingPerStay: number
  averageSpendingPerService: number
  spendingByCategory: SpendingByCategory[]
}

export interface SpendingByCategory {
  category: string
  amount: number
  count: number
}

export interface PreferenceAnalysis {
  roomPreferences: RoomPreference[]
  servicePreferences: ServicePreference[]
  preferredCheckInTime?: string
  preferredCheckOutTime?: string
}

export interface RoomPreference {
  roomType: string
  stayCount: number
  specialRequests?: string
}

export interface ServicePreference {
  serviceType: string
  usageCount: number
  totalSpending?: number
}

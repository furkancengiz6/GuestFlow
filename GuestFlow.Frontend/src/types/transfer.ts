export interface Transfer {
  id: number
  transferDate: string
  pickupTime?: string
  serviceStartTime?: string
  pickupConfirmationTime?: string
  dropoffConfirmationTime?: string
  pickupAddress: string
  dropoffAddress: string
  price: number
  finalPrice: number
  guestId: number
  guest?: { id: number; fullName: string; guestCode: string; email?: string; phoneNumber?: string }
  personnelId?: number
  driverId?: number
  airportId?: number
  vehicleId?: number
  note?: string
  status?: string
  transferType?: string | number
  pickupCityId?: number
  dropoffCityId?: number
  discountPercentage?: number
  currency?: string
  paymentStatus?: 'Unpaid' | 'PartiallyPaid' | 'Paid'
  paidAmount?: number
  remainingAmount?: number
  externalVehiclePlate?: string
  externalDriverName?: string
  externalDriverPhone?: string

  // Guest coordination fields
  contactPersonName?: string
  meetingPointDetails?: string

  // Group management fields
  groupSize?: number
  childCount?: number
  infantCount?: number

  // Communication fields
  guestLanguage?: string
  emergencyContactPhone?: string

  // Service quality fields
  accessibilityRequirements?: string
  specialHandlingNotes?: string

  // Internal coordination fields
  conciergeInternalNotes?: string
  guestVisibleNotes?: string

  // Supplier contact fields
  supplierName?: string
  supplierContactPhone?: string
  supplierEmergencyContact?: string
  supplierCost?: number
  supplierCurrency?: string
  supplierPaymentStatus?: string
  supplierPaymentDate?: string
  supplierInvoiceNumber?: string
  createdDate: string
}

export interface PagedTransfers {
  data: Transfer[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
  isFirstPage: boolean
  isLastPage: boolean
}

export interface TransferGuest {
  id: number
  fullName: string
  guestCode: string
  email?: string
  phoneNumber?: string
  nationality: string
  isSpecialGuest: boolean
}

export interface TransferPersonnel {
  id: number
  fullName: string
  email?: string
  phoneNumber?: string
  userType: string
}

export interface TransferVehicle {
  id: number
  vehicleName: string
  vehicleType: string
  capacity: number
  licensePlate?: string
}

export interface TransferAirport {
  id: number
  airportName: string
  cityName?: string
  country?: string
}

export interface TransferCity {
  id: number
  cityName: string
  country?: string
}

export interface TransferStatistics {
  totalTransfers: number
  completedTransfers: number
  pendingTransfers: number
  inProgressTransfers: number
  totalRevenue: number
  averagePrice: number
}

export interface TransferDetail {
  id: number
  transferDate: string
  pickupTime?: string
  serviceStartTime?: string
  pickupConfirmationTime?: string
  dropoffConfirmationTime?: string
  pickupAddress: string
  dropoffAddress: string
  price: number
  finalPrice: number
  note?: string
  status?: string
  transferType?: string | number
  discountPercentage?: number
  currency?: string
  paymentStatus?: 'Unpaid' | 'PartiallyPaid' | 'Paid'
  paidAmount?: number
  remainingAmount?: number
  paidAmountByCurrency?: Record<string, number>
  remainingAmountByCurrency?: Record<string, number>
  driverName?: string
  externalVehiclePlate?: string
  externalDriverName?: string
  externalDriverPhone?: string
  createdDate: string
  guest?: TransferGuest
  personnel?: TransferPersonnel
  vehicle?: TransferVehicle
  airport?: TransferAirport
  pickupCity?: TransferCity
  dropoffCity?: TransferCity
  statistics?: TransferStatistics
}


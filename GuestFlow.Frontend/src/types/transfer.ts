export interface Transfer {
  id: number
  transferDate: string
  pickupAddress: string
  dropoffAddress: string
  price: number
  guestId: number
  personnelId: number
  airportId: number
  vehicleId: number
  note: string
  status: string
  isFromAirport: boolean
  pickupCityId: number
  dropoffCityId: number
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
  pickupAddress: string
  dropoffAddress: string
  price: number
  finalPrice: number
  note?: string
  status: string
  isFromAirport: boolean
  createdDate: string
  guest?: TransferGuest
  personnel?: TransferPersonnel
  vehicle?: TransferVehicle
  airport?: TransferAirport
  pickupCity?: TransferCity
  dropoffCity?: TransferCity
  statistics?: TransferStatistics
}


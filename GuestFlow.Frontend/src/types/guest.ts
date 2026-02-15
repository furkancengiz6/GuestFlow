export interface Guest {
  id: number
  fullName: string
  email?: string
  phoneNumber?: string
  nationality: string
  guestCode: string
  isSpecialGuest: boolean
  roomNumber?: string
  checkInDate?: string
  checkOutDate?: string
  createdDate: string
}

export interface PagedGuests {
  data: Guest[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
  isFirstPage: boolean
  isLastPage: boolean
}

export interface GuestStatistics {
  totalTransfers: number
  totalCityTours: number
  totalYachtTours: number
  totalBookings: number
  totalInvoices: number
  totalSpent: number
  averageBookingValue: number
  firstBookingDate?: string
  lastBookingDate?: string
}

export interface GuestTransfer {
  id: number
  transferDate: string
  pickupAddress: string
  dropoffAddress: string
  price: number
  finalPrice: number
  status: string
  isFromAirport: boolean
  createdDate: string
}

export interface GuestCityTour {
  id: number
  tourDate: string
  numberOfPeople: number
  price: number
  finalPrice: number
  cityName?: string
  specialRequest?: string
  createdDate: string
}

export interface GuestYachtTour {
  id: number
  tourDate: string
  numberOfPeople: number
  price: number
  finalPrice: number
  yachtName: string
  cityName?: string
  specialRequest?: string
  createdDate: string
}

export interface GuestInvoice {
  id: number
  invoiceNumber: number
  issueDate: string
  totalAmount: number
  currency: string
  notes?: string
  pdfUrl: string
  hasPdf: boolean
  serviceType?: string
  serviceId?: number
  createdDate: string
}

export interface GuestTimelineItem {
  id: number
  type: string
  title: string
  description: string
  date: string
  amount?: number
  status: string
  createdDate: string
}

export interface GuestDetail {
  id: number
  fullName: string
  email?: string
  phoneNumber?: string
  nationality: string
  guestCode: string
  isSpecialGuest: boolean
  roomNumber?: string
  checkInDate?: string
  checkOutDate?: string
  createdDate: string
  statistics: GuestStatistics
  transfers: GuestTransfer[]
  cityTours: GuestCityTour[]
  yachtTours: GuestYachtTour[]
  invoices: GuestInvoice[]
  timeline: GuestTimelineItem[]
  isAnonymized?: boolean
  pmsIntegrationId?: number
  pmsGuestId?: string
}


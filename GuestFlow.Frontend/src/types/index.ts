// API Response Types
export interface ApiResponse<T = any> {
  success: boolean
  message: string
  data?: T
  errors?: any
  statusCode?: number
  timestamp?: string
}

export interface PagedResult<T> {
  items: T[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

// Auth Types
export interface LoginRequest {
  email: string
  password: string
}

export interface LoginResponse {
  accessToken: string
  expiresIn?: number
}

export interface User {
  id: number
  email: string
  fullName: string
  role: string
}

// Guest Types
export interface Guest {
  id: number
  fullName: string
  email?: string
  phoneNumber?: string
  nationality: string
  isSpecialGuest: boolean
  createdAt: string
  updatedAt: string
}

// Transfer Types
export interface Transfer {
  id: number
  transferDate: string
  pickupAddress: string
  dropoffAddress: string
  price: number
  guestId: number
  vehicleId?: number
  status: string
}

// Invoice Types
export interface Invoice {
  id: number
  invoiceNumber: string
  amount: number
  currency: string
  guestId: number
  serviceType: string
  serviceId: number
  pdfUrl?: string
  createdAt: string
}


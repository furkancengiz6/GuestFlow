export interface Hotel {
  id: number
  hotelName: string
  address: string
  cityId: number
  cityName?: string
  phone?: string
  email?: string
  starRating: number
  checkInTime?: string
  checkOutTime?: string
  roomTypes?: string
  amenities?: string
  createdDate: string
}

export interface PagedHotels {
  data: Hotel[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

export interface CreateHotelRequest {
  hotelName: string
  address: string
  cityId: number
  phone?: string
  email?: string
  starRating: number
  checkInTime?: string
  checkOutTime?: string
  roomTypes?: string
  amenities?: string
}

export interface UpdateHotelRequest {
  hotelName: string
  address: string
  cityId: number
  phone?: string
  email?: string
  starRating: number
  checkInTime?: string
  checkOutTime?: string
  roomTypes?: string
  amenities?: string
}

export interface HotelFilters {
  searchTerm?: string
  cityId?: number
  starRating?: number
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}


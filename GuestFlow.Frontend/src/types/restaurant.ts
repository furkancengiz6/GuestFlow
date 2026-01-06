export interface Restaurant {
  id: number
  restaurantName: string
  address: string
  cityId: number
  cityName?: string
  phone?: string
  email?: string
  cuisineType?: string
  capacity: number
  operatingHours?: string
  reservationRequired: boolean
  createdDate: string
}

export interface PagedRestaurants {
  data: Restaurant[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

export interface CreateRestaurantRequest {
  restaurantName: string
  address: string
  cityId: number
  phone?: string
  email?: string
  cuisineType?: string
  capacity: number
  operatingHours?: string
  reservationRequired: boolean
}

export interface UpdateRestaurantRequest {
  restaurantName: string
  address: string
  cityId: number
  phone?: string
  email?: string
  cuisineType?: string
  capacity: number
  operatingHours?: string
  reservationRequired: boolean
}

export interface RestaurantFilters {
  searchTerm?: string
  cityId?: number
  cuisineType?: string
  reservationRequired?: boolean
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}


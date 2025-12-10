export interface CityTour {
  id: number
  tourDate: string
  language: string
  durationHours: number
  price: number
  ownerGuestId: number
  personnelId: number
  cityId: number
  createdDate: string
}

export interface YachtTour {
  id: number
  tourDate: string
  numberOfPeople: number
  price: number
  specialRequest: string
  yachtName: string
  ownerGuestId: number
  personnelId: number
  cityId: number
  createdDate: string
}

export interface PagedCityTours {
  data: CityTour[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
  isFirstPage: boolean
  isLastPage: boolean
}

export interface PagedYachtTours {
  data: YachtTour[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
  isFirstPage: boolean
  isLastPage: boolean
}

export interface TourGuest {
  id: number
  fullName: string
  guestCode: string
  email?: string
  phoneNumber?: string
  nationality: string
  isSpecialGuest: boolean
}

export interface TourPersonnel {
  id: number
  fullName: string
  email?: string
  userType: string
}

export interface TourCity {
  id: number
  cityName: string
  country?: string
}

export interface CityTourDetail {
  id: number
  tourDate: string
  language: string
  durationHours: number
  price: number
  finalPrice: number
  createdDate: string
  guest?: TourGuest
  personnel?: TourPersonnel
  city?: TourCity
}

export interface YachtTourDetail {
  id: number
  tourDate: string
  numberOfPeople: number
  price: number
  finalPrice: number
  specialRequest: string
  yachtName: string
  createdDate: string
  guest?: TourGuest
  personnel?: TourPersonnel
  city?: TourCity
}


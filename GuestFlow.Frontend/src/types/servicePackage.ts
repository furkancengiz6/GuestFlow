import { PackageType } from './enums'

export interface ServicePackage {
  id: number
  packageName: string
  description?: string
  packageType: PackageType | number
  startDate?: string
  endDate?: string
  totalPrice: number
  discountPercentage?: number
  finalPrice: number
  currency: string
  isActive: boolean
  packageContent?: string
  notes?: string
  createdDate: string
  transferIds: number[]
  cityTourIds: number[]
  yachtTourIds: number[]
  restaurantReservationIds: number[]
}

export interface PagedServicePackages {
  data: ServicePackage[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

export interface CreateServicePackageRequest {
  id?: number
  packageName: string
  description?: string
  packageType: PackageType | number
  startDate?: string
  endDate?: string
  discountPercentage?: number
  currency?: string
  isActive?: boolean
  packageContent?: string
  notes?: string
  transferIds?: number[]
  cityTourIds?: number[]
  yachtTourIds?: number[]
  restaurantReservationIds?: number[]
}

export interface UpdateServicePackageRequest {
  id: number
  packageName: string
  description?: string
  packageType: PackageType | number
  startDate?: string
  endDate?: string
  discountPercentage?: number
  currency?: string
  isActive?: boolean
  packageContent?: string
  notes?: string
}

export interface ServicePackageFilters {
  packageType?: PackageType | number
  isActive?: boolean
  startDate?: string
  endDate?: string
  searchTerm?: string
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}


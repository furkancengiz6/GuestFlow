import apiClient from './api'
import type { CityTour, YachtTour, PagedCityTours, PagedYachtTours, CityTourDetail, YachtTourDetail } from '../types/tour'
import { TourCategory } from '../types/enums'

// Re-export types for convenience
export type { CityTour, YachtTour, PagedCityTours, PagedYachtTours, CityTourDetail, YachtTourDetail }

export interface AddCityTourResponse {
  cityTourId: number
  invoiceId?: number
  invoicePdfUrl?: string
}

export interface AddYachtTourResponse {
  yachtTourId: number
  invoiceId?: number
  invoicePdfUrl?: string
}

export interface CreateCityTourRequest {
  tourDate: string
  language: string
  durationHours: number
  price: number

  // Group composition fields
  adultCount?: number
  childCount?: number
  infantCount?: number

  ownerGuestId: number
  personnelId?: number
  tourGuideId?: number
  assistantGuideId?: number
  cityId: number
  tourId: number
  createInvoice?: boolean
  discountPercentage?: number
  invoiceDescription?: string
  currency?: string
  vehicleId?: number
  driverName?: string
  driverPhone?: string
  guideName?: string
  guidePhone?: string

  // Guide fields
  guideLanguages?: string
  backupGuideName?: string
  backupGuidePhone?: string

  // Time fields
  startTime?: string
  endTime?: string
  pickupTime?: string
  tourConfirmationTime?: string
  externalVehiclePlate?: string
  externalDriverName?: string
  externalDriverPhone?: string

  // Safety & emergency fields
  groupLeaderName?: string
  groupLeaderPhone?: string
  emergencyContactName?: string
  emergencyContactPhone?: string
  emergencyContactRelation?: string

  // Coordination fields
  meetingPersonName?: string
  meetingPointDetails?: string

  // Operational details
  tourDifficultyLevel?: string
  weatherDependent?: boolean
  minimumParticipantCount?: number
  maximumParticipantCount?: number

  // Guest experience fields
  dietaryRequirements?: string
  accessibilityNeeds?: string
  photographyAllowed?: boolean
  specialEquipment?: string

  // isPaymentReceived removed - payment status is calculated from PaymentEntity
  paymentNote?: string
  supplierName?: string
  supplierCost?: number
  supplierCurrency?: string
  supplierPaymentStatus?: string
  supplierPaymentDate?: string
  supplierInvoiceNumber?: string

  // Internal coordination fields
  conciergeInternalNotes?: string
}

export interface UpdateCityTourRequest {
  tourDate: string
  language: string
  durationHours: number
  price: number
  ownerGuestId: number
  personnelId?: number
  tourGuideId?: number
  assistantGuideId?: number
  cityId: number
  tourId: number
  discountPercentage?: number
  currency?: string
  vehicleId?: number
  driverName?: string
  driverPhone?: string
  guideName?: string
  guidePhone?: string
  externalVehiclePlate?: string
  externalDriverName?: string
  externalDriverPhone?: string
  startTime?: string
  endTime?: string
  pickupTime?: string
  tourConfirmationTime?: string
  // isPaymentReceived removed - payment status is calculated from PaymentEntity
  paymentNote?: string
  supplierName?: string
  supplierCost?: number
  supplierCurrency?: string
  supplierPaymentStatus?: string
  supplierPaymentDate?: string
  supplierInvoiceNumber?: string
}

export interface CreateYachtTourRequest {
  tourDate: string
  numberOfPeople: number

  // Group composition fields
  childCount?: number
  infantCount?: number

  price: number
  specialRequest?: string
  yachtName?: string

  // Group coordination fields
  groupLeaderName?: string
  groupLeaderPhone?: string
  emergencyContactName?: string
  emergencyContactPhone?: string
  emergencyContactRelation?: string

  ownerGuestId: number
  personnelId?: number
  yachtId?: number
  captainId?: number
  cityId: number
  createInvoice?: boolean
  discountPercentage?: number
  invoiceDescription?: string
  currency?: string
  pickupPier?: string
  dropoffPier?: string
  pierAddress?: string
  startTime?: string
  endTime?: string
  tourCategory?: TourCategory

  // Safety & regulatory fields
  lifeJacketsProvided?: boolean
  lifeJacketCount?: number
  safetyEquipmentCheck?: boolean
  emergencyEquipment?: string

  // Capacity & compliance fields
  yachtCapacity?: number
  yachtType?: string
  yachtLicenceRequired?: boolean
  coastGuardApproved?: boolean

  // Operational details
  crewSize?: number
  captainExperience?: string
  fuelRange?: number
  weatherBackupPlan?: string

  captainPhone?: string

  // Guest safety fields
  swimmingProficiency?: string
  medicalConditions?: string
  alcoholPolicy?: string

  // Amenities & experience fields
  foodBeverageIncluded?: boolean
  beverageType?: string
  musicSystem?: boolean
  waterSportsEquipment?: string

  // Coordination fields
  marinaContactName?: string
  marinaContactPhone?: string

  // isPaymentReceived removed - payment status is calculated from PaymentEntity
  paymentNote?: string
  supplierName?: string
  supplierCost?: number
  supplierCurrency?: string
  supplierPaymentStatus?: string
  supplierPaymentDate?: string
  supplierInvoiceNumber?: string

  // Internal coordination fields
  conciergeInternalNotes?: string
}

export interface UpdateYachtTourRequest {
  tourDate: string
  numberOfPeople: number
  price: number
  specialRequest?: string
  yachtName?: string
  ownerGuestId: number
  personnelId?: number
  yachtId?: number
  captainId?: number
  cityId: number
  discountPercentage?: number
  currency?: string
  pickupPier?: string
  dropoffPier?: string
  pierAddress?: string
  startTime?: string
  endTime?: string
  tourCategory?: TourCategory
  captainPhone?: string
  // isPaymentReceived removed - payment status is calculated from PaymentEntity
  paymentNote?: string
  supplierName?: string
  supplierCost?: number
  supplierCurrency?: string
  supplierPaymentStatus?: string
  supplierPaymentDate?: string
  supplierInvoiceNumber?: string
}

export interface CityTourFilters {
  startDate?: string
  endDate?: string
  cityId?: number
  guestId?: number
  personnelId?: number
  searchTerm?: string
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

export interface YachtTourFilters {
  startDate?: string
  endDate?: string
  cityId?: number
  guestId?: number
  personnelId?: number
  searchTerm?: string
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

export const tourService = {
  getCityTours: async (
    pageNumber: number = 1,
    pageSize: number = 10,
    filters?: CityTourFilters
  ): Promise<PagedCityTours> => {
    const params: any = { pageNumber, pageSize }
    
    if (filters) {
      if (filters.startDate) params.startDate = filters.startDate
      if (filters.endDate) params.endDate = filters.endDate
      if (filters.cityId) params.cityId = filters.cityId
      if (filters.guestId) params.guestId = filters.guestId
      if (filters.personnelId) params.personnelId = filters.personnelId
      if (filters.searchTerm) params.searchTerm = filters.searchTerm
      if (filters.sortBy) params.sortBy = filters.sortBy
      if (filters.sortOrder) params.sortOrder = filters.sortOrder
    }
    
    const response = await apiClient.get('/CityTours', { params })
    return response.data.data
  },

  getCityTourById: async (id: number): Promise<CityTour> => {
    const response = await apiClient.get(`/CityTours/${id}`)
    return response.data.data
  },

  getYachtTours: async (
    pageNumber: number = 1,
    pageSize: number = 10,
    filters?: YachtTourFilters
  ): Promise<PagedYachtTours> => {
    const params: any = { pageNumber, pageSize }
    
    if (filters) {
      if (filters.startDate) params.startDate = filters.startDate
      if (filters.endDate) params.endDate = filters.endDate
      if (filters.cityId) params.cityId = filters.cityId
      if (filters.guestId) params.guestId = filters.guestId
      if (filters.personnelId) params.personnelId = filters.personnelId
      if (filters.searchTerm) params.searchTerm = filters.searchTerm
      if (filters.sortBy) params.sortBy = filters.sortBy
      if (filters.sortOrder) params.sortOrder = filters.sortOrder
    }
    
    const response = await apiClient.get('/YachtTours', { params })
    return response.data.data
  },

  getYachtTourById: async (id: number): Promise<YachtTour> => {
    const response = await apiClient.get(`/YachtTours/${id}`)
    return response.data.data
  },

  getCityTourDetail: async (id: number): Promise<CityTourDetail> => {
    const response = await apiClient.get(`/CityTours/${id}/detail`)
    return response.data.data
  },

  getYachtTourDetail: async (id: number): Promise<YachtTourDetail> => {
    const response = await apiClient.get(`/YachtTours/${id}/detail`)
    return response.data.data
  },

  createCityTour: async (data: CreateCityTourRequest): Promise<AddCityTourResponse> => {
    const response = await apiClient.post('/CityTours', data)
    return response.data.data
  },

  updateCityTour: async (id: number, data: UpdateCityTourRequest): Promise<CityTour> => {
    const response = await apiClient.put(`/CityTours/${id}`, data)
    return response.data.data
  },

  deleteCityTour: async (id: number): Promise<void> => {
    await apiClient.delete(`/CityTours/${id}`)
  },

  createYachtTour: async (data: CreateYachtTourRequest): Promise<AddYachtTourResponse> => {
    const response = await apiClient.post('/YachtTours', data)
    return response.data.data
  },

  updateYachtTour: async (id: number, data: UpdateYachtTourRequest): Promise<YachtTour> => {
    const response = await apiClient.put(`/YachtTours/${id}`, data)
    return response.data.data
  },

  deleteYachtTour: async (id: number): Promise<void> => {
    await apiClient.delete(`/YachtTours/${id}`)
  },

  // Action methods for City Tour Detail Page
  createCityTourInvoice: async (id: number): Promise<void> => {
    await apiClient.post(`/Tours/city/${id}/invoice`)
  },

  sendCityTourConfirmation: async (id: number): Promise<void> => {
    await apiClient.post(`/Tours/city/${id}/send-confirmation`)
  },

  // Action methods for Yacht Tour Detail Page
  createYachtTourInvoice: async (id: number): Promise<void> => {
    await apiClient.post(`/Tours/yacht/${id}/invoice`)
  },

  sendYachtTourConfirmation: async (id: number): Promise<void> => {
    await apiClient.post(`/Tours/yacht/${id}/send-confirmation`)
  },

  // Additional action methods
  markCityTourCompleted: async (id: number): Promise<void> => {
    await apiClient.patch(`/Tours/city/${id}/status`, { status: 'Completed' })
  },

  cancelCityTour: async (id: number): Promise<void> => {
    await apiClient.patch(`/Tours/city/${id}/status`, { status: 'Cancelled' })
  },

  markYachtTourCompleted: async (id: number): Promise<void> => {
    await apiClient.patch(`/Tours/yacht/${id}/status`, { status: 'Completed' })
  },

  cancelYachtTour: async (id: number): Promise<void> => {
    await apiClient.patch(`/Tours/yacht/${id}/status`, { status: 'Cancelled' })
  },
}


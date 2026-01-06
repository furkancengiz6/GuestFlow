export interface CityTour {
  id: number
  tourDate: string
  language: string
  durationHours: number // Total tour duration in hours
  price: number
  finalPrice: number

  // Group composition fields
  adultCount?: number
  childCount?: number
  infantCount?: number
  ownerGuestId: number
  personnelId?: number
  tourGuideId?: number // Main tour guide (from Personnel)
  assistantGuideId?: number // Assistant guide (from Personnel)
  cityId: number
  tourId?: number
  pickupHotelId?: number
  discountPercentage?: number
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

  externalVehiclePlate?: string
  externalDriverName?: string
  externalDriverPhone?: string
  startTime?: string
  endTime?: string
  pickupTime?: string // Hotel pickup time
  tourConfirmationTime?: string // When tour starts

  // Safety & emergency fields
  groupLeaderName?: string
  groupLeaderPhone?: string
  emergencyContactName?: string
  emergencyContactPhone?: string

  // Coordination fields
  meetingPersonName?: string
  meetingPointDetails?: string

  // Operational details
  tourDifficultyLevel?: string // Easy/Moderate/Challenging with specific requirements
  weatherDependent?: boolean
  minimumParticipantCount?: number
  maximumParticipantCount?: number

  // Guest experience fields
  dietaryRequirements?: string
  accessibilityNeeds?: string
  photographyAllowed?: boolean // Commercial photography rights
  specialEquipment?: string // Wheelchair, mobility aids, etc.
  // isPaymentReceived removed - use paymentStatus instead (calculated from PaymentEntity)
  paymentStatus?: 'Unpaid' | 'PartiallyPaid' | 'Paid'
  paidAmount?: number
  remainingAmount?: number
  paymentNote?: string
  supplierName?: string
  supplierCost?: number
  supplierCurrency?: string
  supplierPaymentStatus?: string
  supplierPaymentDate?: string
  supplierInvoiceNumber?: string

  // Internal coordination fields
  conciergeInternalNotes?: string

  createdDate: string
}

export interface YachtTour {
  id: number
  tourDate: string
  numberOfPeople: number

  // Group composition fields
  childCount?: number
  infantCount?: number

  price: number
  finalPrice: number
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
  yachtId?: number // Specific yacht (from Yacht inventory)
  captainId?: number // Licensed captain (from Personnel)
  cityId: number
  pickupHotelId?: number
  discountPercentage?: number
  currency?: string
  pickupPier?: string
  dropoffPier?: string
  pierAddress?: string
  startTime?: string
  endTime?: string
  safetyBriefingTime?: string // When safety briefing occurs
  marinaPickupTime?: string // Marina pickup time
  weatherCheckTime?: string // Last weather check time
  fuelLevelCheck?: string // When fuel was last checked
  tourCategory?: string

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
  crewSize?: number // Total crew count (captain, crew, hostess, security)
  captainExperience?: string // Captain certifications and experience level
  fuelRange?: number
  weatherBackupPlan?: string

  captainPhone?: string
  // isPaymentReceived removed - use paymentStatus instead (calculated from PaymentEntity)
  paymentStatus?: 'Unpaid' | 'PartiallyPaid' | 'Paid'
  paidAmount?: number
  remainingAmount?: number
  paymentNote?: string

  // Guest safety fields
  swimmingProficiency?: string
  medicalConditions?: string
  alcoholPolicy?: string // Alcohol service policy and restrictions

  // Amenities & experience fields
  foodBeverageIncluded?: boolean
  beverageType?: string // Available beverages (non-alcoholic and alcoholic)
  musicSystem?: boolean
  waterSportsEquipment?: string
  lifeGuardCertified?: boolean // Certified lifeguard on board
  coastGuardInspectionDate?: string // Last Coast Guard inspection

  // Coordination fields
  marinaContactName?: string
  marinaContactPhone?: string

  supplierName?: string
  supplierCost?: number
  supplierCurrency?: string
  supplierPaymentStatus?: string
  supplierPaymentDate?: string
  supplierInvoiceNumber?: string

  // Internal coordination fields
  conciergeInternalNotes?: string

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
  durationHours: number // Total tour duration in hours
  price: number
  finalPrice: number
  tourId?: number
  discountPercentage?: number
  currency?: string
  pickupHotelId?: number
  startTime?: string
  endTime?: string
  pickupTime?: string // Hotel pickup time
  tourConfirmationTime?: string // When tour starts
  vehicleId?: number
  tourGuideId?: number // Main tour guide
  assistantGuideId?: number // Assistant guide
  driverName?: string
  guideName?: string
  guidePhone?: string
  externalVehiclePlate?: string
  externalDriverName?: string
  externalDriverPhone?: string
  createdDate: string
  paymentStatus?: 'Unpaid' | 'PartiallyPaid' | 'Paid'
  paidAmount?: number
  remainingAmount?: number
  paidAmountByCurrency?: Record<string, number>
  remainingAmountByCurrency?: Record<string, number>

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

  // Guide fields
  guideLanguages?: string
  backupGuideName?: string
  backupGuidePhone?: string

  // Internal coordination fields
  conciergeInternalNotes?: string

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
  specialRequest?: string
  yachtName?: string
  discountPercentage?: number
  currency?: string
  pickupHotelId?: number
  pickupPier?: string
  dropoffPier?: string
  startTime?: string
  endTime?: string
  createdDate: string
  paymentStatus?: 'Unpaid' | 'PartiallyPaid' | 'Paid'
  paidAmount?: number
  remainingAmount?: number
  paidAmountByCurrency?: Record<string, number>
  remainingAmountByCurrency?: Record<string, number>

  // Safety & emergency fields
  groupLeaderName?: string
  groupLeaderPhone?: string
  emergencyContactName?: string
  emergencyContactPhone?: string
  emergencyContactRelation?: string

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
  fuelLevelCheck?: string

  // Guest safety fields
  swimmingProficiency?: string
  medicalConditions?: string
  alcoholPolicy?: string

  // Amenities & experience fields
  foodBeverageIncluded?: boolean
  beverageType?: string
  musicSystem?: boolean
  waterSportsEquipment?: string
  lifeGuardCertified?: boolean
  coastGuardInspectionDate?: string

  // Coordination fields
  marinaContactName?: string
  marinaContactPhone?: string

  // Internal coordination fields
  conciergeInternalNotes?: string

  guest?: TourGuest
  personnel?: TourPersonnel
  city?: TourCity
}

export interface Tour {
  id: number
  name: string
  description?: string
  cityId: number
  isActive: boolean
  createdDate: string
}


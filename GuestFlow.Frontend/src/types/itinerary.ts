export interface Itinerary {
  id: number
  itineraryNumber: string
  guestId: number
  guestName: string
  personnelId: number
  personnelName: string
  startDate: string
  endDate: string
  status: string | number // Can be enum number (1-5) or string
  totalCost: number
  currency: string
  notes?: string
  createdDate: string
  items: ItineraryItem[]
}

export interface ItineraryItem {
  id: number
  itineraryId: number
  itemType: string
  serviceId: number
  scheduledDateTime: string
  order: number
  status?: string
  notes?: string
  createdDate: string
}

export interface TimelineItem {
  id: number
  itemType: string
  itemTypeTurkish: string
  serviceId: number
  scheduledDateTime: string
  order: number
  status?: string
  serviceName?: string
  description?: string
  location?: string
  pickupLocation?: string
  dropoffLocation?: string
  icon?: string
  price?: number
  currency?: string
  duration?: string
  notes?: string
  additionalInfo?: Record<string, any>
}

export interface ItineraryTimeline {
  itineraryId: number
  itineraryNumber: string
  guestName: string
  startDate: string
  endDate: string
  status: string
  totalCost: number
  currency: string
  items: TimelineItem[]
}

export interface PagedItineraries {
  data: Itinerary[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
}

export interface CreateItineraryRequest {
  guestId: number
  personnelId: number
  startDate: string
  endDate: string
  notes?: string
  currency?: string
  items: CreateItineraryItemRequest[]
}

export interface CreateItineraryItemRequest {
  itemType: string
  serviceId: number
  scheduledDateTime: string
  order: number
  notes?: string
}

export interface UpdateItineraryRequest {
  guestId: number
  personnelId: number
  startDate: string
  endDate: string
  status: string
  notes?: string
  currency?: string
}

export interface ItineraryFilters {
  guestId?: number
  personnelId?: number
  status?: string
  startDate?: string
  endDate?: string
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}


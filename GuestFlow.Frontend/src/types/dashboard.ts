export interface DashboardOverview {
  totalGuests: number
  totalPersonnel: number
  totalCities: number
  totalVehicles: number
  todayRevenue: number
  thisWeekRevenue: number
  thisMonthRevenue: number
  lastMonthRevenue: number
  yearToDateRevenue: number
  activeTransfers: number
  upcomingTours: number
  pendingInvoices: number
  todayBookings: number
  averageBookingValue: number
  totalBookingsThisMonth: number
  totalBookingsLastMonth: number
  revenueGrowthPercentage: number
  recentBookings: RecentBooking[]
  popularServices: PopularService[]
}

export interface QuickStats {
  totalGuests: number
  activeGuests: number
  totalPersonnel: number
  totalTransfers: number
  totalCityTours: number
  totalYachtTours: number
  totalInvoices: number
  totalRevenue: number
}

export interface RecentBooking {
  id: number
  type: 'Transfer' | 'CityTour' | 'YachtTour'
  guestName: string
  guestCode: string
  bookingDate: string
  amount: number
  status: string
  createdDate: string
}

export interface PopularService {
  serviceType: string
  bookingCount: number
  totalRevenue: number
  averagePrice: number
}

export interface RecentActivity {
  recentBookings: RecentBooking[]
  recentGuests: RecentGuest[]
  recentInvoices: RecentInvoice[]
}

export interface RecentGuest {
  id: number
  fullName: string
  guestCode: string
  email: string
  nationality: string
  isSpecialGuest: boolean
  createdDate: string
}

export interface RecentInvoice {
  id: number
  invoiceNumber: number
  guestName: string
  totalAmount: number
  currency: string
  issueDate: string
  hasPdf: boolean
  createdDate: string
}

export interface RevenueChartData {
  period: string
  data: RevenueChartItem[]
}

export interface RevenueChartItem {
  label: string
  revenue: number
  bookingCount: number
  date: string
}

export interface UpcomingBooking {
  id: number
  type: 'Transfer' | 'CityTour' | 'YachtTour'
  guestName: string
  guestCode: string
  bookingDate: string
  startTime?: string
  location: string
  description: string
  amount: number
  status: string
  personnelId?: number
  personnelName: string
}

export interface UpcomingBookings {
  today: UpcomingBooking[]
  thisWeek: UpcomingBooking[]
  thisMonth: UpcomingBooking[]
  totalUpcoming: number
}

export interface GuestStatistics {
  totalGuests: number
  activeGuests: number
  specialGuests: number
  newGuestsThisMonth: number
  newGuestsLastMonth: number
  guestGrowthPercentage: number
  topGuests: TopGuest[]
}

export interface UnpaidServiceItem {
  serviceType: 'Transfer' | 'CityTour' | 'YachtTour'
  serviceId: number
  serviceDate: string
  guestName: string
  roomNumber?: string
  cityName?: string
  amount: number
  currency?: string
  status?: string
  remainingAmount: number
  daysOverdue: number
}

export interface UnpaidServices {
  items: UnpaidServiceItem[]
}

export interface UpcomingServiceItem {
  serviceType: 'Transfer' | 'CityTour' | 'YachtTour'
  serviceId: number
  serviceDate: string
  guestName: string
  roomNumber?: string
  cityName?: string
  status?: string
  isUrgent: boolean
}

export interface UpcomingServices {
  items: UpcomingServiceItem[]
}

export interface TopGuest {
  guestId: number
  fullName: string
  guestCode: string
  bookingCount: number
  totalSpent: number
}


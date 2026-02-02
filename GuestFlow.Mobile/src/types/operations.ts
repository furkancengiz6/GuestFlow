export interface CheckInOutItem {
    guestId: number;
    guestName: string;
    guestCode: string;
    roomNumber?: string;
    checkInDate?: string;
    checkOutDate?: string;
    status: string;
    source: string;
}

export interface ActiveGuest {
    guestId: number;
    guestName: string;
    guestCode: string;
    roomNumber?: string;
    checkInDate?: string;
    checkOutDate?: string;
    numberOfNights?: number;
    isVIP: boolean;
    email?: string;
    phoneNumber?: string;
    source: string;
    pmsProviderName?: string;
    upcomingServices?: UpcomingServiceItem[];
}

export interface UpcomingServiceItem {
    serviceId: number;
    serviceType: string;
    serviceDate: string;
    guestName: string;
    roomNumber?: string;
    cityName?: string;
    status: string;
    isUrgent: boolean;
}

export interface DashboardSummary {
    todayCheckIns: number;
    todayCheckOuts: number;
    activeGuestsCount: number;
    pendingServicesCount: number;
    averageRating: number;
}

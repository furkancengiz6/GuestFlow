export interface Guest {
    id: number;
    fullName: string;
    email?: string;
    phoneNumber?: string;
    roomNumber?: string;
    pmsGuestId?: string;
    nationality?: string;
    isVIP: boolean;
    status?: string;
}

export interface GuestListResponse {
    items: Guest[];
    totalCount: number;
}

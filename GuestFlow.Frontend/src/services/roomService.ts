import apiClient from './api';

export interface RoomContextRequest {
  roomNumber: string;
  startDate: string;
  endDate: string;
  hotelId?: number;
}

export interface GuestAssignment {
  guestId: number;
  guestName: string;
  guestCode: string;
  assignmentStart: string;
  assignmentEnd?: string;
  notes?: string;
}

export interface ServiceSummary {
  serviceId: number;
  serviceType: string;
  description: string;
  serviceDate: string;
  amount: number;
  currency: string;
  guestName: string;
  status: string;
}

export interface RoomFinancialSummary {
  totalInvoices: number;
  totalPayments: number;
  totalInvoicedAmount: number;
  totalPaidAmount: number;
  currency: string;
}

export interface RoomContext {
  roomNumber: string;
  hotelName: string;
  searchStartDate: string;
  searchEndDate: string;
  guests: GuestAssignment[];
  transfers: ServiceSummary[];
  cityTours: ServiceSummary[];
  yachtTours: ServiceSummary[];
  financialSummary: RoomFinancialSummary;
}

export interface RoomAssignment {
  id: number
  guestId: number
  hotelId?: number
  roomNumber: string
  startDate: string
  endDate?: string
  notes?: string
  createdDate: string
  createdByPersonnelId: number
  updatedDate: string
  updatedByPersonnelId: number
  isDeleted: boolean
}

export interface CreateRoomAssignmentRequest {
  guestId: number
  hotelId?: number
  roomNumber: string
  startDate: string
  endDate?: string
  notes?: string
}

export interface UpdateRoomAssignmentRequest {
  roomNumber: string
  startDate: string
  endDate?: string
  notes?: string
}

export interface CloseRoomAssignmentRequest {
  endDate: string
  notes?: string
}

export const roomService = {
  // Room context (existing)
  getRoomContext: async (request: RoomContextRequest): Promise<RoomContext> => {
    const response = await apiClient.post('/RoomAssignments/context', request);
    return response.data.data;
  },

  // Room assignments
  createRoomAssignment: async (guestId: number, request: CreateRoomAssignmentRequest): Promise<RoomAssignment> => {
    const response = await apiClient.post(`/Guests/${guestId}/room-assignments`, request);
    return response.data.data;
  },

  updateRoomAssignment: async (id: number, request: UpdateRoomAssignmentRequest): Promise<RoomAssignment> => {
    const response = await apiClient.put(`/RoomAssignments/${id}`, request);
    return response.data.data;
  },

  closeRoomAssignment: async (id: number, request: CloseRoomAssignmentRequest): Promise<RoomAssignment> => {
    const response = await apiClient.post(`/RoomAssignments/${id}/close`, request);
    return response.data.data;
  },

  getGuestRoomAssignments: async (guestId: number): Promise<RoomAssignment[]> => {
    const response = await apiClient.get(`/Guests/${guestId}/room-assignments`);
    return response.data.data;
  },

  getCurrentRoomAssignment: async (guestId: number): Promise<RoomAssignment | null> => {
    const response = await apiClient.get(`/Guests/${guestId}/current-room`);
    return response.data.data;
  }
};

import api from './api'
import { ApiResponse } from '../types'

export enum RoomCleaningStatus {
  Dirty = 'Dirty',
  Cleaning = 'Cleaning',
  Clean = 'Clean',
  Inspected = 'Inspected',
  OutOfOrder = 'OutOfOrder'
}

export enum RoomOccupancyStatus {
  Vacant = 'Vacant',
  Occupied = 'Occupied',
  ExpectedArrival = 'ExpectedArrival',
  ExpectedDeparture = 'ExpectedDeparture'
}

export enum MaintenanceStatus {
  Pending = 'Pending',
  InProgress = 'InProgress',
  Resolved = 'Resolved',
  Cancelled = 'Cancelled'
}

export enum MaintenancePriority {
  Low = 'Low',
  Medium = 'Medium',
  High = 'High',
  Urgent = 'Urgent'
}

export interface RoomStatus {
  id: number
  roomNumber: string
  cleaningStatus: RoomCleaningStatus
  cleaningStatusDisplay: string
  occupancyStatus: RoomOccupancyStatus
  occupancyStatusDisplay: string
  lastCleaned: string
  nextInspection?: string
  assignedHousekeeperId?: number
  assignedHousekeeperName?: string
  notes?: string
  hotelId: number
  hotelName?: string
}

export interface MaintenanceRequest {
  id: number
  roomNumber: string
  issueDescription: string
  priority: MaintenancePriority
  status: MaintenanceStatus
  reportedDate: string
  resolvedDate?: string
  reportedByPersonnelId: number
  reportedByPersonnelName?: string
  assignedToPersonnelId?: number
  assignedToPersonnelName?: string
  resolutionNotes?: string
  hotelId: number
}

export interface LostAndFoundItem {
  id: number
  itemDescription: string
  roomNumber: string
  foundDate: string
  storageLocation: string
  itemCategory: string
  foundByPersonnelId: number
  foundByPersonnelName?: string
  guestId?: number
  guestName?: string
  isReturned: boolean
  returnedDate?: string
  hotelId: number
}

const housekeepingService = {
  // Room Status
  getRoomStatuses: async (params?: { hotelId?: number; cleaningStatus?: string; occupancyStatus?: string }) => {
    const response = await api.get<ApiResponse<RoomStatus[]>>('/housekeeping/rooms', { params })
    return response.data
  },

  updateRoomStatus: async (id: number, data: Partial<RoomStatus>) => {
    const response = await api.put<ApiResponse<RoomStatus>>(`/housekeeping/rooms/${id}`, data)
    return response.data
  },

  assignRoom: async (id: number, housekeeperId: number) => {
    const response = await api.post<ApiResponse<void>>(`/housekeeping/rooms/${id}/assign`, { housekeeperId })
    return response.data
  },

  markAsCleaned: async (id: number) => {
    const response = await api.post<ApiResponse<void>>(`/housekeeping/rooms/${id}/cleaned`)
    return response.data
  },

  // Maintenance
  getMaintenanceRequests: async (params?: { status?: string; priority?: string; hotelId?: number }) => {
    const response = await api.get<ApiResponse<MaintenanceRequest[]>>('/housekeeping/maintenance', { params })
    return response.data
  },

  createMaintenanceRequest: async (data: any) => {
    const response = await api.post<ApiResponse<MaintenanceRequest>>('/housekeeping/maintenance', data)
    return response.data
  },

  resolveMaintenanceRequest: async (id: number, resolutionNotes: string) => {
    const response = await api.post<ApiResponse<MaintenanceRequest>>(`/housekeeping/maintenance/${id}/resolve`, { resolutionNotes })
    return response.data
  },

  // Lost and Found
  getLostAndFoundItems: async (params?: { isReturned?: boolean; hotelId?: number }) => {
    const response = await api.get<ApiResponse<LostAndFoundItem[]>>('/housekeeping/lost-found', { params })
    return response.data
  },

  createLostAndFoundItem: async (data: any) => {
    const response = await api.post<ApiResponse<LostAndFoundItem>>('/housekeeping/lost-found', data)
    return response.data
  },

  returnLostItem: async (id: number, guestId: number) => {
    const response = await api.post<ApiResponse<LostAndFoundItem>>(`/housekeeping/lost-found/${id}/return`, { guestId })
    return response.data
  }
}

export default housekeepingService

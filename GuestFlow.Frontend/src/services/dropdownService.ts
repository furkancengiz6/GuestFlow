import apiClient from './api'

export interface GuestOption {
  id: number
  fullName: string
  guestCode: string
  email?: string
}

export interface PersonnelOption {
  id: number
  fullName: string
  email: string
}

export interface AirportOption {
  id: number
  airportName: string
  cityName?: string
}

export interface VehicleOption {
  id: number
  plateNumber: string
  vehicleType: string
  capacity?: number
}

export interface CityOption {
  id: number
  cityName: string
}

export const dropdownService = {
  getGuests: async (): Promise<GuestOption[]> => {
    const response = await apiClient.get('/Guests', {
      params: { pageNumber: 1, pageSize: 1000 }, // Tüm misafirleri al
    })
    return response.data.data.data.map((g: any) => ({
      id: g.id,
      fullName: g.fullName,
      guestCode: g.guestCode,
      email: g.email,
    }))
  },

  getPersonnel: async (): Promise<PersonnelOption[]> => {
    const response = await apiClient.get('/Personnel', {
      params: { pageNumber: 1, pageSize: 1000 }, // Tüm personelleri al
    })
    return response.data.data.data.map((p: any) => ({
      id: p.id,
      fullName: p.fullName,
      email: p.email,
    }))
  },

  getAirports: async (): Promise<AirportOption[]> => {
    const response = await apiClient.get('/Airports', {
      params: { pageNumber: 1, pageSize: 1000 }, // Tüm havaalanlarını al
    })
    return response.data.data.data.map((a: any) => ({
      id: a.id,
      airportName: a.airportName,
      cityName: a.cityName,
    }))
  },

  getVehicles: async (): Promise<VehicleOption[]> => {
    const response = await apiClient.get('/Vehicles', {
      params: { pageNumber: 1, pageSize: 1000 }, // Tüm araçları al
    })
    return response.data.data.data.map((v: any) => ({
      id: v.id,
      plateNumber: v.plateNumber,
      vehicleType: v.vehicleType,
      capacity: v.capacity,
    }))
  },

  getCities: async (): Promise<CityOption[]> => {
    const response = await apiClient.get('/Cities', {
      params: { pageNumber: 1, pageSize: 1000 }, // Tüm şehirleri al
    })
    return response.data.data.data.map((c: any) => ({
      id: c.id,
      cityName: c.cityName,
    }))
  },
}


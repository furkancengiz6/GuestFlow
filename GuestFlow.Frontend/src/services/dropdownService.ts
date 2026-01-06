import apiClient from './api'

export interface GuestOption {
  id: number
  fullName: string
  guestCode: string
  email?: string
  phoneNumber?: string
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

export interface TourOption {
  id: number
  name: string
  cityId: number
  isActive: boolean
}

export const dropdownService = {
  getGuests: async (): Promise<GuestOption[]> => {
    try {
      const response = await apiClient.get('/Guests', {
        params: { pageNumber: 1, pageSize: 1000 }, // Tüm misafirleri al
      })
      if (response.data?.data?.data) {
        return response.data.data.data.map((g: any) => ({
          id: g.id,
          fullName: g.fullName,
          guestCode: g.guestCode,
          email: g.email,
        }))
      }
      return []
    } catch (error) {
      console.error('Guests dropdown fetch error:', error)
      return []
    }
  },

  getPersonnel: async (): Promise<PersonnelOption[]> => {
    try {
      const response = await apiClient.get('/Personnel', {
        params: { pageNumber: 1, pageSize: 1000 }, // Tüm personelleri al
      })
      // Response format: { data: { data: { data: [...], totalCount, ... } } }
      if (response.data?.data?.data) {
        return response.data.data.data.map((p: any) => ({
          id: p.id,
          fullName: p.fullName,
          email: p.email || '',
        }))
      }
      return []
    } catch (error) {
      console.error('Personnel dropdown fetch error:', error)
      return []
    }
  },

  getAirports: async (): Promise<AirportOption[]> => {
    try {
      const response = await apiClient.get('/Airports', {
        params: { pageNumber: 1, pageSize: 1000 }, // Tüm havaalanlarını al
      })
      if (response.data?.data?.data) {
        return response.data.data.data.map((a: any) => ({
          id: a.id,
          airportName: a.airportName,
          cityName: a.cityName,
        }))
      }
      return []
    } catch (error) {
      console.error('Airports dropdown fetch error:', error)
      return []
    }
  },

  getVehicles: async (): Promise<VehicleOption[]> => {
    try {
      const response = await apiClient.get('/Vehicles', {
        params: { pageNumber: 1, pageSize: 1000 }, // Tüm araçları al
      })
      if (response.data?.data?.data) {
        return response.data.data.data.map((v: any) => ({
          id: v.id,
          plateNumber: v.plateNumber,
          vehicleType: v.vehicleType,
          capacity: v.capacity,
        }))
      }
      return []
    } catch (error) {
      console.error('Vehicles dropdown fetch error:', error)
      return []
    }
  },

  getCities: async (): Promise<CityOption[]> => {
    try {
      const response = await apiClient.get('/Cities', {
        params: { pageNumber: 1, pageSize: 1000 }, // Tüm şehirleri al
      })
      if (response.data?.data?.data) {
        return response.data.data.data.map((c: any) => ({
          id: c.id,
          cityName: c.cityName,
        }))
      }
      return []
    } catch (error) {
      console.error('Cities dropdown fetch error:', error)
      return []
    }
  },

  getTours: async (cityId?: number): Promise<TourOption[]> => {
    try {
      const params: any = { pageNumber: 1, pageSize: 1000 }
      if (cityId) params.cityId = cityId
      const response = await apiClient.get('/Tours', { params })
      if (response.data?.data) {
        return response.data.data.map((t: any) => ({
          id: t.id,
          name: t.name,
          cityId: t.cityId,
          isActive: t.isActive,
        }))
      }
      return []
    } catch (error) {
      console.error('Tours dropdown fetch error:', error)
      return []
    }
  },
}


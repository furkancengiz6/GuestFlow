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
  // Optional fields used by some consumers
  tourName?: string
  tourDate?: string
  yachtName?: string
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
          tourName: t.tourName || t.name,
          tourDate: t.tourDate || t.date || undefined,
          yachtName: t.yachtName || undefined,
        }))
      }
      return []
    } catch (error) {
      console.error('Tours dropdown fetch error:', error)
      return []
    }
  },
  getHotels: async (): Promise<{ id: number; hotelName: string }[]> => {
    try {
      const response = await apiClient.get('/Hotels', { params: { pageNumber: 1, pageSize: 1000 } })
      if (response.data?.data?.data) {
        return response.data.data.data.map((h: any) => ({ id: h.id, hotelName: h.hotelName }))
      }
      return []
    } catch (error) {
      console.error('Hotels dropdown fetch error:', error)
      return []
    }
  },
  getInvoices: async (): Promise<any[]> => {
    try {
      const response = await apiClient.get('/Invoices', { params: { pageNumber: 1, pageSize: 1000 } })
      return response.data?.data?.data || []
    } catch (error) {
      console.error('Invoices dropdown fetch error:', error)
      return []
    }
  },
  getTransfers: async (): Promise<any[]> => {
    try {
      const response = await apiClient.get('/Transfers', { params: { pageNumber: 1, pageSize: 1000 } })
      return response.data?.data?.data || []
    } catch (error) {
      console.error('Transfers dropdown fetch error:', error)
      return []
    }
  },
  getCityTours: async (cityId?: number): Promise<TourOption[]> => {
    return await (dropdownService.getTours(cityId))
  },
  getYachtTours: async (cityId?: number): Promise<TourOption[]> => {
    return await (dropdownService.getTours(cityId))
  },
}


/**
 * Query key factory for consistent query key management
 * This helps with cache invalidation and type safety
 */

// Base query keys
const authKeys = ['auth'] as const
const guestsKeys = ['guests'] as const
const transfersKeys = ['transfers'] as const
const toursKeys = ['tours'] as const
const hotelsKeys = ['hotels'] as const
const restaurantsKeys = ['restaurants'] as const
const itinerariesKeys = ['itineraries'] as const
const invoicesKeys = ['invoices'] as const
const reservationsKeys = ['reservations'] as const
const personnelKeys = ['personnel'] as const
const airportsKeys = ['airports'] as const
const citiesKeys = ['cities'] as const
const vehiclesKeys = ['vehicles'] as const
const dailyNotesKeys = ['dailyNotes'] as const
const dailyRevenuesKeys = ['dailyRevenues'] as const
const dashboardKeys = ['dashboard'] as const
const reportsKeys = ['reports'] as const
const settingsKeys = ['settings'] as const
const currencyKeys = ['currency'] as const
const filesKeys = ['files'] as const

export const queryKeys = {
  // Auth
  auth: {
    all: authKeys,
    user: () => [...authKeys, 'user'] as const,
    permissions: () => [...authKeys, 'permissions'] as const,
  },

  // Guests
  guests: {
    all: guestsKeys,
    lists: () => [...guestsKeys, 'list'] as const,
    list: (filters?: Record<string, any>) => [...guestsKeys, 'list', filters] as const,
    details: () => [...guestsKeys, 'detail'] as const,
    detail: (id: number) => [...guestsKeys, 'detail', id] as const,
  },

  // Transfers
  transfers: {
    all: transfersKeys,
    lists: () => [...transfersKeys, 'list'] as const,
    list: (filters?: Record<string, any>) => [...transfersKeys, 'list', filters] as const,
    details: () => [...transfersKeys, 'detail'] as const,
    detail: (id: number) => [...transfersKeys, 'detail', id] as const,
  },

  // Tours
  tours: {
    all: toursKeys,
    lists: () => [...toursKeys, 'list'] as const,
    list: (filters?: Record<string, any>) => [...toursKeys, 'list', filters] as const,
    cityTours: {
      all: [...toursKeys, 'city'] as const,
      details: () => [...toursKeys, 'city', 'detail'] as const,
      detail: (id: number) => [...toursKeys, 'city', 'detail', id] as const,
    },
    yachtTours: {
      all: [...toursKeys, 'yacht'] as const,
      details: () => [...toursKeys, 'yacht', 'detail'] as const,
      detail: (id: number) => [...toursKeys, 'yacht', 'detail', id] as const,
    },
  },

  // Hotels
  hotels: {
    all: hotelsKeys,
    lists: () => [...hotelsKeys, 'list'] as const,
    list: (filters?: Record<string, any>) => [...hotelsKeys, 'list', filters] as const,
    details: () => [...hotelsKeys, 'detail'] as const,
    detail: (id: number) => [...hotelsKeys, 'detail', id] as const,
  },

  // Restaurants
  restaurants: {
    all: restaurantsKeys,
    lists: () => [...restaurantsKeys, 'list'] as const,
    list: (filters?: Record<string, any>) => [...restaurantsKeys, 'list', filters] as const,
    details: () => [...restaurantsKeys, 'detail'] as const,
    detail: (id: number) => [...restaurantsKeys, 'detail', id] as const,
  },

  // Itineraries
  itineraries: {
    all: itinerariesKeys,
    lists: () => [...itinerariesKeys, 'list'] as const,
    list: (filters?: Record<string, any>) => [...itinerariesKeys, 'list', filters] as const,
    details: () => [...itinerariesKeys, 'detail'] as const,
    detail: (id: number) => [...itinerariesKeys, 'detail', id] as const,
    timeline: (id: number) => [...itinerariesKeys, 'detail', id, 'timeline'] as const,
  },

  // Invoices
  invoices: {
    all: invoicesKeys,
    lists: () => [...invoicesKeys, 'list'] as const,
    list: (filters?: Record<string, any>) => [...invoicesKeys, 'list', filters] as const,
    details: () => [...invoicesKeys, 'detail'] as const,
    detail: (id: number) => [...invoicesKeys, 'detail', id] as const,
  },

  // Reservations
  reservations: {
    all: reservationsKeys,
    lists: () => [...reservationsKeys, 'list'] as const,
    list: (filters?: Record<string, any>) => [...reservationsKeys, 'list', filters] as const,
    details: () => [...reservationsKeys, 'detail'] as const,
    detail: (id: number) => [...reservationsKeys, 'detail', id] as const,
  },

  // Personnel
  personnel: {
    all: personnelKeys,
    lists: () => [...personnelKeys, 'list'] as const,
    list: (filters?: Record<string, any>) => [...personnelKeys, 'list', filters] as const,
    details: () => [...personnelKeys, 'detail'] as const,
    detail: (id: number) => [...personnelKeys, 'detail', id] as const,
  },

  // Airports
  airports: {
    all: airportsKeys,
    lists: () => [...airportsKeys, 'list'] as const,
    list: (filters?: Record<string, any>) => [...airportsKeys, 'list', filters] as const,
    details: () => [...airportsKeys, 'detail'] as const,
    detail: (id: number) => [...airportsKeys, 'detail', id] as const,
  },

  // Cities
  cities: {
    all: citiesKeys,
    lists: () => [...citiesKeys, 'list'] as const,
    list: (filters?: Record<string, any>) => [...citiesKeys, 'list', filters] as const,
    details: () => [...citiesKeys, 'detail'] as const,
    detail: (id: number) => [...citiesKeys, 'detail', id] as const,
    dropdown: () => [...citiesKeys, 'dropdown'] as const,
  },

  // Vehicles
  vehicles: {
    all: vehiclesKeys,
    lists: () => [...vehiclesKeys, 'list'] as const,
    list: (filters?: Record<string, any>) => [...vehiclesKeys, 'list', filters] as const,
    details: () => [...vehiclesKeys, 'detail'] as const,
    detail: (id: number) => [...vehiclesKeys, 'detail', id] as const,
  },

  // Daily Notes
  dailyNotes: {
    all: dailyNotesKeys,
    lists: () => [...dailyNotesKeys, 'list'] as const,
    list: (filters?: Record<string, any>) => [...dailyNotesKeys, 'list', filters] as const,
  },

  // Daily Revenues
  dailyRevenues: {
    all: dailyRevenuesKeys,
    lists: () => [...dailyRevenuesKeys, 'list'] as const,
    list: (filters?: Record<string, any>) => [...dailyRevenuesKeys, 'list', filters] as const,
  },

  // Dashboard
  dashboard: {
    all: dashboardKeys,
    stats: () => [...dashboardKeys, 'stats'] as const,
    recent: () => [...dashboardKeys, 'recent'] as const,
  },

  // Reports
  reports: {
    all: reportsKeys,
    revenue: (filters?: Record<string, any>) => [...reportsKeys, 'revenue', filters] as const,
    statistics: (filters?: Record<string, any>) => [...reportsKeys, 'statistics', filters] as const,
  },

  // Settings
  settings: {
    all: settingsKeys,
    general: () => [...settingsKeys, 'general'] as const,
    currency: () => [...settingsKeys, 'currency'] as const,
  },

  // Currency
  currency: {
    all: currencyKeys,
    default: () => [...currencyKeys, 'default'] as const,
    supported: () => [...currencyKeys, 'supported'] as const,
    settings: () => [...currencyKeys, 'settings'] as const,
  },

  // Files
  files: {
    all: filesKeys,
    lists: () => [...filesKeys, 'list'] as const,
    list: (filters?: Record<string, any>) => [...filesKeys, 'list', filters] as const,
    categories: () => [...filesKeys, 'categories'] as const,
    statistics: () => [...filesKeys, 'statistics'] as const,
  },
}


import apiClient from './api'

export interface Currency {
  code: string
  name: string
  symbol: string
}

export interface CurrencySettings {
  defaultCurrency: string
  supportedCurrencies: Currency[]
}

export interface CurrencyValidation {
  isValid: boolean
  currencyCode: string
}

export interface CurrencySymbol {
  currencyCode: string
  symbol: string
}

export const currencyService = {
  getDefaultCurrency: async (): Promise<string> => {
    const response = await apiClient.get('/Currency/default')
    return response.data.data.currency
  },

  getSupportedCurrencies: async (): Promise<Currency[]> => {
    const response = await apiClient.get('/Currency/supported')
    return response.data.data
  },

  validateCurrency: async (currencyCode: string): Promise<CurrencyValidation> => {
    const response = await apiClient.get(`/Currency/validate/${currencyCode}`)
    return response.data.data
  },

  getCurrencySymbol: async (currencyCode: string): Promise<CurrencySymbol> => {
    const response = await apiClient.get(`/Currency/symbol/${currencyCode}`)
    return response.data.data
  },

  getCurrencySettings: async (): Promise<CurrencySettings> => {
    const response = await apiClient.get('/Configuration/currency')
    return response.data.data
  },
}


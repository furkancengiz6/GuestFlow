import api from './api'
import { PaymentMethod, PaymentStatus } from '../types/enums'

export interface CreatePaymentRequest {
  amount: number
  currency: string
  paymentMethod: PaymentMethod
  paymentDate: string
  guestId: number
  invoiceId?: number
  transferId?: number
  cityTourId?: number
  yachtTourId?: number
  notes?: string
}

export interface Payment {
  id: number
  amount: number
  currency: string
  paymentMethod: PaymentMethod
  paymentDate: string
  guestId: number
  invoiceId?: number
  transferId?: number
  cityTourId?: number
  yachtTourId?: number
  collectedByPersonnelId: number
  collectedByPersonnelName: string
  notes?: string
  status: PaymentStatus
  transactionId?: string
  createdDate: string
}

export interface PaymentSummary {
  totalPaid: number
  currency: string
  paymentMethodBreakdown: { [key in PaymentMethod]?: number }
  recentPayments: Payment[]
}

export const paymentService = {
  // Create a new payment
  createPayment: async (data: CreatePaymentRequest): Promise<Payment> => {
    const response = await api.post('/payments', data)
    return response.data.data
  },

  // Get payments by guest
  getPaymentsByGuest: async (guestId: number): Promise<Payment[]> => {
    const response = await api.get(`/payments/by-guest/${guestId}`)
    return response.data.data
  },

  // Get payment summary for guest
  getPaymentSummary: async (guestId: number): Promise<PaymentSummary> => {
    const response = await api.get(`/payments/by-guest/${guestId}/summary`)
    return response.data.data
  },

  // Get all payments with optional filters
  getPayments: async (params?: {
    guestId?: number
    invoiceId?: number
    startDate?: string
    endDate?: string
  }): Promise<Payment[]> => {
    const response = await api.get('/payments', { params })
    return response.data.data
  },

  // Get payment by ID
  getPayment: async (id: number): Promise<Payment> => {
    const response = await api.get(`/payments/${id}`)
    return response.data.data
  },

  // Create Stripe Payment Intent
  createPaymentIntent: async (data: {
    amount: number
    currency: string
    paymentMethodId: string
    guestId: number
    invoiceId?: number
  }): Promise<{ clientSecret: string }> => {
    const response = await api.post('/stripe/create-payment-intent', data)
    return response.data
  }
}

import apiClient from './api';
import { Supplier } from '../types/supplier';

export interface CreateSupplierRequest {
  name: string;
  type: string;
  contactName?: string;
  phoneNumber?: string;
  email?: string;
  address?: string;
  website?: string;
  notes?: string;
  isActive: boolean;
  defaultCurrency?: string;
  defaultCost?: number;
}

export interface UpdateSupplierRequest extends Partial<CreateSupplierRequest> {}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  errors?: string[];
}

export const supplierService = {
  // Get all suppliers
  getAll: async (type?: string, isActive?: boolean): Promise<ApiResponse<Supplier[]>> => {
    const params = new URLSearchParams();
    if (type) params.append('type', type);
    if (isActive !== undefined) params.append('isActive', isActive.toString());

    const response = await apiClient.get(`/suppliers?${params.toString()}`);
    return response.data;
  },

  // Get supplier by ID
  getById: async (id: number): Promise<ApiResponse<Supplier>> => {
    const response = await apiClient.get(`/suppliers/${id}`);
    return response.data;
  },

  // Get suppliers by type
  getByType: async (type: string): Promise<ApiResponse<Supplier[]>> => {
    const response = await apiClient.get(`/suppliers/by-type/${type}`);
    return response.data;
  },

  // Create new supplier
  create: async (data: CreateSupplierRequest): Promise<ApiResponse<Supplier>> => {
    const response = await apiClient.post('/suppliers', data);
    return response.data;
  },

  // Update supplier
  update: async (id: number, data: UpdateSupplierRequest): Promise<ApiResponse<Supplier>> => {
    const response = await apiClient.put(`/suppliers/${id}`, data);
    return response.data;
  },

  // Delete supplier
  delete: async (id: number): Promise<ApiResponse<boolean>> => {
    const response = await apiClient.delete(`/suppliers/${id}`);
    return response.data;
  },

  // Profitability reports
  getProfitabilityReport: async (
    startDate: Date,
    endDate: Date,
    supplierId?: string
  ): Promise<ApiResponse<any>> => {
    const params = new URLSearchParams({
      startDate: startDate.toISOString(),
      endDate: endDate.toISOString()
    });
    if (supplierId) params.append('supplierId', supplierId);

    const response = await apiClient.get(`/suppliers/profitability/report?${params.toString()}`);
    return response.data;
  },

  // Top suppliers by profit
  getTopSuppliersByProfit: async (
    startDate: Date,
    endDate: Date,
    topCount: number = 10
  ): Promise<ApiResponse<any>> => {
    const params = new URLSearchParams({
      startDate: startDate.toISOString(),
      endDate: endDate.toISOString(),
      topCount: topCount.toString()
    });

    const response = await apiClient.get(`/suppliers/profitability/top-suppliers?${params.toString()}`);
    return response.data;
  }
};
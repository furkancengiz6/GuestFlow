export interface Supplier {
  id: number;
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
  isDeleted: boolean;
  createdDate: string;
  updatedDate: string;
  createdBy?: string;
  updatedBy?: string;
}

export interface SupplierCost {
  id: number;
  supplierId: number;
  transferId?: number;
  cityTourId?: number;
  yachtTourId?: number;
  restaurantReservationId?: number;
  costAmount: number;
  currency: string;
  costType?: string;
  description?: string;
  validFrom?: string;
  validTo?: string;
  isActive: boolean;
  isDeleted: boolean;
  createdDate: string;
  updatedDate: string;
  createdBy?: string;
  updatedBy?: string;
}

export type SupplierType = 'Yacht' | 'Transfer' | 'Restaurant' | 'Activity' | 'Hotel' | 'General';

export type CostType = 'BaseCost' | 'AdditionalFee' | 'Commission' | 'FuelCost' | 'DriverFee' | 'EquipmentCost' | 'SeasonalAdjustment' | 'Discount';
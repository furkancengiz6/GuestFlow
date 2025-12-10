export interface Invoice {
  id: number
  invoiceNumber: number
  totalAmount: number
  issueDate: string
  currency: string
  notes: string
  pdfUrl: string
  guestId: number
  personnelId?: number
  transferId?: number
  cityTourId?: number
  yachtTourId?: number
  createdDate: string
}

export interface PagedInvoices {
  data: Invoice[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
  isFirstPage: boolean
  isLastPage: boolean
}

export interface InvoiceGuest {
  id: number
  fullName: string
  guestCode: string
  email?: string
  phoneNumber?: string
  nationality: string
  isSpecialGuest: boolean
}

export interface InvoicePersonnel {
  id: number
  fullName: string
  email?: string
  userType: string
}

export interface InvoiceService {
  serviceType: string
  serviceId: number
  serviceName?: string
  serviceDate?: string
  serviceAmount?: number
  additionalInfo?: string
}

export interface InvoiceDetail {
  id: number
  invoiceNumber: number
  issueDate: string
  totalAmount: number
  currency: string
  notes?: string
  pdfUrl: string
  hasPdf: boolean
  createdDate: string
  guest?: InvoiceGuest
  personnel?: InvoicePersonnel
  service?: InvoiceService
}


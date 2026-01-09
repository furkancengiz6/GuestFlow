import { transferService } from '../../services/transferService';

// Mock the entire api module
jest.mock('../../services/api', () => ({
  __esModule: true,
  default: {
    get: jest.fn(),
    post: jest.fn(),
    put: jest.fn(),
    patch: jest.fn(),
    delete: jest.fn(),
  },
}));

describe('TransferService', () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  describe('getTransfers', () => {
    it('should fetch transfers with pagination', async () => {
      const mockResponse = {
        data: {
          data: [
            {
              id: 1,
              guestName: 'John Doe',
              transferDate: '2024-01-15T10:00:00Z',
              status: 'Confirmed',
              price: 150.00,
            },
          ],
          totalCount: 1,
          pageNumber: 1,
          pageSize: 10,
        },
      };

      // Mock the api response
      const mockApi = require('../../services/api').default;
      mockApi.get.mockResolvedValue(mockResponse);

      const result = await transferService.getTransfers(1, 10);

      expect(mockApi.get).toHaveBeenCalledWith('/Transfers', {
        params: { pageNumber: 1, pageSize: 10 },
      });
      expect(result).toEqual(mockResponse.data);
    });

    it('should handle filters in getTransfers', async () => {
      const filters = {
        startDate: '2024-01-01',
        endDate: '2024-01-31',
        status: 'Confirmed',
        guestId: 1,
      };

      const mockResponse = { data: { data: [], totalCount: 0 } };
      (apiClient.get as any).mockResolvedValue(mockResponse);

      await transferService.getTransfers(1, 10, filters);

      expect(apiClient.get).toHaveBeenCalledWith('/Transfers', {
        params: {
          pageNumber: 1,
          pageSize: 10,
          startDate: '2024-01-01',
          endDate: '2024-01-31',
          status: 'Confirmed',
          guestId: 1,
        },
      });
    });
  });

  describe('getTransferDetail', () => {
    it('should fetch transfer detail by id', async () => {
      const mockTransfer = {
        id: 1,
        guestName: 'John Doe',
        transferDate: '2024-01-15T10:00:00Z',
        pickupAddress: 'Airport',
        dropoffAddress: 'Hotel',
        status: 'Confirmed',
        price: 150.00,
        driverName: 'Driver One',
        vehiclePlate: 'ABC123',
      };

      const mockResponse = { data: mockTransfer };
      (apiClient.get as any).mockResolvedValue(mockResponse);

      const result = await transferService.getTransferDetail(1);

      expect(apiClient.get).toHaveBeenCalledWith('/Transfers/1');
      expect(result).toEqual(mockTransfer);
    });
  });

  describe('createTransfer', () => {
    it('should create a new transfer', async () => {
      const transferData = {
        guestId: 1,
        transferDate: '2024-01-15T10:00:00Z',
        pickupAddress: 'Airport',
        dropoffAddress: 'Hotel',
        price: 150.00,
      };

      const mockResponse = {
        data: {
          data: {
            transferId: 1,
            invoiceId: null,
          },
        },
      };

      (apiClient.post as any).mockResolvedValue(mockResponse);

      const result = await transferService.createTransfer(transferData);

      expect(apiClient.post).toHaveBeenCalledWith('/Transfers', transferData);
      expect(result).toEqual(mockResponse.data.data);
    });
  });

  describe('updateTransfer', () => {
    it('should update an existing transfer', async () => {
      const transferId = 1;
      const updateData = {
        pickupAddress: 'Updated Airport',
        price: 200.00,
      };

      const mockResponse = { data: null };
      (apiClient.put as any).mockResolvedValue(mockResponse);

      await transferService.updateTransfer(transferId, updateData);

      expect(apiClient.put).toHaveBeenCalledWith(`/Transfers/${transferId}`, updateData);
    });
  });

  describe('markTransferCompleted', () => {
    it('should mark transfer as completed', async () => {
      const transferId = 1;
      const mockResponse = { data: null };
      (apiClient.post as any).mockResolvedValue(mockResponse);

      await transferService.markTransferCompleted(transferId);

      expect(apiClient.post).toHaveBeenCalledWith(`/Transfers/${transferId}/complete`);
    });
  });

  describe('createTransferInvoice', () => {
    it('should create invoice for transfer', async () => {
      const transferId = 1;
      const mockResponse = {
        data: {
          invoiceId: 123,
          pdfUrl: '/invoices/123.pdf',
        },
      };

      (apiClient.post as any).mockResolvedValue(mockResponse);

      const result = await transferService.createTransferInvoice(transferId);

      expect(apiClient.post).toHaveBeenCalledWith(`/Transfers/${transferId}/invoice`);
      expect(result).toEqual(mockResponse.data);
    });
  });

  describe('bulkUpdateTransfers', () => {
    it('should bulk update transfers', async () => {
      const bulkData = {
        transferIds: [1, 2, 3],
        operation: 'status_change' as const,
        newStatus: 'Completed',
      };

      const mockResponse = {
        data: {
          successCount: 3,
          failCount: 0,
          message: 'All transfers updated successfully',
        },
      };

      (apiClient.post as any).mockResolvedValue(mockResponse);

      const result = await transferService.bulkUpdateTransfers(bulkData);

      expect(apiClient.post).toHaveBeenCalledWith('/Transfers/bulk-update', bulkData);
      expect(result).toEqual(mockResponse.data);
    });
  });

  describe('bulkDeleteTransfers', () => {
    it('should bulk delete transfers', async () => {
      const transferIds = [1, 2, 3];

      const mockResponse = {
        data: {
          successCount: 3,
          failCount: 0,
          message: 'All transfers deleted successfully',
        },
      };

      (apiClient.post as any).mockResolvedValue(mockResponse);

      const result = await transferService.bulkDeleteTransfers(transferIds);

      expect(apiClient.post).toHaveBeenCalledWith('/Transfers/bulk-delete', {
        transferIds,
      });
      expect(result).toEqual(mockResponse.data);
    });
  });

  describe('error handling', () => {
    it('should handle API errors gracefully', async () => {
      const error = new Error('Network error');
      (apiClient.get as any).mockRejectedValue(error);

      await expect(transferService.getTransfers(1, 10)).rejects.toThrow('Network error');
    });

    it('should handle validation errors', async () => {
      const mockResponse = {
        data: {
          errors: {
            TransferDate: ['Transfer date cannot be in the past'],
            Price: ['Price must be greater than 0'],
          },
        },
      };

      (apiClient.post as any).mockResolvedValue(mockResponse);

      const transferData = {
        guestId: 1,
        transferDate: '2020-01-01T10:00:00Z', // Past date
        price: -50, // Invalid price
      };

      await expect(transferService.createTransfer(transferData)).rejects.toThrow();
    });
  });
});

import { transferService } from '../../services/transferService'

jest.mock('../../services/api', () => ({
  __esModule: true,
  default: {
    get: jest.fn(),
    post: jest.fn(),
    put: jest.fn(),
    patch: jest.fn(),
    delete: jest.fn(),
  },
}))

const apiClient = require('../../services/api').default

describe('transferService', () => {
  beforeEach(() => {
    jest.clearAllMocks()
  })

  it('getTransfers: calls GET /Transfers with params and returns response.data', async () => {
    apiClient.get.mockResolvedValue({ data: { data: [], totalCount: 0 } })

    const result = await transferService.getTransfers(1, 10, { status: 'Confirmed', guestId: 1 })

    expect(apiClient.get).toHaveBeenCalledWith('/Transfers', {
      params: { pageNumber: 1, pageSize: 10, status: 'Confirmed', guestId: 1 },
    })
    expect(result).toEqual({ data: [], totalCount: 0 })
  })

  it('getTransferDetail: calls GET /Transfers/:id/detail and returns response.data.data', async () => {
    apiClient.get.mockResolvedValue({ data: { data: { id: 1 } } })

    const result = await transferService.getTransferDetail(1)

    expect(apiClient.get).toHaveBeenCalledWith('/Transfers/1/detail')
    expect(result).toEqual({ id: 1 })
  })

  it('createTransfer: calls POST /Transfers and returns response.data.data', async () => {
    const payload = { guestId: 1, transferDate: '2026-01-01T10:00:00Z', pickupAddress: 'A', dropoffAddress: 'B', price: 100 }
    apiClient.post.mockResolvedValue({ data: { data: { transferId: 123 } } })

    const result = await transferService.createTransfer(payload as any)

    expect(apiClient.post).toHaveBeenCalledWith('/Transfers', payload)
    expect(result).toEqual({ transferId: 123 })
  })

  it('updateTransfer: calls PUT /Transfers/:id and returns response.data.data', async () => {
    const payload = { guestId: 1, transferDate: '2026-01-01T10:00:00Z', pickupAddress: 'A', dropoffAddress: 'B', price: 100 }
    apiClient.put.mockResolvedValue({ data: { data: { id: 1 } } })

    const result = await transferService.updateTransfer(1, payload as any)

    expect(apiClient.put).toHaveBeenCalledWith('/Transfers/1', payload)
    expect(result).toEqual({ id: 1 })
  })

  it('markTransferCompleted: calls PATCH /Transfers/:id/status with Completed', async () => {
    apiClient.patch.mockResolvedValue({ data: {} })

    await transferService.markTransferCompleted(1)

    expect(apiClient.patch).toHaveBeenCalledWith('/Transfers/1/status', { status: 'Completed' })
  })

  it('createTransferInvoice: calls POST /Transfers/:id/invoice', async () => {
    apiClient.post.mockResolvedValue({ data: {} })

    await transferService.createTransferInvoice(1)

    expect(apiClient.post).toHaveBeenCalledWith('/Transfers/1/invoice')
  })

  it('bulkUpdateTransfers: calls POST /Transfers/bulk-update and returns response.data.data', async () => {
    apiClient.post.mockResolvedValue({ data: { data: { successCount: 1, failureCount: 0, errors: [] } } })

    const result = await transferService.bulkUpdateTransfers({ operation: 'status_change', transferIds: [1], newStatus: 'Completed' } as any)

    expect(apiClient.post).toHaveBeenCalledWith('/Transfers/bulk-update', { operation: 'status_change', transferIds: [1], newStatus: 'Completed' })
    expect(result).toEqual({ successCount: 1, failureCount: 0, errors: [] })
  })

  it('bulkDeleteTransfers: calls POST /Transfers/bulk-delete and returns response.data.data', async () => {
    apiClient.post.mockResolvedValue({ data: { data: { successCount: 1, failureCount: 0, errors: [] } } })

    const result = await transferService.bulkDeleteTransfers([1], 'test')

    expect(apiClient.post).toHaveBeenCalledWith('/Transfers/bulk-delete', { transferIds: [1], reason: 'test' })
    expect(result).toEqual({ successCount: 1, failureCount: 0, errors: [] })
  })
})

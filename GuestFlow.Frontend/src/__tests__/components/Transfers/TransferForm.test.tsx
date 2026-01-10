import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import TransferForm from '../../../components/Transfers/TransferForm'

jest.mock('@tanstack/react-query', () => {
  const actual = jest.requireActual('@tanstack/react-query')
  return {
    ...actual,
    useQuery: ({ queryKey }: any) => {
      const key = Array.isArray(queryKey) ? queryKey[0] : queryKey
      if (key === 'guests-dropdown') {
        return { data: [{ id: 1, fullName: 'John Doe', guestCode: 'G001' }] }
      }
      if (key === 'airports-dropdown') return { data: [] }
      if (key === 'vehicles-dropdown') return { data: [] }
      if (key === 'cities-dropdown') return { data: [] }
      return { data: undefined }
    },
  }
})

jest.mock('../../../services/dropdownService', () => ({
  dropdownService: {
    getGuests: jest.fn().mockResolvedValue([{ id: 1, fullName: 'John Doe', guestCode: 'G001' }]),
    getVehicles: jest.fn().mockResolvedValue([]),
    getAirports: jest.fn().mockResolvedValue([]),
    getCities: jest.fn().mockResolvedValue([]),
  },
}))

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  })

  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  )
}

describe('TransferForm', () => {
  beforeEach(() => {
    jest.clearAllMocks()
  })

  it('renders create dialog title and actions', () => {
    render(
      <TransferForm open onClose={jest.fn()} onSubmit={jest.fn() as any} transfer={null} />,
      { wrapper: createWrapper() }
    )

    expect(screen.getByText('Yeni Transfer Ekle')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'İptal' })).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Ekle' })).toBeInTheDocument()
  })

  it('submits in edit mode, transforms transferDate to ISO, and closes dialog', async () => {
    const onClose = jest.fn()
    const onSubmit = jest.fn().mockResolvedValue(undefined)

    const transfer = {
      id: 99,
      guestId: 1,
      transferDate: '2026-01-02T10:00:00Z',
      pickupAddress: 'Airport',
      dropoffAddress: 'Hotel',
      price: 123.45,
      status: 'Pending',
    }

    render(<TransferForm open onClose={onClose} onSubmit={onSubmit} transfer={transfer as any} />, {
      wrapper: createWrapper(),
    })

    fireEvent.click(screen.getByRole('button', { name: 'Güncelle' }))

    await waitFor(() => {
      expect(onSubmit).toHaveBeenCalled()
    })

    const submitted = (onSubmit as jest.Mock).mock.calls[0][0]
    expect(submitted).toMatchObject({
      pickupAddress: 'Airport',
      dropoffAddress: 'Hotel',
      price: 123.45,
      guestId: 1,
      status: 'Pending',
    })
    expect(typeof submitted.transferDate).toBe('string')
    expect(submitted.transferDate).toContain('T')

    await waitFor(() => {
      expect(onClose).toHaveBeenCalled()
    })
  })
})

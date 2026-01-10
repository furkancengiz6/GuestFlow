import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import LoginPage from '../../pages/Auth/LoginPage'
import { useAuthStore } from '../../stores/authStore'

const mockNavigate = jest.fn()
jest.mock('react-router-dom', () => {
  const actual = jest.requireActual('react-router-dom')
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  }
})

const apiPost = jest.fn()
const apiGet = jest.fn()
jest.mock('../../services/api', () => ({
  __esModule: true,
  default: {
    post: (...args: any[]) => apiPost(...args),
    get: (...args: any[]) => apiGet(...args),
  },
}))

const storeLoginHook = jest.fn()
const storeLoginGetState = jest.fn()
const storeLogout = jest.fn()

jest.mock('../../stores/authStore', () => {
  const fn = jest.fn(() => ({
    user: null,
    isAuthenticated: false,
    login: storeLoginHook,
    logout: storeLogout,
  }))
  ;(fn as any).getState = jest.fn(() => ({
    login: storeLoginGetState,
    logout: storeLogout,
    user: null,
    isAuthenticated: false,
  }))
  return { useAuthStore: fn }
})

describe('Auth Integration (LoginPage)', () => {
  let queryClient: QueryClient

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    })
    jest.clearAllMocks()
    window.localStorage.clear()
  })

  it('handles login flow: POST /auth/login, GET /auth/me, updates store, navigates to dashboard', async () => {
    const user = userEvent.setup()

    apiPost.mockResolvedValue({ data: { accessToken: 'mock-token' } })
    apiGet.mockResolvedValue({
      data: {
        data: {
          id: 1,
          email: 'test@example.com',
          fullName: 'Test User',
          userType: 'Admin',
          createdDate: '2026-01-01T00:00:00Z',
        },
      },
    })

    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter>
          <LoginPage />
        </MemoryRouter>
      </QueryClientProvider>
    )

    await user.type(screen.getByLabelText('E-posta'), 'test@example.com')
    await user.type(screen.getByLabelText('Şifre'), 'password123')
    await user.click(screen.getByRole('button', { name: 'Giriş Yap' }))

    await waitFor(() => {
      expect(apiPost).toHaveBeenCalledWith('/auth/login', {
        email: 'test@example.com',
        password: 'password123',
      })
    })

    await waitFor(() => {
      expect(storeLoginHook).toHaveBeenCalledWith('mock-token', null)
      expect(apiGet).toHaveBeenCalledWith('/auth/me')
      expect(storeLoginGetState).toHaveBeenCalledWith('mock-token', {
        id: 1,
        email: 'test@example.com',
        fullName: 'Test User',
        role: 'Admin',
        userType: 'Admin',
        createdDate: '2026-01-01T00:00:00Z',
      })
      expect(mockNavigate).toHaveBeenCalledWith('/dashboard')
    })
  })

  it('shows error message when login fails', async () => {
    const user = userEvent.setup()

    apiPost.mockRejectedValue({
      response: { data: { message: 'Invalid credentials' } },
    })

    const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation(() => {})
    try {
      render(
        <QueryClientProvider client={queryClient}>
          <MemoryRouter>
            <LoginPage />
          </MemoryRouter>
        </QueryClientProvider>
      )

      await user.type(screen.getByLabelText('E-posta'), 'wrong@example.com')
      await user.type(screen.getByLabelText('Şifre'), 'wrongpassword')
      await user.click(screen.getByRole('button', { name: 'Giriş Yap' }))

      expect(await screen.findByText('Invalid credentials')).toBeInTheDocument()
      expect(mockNavigate).not.toHaveBeenCalledWith('/dashboard')
    } finally {
      consoleErrorSpy.mockRestore()
    }
  })

  it('navigates to dashboard immediately when auth storage exists', async () => {
    window.localStorage.setItem('auth-storage', JSON.stringify({ state: { isAuthenticated: true } }))

    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter>
          <LoginPage />
        </MemoryRouter>
      </QueryClientProvider>
    )

    await waitFor(() => {
      expect(mockNavigate).toHaveBeenCalledWith('/dashboard')
    })
  })
})


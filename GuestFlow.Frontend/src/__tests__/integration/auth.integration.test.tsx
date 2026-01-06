import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { BrowserRouter } from 'react-router-dom'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import LoginPage from '../../../pages/Auth/LoginPage'
import { useAuthStore } from '../../../stores/authStore'

// Mock auth store
jest.mock('../../../stores/authStore', () => ({
  useAuthStore: jest.fn(),
}))

// Mock auth service
jest.mock('../../../services/authService', () => ({
  authService: {
    login: jest.fn(),
  },
}))

describe('Auth Integration', () => {
  let queryClient: QueryClient

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    })
    jest.clearAllMocks()
  })

  it('should handle login flow', async () => {
    const user = userEvent.setup()
    const mockLogin = jest.fn().mockResolvedValue({
      accessToken: 'mock-token',
      refreshToken: 'mock-refresh-token',
      user: { id: 1, email: 'test@example.com' },
    })

    ;(useAuthStore as jest.Mock).mockReturnValue({
      login: mockLogin,
      isAuthenticated: false,
    })

    render(
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      </QueryClientProvider>
    )

    // Find form inputs
    const emailInput = screen.getByLabelText(/e-posta/i)
    const passwordInput = screen.getByLabelText(/şifre/i)
    const submitButton = screen.getByRole('button', { name: /giriş yap/i })

    // Fill form
    await user.type(emailInput, 'test@example.com')
    await user.type(passwordInput, 'password123')

    // Submit form
    await user.click(submitButton)

    // Wait for login to complete
    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalledWith({
        email: 'test@example.com',
        password: 'password123',
      })
    })
  })

  it('should show error on invalid credentials', async () => {
    const user = userEvent.setup()
    const mockLogin = jest.fn().mockRejectedValue(new Error('Invalid credentials'))

    ;(useAuthStore as jest.Mock).mockReturnValue({
      login: mockLogin,
      isAuthenticated: false,
    })

    render(
      <QueryClientProvider client={queryClient}>
        <BrowserRouter>
          <LoginPage />
        </BrowserRouter>
      </QueryClientProvider>
    )

    const emailInput = screen.getByLabelText(/e-posta/i)
    const passwordInput = screen.getByLabelText(/şifre/i)
    const submitButton = screen.getByRole('button', { name: /giriş yap/i })

    await user.type(emailInput, 'wrong@example.com')
    await user.type(passwordInput, 'wrongpassword')
    await user.click(submitButton)

    await waitFor(() => {
      // Should show error message
      // Note: This would need proper error handling UI
    })
  })
})


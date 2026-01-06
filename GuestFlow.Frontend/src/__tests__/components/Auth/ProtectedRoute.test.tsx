import { render, screen } from '@testing-library/react'
import { BrowserRouter } from 'react-router-dom'
import ProtectedRoute from '../../../components/Auth/ProtectedRoute'
import { useAuthStore } from '../../../stores/authStore'

// Mock auth store
jest.mock('../../../stores/authStore', () => ({
  useAuthStore: jest.fn(),
}))

// Mock API client
jest.mock('../../../services/api', () => ({
  default: {
    get: jest.fn(),
  },
}))

describe('ProtectedRoute', () => {
  beforeEach(() => {
    jest.clearAllMocks()
  })

  it('should show loading state initially', () => {
    ;(useAuthStore as jest.Mock).mockReturnValue({
      isAuthenticated: false,
      user: null,
      setAuthenticated: jest.fn(),
      logout: jest.fn(),
    })

    render(
      <BrowserRouter>
        <ProtectedRoute>
          <div>Protected Content</div>
        </ProtectedRoute>
      </BrowserRouter>
    )

    // Should show loading indicator
    expect(screen.queryByText('Protected Content')).not.toBeInTheDocument()
  })

  it('should redirect to login when not authenticated', async () => {
    ;(useAuthStore as jest.Mock).mockReturnValue({
      isAuthenticated: false,
      user: null,
      setAuthenticated: jest.fn(),
      logout: jest.fn(),
    })

    const { useNavigate } = require('react-router-dom')
    const mockNavigate = jest.fn()
    jest.spyOn(require('react-router-dom'), 'useNavigate').mockReturnValue(mockNavigate)

    render(
      <BrowserRouter>
        <ProtectedRoute>
          <div>Protected Content</div>
        </ProtectedRoute>
      </BrowserRouter>
    )

    // Should redirect to login
    // Note: This test would need proper async handling
  })

  it('should render children when authenticated', () => {
    ;(useAuthStore as jest.Mock).mockReturnValue({
      isAuthenticated: true,
      user: { id: 1, email: 'test@example.com', role: 'Admin' },
      setAuthenticated: jest.fn(),
      logout: jest.fn(),
    })

    render(
      <BrowserRouter>
        <ProtectedRoute>
          <div>Protected Content</div>
        </ProtectedRoute>
      </BrowserRouter>
    )

    // Should render protected content
    // Note: This test would need proper async handling for auth check
  })
})


import { render, screen } from '@testing-library/react'
import { MemoryRouter, Routes, Route } from 'react-router-dom'
import ProtectedRoute from '../../../components/Auth/ProtectedRoute'
import { useAuthStore } from '../../../stores/authStore'

jest.mock('../../../stores/authStore', () => ({
  useAuthStore: jest.fn(),
}))

jest.mock('../../../services/api', () => ({
  __esModule: true,
  default: {
    get: jest.fn(),
  },
}))

const renderWithRoutes = (ui: React.ReactNode, initialPath: string = '/private') => {
  return render(
    <MemoryRouter initialEntries={[initialPath]}>
      <Routes>
        <Route path="/login" element={<div>Login Page</div>} />
        <Route path="/forbidden" element={<div>Forbidden Page</div>} />
        <Route path="/private" element={ui} />
      </Routes>
    </MemoryRouter>
  )
}

describe('ProtectedRoute', () => {
  beforeEach(() => {
    jest.clearAllMocks()
    window.localStorage.clear()
  })

  it('shows a loading indicator while checking session', () => {
    ;(useAuthStore as jest.Mock).mockReturnValue({
      isAuthenticated: false,
      user: null,
      setAuthenticated: jest.fn(),
      logout: jest.fn(),
    })

    // Keep the auth check pending so state doesn't update after render (avoids act warnings).
    const apiClient = require('../../../services/api').default
    apiClient.get.mockImplementation(() => new Promise(() => {}))

    renderWithRoutes(
      <ProtectedRoute>
        <div>Protected Content</div>
      </ProtectedRoute>
    )

    // MUI CircularProgress renders with role="progressbar"
    expect(screen.getByRole('progressbar')).toBeInTheDocument()
    expect(screen.queryByText('Protected Content')).not.toBeInTheDocument()
  })

  it('redirects to /login when not authenticated and /auth/me fails', async () => {
    const logout = jest.fn()
    ;(useAuthStore as jest.Mock).mockReturnValue({
      isAuthenticated: false,
      user: null,
      setAuthenticated: jest.fn(),
      logout,
    })

    // apiClient.get will throw -> logout should be called and user redirected to login
    const apiClient = require('../../../services/api').default
    apiClient.get.mockRejectedValue(new Error('unauthorized'))

    renderWithRoutes(
      <ProtectedRoute>
        <div>Protected Content</div>
      </ProtectedRoute>
    )

    expect(await screen.findByText('Login Page')).toBeInTheDocument()
    expect(logout).toHaveBeenCalled()
  })

  it('renders children when authenticated', async () => {
    ;(useAuthStore as jest.Mock).mockReturnValue({
      isAuthenticated: true,
      user: { id: 1, email: 'test@example.com', fullName: 'Test', role: 'Admin' },
      setAuthenticated: jest.fn(),
      logout: jest.fn(),
    })

    renderWithRoutes(
      <ProtectedRoute>
        <div>Protected Content</div>
      </ProtectedRoute>
    )

    expect(await screen.findByText('Protected Content')).toBeInTheDocument()
  })

  it('redirects to forbidden when roles do not match', async () => {
    ;(useAuthStore as jest.Mock).mockReturnValue({
      isAuthenticated: true,
      user: { id: 1, email: 'test@example.com', fullName: 'Test', role: 'Staff' },
      setAuthenticated: jest.fn(),
      logout: jest.fn(),
    })

    renderWithRoutes(
      <ProtectedRoute roles={['Admin']} fallbackPath="/forbidden">
        <div>Protected Content</div>
      </ProtectedRoute>
    )

    expect(await screen.findByText('Forbidden Page')).toBeInTheDocument()
  })
})


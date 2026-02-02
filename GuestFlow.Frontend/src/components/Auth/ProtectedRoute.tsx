import { ReactNode, useEffect, useState } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import apiClient from '../../services/api'
import { useAuthStore } from '../../stores/authStore'
import { CircularProgress, Box } from '@mui/material'

type Props = {
  children: ReactNode
  roles?: string[]
  fallbackPath?: string
}

const ProtectedRoute = ({ children, roles, fallbackPath = '/forbidden' }: Props) => {
  const location = useLocation()
  const { isAuthenticated, setAuthenticated, logout, user } = useAuthStore()
  const [checking, setChecking] = useState(true)
  // E2E bypass toggle (set localStorage VITE_E2E_BYPASS=true in dev/test to bypass auth).
  // NOTE: We intentionally avoid `import.meta` here because Jest (CJS transform) can't parse it.
  const e2eBypass =
    typeof window !== 'undefined' &&
    (() => {
      try {
        return window.localStorage.getItem('VITE_E2E_BYPASS') === 'true'
      } catch {
        return false
      }
    })()

  useEffect(() => {
    let cancelled = false
    const ensureSession = async () => {
      // 1. E2E Bypass
      if (e2eBypass) {
        const mockedUser = { id: 1, email: 'test@guestflow.local', fullName: 'Test User', role: 'Admin' }
        setAuthenticated(mockedUser)
        setChecking(false)
        return
      }

      // 2. If we already have a user in state, we are likely fine for now.
      // The global API interceptor will handle any later 401s.
      if (isAuthenticated && user) {
        setChecking(false)
        return
      }

      // 3. Fallback: Check /auth/me to sync session
      try {
        const res = await apiClient.get('/auth/me')
        const data = res.data.data || res.data
        if (!cancelled) {
          setAuthenticated({
            id: data.id,
            email: data.email,
            fullName: data.fullName,
            role: data.userType || data.role,
            userType: data.userType,
            createdDate: data.createdDate,
          })
        }
      } catch (err) {
        // If /auth/me fails, the interceptor will have tried to refresh.
        // If we're here, it means both failed.
        if (!cancelled) {
          logout()
        }
      } finally {
        if (!cancelled) setChecking(false)
      }
    }

    ensureSession()
    return () => {
      cancelled = true
    }
  }, [e2eBypass, isAuthenticated, logout, setAuthenticated, user])

  if (checking) {
    return (
      <Box sx={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <CircularProgress />
      </Box>
    )
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  if (roles && roles.length > 0) {
    const userRole = user?.role || user?.userType
    if (!userRole || !roles.includes(userRole)) {
      return <Navigate to={fallbackPath} replace state={{ from: location, reason: 'forbidden' }} />
    }
  }

  return <>{children}</>
}

export default ProtectedRoute


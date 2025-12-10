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

  useEffect(() => {
    let cancelled = false
    const ensureSession = async () => {
      try {
        if (isAuthenticated) return
        const res = await apiClient.get('/auth/me', { withCredentials: true })
        const data = res.data.data || res.data
        const usr = {
          id: data.id,
          email: data.email,
          fullName: data.fullName,
          role: data.userType || data.role,
          userType: data.userType,
          createdDate: data.createdDate,
        }
        if (!cancelled) {
          setAuthenticated(usr)
        }
      } catch (err) {
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
  }, [isAuthenticated, logout, setAuthenticated])

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


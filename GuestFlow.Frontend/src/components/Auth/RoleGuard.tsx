import { ReactNode } from 'react'
import { useAuthStore } from '../../stores/authStore'

interface RoleGuardProps {
  children: ReactNode
  roles: string[]
  fallback?: ReactNode
}

/**
 * Component that conditionally renders children based on user role
 */
export const RoleGuard = ({ children, roles, fallback = null }: RoleGuardProps) => {
  const { user } = useAuthStore()
  const userRole = user?.role || user?.userType

  const hasAccess = userRole ? roles.includes(userRole) : false

  return <>{hasAccess ? children : fallback}</>
}

export default RoleGuard


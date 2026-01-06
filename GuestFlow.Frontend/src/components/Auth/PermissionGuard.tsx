import { ReactNode } from 'react'
import { usePermissions } from '../../hooks/usePermissions'
import { Permission } from '../../utils/permissions'

interface PermissionGuardProps {
  children: ReactNode
  permission?: Permission
  permissions?: Permission[]
  requireAll?: boolean // If true, requires all permissions; if false, requires any
  fallback?: ReactNode
}

/**
 * Component that conditionally renders children based on user permissions
 */
export const PermissionGuard = ({
  children,
  permission,
  permissions,
  requireAll = false,
  fallback = null,
}: PermissionGuardProps) => {
  const { checkPermission, checkAnyPermission, checkAllPermissions } = usePermissions()

  let hasAccess = false

  if (permission) {
    hasAccess = checkPermission(permission)
  } else if (permissions && permissions.length > 0) {
    hasAccess = requireAll ? checkAllPermissions(permissions) : checkAnyPermission(permissions)
  } else {
    // No permission specified, allow access
    hasAccess = true
  }

  return <>{hasAccess ? children : fallback}</>
}

export default PermissionGuard


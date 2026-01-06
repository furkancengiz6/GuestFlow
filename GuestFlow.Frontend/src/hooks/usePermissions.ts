import { useAuthStore } from '../stores/authStore'
import { Permission, hasPermission, hasAnyPermission, hasAllPermissions, getRolePermissions } from '../utils/permissions'

/**
 * Hook to check user permissions
 */
export const usePermissions = () => {
  const { user } = useAuthStore()
  const userRole = user?.role || user?.userType

  const checkPermission = (permission: Permission): boolean => {
    return hasPermission(userRole, permission)
  }

  const checkAnyPermission = (permissions: Permission[]): boolean => {
    return hasAnyPermission(userRole, permissions)
  }

  const checkAllPermissions = (permissions: Permission[]): boolean => {
    return hasAllPermissions(userRole, permissions)
  }

  const getUserPermissions = (): Permission[] => {
    return getRolePermissions(userRole)
  }

  return {
    checkPermission,
    checkAnyPermission,
    checkAllPermissions,
    getUserPermissions,
    userRole,
  }
}


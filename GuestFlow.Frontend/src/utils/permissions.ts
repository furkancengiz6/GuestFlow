/**
 * Permission system for role-based access control
 */

export enum Permission {
  // Guest permissions
  GUEST_VIEW = 'guest:view',
  GUEST_CREATE = 'guest:create',
  GUEST_EDIT = 'guest:edit',
  GUEST_DELETE = 'guest:delete',

  // Transfer permissions
  TRANSFER_VIEW = 'transfer:view',
  TRANSFER_CREATE = 'transfer:create',
  TRANSFER_EDIT = 'transfer:edit',
  TRANSFER_DELETE = 'transfer:delete',

  // Tour permissions
  TOUR_VIEW = 'tour:view',
  TOUR_CREATE = 'tour:create',
  TOUR_EDIT = 'tour:edit',
  TOUR_DELETE = 'tour:delete',

  // Invoice permissions
  INVOICE_VIEW = 'invoice:view',
  INVOICE_CREATE = 'invoice:create',
  INVOICE_EDIT = 'invoice:edit',
  INVOICE_DELETE = 'invoice:delete',

  // Hotel permissions
  HOTEL_VIEW = 'hotel:view',
  HOTEL_CREATE = 'hotel:create',
  HOTEL_EDIT = 'hotel:edit',
  HOTEL_DELETE = 'hotel:delete',

  // Restaurant permissions
  RESTAURANT_VIEW = 'restaurant:view',
  RESTAURANT_CREATE = 'restaurant:create',
  RESTAURANT_EDIT = 'restaurant:edit',
  RESTAURANT_DELETE = 'restaurant:delete',

  // Itinerary permissions
  ITINERARY_VIEW = 'itinerary:view',
  ITINERARY_CREATE = 'itinerary:create',
  ITINERARY_EDIT = 'itinerary:edit',
  ITINERARY_DELETE = 'itinerary:delete',

  // Admin permissions
  ADMIN_VIEW = 'admin:view',
  ADMIN_EDIT = 'admin:edit',
  ADMIN_DELETE = 'admin:delete',

  // Settings permissions
  SETTINGS_VIEW = 'settings:view',
  SETTINGS_EDIT = 'settings:edit',

  // Reports permissions
  REPORTS_VIEW = 'reports:view',
  REPORTS_EXPORT = 'reports:export',
}

/**
 * Role to permissions mapping
 */
export const rolePermissions: Record<string, Permission[]> = {
  Owner: [
    // Full system access - business owner
    ...Object.values(Permission),
  ],
  Manager: [
    // Management level permissions
    Permission.GUEST_VIEW,
    Permission.GUEST_CREATE,
    Permission.GUEST_EDIT,
    Permission.GUEST_DELETE,
    Permission.TRANSFER_VIEW,
    Permission.TRANSFER_CREATE,
    Permission.TRANSFER_EDIT,
    Permission.TRANSFER_DELETE,
    Permission.TOUR_VIEW,
    Permission.TOUR_CREATE,
    Permission.TOUR_EDIT,
    Permission.TOUR_DELETE,
    Permission.INVOICE_VIEW,
    Permission.INVOICE_CREATE,
    Permission.INVOICE_EDIT,
    Permission.INVOICE_DELETE,
    Permission.HOTEL_VIEW,
    Permission.HOTEL_CREATE,
    Permission.HOTEL_EDIT,
    Permission.RESTAURANT_VIEW,
    Permission.RESTAURANT_CREATE,
    Permission.RESTAURANT_EDIT,
    Permission.ITINERARY_VIEW,
    Permission.ITINERARY_CREATE,
    Permission.ITINERARY_EDIT,
    Permission.ITINERARY_DELETE,
    Permission.REPORTS_VIEW,
    Permission.REPORTS_EXPORT,
    Permission.SETTINGS_VIEW,
    Permission.ADMIN_VIEW,
    Permission.ADMIN_EDIT,
  ],
  Admin: [
    // Technical administration
    ...Object.values(Permission),
  ],
  Concierge: [
    // Front desk operations
    Permission.GUEST_VIEW,
    Permission.GUEST_CREATE,
    Permission.GUEST_EDIT,
    Permission.TRANSFER_VIEW,
    Permission.TRANSFER_CREATE,
    Permission.TRANSFER_EDIT,
    Permission.TOUR_VIEW,
    Permission.TOUR_CREATE,
    Permission.TOUR_EDIT,
    Permission.INVOICE_VIEW,
    Permission.INVOICE_CREATE,
    Permission.INVOICE_EDIT,
    Permission.HOTEL_VIEW,
    Permission.RESTAURANT_VIEW,
    Permission.ITINERARY_VIEW,
    Permission.ITINERARY_CREATE,
    Permission.ITINERARY_EDIT,
    Permission.REPORTS_VIEW,
  ],
  Reception: [
    // Basic operations
    Permission.GUEST_VIEW,
    Permission.GUEST_CREATE,
    Permission.TRANSFER_VIEW,
    Permission.TRANSFER_CREATE,
    Permission.TOUR_VIEW,
    Permission.TOUR_CREATE,
    Permission.INVOICE_VIEW,
    Permission.HOTEL_VIEW,
    Permission.RESTAURANT_VIEW,
    Permission.ITINERARY_VIEW,
  ],
  Staff: [
    // Basic operational staff
    Permission.GUEST_VIEW,
    Permission.TRANSFER_VIEW,
    Permission.TRANSFER_CREATE,
    Permission.TOUR_VIEW,
    Permission.TOUR_CREATE,
    Permission.INVOICE_VIEW,
    Permission.HOTEL_VIEW,
    Permission.RESTAURANT_VIEW,
    Permission.ITINERARY_VIEW,
    Permission.ITINERARY_CREATE,
    Permission.ITINERARY_EDIT,
  ],
  Guest: [
    // External guest access
    Permission.GUEST_VIEW,
    Permission.TRANSFER_VIEW,
    Permission.TOUR_VIEW,
    Permission.INVOICE_VIEW,
  ],
}

/**
 * Check if user has a specific permission
 */
export const hasPermission = (userRole: string | undefined, permission: Permission): boolean => {
  if (!userRole) return false

  const permissions = rolePermissions[userRole] || []
  return permissions.includes(permission)
}

/**
 * Check if user has any of the specified permissions
 */
export const hasAnyPermission = (userRole: string | undefined, permissions: Permission[]): boolean => {
  return permissions.some((permission) => hasPermission(userRole, permission))
}

/**
 * Check if user has all of the specified permissions
 */
export const hasAllPermissions = (userRole: string | undefined, permissions: Permission[]): boolean => {
  return permissions.every((permission) => hasPermission(userRole, permission))
}

/**
 * Get all permissions for a role
 */
export const getRolePermissions = (userRole: string | undefined): Permission[] => {
  if (!userRole) return []
  return rolePermissions[userRole] || []
}


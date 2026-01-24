using GuestFlow.Application.Models.Responses.Authorization;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Enum;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Authorization
{
    /// <summary>
    /// Permission service implementation
    /// Manages role-permission matrix for centralized authorization
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PermissionService> _logger;

        public PermissionService(
            IUnitOfWork unitOfWork,
            ILogger<PermissionService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> HasPermissionAsync(string userRole, string permissionCode)
        {
            try
            {
                // Check if permission exists and is active
                var permission = await _unitOfWork.Permissions
                    .GetAll(p => p.Code == permissionCode && p.IsActive && !p.IsDeleted)
                    .FirstOrDefaultAsync();

                if (permission == null)
                {
                    _logger.LogWarning($"Permission '{permissionCode}' not found or inactive");
                    return false;
                }

                // Check if role has this permission
                var rolePermission = await _unitOfWork.RolePermissions
                    .GetAll(rp => rp.RoleName == userRole && 
                                  rp.PermissionId == permission.Id && 
                                  !rp.IsDeleted)
                    .FirstOrDefaultAsync();

                return rolePermission != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking permission '{permissionCode}' for role '{userRole}'");
                // Fail closed: return false on error
                return false;
            }
        }

        public async Task<List<PermissionDto>> GetPermissionsForRoleAsync(string roleName)
        {
            try
            {
                var rolePermissions = await _unitOfWork.RolePermissions
                    .GetAll(rp => rp.RoleName == roleName && !rp.IsDeleted, rp => rp.Permission)
                    .Where(rp => rp.Permission != null && rp.Permission.IsActive && !rp.Permission.IsDeleted)
                    .Select(rp => rp.Permission!)
                    .ToListAsync();

                return rolePermissions.Select(p => MapToDto(p)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting permissions for role '{roleName}'");
                throw;
            }
        }

        public async Task<List<PermissionDto>> GetAllPermissionsAsync()
        {
            try
            {
                var permissions = await _unitOfWork.Permissions
                    .GetAll(p => p.IsActive && !p.IsDeleted)
                    .OrderBy(p => p.Category)
                    .ThenBy(p => p.Code)
                    .ToListAsync();

                return permissions.Select(p => MapToDto(p)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all permissions");
                throw;
            }
        }

        public async Task<RolePermissionMatrixDto> GetRolePermissionMatrixAsync()
        {
            try
            {
                var permissions = await GetAllPermissionsAsync();
                var roles = Enum.GetNames(typeof(UserType)).ToList();

                var rolePermissions = new Dictionary<string, List<string>>();

                foreach (var role in roles)
                {
                    var rolePerms = await GetPermissionsForRoleAsync(role);
                    rolePermissions[role] = rolePerms.Select(p => p.Code).ToList();
                }

                return new RolePermissionMatrixDto
                {
                    Permissions = permissions,
                    Roles = roles,
                    RolePermissions = rolePermissions
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting role-permission matrix");
                throw;
            }
        }

        public async Task<bool> AssignPermissionToRoleAsync(string roleName, string permissionCode)
        {
            try
            {
                var permission = await _unitOfWork.Permissions
                    .GetAll(p => p.Code == permissionCode && p.IsActive && !p.IsDeleted)
                    .FirstOrDefaultAsync();

                if (permission == null)
                {
                    _logger.LogWarning($"Permission '{permissionCode}' not found");
                    return false;
                }

                // Check if already assigned
                var existing = await _unitOfWork.RolePermissions
                    .GetAll(rp => rp.RoleName == roleName && rp.PermissionId == permission.Id && !rp.IsDeleted)
                    .FirstOrDefaultAsync();

                if (existing != null)
                {
                    _logger.LogInformation($"Permission '{permissionCode}' already assigned to role '{roleName}'");
                    return true;
                }

                var rolePermission = new RolePermissionEntity
                {
                    RoleName = roleName,
                    PermissionId = permission.Id
                };

                await _unitOfWork.RolePermissions.AddAsync(rolePermission);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Permission '{permissionCode}' assigned to role '{roleName}'");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning permission '{permissionCode}' to role '{roleName}'");
                return false;
            }
        }

        public async Task<bool> RemovePermissionFromRoleAsync(string roleName, string permissionCode)
        {
            try
            {
                var permission = await _unitOfWork.Permissions
                    .GetAll(p => p.Code == permissionCode && !p.IsDeleted)
                    .FirstOrDefaultAsync();

                if (permission == null)
                {
                    _logger.LogWarning($"Permission '{permissionCode}' not found");
                    return false;
                }

                var rolePermission = await _unitOfWork.RolePermissions
                    .GetAll(rp => rp.RoleName == roleName && rp.PermissionId == permission.Id && !rp.IsDeleted)
                    .FirstOrDefaultAsync();

                if (rolePermission == null)
                {
                    _logger.LogInformation($"Permission '{permissionCode}' not assigned to role '{roleName}'");
                    return true;
                }

                rolePermission.IsDeleted = true;
                rolePermission.MarkAsUpdated();

                await _unitOfWork.RolePermissions.UpdateAsync(rolePermission);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation($"Permission '{permissionCode}' removed from role '{roleName}'");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing permission '{permissionCode}' from role '{roleName}'");
                return false;
            }
        }

        public async Task InitializeDefaultPermissionsAsync()
        {
            try
            {
                // Check if permissions already exist
                var existingCount = await _unitOfWork.Permissions.GetAll().CountAsync();
                if (existingCount > 0)
                {
                    _logger.LogInformation("Permissions already initialized, skipping seed");
                    return;
                }

                var permissions = new List<PermissionEntity>
                {
                    // Guest permissions
                    new() { Code = "guest:view", Name = "View Guests", Description = "View guest information", Category = "Guest", IsActive = true },
                    new() { Code = "guest:create", Name = "Create Guest", Description = "Create new guest", Category = "Guest", IsActive = true },
                    new() { Code = "guest:edit", Name = "Edit Guest", Description = "Edit guest information", Category = "Guest", IsActive = true },
                    new() { Code = "guest:delete", Name = "Delete Guest", Description = "Delete guest", Category = "Guest", IsActive = true },

                    // Transfer permissions
                    new() { Code = "transfer:view", Name = "View Transfers", Description = "View transfer information", Category = "Transfer", IsActive = true },
                    new() { Code = "transfer:create", Name = "Create Transfer", Description = "Create new transfer", Category = "Transfer", IsActive = true },
                    new() { Code = "transfer:edit", Name = "Edit Transfer", Description = "Edit transfer information", Category = "Transfer", IsActive = true },
                    new() { Code = "transfer:delete", Name = "Delete Transfer", Description = "Delete transfer", Category = "Transfer", IsActive = true },

                    // Tour permissions
                    new() { Code = "tour:view", Name = "View Tours", Description = "View tour information", Category = "Tour", IsActive = true },
                    new() { Code = "tour:create", Name = "Create Tour", Description = "Create new tour", Category = "Tour", IsActive = true },
                    new() { Code = "tour:edit", Name = "Edit Tour", Description = "Edit tour information", Category = "Tour", IsActive = true },
                    new() { Code = "tour:delete", Name = "Delete Tour", Description = "Delete tour", Category = "Tour", IsActive = true },

                    // Invoice permissions
                    new() { Code = "invoice:view", Name = "View Invoices", Description = "View invoice information", Category = "Invoice", IsActive = true },
                    new() { Code = "invoice:create", Name = "Create Invoice", Description = "Create new invoice", Category = "Invoice", IsActive = true },
                    new() { Code = "invoice:edit", Name = "Edit Invoice", Description = "Edit invoice information", Category = "Invoice", IsActive = true },
                    new() { Code = "invoice:delete", Name = "Delete Invoice", Description = "Delete invoice", Category = "Invoice", IsActive = true },

                    // Reports permissions
                    new() { Code = "reports:view", Name = "View Reports", Description = "View reports", Category = "Reports", IsActive = true },
                    new() { Code = "reports:export", Name = "Export Reports", Description = "Export reports", Category = "Reports", IsActive = true },

                    // Settings permissions
                    new() { Code = "settings:view", Name = "View Settings", Description = "View system settings", Category = "Settings", IsActive = true },
                    new() { Code = "settings:edit", Name = "Edit Settings", Description = "Edit system settings", Category = "Settings", IsActive = true },

                    // Admin permissions
                    new() { Code = "admin:view", Name = "Admin View", Description = "View admin features", Category = "Admin", IsActive = true },
                    new() { Code = "admin:edit", Name = "Admin Edit", Description = "Edit admin features", Category = "Admin", IsActive = true },
                    new() { Code = "admin:delete", Name = "Admin Delete", Description = "Delete admin features", Category = "Admin", IsActive = true },
                };

                foreach (var permission in permissions)
                {
                    await _unitOfWork.Permissions.AddAsync(permission);
                }

                await _unitOfWork.SaveChangesAsync();

                // Initialize default role-permission mappings based on existing frontend permissions.ts
                var roleMappings = new Dictionary<string, List<string>>
                {
                    ["Owner"] = permissions.Select(p => p.Code).ToList(), // Full access
                    ["Admin"] = permissions.Select(p => p.Code).ToList(), // Full access
                    ["Manager"] = new List<string>
                    {
                        "guest:view", "guest:create", "guest:edit", "guest:delete",
                        "transfer:view", "transfer:create", "transfer:edit", "transfer:delete",
                        "tour:view", "tour:create", "tour:edit", "tour:delete",
                        "invoice:view", "invoice:create", "invoice:edit", "invoice:delete",
                        "reports:view", "reports:export",
                        "settings:view",
                        "admin:view", "admin:edit"
                    },
                    ["Concierge"] = new List<string>
                    {
                        "guest:view", "guest:create", "guest:edit",
                        "transfer:view", "transfer:create", "transfer:edit",
                        "tour:view", "tour:create", "tour:edit",
                        "invoice:view", "invoice:create", "invoice:edit",
                        "reports:view"
                    },
                    ["Reception"] = new List<string>
                    {
                        "guest:view", "guest:create",
                        "transfer:view", "transfer:create",
                        "tour:view", "tour:create",
                        "invoice:view"
                    },
                    ["Staff"] = new List<string>
                    {
                        "guest:view",
                        "transfer:view", "transfer:create",
                        "tour:view", "tour:create",
                        "invoice:view"
                    }
                };

                foreach (var roleMapping in roleMappings)
                {
                    var roleName = roleMapping.Key;
                    foreach (var permissionCode in roleMapping.Value)
                    {
                        var perm = permissions.FirstOrDefault(p => p.Code == permissionCode);
                        if (perm != null)
                        {
                            var rolePermission = new RolePermissionEntity
                            {
                                RoleName = roleName,
                                PermissionId = perm.Id
                            };
                            await _unitOfWork.RolePermissions.AddAsync(rolePermission);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Default permissions and role mappings initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing default permissions");
                throw;
            }
        }

        private PermissionDto MapToDto(PermissionEntity permission)
        {
            return new PermissionDto
            {
                Id = permission.Id,
                Code = permission.Code,
                Name = permission.Name,
                Description = permission.Description,
                Category = permission.Category,
                IsActive = permission.IsActive
            };
        }
    }
}

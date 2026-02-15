// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Api.Models;
using GuestFlow.Application;
using GuestFlow.Application.Models;
using GuestFlow.Application.Operations.Production;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GuestFlow.Api.Controllers
{
    /// <summary>
    /// Production Configuration Validation Controller
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Only Admin can access production validation
    [Tags("Production")]
    public class ProductionController : BaseController
    {
        private readonly IProductionConfigurationValidator _validator;
        private readonly IMigrationDriftChecker _migrationDriftChecker;
        private readonly IDependencyVulnerabilityChecker _dependencyVulnerabilityChecker;
        private readonly IDatabaseBackupService _databaseBackupService;

        public ProductionController(
            IProductionConfigurationValidator validator,
            IMigrationDriftChecker migrationDriftChecker,
            IDependencyVulnerabilityChecker dependencyVulnerabilityChecker,
            IDatabaseBackupService databaseBackupService)
        {
            _validator = validator;
            _migrationDriftChecker = migrationDriftChecker;
            _dependencyVulnerabilityChecker = dependencyVulnerabilityChecker;
            _databaseBackupService = databaseBackupService;
        }

        /// <summary>
        /// Validate all production configurations
        /// </summary>
        [HttpGet("validate")]
        [ProducesResponseType(typeof(GuestFlow.Api.Models.ApiResponse<ProductionConfigurationValidationResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ValidateAll()
        {
            try
            {
                var result = await _validator.ValidateAllAsync();
                return result.Success ? Success(result.Data, "Production configuration validation completed") : Error(result.Message, (int)result.StatusCode);
            }
            catch (Exception ex)
            {
                return Error("Failed to validate production configuration", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Validate secrets and security configurations
        /// </summary>
        [HttpGet("validate/secrets")]
        [ProducesResponseType(typeof(GuestFlow.Api.Models.ApiResponse<SecretsValidationResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ValidateSecrets()
        {
            try
            {
                var result = await _validator.ValidateSecretsAsync();
                return result.Success ? Success(result.Data, "Secrets validation completed") : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Failed to validate secrets", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Validate database and migration status
        /// </summary>
        [HttpGet("validate/database")]
        [ProducesResponseType(typeof(GuestFlow.Api.Models.ApiResponse<DatabaseValidationResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ValidateDatabase()
        {
            try
            {
                var result = await _validator.ValidateDatabaseAsync();
                return result.Success ? Success(result.Data, "Database validation completed") : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Failed to validate database", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Validate logging and monitoring configurations
        /// </summary>
        [HttpGet("validate/logging")]
        [ProducesResponseType(typeof(GuestFlow.Api.Models.ApiResponse<LoggingValidationResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ValidateLogging()
        {
            try
            {
                var result = await _validator.ValidateLoggingAsync();
                return result.Success ? Success(result.Data, "Logging validation completed") : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Failed to validate logging", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Check migration drift (pending migrations)
        /// </summary>
        [HttpGet("migrations/drift")]
        [ProducesResponseType(typeof(GuestFlow.Api.Models.ApiResponse<MigrationDriftResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckMigrationDrift()
        {
            try
            {
                var result = await _migrationDriftChecker.CheckMigrationDriftAsync();
                return result.Success ? Success(result.Data, "Migration drift check completed") : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Failed to check migration drift", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// List all migrations
        /// </summary>
        [HttpGet("migrations")]
        [ProducesResponseType(typeof(GuestFlow.Api.Models.ApiResponse<List<MigrationInfo>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListMigrations()
        {
            try
            {
                var result = await _migrationDriftChecker.ListMigrationsAsync();
                return result.Success ? Success(result.Data, "Migrations listed successfully") : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Failed to list migrations", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Get last applied migration
        /// </summary>
        [HttpGet("migrations/last-applied")]
        [ProducesResponseType(typeof(GuestFlow.Api.Models.ApiResponse<MigrationInfo>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLastAppliedMigration()
        {
            try
            {
                var result = await _migrationDriftChecker.GetLastAppliedMigrationAsync();
                return result.Success ? Success(result.Data, "Last applied migration retrieved") : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Failed to get last applied migration", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Check all dependencies for vulnerabilities (backend + frontend)
        /// </summary>
        [HttpGet("dependencies/vulnerabilities")]
        [ProducesResponseType(typeof(GuestFlow.Api.Models.ApiResponse<DependencyVulnerabilityResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckAllDependencies()
        {
            try
            {
                var result = await _dependencyVulnerabilityChecker.CheckAllDependenciesAsync();
                return result.Success ? Success(result.Data, "Dependency vulnerability check completed") : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Failed to check dependencies", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Check backend (NuGet) dependencies for vulnerabilities
        /// </summary>
        [HttpGet("dependencies/backend")]
        [ProducesResponseType(typeof(GuestFlow.Api.Models.ApiResponse<BackendVulnerabilityResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckBackendDependencies()
        {
            try
            {
                var result = await _dependencyVulnerabilityChecker.CheckBackendDependenciesAsync();
                return result.Success ? Success(result.Data, "Backend dependency check completed") : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Failed to check backend dependencies", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Check frontend (npm) dependencies for vulnerabilities
        /// </summary>
        [HttpGet("dependencies/frontend")]
        [ProducesResponseType(typeof(GuestFlow.Api.Models.ApiResponse<FrontendVulnerabilityResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckFrontendDependencies()
        {
            try
            {
                var result = await _dependencyVulnerabilityChecker.CheckFrontendDependenciesAsync();
                return result.Success ? Success(result.Data, "Frontend dependency check completed") : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Failed to check frontend dependencies", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Create database backup
        /// </summary>
        [HttpPost("backup/create")]
        [ProducesResponseType(typeof(GuestFlow.Api.Models.ApiResponse<BackupResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateBackup([FromQuery] string? backupPath = null)
        {
            try
            {
                var result = await _databaseBackupService.CreateBackupAsync(backupPath);
                return result.Success ? Success(result.Data, "Database backup created successfully") : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Failed to create backup", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Get last backup
        /// </summary>
        [HttpGet("backup/last")]
        [ProducesResponseType(typeof(GuestFlow.Api.Models.ApiResponse<BackupInfo>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLastBackup()
        {
            try
            {
                var result = await _databaseBackupService.GetLastBackupAsync();
                return result.Success ? Success(result.Data, "Last backup retrieved") : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Failed to get last backup", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// List all backups
        /// </summary>
        [HttpGet("backup/list")]
        [ProducesResponseType(typeof(GuestFlow.Api.Models.ApiResponse<List<BackupInfo>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListBackups([FromQuery] int? limit = null)
        {
            try
            {
                var result = await _databaseBackupService.ListBackupsAsync(limit);
                return result.Success ? Success(result.Data, "Backups listed successfully") : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Failed to list backups", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Restore database from backup (WARNING: Destructive operation)
        /// </summary>
        [HttpPost("backup/restore")]
        [ProducesResponseType(typeof(GuestFlow.Api.Models.ApiResponse<RestoreResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RestoreBackup([FromBody] RestoreBackupRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.BackupFilePath))
                {
                    return Error("Backup file path is required", 400);
                }

                var result = await _databaseBackupService.RestoreBackupAsync(request.BackupFilePath);
                return result.Success ? Success(result.Data, "Database restored successfully") : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Failed to restore backup", 500, new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Validate backup strategy
        /// </summary>
        [HttpGet("backup/validate-strategy")]
        [ProducesResponseType(typeof(GuestFlow.Api.Models.ApiResponse<BackupStrategyValidationResult>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ValidateBackupStrategy()
        {
            try
            {
                var result = await _databaseBackupService.ValidateBackupStrategyAsync();
                return result.Success ? Success(result.Data, "Backup strategy validation completed") : Error(result.Message, 400);
            }
            catch (Exception ex)
            {
                return Error("Failed to validate backup strategy", 500, new { Error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Restore Backup Request
    /// </summary>
    public class RestoreBackupRequest
    {
        public string BackupFilePath { get; set; } = string.Empty;
    }
}

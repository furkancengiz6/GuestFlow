// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses;
using GuestFlow.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Production
{
    /// <summary>
    /// Database Backup Service implementation
    /// Note: This service requires SQL Server backup/restore permissions
    /// </summary>
    public class DatabaseBackupService : IDatabaseBackupService
    {
        private readonly GuestFlowDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;
        private readonly ILogger<DatabaseBackupService> _logger;
        private readonly string _backupDirectory;

        public DatabaseBackupService(
            GuestFlowDbContext dbContext,
            IConfiguration configuration,
            IHostEnvironment environment,
            ILogger<DatabaseBackupService> logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _environment = environment;
            _logger = logger;

            // Get backup directory from configuration or use default
            _backupDirectory = _configuration["Backup:Directory"] 
                ?? Path.Combine(Directory.GetCurrentDirectory(), "Backups");
        }

        public async Task<ApiResponse<BackupResult>> CreateBackupAsync(string? backupPath = null)
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    return ApiResponse<BackupResult>.Fail("Database connection string not configured");
                }

                // Parse connection string to get database name
                var builder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = builder.InitialCatalog ?? "GuestFlowDb";

                // Generate backup file name
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var backupFileName = $"{databaseName}_Backup_{timestamp}.bak";
                var finalBackupPath = backupPath ?? Path.Combine(_backupDirectory, backupFileName);

                // Ensure backup directory exists
                var backupDir = Path.GetDirectoryName(finalBackupPath);
                if (!string.IsNullOrEmpty(backupDir) && !Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }

                // Create backup using SQL command
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                var backupQuery = $@"
                    BACKUP DATABASE [{databaseName}]
                    TO DISK = '{finalBackupPath}'
                    WITH FORMAT, 
                         MEDIANAME = 'GuestFlow_Backup', 
                         NAME = 'Full Backup of {databaseName}',
                         COMPRESSION,
                         STATS = 10";

                using var command = new SqlCommand(backupQuery, connection);
                command.CommandTimeout = 300; // 5 minutes timeout
                await command.ExecuteNonQueryAsync();

                // Get backup file size
                long backupSize = 0;
                if (System.IO.File.Exists(finalBackupPath))
                {
                    var fileInfo = new FileInfo(finalBackupPath);
                    backupSize = fileInfo.Length;
                }

                var result = new BackupResult
                {
                    Success = true,
                    BackupFilePath = finalBackupPath,
                    BackupSizeBytes = backupSize,
                    BackupDate = DateTime.UtcNow
                };

                _logger.LogInformation("Database backup created successfully: {BackupPath}, Size: {Size} bytes", 
                    finalBackupPath, backupSize);

                return ApiResponse<BackupResult>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create database backup");
                return ApiResponse<BackupResult>.Fail($"Failed to create backup: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BackupInfo?>> GetLastBackupAsync()
        {
            try
            {
                var backups = await ListBackupsAsync(1);
                if (backups.Success && backups.Data != null && backups.Data.Any())
                {
                    return ApiResponse<BackupInfo?>.SuccessResponse(backups.Data.First());
                }

                return ApiResponse<BackupInfo?>.SuccessResponse(null, "No backups found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get last backup");
                return ApiResponse<BackupInfo?>.Fail($"Failed to get last backup: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<BackupInfo>>> ListBackupsAsync(int? limit = null)
        {
            try
            {
                if (!Directory.Exists(_backupDirectory))
                {
                    return ApiResponse<List<BackupInfo>>.SuccessResponse(new List<BackupInfo>());
                }

                var backupFiles = Directory.GetFiles(_backupDirectory, "*.bak")
                    .Select(filePath => new FileInfo(filePath))
                    .OrderByDescending(f => f.CreationTime)
                    .ToList();

                if (limit.HasValue)
                {
                    backupFiles = backupFiles.Take(limit.Value).ToList();
                }

                var backups = backupFiles.Select(file => new BackupInfo
                {
                    BackupFilePath = file.FullName,
                    BackupFileName = file.Name,
                    BackupSizeBytes = file.Length,
                    BackupDate = file.CreationTime,
                    DatabaseName = ExtractDatabaseNameFromFileName(file.Name)
                }).ToList();

                return ApiResponse<List<BackupInfo>>.SuccessResponse(backups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list backups");
                return ApiResponse<List<BackupInfo>>.Fail($"Failed to list backups: {ex.Message}");
            }
        }

        public async Task<ApiResponse<RestoreResult>> RestoreBackupAsync(string backupFilePath)
        {
            try
            {
                if (!System.IO.File.Exists(backupFilePath))
                {
                    return ApiResponse<RestoreResult>.Fail($"Backup file not found: {backupFilePath}");
                }

                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    return ApiResponse<RestoreResult>.Fail("Database connection string not configured");
                }

                // Parse connection string to get database name
                var builder = new SqlConnectionStringBuilder(connectionString);
                var databaseName = builder.InitialCatalog ?? "GuestFlowDb";

                // WARNING: Restore operation is destructive - should only be done in maintenance mode
                if (!_environment.IsDevelopment())
                {
                    _logger.LogWarning("Restore operation attempted in non-development environment: {Environment}", 
                        _environment.EnvironmentName);
                    // In production, this should require additional confirmation/authorization
                }

                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                // Set database to single user mode for restore
                var setSingleUserQuery = $@"
                    ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE";

                using (var command = new SqlCommand(setSingleUserQuery, connection))
                {
                    command.CommandTimeout = 60;
                    await command.ExecuteNonQueryAsync();
                }

                try
                {
                    // Restore database
                    var restoreQuery = $@"
                        RESTORE DATABASE [{databaseName}]
                        FROM DISK = '{backupFilePath}'
                        WITH REPLACE,
                             STATS = 10";

                    using var restoreCommand = new SqlCommand(restoreQuery, connection);
                    restoreCommand.CommandTimeout = 600; // 10 minutes timeout
                    await restoreCommand.ExecuteNonQueryAsync();
                }
                finally
                {
                    // Set database back to multi-user mode
                    var setMultiUserQuery = $@"
                        ALTER DATABASE [{databaseName}] SET MULTI_USER";

                    using (var command = new SqlCommand(setMultiUserQuery, connection))
                    {
                        command.CommandTimeout = 60;
                        await command.ExecuteNonQueryAsync();
                    }
                }

                var result = new RestoreResult
                {
                    Success = true,
                    RestoreDate = DateTime.UtcNow,
                    RestoredDatabaseName = databaseName
                };

                _logger.LogInformation("Database restored successfully from: {BackupPath}", backupFilePath);

                return ApiResponse<RestoreResult>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restore database backup");
                return ApiResponse<RestoreResult>.Fail($"Failed to restore backup: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BackupStrategyValidationResult>> ValidateBackupStrategyAsync()
        {
            try
            {
                var result = new BackupStrategyValidationResult
                {
                    BackupDirectoryPath = _backupDirectory,
                    Issues = new List<ValidationIssue>()
                };

                // Check if backup directory exists
                if (Directory.Exists(_backupDirectory))
                {
                    result.BackupDirectoryExists = true;

                    // Check if directory is writable
                    try
                    {
                        var testFile = Path.Combine(_backupDirectory, ".test_write");
                       if (System.IO.File.Exists(testFile)) System.IO.File.Delete(testFile);
                        System.IO.File.WriteAllText(testFile, "test");
                        System.IO.File.Delete(testFile);
                        result.BackupDirectoryWritable = true;
                    }
                    catch (Exception ex)
                    {
                        result.BackupDirectoryWritable = false;
                        result.Issues.Add(new ValidationIssue
                        {
                            Category = "Backup",
                            Severity = "Critical",
                            Message = $"Backup directory is not writable: {ex.Message}",
                            Recommendation = "Check directory permissions and ensure the application has write access"
                        });
                    }
                }
                else
                {
                    result.Issues.Add(new ValidationIssue
                    {
                        Category = "Backup",
                        Severity = "Warning",
                        Message = $"Backup directory does not exist: {_backupDirectory}",
                        Recommendation = "The directory will be created automatically on first backup, or create it manually"
                    });
                }

                // Check for automated backup configuration (cron job, scheduled task, etc.)
                // This is a simplified check - in production, you might check for actual scheduled tasks
                var automatedBackupEnabled = _configuration["Backup:Automated:Enabled"];
                if (string.IsNullOrEmpty(automatedBackupEnabled) || 
                    !string.Equals(automatedBackupEnabled, "true", StringComparison.OrdinalIgnoreCase))
                {
                    result.Issues.Add(new ValidationIssue
                    {
                        Category = "Backup",
                        Severity = "Warning",
                        Message = "Automated backup is not configured",
                        Recommendation = "Configure automated daily backups using Backup:Automated:Enabled=true in configuration"
                    });
                }
                else
                {
                    result.AutomatedBackupConfigured = true;
                }

                result.IsValid = result.Issues.All(i => i.Severity != "Critical");

                return ApiResponse<BackupStrategyValidationResult>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate backup strategy");
                return ApiResponse<BackupStrategyValidationResult>.Fail($"Failed to validate backup strategy: {ex.Message}");
            }
        }

        private string? ExtractDatabaseNameFromFileName(string fileName)
        {
            // Extract database name from backup file name
            // Format: {DatabaseName}_Backup_{Timestamp}.bak
            var parts = fileName.Split('_');
            if (parts.Length >= 2)
            {
                return parts[0];
            }
            return null;
        }
    }
}

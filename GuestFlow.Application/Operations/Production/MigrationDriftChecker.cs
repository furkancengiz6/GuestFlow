// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses;
using GuestFlow.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Production
{
    /// <summary>
    /// Migration Drift Checker implementation
    /// </summary>
    public class MigrationDriftChecker : IMigrationDriftChecker
    {
        private readonly GuestFlowDbContext _dbContext;
        private readonly ILogger<MigrationDriftChecker> _logger;

        public MigrationDriftChecker(
            GuestFlowDbContext dbContext,
            ILogger<MigrationDriftChecker> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<ApiResponse<MigrationDriftResult>> CheckMigrationDriftAsync()
        {
            try
            {
                var result = new MigrationDriftResult();

                // Get all migrations
                var allMigrations = await GetAllMigrationsAsync(_dbContext);
                var appliedMigrations = await GetAppliedMigrationsAsync(_dbContext);

                // Find pending migrations
                var pendingMigrations = allMigrations
                    .Where(m => !appliedMigrations.Contains(m))
                    .ToList();

                result.PendingMigrationsCount = pendingMigrations.Count;
                result.HasDrift = pendingMigrations.Count > 0;
                result.PendingMigrations = pendingMigrations.Select(m => new MigrationInfo
                {
                    MigrationId = m,
                    Name = m,
                    IsApplied = false
                }).ToList();

                // Get last applied migration
                if (appliedMigrations.Any())
                {
                    var lastApplied = appliedMigrations.Last();
                    result.LastAppliedMigration = new MigrationInfo
                    {
                        MigrationId = lastApplied,
                        Name = lastApplied,
                        IsApplied = true
                    };
                }

                result.Message = result.HasDrift
                    ? $"Found {result.PendingMigrationsCount} pending migration(s). Run 'dotnet ef database update' to apply them."
                    : "Database is up to date. No pending migrations.";

                return ApiResponse<MigrationDriftResult>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check migration drift");
                return ApiResponse<MigrationDriftResult>.Fail($"Failed to check migration drift: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<MigrationInfo>>> ListMigrationsAsync()
        {
            try
            {
                var allMigrations = await GetAllMigrationsAsync(_dbContext);
                var appliedMigrations = await GetAppliedMigrationsAsync(_dbContext);

                var migrations = allMigrations.Select(m => new MigrationInfo
                {
                    MigrationId = m,
                    Name = m,
                    IsApplied = appliedMigrations.Contains(m)
                }).ToList();

                return ApiResponse<List<MigrationInfo>>.SuccessResponse(migrations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list migrations");
                return ApiResponse<List<MigrationInfo>>.Fail($"Failed to list migrations: {ex.Message}");
            }
        }

        public async Task<ApiResponse<MigrationInfo?>> GetLastAppliedMigrationAsync()
        {
            try
            {
                var appliedMigrations = await GetAppliedMigrationsAsync(_dbContext);
                if (!appliedMigrations.Any())
                {
                    return ApiResponse<MigrationInfo?>.SuccessResponse(null, "No migrations have been applied");
                }

                var lastApplied = appliedMigrations.Last();
                var migrationInfo = new MigrationInfo
                {
                    MigrationId = lastApplied,
                    Name = lastApplied,
                    IsApplied = true
                };

                return ApiResponse<MigrationInfo?>.SuccessResponse(migrationInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get last applied migration");
                return ApiResponse<MigrationInfo?>.Fail($"Failed to get last applied migration: {ex.Message}");
            }
        }

        private async Task<List<string>> GetAllMigrationsAsync(DbContext dbContext)
        {
            try
            {
                var migrationsAssembly = dbContext.Database.GetMigrations();
                return migrationsAssembly.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get all migrations, returning empty list");
                return new List<string>();
            }
        }

        private async Task<List<string>> GetAppliedMigrationsAsync(DbContext dbContext)
        {
            try
            {
                var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
                return appliedMigrations.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get applied migrations, returning empty list");
                return new List<string>();
            }
        }
    }
}

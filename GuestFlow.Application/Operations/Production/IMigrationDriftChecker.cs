// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Production
{
    /// <summary>
    /// Migration Drift Checker - Database migration durumunu kontrol eder
    /// </summary>
    public interface IMigrationDriftChecker
    {
        /// <summary>
        /// Pending migrations olup olmadığını kontrol eder
        /// </summary>
        Task<ApiResponse<MigrationDriftResult>> CheckMigrationDriftAsync();

        /// <summary>
        /// Tüm migration'ları listeler
        /// </summary>
        Task<ApiResponse<List<MigrationInfo>>> ListMigrationsAsync();

        /// <summary>
        /// Son uygulanan migration'ı getirir
        /// </summary>
        Task<ApiResponse<MigrationInfo?>> GetLastAppliedMigrationAsync();
    }

    /// <summary>
    /// Migration Drift Result
    /// </summary>
    public class MigrationDriftResult
    {
        public bool HasDrift { get; set; }
        public int PendingMigrationsCount { get; set; }
        public List<MigrationInfo> PendingMigrations { get; set; } = new List<MigrationInfo>();
        public MigrationInfo? LastAppliedMigration { get; set; }
        public string? Message { get; set; }
    }

    /// <summary>
    /// Migration Info
    /// </summary>
    public class MigrationInfo
    {
        public string MigrationId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsApplied { get; set; }
        public string? AppliedDate { get; set; }
    }
}

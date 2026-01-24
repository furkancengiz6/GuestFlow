// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Production
{
    /// <summary>
    /// Database Backup Service - Database backup ve restore işlemlerini yönetir
    /// </summary>
    public interface IDatabaseBackupService
    {
        /// <summary>
        /// Database backup oluşturur
        /// </summary>
        Task<ApiResponse<BackupResult>> CreateBackupAsync(string? backupPath = null);

        /// <summary>
        /// Son backup'ı getirir
        /// </summary>
        Task<ApiResponse<BackupInfo?>> GetLastBackupAsync();

        /// <summary>
        /// Tüm backup'ları listeler
        /// </summary>
        Task<ApiResponse<List<BackupInfo>>> ListBackupsAsync(int? limit = null);

        /// <summary>
        /// Backup'ı restore eder
        /// </summary>
        Task<ApiResponse<RestoreResult>> RestoreBackupAsync(string backupFilePath);

        /// <summary>
        /// Backup stratejisini doğrular (backup dizini, izinler, vb.)
        /// </summary>
        Task<ApiResponse<BackupStrategyValidationResult>> ValidateBackupStrategyAsync();
    }

    /// <summary>
    /// Backup Result
    /// </summary>
    public class BackupResult
    {
        public bool Success { get; set; }
        public string BackupFilePath { get; set; } = string.Empty;
        public long BackupSizeBytes { get; set; }
        public DateTime BackupDate { get; set; }
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Backup Info
    /// </summary>
    public class BackupInfo
    {
        public string BackupFilePath { get; set; } = string.Empty;
        public string BackupFileName { get; set; } = string.Empty;
        public long BackupSizeBytes { get; set; }
        public DateTime BackupDate { get; set; }
        public string? DatabaseName { get; set; }
    }

    /// <summary>
    /// Restore Result
    /// </summary>
    public class RestoreResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime RestoreDate { get; set; }
        public string? RestoredDatabaseName { get; set; }
    }

    /// <summary>
    /// Backup Strategy Validation Result
    /// </summary>
    public class BackupStrategyValidationResult
    {
        public bool IsValid { get; set; }
        public bool BackupDirectoryExists { get; set; }
        public bool BackupDirectoryWritable { get; set; }
        public string? BackupDirectoryPath { get; set; }
        public bool AutomatedBackupConfigured { get; set; }
        public List<ValidationIssue> Issues { get; set; } = new List<ValidationIssue>();
    }
}

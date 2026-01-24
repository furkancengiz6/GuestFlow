// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Production
{
    /// <summary>
    /// Production Configuration Validator - Production ortamında kritik konfigürasyonları doğrular
    /// </summary>
    public interface IProductionConfigurationValidator
    {
        /// <summary>
        /// Tüm production konfigürasyonlarını doğrular
        /// </summary>
        Task<ApiResponse<ProductionConfigurationValidationResult>> ValidateAllAsync();

        /// <summary>
        /// Secrets ve güvenlik konfigürasyonlarını doğrular
        /// </summary>
        Task<ApiResponse<SecretsValidationResult>> ValidateSecretsAsync();

        /// <summary>
        /// Database ve migration durumunu doğrular
        /// </summary>
        Task<ApiResponse<DatabaseValidationResult>> ValidateDatabaseAsync();

        /// <summary>
        /// Logging ve monitoring konfigürasyonlarını doğrular
        /// </summary>
        Task<ApiResponse<LoggingValidationResult>> ValidateLoggingAsync();
    }

    /// <summary>
    /// Production Configuration Validation Result
    /// </summary>
    public class ProductionConfigurationValidationResult
    {
        public bool IsValid { get; set; }
        public List<ValidationIssue> Issues { get; set; } = new List<ValidationIssue>();
        public SecretsValidationResult? Secrets { get; set; }
        public DatabaseValidationResult? Database { get; set; }
        public LoggingValidationResult? Logging { get; set; }
    }

    /// <summary>
    /// Validation Issue
    /// </summary>
    public class ValidationIssue
    {
        public string Category { get; set; } = string.Empty; // Secrets, Database, Logging, Security
        public string Severity { get; set; } = "Warning"; // Critical, Warning, Info
        public string Message { get; set; } = string.Empty;
        public string? Recommendation { get; set; }
    }

    /// <summary>
    /// Secrets Validation Result
    /// </summary>
    public class SecretsValidationResult
    {
        public bool IsValid { get; set; }
        public bool JWTSecretKeyConfigured { get; set; }
        public bool JWTSecretKeySecure { get; set; } // Minimum 256-bit
        public bool DatabasePasswordConfigured { get; set; }
        public bool EmailPasswordConfigured { get; set; }
        public bool CORSOriginsRestricted { get; set; }
        public bool SeedDemoDataDisabled { get; set; }
        public List<ValidationIssue> Issues { get; set; } = new List<ValidationIssue>();
    }

    /// <summary>
    /// Database Validation Result
    /// </summary>
    public class DatabaseValidationResult
    {
        public bool IsValid { get; set; }
        public bool ConnectionSuccessful { get; set; }
        public bool MigrationsUpToDate { get; set; }
        public int PendingMigrationsCount { get; set; }
        public bool CriticalIndexesExist { get; set; }
        public List<ValidationIssue> Issues { get; set; } = new List<ValidationIssue>();
    }

    /// <summary>
    /// Logging Validation Result
    /// </summary>
    public class LoggingValidationResult
    {
        public bool IsValid { get; set; }
        public bool CentralizedLoggingConfigured { get; set; }
        public bool HealthEndpointsAccessible { get; set; }
        public bool AlertingConfigured { get; set; }
        public List<ValidationIssue> Issues { get; set; } = new List<ValidationIssue>();
    }
}

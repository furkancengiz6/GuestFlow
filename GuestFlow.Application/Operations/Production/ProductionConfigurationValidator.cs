// Copyright (c) 2025 Furkan Cengiz
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using GuestFlow.Application.Models.Responses;
using GuestFlow.Domain.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Production
{
    /// <summary>
    /// Production Configuration Validator implementation
    /// </summary>
    public class ProductionConfigurationValidator : IProductionConfigurationValidator
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductionConfigurationValidator> _logger;

        public ProductionConfigurationValidator(
            IConfiguration configuration,
            IHostEnvironment environment,
            IUnitOfWork unitOfWork,
            ILogger<ProductionConfigurationValidator> logger)
        {
            _configuration = configuration;
            _environment = environment;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<ProductionConfigurationValidationResult>> ValidateAllAsync()
        {
            try
            {
                var result = new ProductionConfigurationValidationResult
                {
                    Issues = new List<ValidationIssue>()
                };

                // Secrets validation
                var secretsResult = await ValidateSecretsAsync();
                result.Secrets = secretsResult.Data;
                if (secretsResult.Data != null && !secretsResult.Data.IsValid)
                {
                    result.Issues.AddRange(secretsResult.Data.Issues);
                }

                // Database validation
                var databaseResult = await ValidateDatabaseAsync();
                result.Database = databaseResult.Data;
                if (databaseResult.Data != null && !databaseResult.Data.IsValid)
                {
                    result.Issues.AddRange(databaseResult.Data.Issues);
                }

                // Logging validation
                var loggingResult = await ValidateLoggingAsync();
                result.Logging = loggingResult.Data;
                if (loggingResult.Data != null && !loggingResult.Data.IsValid)
                {
                    result.Issues.AddRange(loggingResult.Data.Issues);
                }

                result.IsValid = result.Issues.All(i => i.Severity != "Critical");

                return ApiResponse<ProductionConfigurationValidationResult>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate production configuration");
                return ApiResponse<ProductionConfigurationValidationResult>.Fail($"Failed to validate production configuration: {ex.Message}");
            }
        }

        public async Task<ApiResponse<SecretsValidationResult>> ValidateSecretsAsync()
        {
            try
            {
                var result = new SecretsValidationResult
                {
                    Issues = new List<ValidationIssue>()
                };

                // JWT Secret Key validation
                var jwtSecretKey = _configuration["JWT:SecretKey"] ?? _configuration["JWT__SecretKey"];
                if (string.IsNullOrEmpty(jwtSecretKey))
                {
                    result.Issues.Add(new ValidationIssue
                    {
                        Category = "Secrets",
                        Severity = "Critical",
                        Message = "JWT Secret Key is not configured",
                        Recommendation = "Set JWT:SecretKey in configuration (minimum 256-bit, 32 characters)"
                    });
                }
                else
                {
                    result.JWTSecretKeyConfigured = true;
                    
                    // Check if JWT secret is secure (minimum 256-bit = 32 characters)
                    if (jwtSecretKey.Length < 32)
                    {
                        result.Issues.Add(new ValidationIssue
                        {
                            Category = "Secrets",
                            Severity = "Critical",
                            Message = "JWT Secret Key is too short (minimum 32 characters required for 256-bit security)",
                            Recommendation = "Generate a new JWT secret key with at least 32 characters"
                        });
                    }
                    else
                    {
                        result.JWTSecretKeySecure = true;
                    }
                }

                // Database password validation
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    result.Issues.Add(new ValidationIssue
                    {
                        Category = "Secrets",
                        Severity = "Critical",
                        Message = "Database connection string is not configured",
                        Recommendation = "Set ConnectionStrings:DefaultConnection in configuration"
                    });
                }
                else
                {
                    result.DatabasePasswordConfigured = true;
                }

                // Email password validation (if email is configured)
                var smtpServer = _configuration["Email:SmtpServer"];
                if (!string.IsNullOrEmpty(smtpServer))
                {
                    var emailPassword = _configuration["Email:Password"] ?? _configuration["Email__Password"];
                    if (string.IsNullOrEmpty(emailPassword))
                    {
                        result.Issues.Add(new ValidationIssue
                        {
                            Category = "Secrets",
                            Severity = "Warning",
                            Message = "Email password is not configured but SMTP server is set",
                            Recommendation = "Set Email:Password in configuration if email functionality is required"
                        });
                    }
                    else
                    {
                        result.EmailPasswordConfigured = true;
                    }
                }

                // CORS origins validation (production only)
                if (_environment.IsProduction())
                {
                    var corsOrigins = _configuration["CORS:AllowedOrigins"] ?? _configuration["CORS__AllowedOrigins"];
                    if (string.IsNullOrEmpty(corsOrigins) || corsOrigins.Contains("*"))
                    {
                        result.Issues.Add(new ValidationIssue
                        {
                            Category = "Security",
                            Severity = "Critical",
                            Message = "CORS origins are not restricted in production",
                            Recommendation = "Set CORS:AllowedOrigins to specific production domains (e.g., https://app.guestflow.com)"
                        });
                    }
                    else
                    {
                        result.CORSOriginsRestricted = true;
                    }
                }

                // SeedDemoData validation (production only)
                if (_environment.IsProduction())
                {
                    var seedDemoData = _configuration["SeedDemoData"];
                    var seedDemoDataEnabled = string.Equals(seedDemoData, "true", StringComparison.OrdinalIgnoreCase) ||
                                            string.Equals(seedDemoData, "1", StringComparison.OrdinalIgnoreCase);
                    
                    if (seedDemoDataEnabled)
                    {
                        result.Issues.Add(new ValidationIssue
                        {
                            Category = "Security",
                            Severity = "Critical",
                            Message = "SeedDemoData is enabled in production",
                            Recommendation = "Set SeedDemoData=false in production configuration"
                        });
                    }
                    else
                    {
                        result.SeedDemoDataDisabled = true;
                    }
                }

                result.IsValid = result.Issues.All(i => i.Severity != "Critical");

                return ApiResponse<SecretsValidationResult>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate secrets");
                return ApiResponse<SecretsValidationResult>.Fail($"Failed to validate secrets: {ex.Message}");
            }
        }

        public async Task<ApiResponse<DatabaseValidationResult>> ValidateDatabaseAsync()
        {
            try
            {
                var result = new DatabaseValidationResult
                {
                    Issues = new List<ValidationIssue>()
                };

                // Database connection test
                try
                {
                    await _unitOfWork.Guests.GetAll().Take(1).ToListAsync();
                    result.ConnectionSuccessful = true;
                }
                catch (Exception ex)
                {
                    result.Issues.Add(new ValidationIssue
                    {
                        Category = "Database",
                        Severity = "Critical",
                        Message = $"Database connection failed: {ex.Message}",
                        Recommendation = "Check database connection string and ensure database server is accessible"
                    });
                }

                // Migration status check (simplified - would need EF Core migration API)
                // TODO: Implement actual migration check using EF Core migration API
                // For now, we'll just check if we can query the database
                if (result.ConnectionSuccessful)
                {
                    try
                    {
                        // Check if critical tables exist
                        var hasGuests = await _unitOfWork.Guests.GetAll().AnyAsync();
                        var hasInvoices = await _unitOfWork.Invoices.GetAll().AnyAsync();
                        
                        if (!hasGuests && !hasInvoices)
                        {
                            result.Issues.Add(new ValidationIssue
                            {
                                Category = "Database",
                                Severity = "Warning",
                                Message = "Database appears to be empty or migrations not applied",
                                Recommendation = "Run migrations: dotnet ef database update"
                            });
                        }
                        else
                        {
                            result.MigrationsUpToDate = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Issues.Add(new ValidationIssue
                        {
                            Category = "Database",
                            Severity = "Warning",
                            Message = $"Error checking database schema: {ex.Message}",
                            Recommendation = "Verify database migrations are applied"
                        });
                    }
                }

                // Critical indexes check (simplified - would need to query sys.indexes)
                // TODO: Implement actual index check
                result.CriticalIndexesExist = true; // Placeholder

                result.IsValid = result.Issues.All(i => i.Severity != "Critical");

                return ApiResponse<DatabaseValidationResult>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate database");
                return ApiResponse<DatabaseValidationResult>.Fail($"Failed to validate database: {ex.Message}");
            }
        }

        public async Task<ApiResponse<LoggingValidationResult>> ValidateLoggingAsync()
        {
            try
            {
                var result = new LoggingValidationResult
                {
                    Issues = new List<ValidationIssue>()
                };

                // Centralized logging check (Seq/ELK)
                var seqUrl = _configuration["Serilog:WriteTo:0:Args:serverUrl"];
                var elkUrl = _configuration["Serilog:WriteTo:0:Args:nodeUris"];
                
                if (string.IsNullOrEmpty(seqUrl) && string.IsNullOrEmpty(elkUrl))
                {
                    result.Issues.Add(new ValidationIssue
                    {
                        Category = "Logging",
                        Severity = "Warning",
                        Message = "Centralized logging (Seq/ELK) is not configured",
                        Recommendation = "Configure Serilog to write to Seq or ELK stack for production monitoring"
                    });
                }
                else
                {
                    result.CentralizedLoggingConfigured = true;
                }

                // Health endpoints check (assumed accessible if service is running)
                result.HealthEndpointsAccessible = true; // Placeholder - would need actual HTTP check

                // Alerting check (simplified)
                var alertingConfigured = !string.IsNullOrEmpty(_configuration["Alerting:Email"]) ||
                                       !string.IsNullOrEmpty(_configuration["Alerting:Webhook"]);
                
                if (!alertingConfigured && _environment.IsProduction())
                {
                    result.Issues.Add(new ValidationIssue
                    {
                        Category = "Logging",
                        Severity = "Warning",
                        Message = "Alerting is not configured for production",
                        Recommendation = "Configure alerting for 5xx errors, latency, and disk usage"
                    });
                }
                else
                {
                    result.AlertingConfigured = true;
                }

                result.IsValid = result.Issues.All(i => i.Severity != "Critical");

                return ApiResponse<LoggingValidationResult>.SuccessResponse(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate logging");
                return ApiResponse<LoggingValidationResult>.Fail($"Failed to validate logging: {ex.Message}");
            }
        }
    }
}

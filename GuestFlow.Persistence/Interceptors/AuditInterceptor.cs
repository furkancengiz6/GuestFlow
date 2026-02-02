using GuestFlow.Domain.Entities;
using GuestFlow.Domain.Entities.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Linq;

namespace GuestFlow.Persistence.Interceptors
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuditInterceptor> _logger;

        public AuditInterceptor(IHttpContextAccessor httpContextAccessor, ILogger<AuditInterceptor> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            var auditEntries = new List<AuditEntry>();

            try
            {
                foreach (var entry in context.ChangeTracker.Entries())
                {
                    // Skip audit logs to prevent infinite loop
                    if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                        continue;

                    var auditEntry = new AuditEntry(entry);
                    auditEntry.TableName = entry.Metadata.GetTableName();
                    auditEntries.Add(auditEntry);

                    foreach (var property in entry.Properties)
                    {
                        if (property.IsTemporary)
                            continue;

                        string propertyName = property.Metadata.Name;
                        if (property.Metadata.IsPrimaryKey())
                        {
                            auditEntry.KeyValues[propertyName] = property.CurrentValue;
                            continue;
                        }

                        switch (entry.State)
                        {
                            case EntityState.Added:
                                auditEntry.NewValues[propertyName] = property.CurrentValue;
                                break;
                            case EntityState.Deleted:
                                auditEntry.OldValues[propertyName] = property.OriginalValue;
                                break;
                            case EntityState.Modified:
                                if (property.IsModified && !IsSystemProperty(propertyName))
                                {
                                    auditEntry.OldValues[propertyName] = property.OriginalValue;
                                    auditEntry.NewValues[propertyName] = property.CurrentValue;
                                }
                                break;
                        }
                    }
                }

                foreach (var auditEntry in auditEntries)
                {
                    // Only log if there are actual changes
                    if (auditEntry.OldValues.Any() || auditEntry.NewValues.Any())
                    {
                        var auditLog = new GuestFlow.Domain.Entities.Core.AuditLog
                        {
                            TableName = auditEntry.TableName,
                            Action = auditEntry.Action.ToString(),
                            OldValues = JsonSerializer.Serialize(auditEntry.OldValues),
                            NewValues = JsonSerializer.Serialize(auditEntry.NewValues),
                            UserId = GetCurrentUserId(),
                            UserName = GetCurrentUserName(),
                            IpAddress = GetClientIpAddress(),
                            UserAgent = GetUserAgent(),
                            SessionId = GetSessionId(),
                            CorrelationId = GetCorrelationId(),
                            Timestamp = DateTime.UtcNow
                        };

                        // Use Set<T> to avoid casting DbContext to concrete type
                        context.Set<GuestFlow.Domain.Entities.Core.AuditLog>().Add(auditLog);

                        // Log to console for monitoring
                        _logger.LogInformation(
                            "Audit: {User} {Action} on {Table} - Keys: {Keys}",
                            auditLog.UserName ?? "Unknown",
                            auditLog.Action,
                            auditLog.TableName,
                            string.Join(", ", auditEntry.KeyValues.Select(kv => $"{kv.Key}={kv.Value}"))
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during audit logging");
                // Don't fail the operation due to audit logging errors
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private bool IsSystemProperty(string propertyName)
        {
            return propertyName switch
            {
                "CreatedDate" => true,
                "UpdatedDate" => true,
                "CreatedBy" => true,
                "UpdatedBy" => true,
                "IsDeleted" => true,
                _ => false
            };
        }

        private string GetCurrentUserId()
        {
            try
            {
                var httpContext = _httpContextAccessor?.HttpContext;
                return httpContext?.User?.FindFirst("sub")?.Value ??
                       httpContext?.User?.FindFirst("userId")?.Value ??
                       httpContext?.User?.Identity?.Name ?? "0";
            }
            catch
            {
                return "0";
            }
        }

        private string GetCurrentUserName()
        {
            try
            {
                var httpContext = _httpContextAccessor?.HttpContext;
                return httpContext?.User?.FindFirst("preferred_username")?.Value ??
                       httpContext?.User?.FindFirst("name")?.Value ??
                       httpContext?.User?.Identity?.Name ?? "System";
            }
            catch
            {
                return "System";
            }
        }

        private string GetClientIpAddress()
        {
            try
            {
                var httpContext = _httpContextAccessor?.HttpContext;
                if (httpContext == null) return "127.0.0.1";

                // Check for forwarded headers
                var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrEmpty(forwardedFor))
                {
                    return forwardedFor.Split(',').First().Trim();
                }

                var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
                if (!string.IsNullOrEmpty(realIp))
                {
                    return realIp;
                }

                return httpContext.Connection.RemoteIpAddress?.ToString();
            }
            catch
            {
                return null;
            }
        }

        private string GetUserAgent()
        {
            try
            {
                return _httpContextAccessor?.HttpContext?.Request.Headers["User-Agent"].FirstOrDefault() ?? "Internal/System";
            }
            catch
            {
                return "Internal/System";
            }
        }

        private string GetSessionId()
        {
            try
            {
                return _httpContextAccessor?.HttpContext?.Session?.Id ?? "N/A";
            }
            catch
            {
                return "N/A";
            }
        }

        private string GetCorrelationId()
        {
            try
            {
                return _httpContextAccessor?.HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
            }
            catch
            {
                return Guid.NewGuid().ToString();
            }
        }
    }

    public class AuditEntry
    {
        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
        }

        public EntityEntry Entry { get; }
        public string TableName { get; set; }
        public Dictionary<string, object> KeyValues { get; } = new();
        public Dictionary<string, object> OldValues { get; } = new();
        public Dictionary<string, object> NewValues { get; } = new();
        public List<PropertyEntry> TemporaryProperties { get; } = new();

        public bool HasTemporaryProperties => TemporaryProperties.Any();

        public AuditEntry.EntryAction Action
        {
            get
            {
                return Entry.State switch
                {
                    EntityState.Added => AuditEntry.EntryAction.Insert,
                    EntityState.Deleted => AuditEntry.EntryAction.Delete,
                    EntityState.Modified => AuditEntry.EntryAction.Update,
                    _ => AuditEntry.EntryAction.Unknown
                };
            }
        }

        public enum EntryAction
        {
            Unknown,
            Insert,
            Update,
            Delete
        }
    }
}
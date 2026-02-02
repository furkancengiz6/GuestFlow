using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuestFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyRevenues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TransferRevenue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CityTourRevenue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    YachtTourRevenue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    GeneralRevenue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentCount = table.Column<int>(type: "int", nullable: false),
                    RefundedAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NetRevenue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyRevenues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    To = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    From = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TemplateName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RelatedEntityId = table.Column<int>(type: "int", nullable: true),
                    EmailSize = table.Column<long>(type: "bigint", nullable: true),
                    AttachmentCount = table.Column<int>(type: "int", nullable: false),
                    SmtpResponse = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IsOpened = table.Column<bool>(type: "bit", nullable: false),
                    OpenedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClickCount = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailHistories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailQueues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    To = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsHtml = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    MaxRetryCount = table.Column<int>(type: "int", nullable: false),
                    LastAttemptDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TemplateName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TemplateVariables = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Attachments = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RelatedEntityId = table.Column<int>(type: "int", nullable: true),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailQueues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    VariablesDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    LastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FeatureFlags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Environment = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Production"),
                    RolloutPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    IsEnabledForAdmins = table.Column<bool>(type: "bit", nullable: false),
                    TargetRoles = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TargetUserIds = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EnabledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisabledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnabledBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeatureFlags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RuleType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Conditions = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    NotificationChannel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TemplateName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RecipientType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RecipientId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CheckIntervalMinutes = table.Column<int>(type: "int", nullable: false),
                    LastCheckedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastTriggeredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TriggerCount = table.Column<int>(type: "int", nullable: false),
                    Parameters = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NotificationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RecipientEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecipientPersonnelId = table.Column<int>(type: "int", nullable: true),
                    RecipientGuestId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TemplateName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RelatedEntityId = table.Column<int>(type: "int", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OTAIntegrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProviderCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApiEndpoint = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApiSecret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    WebhookUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastSyncStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastSyncDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SyncErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTAIntegrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Personnels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UserType = table.Column<int>(type: "int", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorSecret = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TwoFactorRecoveryCodes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    TwoFactorSetupDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personnels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PMSIntegrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProviderCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ApiEndpoint = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApiSecret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    WebhookUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WebhookSecret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastSyncStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastSyncDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SyncErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SyncMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PollingIntervalMinutes = table.Column<int>(type: "int", nullable: false),
                    LastConnectionTestDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastConnectionTestResult = table.Column<bool>(type: "bit", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PMSIntegrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServicePackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PackageType = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    FinalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PackageContent = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePackages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MainteneceMode = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    DefaultCurrency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultCost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PlateNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    DailyPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Airports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Airports_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hotels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HotelName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    StarRating = table.Column<int>(type: "int", maxLength: 1, nullable: true),
                    CheckInTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    CheckOutTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    RoomTypes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Amenities = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hotels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hotels_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Restaurants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RestaurantName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    CuisineType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: true),
                    OperatingHours = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReservationRequired = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Restaurants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Restaurants_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Tours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tours_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OTAReservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OTAIntegrationId = table.Column<int>(type: "int", nullable: false),
                    OTAReservationId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OTAHotelId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OTARoomTypeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CheckInDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOutDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuestCount = table.Column<int>(type: "int", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GuestName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GuestEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuestPhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OTACreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OTALastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuestFlowReservationId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTAReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OTAReservations_OTAIntegrations_OTAIntegrationId",
                        column: x => x.OTAIntegrationId,
                        principalTable: "OTAIntegrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OTAWebhookLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OTAIntegrationId = table.Column<int>(type: "int", nullable: false),
                    ProviderCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ReservationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Signature = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    MaxRetries = table.Column<int>(type: "int", nullable: false),
                    LastRetryAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextRetryAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ErrorDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeadLetter = table.Column<bool>(type: "bit", nullable: false),
                    DeadLetterAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTAWebhookLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OTAWebhookLogs_OTAIntegrations_OTAIntegrationId",
                        column: x => x.OTAIntegrationId,
                        principalTable: "OTAIntegrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PermissionId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DailyNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NoteDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RoomNumber = table.Column<int>(type: "int", nullable: false),
                    NoteText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PersonnelId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyNotes_Personnels_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Personnels",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "JournalEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: true),
                    PostingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    TotalDebit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalCredit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    PostedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PostedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsReversed = table.Column<bool>(type: "bit", nullable: false),
                    ReversedByJournalEntryId = table.Column<int>(type: "int", nullable: true),
                    ReversedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ReversedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    ReversedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalEntries_Personnels_CreatedByPersonnelId",
                        column: x => x.CreatedByPersonnelId,
                        principalTable: "Personnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalEntries_Personnels_PostedByPersonnelId",
                        column: x => x.PostedByPersonnelId,
                        principalTable: "Personnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JournalEntries_Personnels_ReversedByPersonnelId",
                        column: x => x.ReversedByPersonnelId,
                        principalTable: "Personnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LoginAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    IsSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AttemptDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PersonnelId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoginAttempts_Personnels_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Personnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PersonnelId = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedByIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedByIp = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Personnels_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Personnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PMSReservationMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PMSIntegrationId = table.Column<int>(type: "int", nullable: false),
                    PMSReservationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GuestFlowReservationId = table.Column<int>(type: "int", nullable: true),
                    LastSyncedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SyncStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ConflictDetails = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PMSReservationMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PMSReservationMappings_PMSIntegrations_PMSIntegrationId",
                        column: x => x.PMSIntegrationId,
                        principalTable: "PMSIntegrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PMSSyncHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PMSIntegrationId = table.Column<int>(type: "int", nullable: false),
                    SyncType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SyncStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SyncEndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecordsProcessed = table.Column<int>(type: "int", nullable: true),
                    RecordsSucceeded = table.Column<int>(type: "int", nullable: true),
                    RecordsFailed = table.Column<int>(type: "int", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SyncDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PMSSyncHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PMSSyncHistories_PMSIntegrations_PMSIntegrationId",
                        column: x => x.PMSIntegrationId,
                        principalTable: "PMSIntegrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OTAHotelMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OTAIntegrationId = table.Column<int>(type: "int", nullable: false),
                    HotelId = table.Column<int>(type: "int", nullable: false),
                    OTARoomTypeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OTARoomTypeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GuestFlowRoomType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PriceMultiplier = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTAHotelMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OTAHotelMappings_Hotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OTAHotelMappings_OTAIntegrations_OTAIntegrationId",
                        column: x => x.OTAIntegrationId,
                        principalTable: "OTAIntegrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OTAPriceUpdates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OTAIntegrationId = table.Column<int>(type: "int", nullable: false),
                    HotelId = table.Column<int>(type: "int", nullable: false),
                    OTARoomTypeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    UpdateStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTAPriceUpdates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OTAPriceUpdates_Hotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OTAPriceUpdates_OTAIntegrations_OTAIntegrationId",
                        column: x => x.OTAIntegrationId,
                        principalTable: "OTAIntegrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JournalLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalEntryId = table.Column<int>(type: "int", nullable: false),
                    AccountCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReferenceId = table.Column<int>(type: "int", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalLines_JournalEntries_JournalEntryId",
                        column: x => x.JournalEntryId,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CityTours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TourDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DurationHours = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AdultCount = table.Column<int>(type: "int", nullable: true),
                    ChildCount = table.Column<int>(type: "int", nullable: true),
                    InfantCount = table.Column<int>(type: "int", nullable: true),
                    OwnerGuestId = table.Column<int>(type: "int", nullable: false),
                    PersonnelId = table.Column<int>(type: "int", nullable: true),
                    TourGuideId = table.Column<int>(type: "int", nullable: true),
                    AssistantGuideId = table.Column<int>(type: "int", nullable: true),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    TourId = table.Column<int>(type: "int", nullable: true),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    FinalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    PickupTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    TourConfirmationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GroupLeaderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GroupLeaderPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmergencyContactRelation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MeetingPersonName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MeetingPointDetails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PickupHotelId = table.Column<int>(type: "int", nullable: true),
                    PickupLocation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DropoffLocation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VehicleId = table.Column<int>(type: "int", nullable: true),
                    DriverName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GuideName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GuidePhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    GuideLanguages = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BackupGuideName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BackupGuidePhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ExternalVehiclePlate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ExternalDriverName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExternalDriverPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TourDifficultyLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WeatherDependent = table.Column<bool>(type: "bit", nullable: true),
                    MinimumParticipantCount = table.Column<int>(type: "int", nullable: true),
                    MaximumParticipantCount = table.Column<int>(type: "int", nullable: true),
                    DietaryRequirements = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AccessibilityNeeds = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PhotographyAllowed = table.Column<bool>(type: "bit", nullable: true),
                    SpecialEquipment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SupplierName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SupplierCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SupplierCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    SupplierInvoiceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsVipGroup = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ConciergeInternalNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ServiceInfoPdfUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CityTours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CityTours_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CityTours_Hotels_PickupHotelId",
                        column: x => x.PickupHotelId,
                        principalTable: "Hotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CityTours_Personnels_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Personnels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CityTours_Tours_TourId",
                        column: x => x.TourId,
                        principalTable: "Tours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CityTours_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PackageCityTours",
                columns: table => new
                {
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    CityTourId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageCityTours", x => new { x.PackageId, x.CityTourId });
                    table.ForeignKey(
                        name: "FK_PackageCityTours_CityTours_CityTourId",
                        column: x => x.CityTourId,
                        principalTable: "CityTours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackageCityTours_ServicePackages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "ServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuestBehaviors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    BehaviorType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BehaviorValue = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    BehaviorDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeOfDay = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DayOfWeek = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Season = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SentimentScore = table.Column<double>(type: "float", nullable: true),
                    SatisfactionScore = table.Column<double>(type: "float", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RelatedEntityId = table.Column<int>(type: "int", nullable: true),
                    SyncedToGraph = table.Column<bool>(type: "bit", nullable: false),
                    GraphSyncDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestBehaviors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GuestCityTours",
                columns: table => new
                {
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    CityTourId = table.Column<int>(type: "int", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestCityTours", x => new { x.GuestId, x.CityTourId });
                    table.ForeignKey(
                        name: "FK_GuestCityTours_CityTours_CityTourId",
                        column: x => x.CityTourId,
                        principalTable: "CityTours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuestPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    PreferredRoomType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RoomSpecialRequests = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BedPreference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SmokingPreference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DietaryPreferences = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FoodAllergies = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SpecialFoodRequests = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ActivityPreferences = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Interests = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrefersEmail = table.Column<bool>(type: "bit", nullable: false),
                    PrefersSMS = table.Column<bool>(type: "bit", nullable: false),
                    PrefersWhatsApp = table.Column<bool>(type: "bit", nullable: false),
                    PrefersPhone = table.Column<bool>(type: "bit", nullable: false),
                    PreferredLanguage = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestPreferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Guests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nationality = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GuestCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsSpecialGuest = table.Column<bool>(type: "bit", nullable: false),
                    IsAnonymized = table.Column<bool>(type: "bit", nullable: false),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmergencyContactRelation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RoomNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CheckInDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckOutDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    HotelId = table.Column<int>(type: "int", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PreferencesId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Guests_GuestPreferences_PreferencesId",
                        column: x => x.PreferencesId,
                        principalTable: "GuestPreferences",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Guests_Hotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "GuestRoomHistoryEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    RoomNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestRoomHistoryEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuestRoomHistoryEntity_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GuestRoomHistoryEntity_Personnels_AssignedByPersonnelId",
                        column: x => x.AssignedByPersonnelId,
                        principalTable: "Personnels",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GuestStaffInteractions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    InteractionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InteractionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    SentimentScore = table.Column<double>(type: "float", nullable: true),
                    SatisfactionScore = table.Column<double>(type: "float", nullable: true),
                    Context = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ServiceId = table.Column<int>(type: "int", nullable: true),
                    ServiceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RelationshipStrength = table.Column<double>(type: "float", nullable: true),
                    SyncedToGraph = table.Column<bool>(type: "bit", nullable: false),
                    GraphSyncDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestStaffInteractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuestStaffInteractions_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GuestStaffInteractions_Personnels_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Personnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Itineraries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    PersonnelId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ItineraryNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Itineraries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Itineraries_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Itineraries_Personnels_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Personnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PMSGuestMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PMSIntegrationId = table.Column<int>(type: "int", nullable: false),
                    PMSGuestId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GuestFlowGuestId = table.Column<int>(type: "int", nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SyncStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ConflictDetails = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PMSGuestMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PMSGuestMappings_Guests_GuestFlowGuestId",
                        column: x => x.GuestFlowGuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PMSGuestMappings_PMSIntegrations_PMSIntegrationId",
                        column: x => x.PMSIntegrationId,
                        principalTable: "PMSIntegrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrivacyActionHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RequestedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivacyActionHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrivacyActionHistories_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrivacyActionHistories_Personnels_RequestedByPersonnelId",
                        column: x => x.RequestedByPersonnelId,
                        principalTable: "Personnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    PersonnelId = table.Column<int>(type: "int", nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReservationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfirmedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reservations_Personnels_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Personnels",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RoomAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    HotelId = table.Column<int>(type: "int", nullable: true),
                    RoomNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomAssignments_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomAssignments_Hotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotels",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SmsHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MessageId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GatewayResponse = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    TemplateName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RelatedEntityId = table.Column<int>(type: "int", nullable: true),
                    GuestId = table.Column<int>(type: "int", nullable: true),
                    PersonnelId = table.Column<int>(type: "int", nullable: true),
                    SmsType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmsHistories_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SmsHistories_Personnels_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Personnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "StaffBehaviors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    BehaviorType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BehaviorValue = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    BehaviorDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuestId = table.Column<int>(type: "int", nullable: true),
                    ServiceId = table.Column<int>(type: "int", nullable: true),
                    ServiceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SuccessScore = table.Column<double>(type: "float", nullable: true),
                    GuestSatisfaction = table.Column<double>(type: "float", nullable: true),
                    ResponseTimeMinutes = table.Column<int>(type: "int", nullable: true),
                    PreferenceLearned = table.Column<bool>(type: "bit", nullable: false),
                    ProblemSolved = table.Column<bool>(type: "bit", nullable: false),
                    SyncedToGraph = table.Column<bool>(type: "bit", nullable: false),
                    GraphSyncDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffBehaviors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffBehaviors_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StaffBehaviors_Personnels_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Personnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transfers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PickupAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DropoffAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TransferDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PickupTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    ServiceStartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    PickupConfirmationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DropoffConfirmationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsFromAirport = table.Column<bool>(type: "bit", nullable: false),
                    TransferType = table.Column<int>(type: "int", nullable: false),
                    ContactPersonName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MeetingPointDetails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    GroupSize = table.Column<int>(type: "int", nullable: true),
                    ChildCount = table.Column<int>(type: "int", nullable: true),
                    InfantCount = table.Column<int>(type: "int", nullable: true),
                    GuestLanguage = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PrimaryContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SecondaryContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    PersonnelId = table.Column<int>(type: "int", nullable: true),
                    DriverId = table.Column<int>(type: "int", nullable: true),
                    AirportId = table.Column<int>(type: "int", nullable: true),
                    VehicleId = table.Column<int>(type: "int", nullable: true),
                    PickupCityId = table.Column<int>(type: "int", nullable: true),
                    DropoffCityId = table.Column<int>(type: "int", nullable: true),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    FinalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    SupplierName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SupplierCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SupplierCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    SupplierInvoiceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SupplierContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SupplierEmergencyContact = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DriverName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExternalVehiclePlate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ExternalDriverName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExternalDriverPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    AccessibilityRequirements = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SpecialHandlingNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ConciergeInternalNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    GuestVisibleNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ServiceInfoPdfUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    TransportMode = table.Column<int>(type: "int", nullable: true),
                    LuggageCount = table.Column<int>(type: "int", nullable: true),
                    ReturnTransferId = table.Column<int>(type: "int", nullable: true),
                    IsVip = table.Column<bool>(type: "bit", nullable: false),
                    HotelEntityId = table.Column<int>(type: "int", nullable: true),
                    HotelEntityId1 = table.Column<int>(type: "int", nullable: true),
                    RestaurantEntityId = table.Column<int>(type: "int", nullable: true),
                    RestaurantEntityId1 = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transfers_Airports_AirportId",
                        column: x => x.AirportId,
                        principalTable: "Airports",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transfers_Cities_DropoffCityId",
                        column: x => x.DropoffCityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transfers_Cities_PickupCityId",
                        column: x => x.PickupCityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transfers_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transfers_Hotels_HotelEntityId",
                        column: x => x.HotelEntityId,
                        principalTable: "Hotels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transfers_Hotels_HotelEntityId1",
                        column: x => x.HotelEntityId1,
                        principalTable: "Hotels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transfers_Personnels_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Personnels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transfers_Restaurants_RestaurantEntityId",
                        column: x => x.RestaurantEntityId,
                        principalTable: "Restaurants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transfers_Restaurants_RestaurantEntityId1",
                        column: x => x.RestaurantEntityId1,
                        principalTable: "Restaurants",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Transfers_Vehicles_VehicleId",
                        column: x => x.VehicleId,
                        principalTable: "Vehicles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WhatsAppHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MessageId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GatewayResponse = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    TemplateName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TemplateParameters = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    MessageType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RichMessageData = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RelatedEntityId = table.Column<int>(type: "int", nullable: true),
                    GuestId = table.Column<int>(type: "int", nullable: true),
                    PersonnelId = table.Column<int>(type: "int", nullable: true),
                    MessageCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhatsAppHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WhatsAppHistories_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_WhatsAppHistories_Personnels_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Personnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "YachtTours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TourDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumberOfPeople = table.Column<int>(type: "int", nullable: false),
                    ChildCount = table.Column<int>(type: "int", nullable: true),
                    InfantCount = table.Column<int>(type: "int", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SpecialRequest = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    YachtName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GroupLeaderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GroupLeaderPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EmergencyContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmergencyContactRelation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OwnerGuestId = table.Column<int>(type: "int", nullable: false),
                    PersonnelId = table.Column<int>(type: "int", nullable: true),
                    YachtId = table.Column<int>(type: "int", nullable: true),
                    CaptainId = table.Column<int>(type: "int", nullable: true),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    FinalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    TourCategory = table.Column<int>(type: "int", nullable: true),
                    LifeJacketsProvided = table.Column<bool>(type: "bit", nullable: true),
                    LifeJacketCount = table.Column<int>(type: "int", nullable: true),
                    SafetyEquipmentCheck = table.Column<bool>(type: "bit", nullable: true),
                    EmergencyEquipment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    YachtCapacity = table.Column<int>(type: "int", nullable: true),
                    YachtType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    YachtLicenceRequired = table.Column<bool>(type: "bit", nullable: true),
                    CoastGuardApproved = table.Column<bool>(type: "bit", nullable: true),
                    PickupPier = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DropoffPier = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PierAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PickupHotelId = table.Column<int>(type: "int", nullable: true),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    SafetyBriefingTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MarinaPickupTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    WeatherCheckTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CrewSize = table.Column<int>(type: "int", nullable: true),
                    CaptainExperience = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FuelRange = table.Column<int>(type: "int", nullable: true),
                    WeatherBackupPlan = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FuelLevelCheck = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CaptainPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SwimmingProficiency = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MedicalConditions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AlcoholPolicy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FoodBeverageIncluded = table.Column<bool>(type: "bit", nullable: true),
                    BeverageType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MusicSystem = table.Column<bool>(type: "bit", nullable: true),
                    WaterSportsEquipment = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    LifeGuardCertified = table.Column<bool>(type: "bit", nullable: true),
                    CoastGuardInspectionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MarinaContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MarinaContactPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SupplierName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SupplierCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    SupplierCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    SupplierInvoiceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    WeatherDependent = table.Column<bool>(type: "bit", nullable: false),
                    ConciergeInternalNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ServiceInfoPdfUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YachtTours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YachtTours_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YachtTours_Guests_OwnerGuestId",
                        column: x => x.OwnerGuestId,
                        principalTable: "Guests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_YachtTours_Hotels_PickupHotelId",
                        column: x => x.PickupHotelId,
                        principalTable: "Hotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_YachtTours_Personnels_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Personnels",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ItineraryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItineraryId = table.Column<int>(type: "int", nullable: false),
                    ItemType = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    ScheduledDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItineraryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItineraryItems_Itineraries_ItineraryId",
                        column: x => x.ItineraryId,
                        principalTable: "Itineraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageTransfers",
                columns: table => new
                {
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    TransferId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageTransfers", x => new { x.PackageId, x.TransferId });
                    table.ForeignKey(
                        name: "FK_PackageTransfers_ServicePackages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "ServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PackageTransfers_Transfers_TransferId",
                        column: x => x.TransferId,
                        principalTable: "Transfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantReservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RestaurantId = table.Column<int>(type: "int", nullable: false),
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    PersonnelId = table.Column<int>(type: "int", nullable: false),
                    ReservationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReservationTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    NumberOfGuests = table.Column<int>(type: "int", nullable: false),
                    TableNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SpecialRequests = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ConfirmationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransferId = table.Column<int>(type: "int", nullable: true),
                    ReturnTransferId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestaurantReservations_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestaurantReservations_Personnels_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Personnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestaurantReservations_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestaurantReservations_Transfers_ReturnTransferId",
                        column: x => x.ReturnTransferId,
                        principalTable: "Transfers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RestaurantReservations_Transfers_TransferId",
                        column: x => x.TransferId,
                        principalTable: "Transfers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GuestYachtTours",
                columns: table => new
                {
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    YachtTourId = table.Column<int>(type: "int", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestYachtTours", x => new { x.GuestId, x.YachtTourId });
                    table.ForeignKey(
                        name: "FK_GuestYachtTours_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GuestYachtTours_YachtTours_YachtTourId",
                        column: x => x.YachtTourId,
                        principalTable: "YachtTours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNumber = table.Column<int>(type: "int", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PdfUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    PersonnelId = table.Column<int>(type: "int", nullable: true),
                    LockedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsPdfGenerated = table.Column<bool>(type: "bit", nullable: false),
                    PdfGeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CityTourEntityId = table.Column<int>(type: "int", nullable: true),
                    TransferEntityId = table.Column<int>(type: "int", nullable: true),
                    YachtTourEntityId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_CityTours_CityTourEntityId",
                        column: x => x.CityTourEntityId,
                        principalTable: "CityTours",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Invoices_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Invoices_Personnels_LockedByPersonnelId",
                        column: x => x.LockedByPersonnelId,
                        principalTable: "Personnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Personnels_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Personnels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Invoices_Transfers_TransferEntityId",
                        column: x => x.TransferEntityId,
                        principalTable: "Transfers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Invoices_YachtTours_YachtTourEntityId",
                        column: x => x.YachtTourEntityId,
                        principalTable: "YachtTours",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PackageYachtTours",
                columns: table => new
                {
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    YachtTourId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageYachtTours", x => new { x.PackageId, x.YachtTourId });
                    table.ForeignKey(
                        name: "FK_PackageYachtTours_ServicePackages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "ServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PackageYachtTours_YachtTours_YachtTourId",
                        column: x => x.YachtTourId,
                        principalTable: "YachtTours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PackageRestaurantReservations",
                columns: table => new
                {
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    RestaurantReservationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageRestaurantReservations", x => new { x.PackageId, x.RestaurantReservationId });
                    table.ForeignKey(
                        name: "FK_PackageRestaurantReservations_RestaurantReservations_RestaurantReservationId",
                        column: x => x.RestaurantReservationId,
                        principalTable: "RestaurantReservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackageRestaurantReservations_ServicePackages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "ServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierCosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    TransferId = table.Column<int>(type: "int", nullable: true),
                    CityTourId = table.Column<int>(type: "int", nullable: true),
                    YachtTourId = table.Column<int>(type: "int", nullable: true),
                    RestaurantReservationId = table.Column<int>(type: "int", nullable: true),
                    CostAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CostType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierCosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierCosts_CityTours_CityTourId",
                        column: x => x.CityTourId,
                        principalTable: "CityTours",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupplierCosts_RestaurantReservations_RestaurantReservationId",
                        column: x => x.RestaurantReservationId,
                        principalTable: "RestaurantReservations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupplierCosts_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplierCosts_Transfers_TransferId",
                        column: x => x.TransferId,
                        principalTable: "Transfers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SupplierCosts_YachtTours_YachtTourId",
                        column: x => x.YachtTourId,
                        principalTable: "YachtTours",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InvoiceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VatRate = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    VatAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: true),
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    CollectedByPersonnelId = table.Column<int>(type: "int", nullable: false),
                    TransferId = table.Column<int>(type: "int", nullable: true),
                    CityTourId = table.Column<int>(type: "int", nullable: true),
                    YachtTourId = table.Column<int>(type: "int", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GatewayResponse = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    RefundDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefundReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TransferEntityId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_CityTours_CityTourId",
                        column: x => x.CityTourId,
                        principalTable: "CityTours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Personnels_CollectedByPersonnelId",
                        column: x => x.CollectedByPersonnelId,
                        principalTable: "Personnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Transfers_TransferEntityId",
                        column: x => x.TransferEntityId,
                        principalTable: "Transfers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Payments_Transfers_TransferId",
                        column: x => x.TransferId,
                        principalTable: "Transfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_YachtTours_YachtTourId",
                        column: x => x.YachtTourId,
                        principalTable: "YachtTours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "CreatedByPersonnelId", "CreatedDate", "IsDeleted", "MainteneceMode", "UpdatedByPersonnelId", "UpdatedDate" },
                values: new object[] { 1, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, false, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Airports_CityId",
                table: "Airports",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_CityTours_CityId",
                table: "CityTours",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_CityTours_IsVipGroup",
                table: "CityTours",
                column: "IsVipGroup");

            migrationBuilder.CreateIndex(
                name: "IX_CityTours_OwnerGuestId",
                table: "CityTours",
                column: "OwnerGuestId");

            migrationBuilder.CreateIndex(
                name: "IX_CityTours_PersonnelId",
                table: "CityTours",
                column: "PersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_CityTours_PickupHotelId",
                table: "CityTours",
                column: "PickupHotelId");

            migrationBuilder.CreateIndex(
                name: "IX_CityTours_Status",
                table: "CityTours",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CityTours_Status_TourDate",
                table: "CityTours",
                columns: new[] { "Status", "TourDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CityTours_TourDate",
                table: "CityTours",
                column: "TourDate");

            migrationBuilder.CreateIndex(
                name: "IX_CityTours_TourDate_Status_IsDeleted",
                table: "CityTours",
                columns: new[] { "TourDate", "Status", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_CityTours_TourGuideId_TourDate",
                table: "CityTours",
                columns: new[] { "TourGuideId", "TourDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CityTours_TourId",
                table: "CityTours",
                column: "TourId");

            migrationBuilder.CreateIndex(
                name: "IX_CityTours_VehicleId_TourDate",
                table: "CityTours",
                columns: new[] { "VehicleId", "TourDate" });

            migrationBuilder.CreateIndex(
                name: "IX_DailyNotes_PersonnelId",
                table: "DailyNotes",
                column: "PersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyRevenues_Date_Currency",
                table: "DailyRevenues",
                columns: new[] { "Date", "Currency" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailHistories_RelatedEntityType_RelatedEntityId",
                table: "EmailHistories",
                columns: new[] { "RelatedEntityType", "RelatedEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailHistories_SentDate",
                table: "EmailHistories",
                column: "SentDate");

            migrationBuilder.CreateIndex(
                name: "IX_EmailHistories_Status",
                table: "EmailHistories",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EmailHistories_TemplateName",
                table: "EmailHistories",
                column: "TemplateName");

            migrationBuilder.CreateIndex(
                name: "IX_EmailHistories_To",
                table: "EmailHistories",
                column: "To");

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueues_ScheduledDate",
                table: "EmailQueues",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueues_Status",
                table: "EmailQueues",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EmailQueues_Status_Priority",
                table: "EmailQueues",
                columns: new[] { "Status", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_Category",
                table: "EmailTemplates",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_IsActive",
                table: "EmailTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_EmailTemplates_Name",
                table: "EmailTemplates",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeatureFlags_Name_Environment",
                table: "FeatureFlags",
                columns: new[] { "Name", "Environment" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuestBehaviors_GuestId",
                table: "GuestBehaviors",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestBehaviors_GuestId_BehaviorDate",
                table: "GuestBehaviors",
                columns: new[] { "GuestId", "BehaviorDate" });

            migrationBuilder.CreateIndex(
                name: "IX_GuestBehaviors_GuestId_BehaviorType",
                table: "GuestBehaviors",
                columns: new[] { "GuestId", "BehaviorType" });

            migrationBuilder.CreateIndex(
                name: "IX_GuestBehaviors_GuestId_SyncedToGraph",
                table: "GuestBehaviors",
                columns: new[] { "GuestId", "SyncedToGraph" });

            migrationBuilder.CreateIndex(
                name: "IX_GuestCityTours_CityTourId",
                table: "GuestCityTours",
                column: "CityTourId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestPreferences_GuestId",
                table: "GuestPreferences",
                column: "GuestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuestRoomHistoryEntity_AssignedByPersonnelId",
                table: "GuestRoomHistoryEntity",
                column: "AssignedByPersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestRoomHistoryEntity_GuestId",
                table: "GuestRoomHistoryEntity",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_Guests_GuestCode",
                table: "Guests",
                column: "GuestCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Guests_HotelId",
                table: "Guests",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_Guests_PreferencesId",
                table: "Guests",
                column: "PreferencesId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestStaffInteractions_GuestId",
                table: "GuestStaffInteractions",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestStaffInteractions_GuestId_InteractionDate",
                table: "GuestStaffInteractions",
                columns: new[] { "GuestId", "InteractionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_GuestStaffInteractions_GuestId_StaffId",
                table: "GuestStaffInteractions",
                columns: new[] { "GuestId", "StaffId" });

            migrationBuilder.CreateIndex(
                name: "IX_GuestStaffInteractions_GuestId_StaffId_InteractionDate",
                table: "GuestStaffInteractions",
                columns: new[] { "GuestId", "StaffId", "InteractionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_GuestStaffInteractions_GuestId_SyncedToGraph",
                table: "GuestStaffInteractions",
                columns: new[] { "GuestId", "SyncedToGraph" });

            migrationBuilder.CreateIndex(
                name: "IX_GuestStaffInteractions_StaffId",
                table: "GuestStaffInteractions",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestYachtTours_GuestId",
                table: "GuestYachtTours",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestYachtTours_GuestId_YachtTourId",
                table: "GuestYachtTours",
                columns: new[] { "GuestId", "YachtTourId" });

            migrationBuilder.CreateIndex(
                name: "IX_GuestYachtTours_YachtTourId",
                table: "GuestYachtTours",
                column: "YachtTourId");

            migrationBuilder.CreateIndex(
                name: "IX_Hotels_CityId",
                table: "Hotels",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Hotels_HotelName",
                table: "Hotels",
                column: "HotelName");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_InvoiceId_ServiceType_ServiceId",
                table: "InvoiceItems",
                columns: new[] { "InvoiceId", "ServiceType", "ServiceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CityTourEntityId",
                table: "Invoices",
                column: "CityTourEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_GuestId",
                table: "Invoices",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber",
                table: "Invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_LockedByPersonnelId",
                table: "Invoices",
                column: "LockedByPersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PersonnelId",
                table: "Invoices",
                column: "PersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_TransferEntityId",
                table: "Invoices",
                column: "TransferEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_YachtTourEntityId",
                table: "Invoices",
                column: "YachtTourEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Itineraries_EndDate",
                table: "Itineraries",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Itineraries_GuestId",
                table: "Itineraries",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_Itineraries_ItineraryNumber",
                table: "Itineraries",
                column: "ItineraryNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Itineraries_PersonnelId",
                table: "Itineraries",
                column: "PersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_Itineraries_StartDate",
                table: "Itineraries",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_ItineraryItems_ItineraryId",
                table: "ItineraryItems",
                column: "ItineraryId");

            migrationBuilder.CreateIndex(
                name: "IX_ItineraryItems_ItineraryId_Order",
                table: "ItineraryItems",
                columns: new[] { "ItineraryId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_ItineraryItems_ScheduledDateTime",
                table: "ItineraryItems",
                column: "ScheduledDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_CreatedByPersonnelId",
                table: "JournalEntries",
                column: "CreatedByPersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_InvoiceId",
                table: "JournalEntries",
                column: "InvoiceId",
                unique: true,
                filter: "[InvoiceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_PostedByPersonnelId",
                table: "JournalEntries",
                column: "PostedByPersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_PostingDate",
                table: "JournalEntries",
                column: "PostingDate");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_ReversedByJournalEntryId",
                table: "JournalEntries",
                column: "ReversedByJournalEntryId",
                filter: "[ReversedByJournalEntryId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_ReversedByPersonnelId",
                table: "JournalEntries",
                column: "ReversedByPersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalLines_JournalEntryId",
                table: "JournalLines",
                column: "JournalEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_AttemptDate",
                table: "LoginAttempts",
                column: "AttemptDate");

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_Email_AttemptDate",
                table: "LoginAttempts",
                columns: new[] { "Email", "AttemptDate" });

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_IpAddress_AttemptDate",
                table: "LoginAttempts",
                columns: new[] { "IpAddress", "AttemptDate" });

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_PersonnelId",
                table: "LoginAttempts",
                column: "PersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRules_Category",
                table: "NotificationRules",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRules_IsActive",
                table: "NotificationRules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRules_IsActive_Priority",
                table: "NotificationRules",
                columns: new[] { "IsActive", "Priority" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRules_RuleType",
                table: "NotificationRules",
                column: "RuleType");

            migrationBuilder.CreateIndex(
                name: "IX_OTAHotelMappings_HotelId",
                table: "OTAHotelMappings",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_OTAHotelMappings_OTAIntegrationId",
                table: "OTAHotelMappings",
                column: "OTAIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_OTAPriceUpdates_HotelId",
                table: "OTAPriceUpdates",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_OTAPriceUpdates_OTAIntegrationId",
                table: "OTAPriceUpdates",
                column: "OTAIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_OTAReservations_OTAIntegrationId",
                table: "OTAReservations",
                column: "OTAIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_OTAWebhookLogs_IdempotencyKey",
                table: "OTAWebhookLogs",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OTAWebhookLogs_IsDeadLetter",
                table: "OTAWebhookLogs",
                column: "IsDeadLetter",
                filter: "[IsDeadLetter] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_OTAWebhookLogs_NextRetryAt",
                table: "OTAWebhookLogs",
                column: "NextRetryAt",
                filter: "[NextRetryAt] IS NOT NULL AND [Status] = 'Failed'");

            migrationBuilder.CreateIndex(
                name: "IX_OTAWebhookLogs_OTAIntegrationId_Status",
                table: "OTAWebhookLogs",
                columns: new[] { "OTAIntegrationId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OTAWebhookLogs_ProviderCode_Status",
                table: "OTAWebhookLogs",
                columns: new[] { "ProviderCode", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PackageCityTours_CityTourId",
                table: "PackageCityTours",
                column: "CityTourId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageRestaurantReservations_RestaurantReservationId",
                table: "PackageRestaurantReservations",
                column: "RestaurantReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageTransfers_TransferId",
                table: "PackageTransfers",
                column: "TransferId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageYachtTours_YachtTourId",
                table: "PackageYachtTours",
                column: "YachtTourId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CityTourId",
                table: "Payments",
                column: "CityTourId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CollectedByPersonnelId",
                table: "Payments",
                column: "CollectedByPersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Currency",
                table: "Payments",
                column: "Currency");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_GuestId",
                table: "Payments",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InvoiceId",
                table: "Payments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentDate",
                table: "Payments",
                column: "PaymentDate");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentNumber",
                table: "Payments",
                column: "PaymentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status",
                table: "Payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TransferEntityId",
                table: "Payments",
                column: "TransferEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TransferId",
                table: "Payments",
                column: "TransferId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_YachtTourId",
                table: "Payments",
                column: "YachtTourId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                table: "Permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Personnels_Email",
                table: "Personnels",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PMSGuestMappings_GuestFlowGuestId",
                table: "PMSGuestMappings",
                column: "GuestFlowGuestId");

            migrationBuilder.CreateIndex(
                name: "IX_PMSGuestMappings_PMSGuestId",
                table: "PMSGuestMappings",
                column: "PMSGuestId");

            migrationBuilder.CreateIndex(
                name: "IX_PMSGuestMappings_PMSIntegrationId",
                table: "PMSGuestMappings",
                column: "PMSIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_PMSGuestMappings_PMSIntegrationId_PMSGuestId",
                table: "PMSGuestMappings",
                columns: new[] { "PMSIntegrationId", "PMSGuestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PMSIntegrations_IsActive",
                table: "PMSIntegrations",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PMSIntegrations_ProviderCode",
                table: "PMSIntegrations",
                column: "ProviderCode");

            migrationBuilder.CreateIndex(
                name: "IX_PMSReservationMappings_PMSIntegrationId",
                table: "PMSReservationMappings",
                column: "PMSIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_PMSReservationMappings_PMSIntegrationId_PMSReservationId",
                table: "PMSReservationMappings",
                columns: new[] { "PMSIntegrationId", "PMSReservationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PMSReservationMappings_PMSReservationId",
                table: "PMSReservationMappings",
                column: "PMSReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_PMSSyncHistories_PMSIntegrationId",
                table: "PMSSyncHistories",
                column: "PMSIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_PMSSyncHistories_PMSIntegrationId_SyncType_Status",
                table: "PMSSyncHistories",
                columns: new[] { "PMSIntegrationId", "SyncType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PMSSyncHistories_SyncStartTime",
                table: "PMSSyncHistories",
                column: "SyncStartTime");

            migrationBuilder.CreateIndex(
                name: "IX_PrivacyActionHistories_ActionDate",
                table: "PrivacyActionHistories",
                column: "ActionDate");

            migrationBuilder.CreateIndex(
                name: "IX_PrivacyActionHistories_GuestId_ActionDate",
                table: "PrivacyActionHistories",
                columns: new[] { "GuestId", "ActionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PrivacyActionHistories_RequestedByPersonnelId",
                table: "PrivacyActionHistories",
                column: "RequestedByPersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_PersonnelId",
                table: "RefreshTokens",
                column: "PersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_PersonnelId_IsRevoked",
                table: "RefreshTokens",
                columns: new[] { "PersonnelId", "IsRevoked" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_Token",
                table: "RefreshTokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_GuestId",
                table: "Reservations",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_PersonnelId",
                table: "Reservations",
                column: "PersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ReservationNumber",
                table: "Reservations",
                column: "ReservationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantReservations_ConfirmationNumber",
                table: "RestaurantReservations",
                column: "ConfirmationNumber");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantReservations_GuestId",
                table: "RestaurantReservations",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantReservations_PersonnelId",
                table: "RestaurantReservations",
                column: "PersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantReservations_ReservationDate",
                table: "RestaurantReservations",
                column: "ReservationDate");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantReservations_RestaurantId",
                table: "RestaurantReservations",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantReservations_ReturnTransferId",
                table: "RestaurantReservations",
                column: "ReturnTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantReservations_TransferId",
                table: "RestaurantReservations",
                column: "TransferId");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_CityId",
                table: "Restaurants",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_RestaurantName",
                table: "Restaurants",
                column: "RestaurantName");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleName_PermissionId",
                table: "RolePermissions",
                columns: new[] { "RoleName", "PermissionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoomAssignments_GuestId",
                table: "RoomAssignments",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomAssignments_HotelId",
                table: "RoomAssignments",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePackages_PackageName",
                table: "ServicePackages",
                column: "PackageName");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePackages_PackageType",
                table: "ServicePackages",
                column: "PackageType");

            migrationBuilder.CreateIndex(
                name: "IX_SmsHistories_GuestId",
                table: "SmsHistories",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsHistories_PersonnelId",
                table: "SmsHistories",
                column: "PersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffBehaviors_GuestId",
                table: "StaffBehaviors",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffBehaviors_StaffId",
                table: "StaffBehaviors",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffBehaviors_StaffId_BehaviorDate",
                table: "StaffBehaviors",
                columns: new[] { "StaffId", "BehaviorDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffBehaviors_StaffId_BehaviorType",
                table: "StaffBehaviors",
                columns: new[] { "StaffId", "BehaviorType" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffBehaviors_StaffId_GuestId",
                table: "StaffBehaviors",
                columns: new[] { "StaffId", "GuestId" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffBehaviors_StaffId_SyncedToGraph",
                table: "StaffBehaviors",
                columns: new[] { "StaffId", "SyncedToGraph" });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierCosts_CityTourId",
                table: "SupplierCosts",
                column: "CityTourId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierCosts_RestaurantReservationId",
                table: "SupplierCosts",
                column: "RestaurantReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierCosts_SupplierId",
                table: "SupplierCosts",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierCosts_TransferId",
                table: "SupplierCosts",
                column: "TransferId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierCosts_YachtTourId",
                table: "SupplierCosts",
                column: "YachtTourId");

            migrationBuilder.CreateIndex(
                name: "IX_Tours_CityId",
                table: "Tours",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_AirportId",
                table: "Transfers",
                column: "AirportId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_DriverId_TransferDate",
                table: "Transfers",
                columns: new[] { "DriverId", "TransferDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_DropoffCityId",
                table: "Transfers",
                column: "DropoffCityId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_GuestId",
                table: "Transfers",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_HotelEntityId",
                table: "Transfers",
                column: "HotelEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_HotelEntityId1",
                table: "Transfers",
                column: "HotelEntityId1");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_IsVip",
                table: "Transfers",
                column: "IsVip");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_PersonnelId",
                table: "Transfers",
                column: "PersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_PickupCityId",
                table: "Transfers",
                column: "PickupCityId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_Priority",
                table: "Transfers",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_RestaurantEntityId",
                table: "Transfers",
                column: "RestaurantEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_RestaurantEntityId1",
                table: "Transfers",
                column: "RestaurantEntityId1");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_Status",
                table: "Transfers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_Status_TransferDate",
                table: "Transfers",
                columns: new[] { "Status", "TransferDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_TransferDate",
                table: "Transfers",
                column: "TransferDate");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_TransferDate_DriverId_Status",
                table: "Transfers",
                columns: new[] { "TransferDate", "DriverId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_TransferDate_Status_IsDeleted",
                table: "Transfers",
                columns: new[] { "TransferDate", "Status", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_TransferDate_VehicleId_Status",
                table: "Transfers",
                columns: new[] { "TransferDate", "VehicleId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_TransportMode",
                table: "Transfers",
                column: "TransportMode");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_VehicleId_TransferDate",
                table: "Transfers",
                columns: new[] { "VehicleId", "TransferDate" });

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppHistories_GuestId",
                table: "WhatsAppHistories",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppHistories_MessageId",
                table: "WhatsAppHistories",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppHistories_PersonnelId",
                table: "WhatsAppHistories",
                column: "PersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppHistories_PhoneNumber",
                table: "WhatsAppHistories",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppHistories_RelatedEntityType_RelatedEntityId",
                table: "WhatsAppHistories",
                columns: new[] { "RelatedEntityType", "RelatedEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppHistories_SentDate",
                table: "WhatsAppHistories",
                column: "SentDate");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppHistories_Status",
                table: "WhatsAppHistories",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_YachtTours_CaptainId_TourDate",
                table: "YachtTours",
                columns: new[] { "CaptainId", "TourDate" });

            migrationBuilder.CreateIndex(
                name: "IX_YachtTours_CityId",
                table: "YachtTours",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_YachtTours_OwnerGuestId",
                table: "YachtTours",
                column: "OwnerGuestId");

            migrationBuilder.CreateIndex(
                name: "IX_YachtTours_PersonnelId",
                table: "YachtTours",
                column: "PersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_YachtTours_PickupHotelId",
                table: "YachtTours",
                column: "PickupHotelId");

            migrationBuilder.CreateIndex(
                name: "IX_YachtTours_Status",
                table: "YachtTours",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_YachtTours_Status_TourDate",
                table: "YachtTours",
                columns: new[] { "Status", "TourDate" });

            migrationBuilder.CreateIndex(
                name: "IX_YachtTours_TourDate",
                table: "YachtTours",
                column: "TourDate");

            migrationBuilder.CreateIndex(
                name: "IX_YachtTours_TourDate_Status_IsDeleted",
                table: "YachtTours",
                columns: new[] { "TourDate", "Status", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_YachtTours_WeatherDependent",
                table: "YachtTours",
                column: "WeatherDependent");

            migrationBuilder.AddForeignKey(
                name: "FK_CityTours_Guests_OwnerGuestId",
                table: "CityTours",
                column: "OwnerGuestId",
                principalTable: "Guests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GuestBehaviors_Guests_GuestId",
                table: "GuestBehaviors",
                column: "GuestId",
                principalTable: "Guests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GuestCityTours_Guests_GuestId",
                table: "GuestCityTours",
                column: "GuestId",
                principalTable: "Guests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GuestPreferences_Guests_GuestId",
                table: "GuestPreferences",
                column: "GuestId",
                principalTable: "Guests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hotels_Cities_CityId",
                table: "Hotels");

            migrationBuilder.DropForeignKey(
                name: "FK_GuestPreferences_Guests_GuestId",
                table: "GuestPreferences");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "DailyNotes");

            migrationBuilder.DropTable(
                name: "DailyRevenues");

            migrationBuilder.DropTable(
                name: "EmailHistories");

            migrationBuilder.DropTable(
                name: "EmailQueues");

            migrationBuilder.DropTable(
                name: "EmailTemplates");

            migrationBuilder.DropTable(
                name: "FeatureFlags");

            migrationBuilder.DropTable(
                name: "GuestBehaviors");

            migrationBuilder.DropTable(
                name: "GuestCityTours");

            migrationBuilder.DropTable(
                name: "GuestRoomHistoryEntity");

            migrationBuilder.DropTable(
                name: "GuestStaffInteractions");

            migrationBuilder.DropTable(
                name: "GuestYachtTours");

            migrationBuilder.DropTable(
                name: "InvoiceItems");

            migrationBuilder.DropTable(
                name: "ItineraryItems");

            migrationBuilder.DropTable(
                name: "JournalLines");

            migrationBuilder.DropTable(
                name: "LoginAttempts");

            migrationBuilder.DropTable(
                name: "NotificationRules");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OTAHotelMappings");

            migrationBuilder.DropTable(
                name: "OTAPriceUpdates");

            migrationBuilder.DropTable(
                name: "OTAReservations");

            migrationBuilder.DropTable(
                name: "OTAWebhookLogs");

            migrationBuilder.DropTable(
                name: "PackageCityTours");

            migrationBuilder.DropTable(
                name: "PackageRestaurantReservations");

            migrationBuilder.DropTable(
                name: "PackageTransfers");

            migrationBuilder.DropTable(
                name: "PackageYachtTours");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "PMSGuestMappings");

            migrationBuilder.DropTable(
                name: "PMSReservationMappings");

            migrationBuilder.DropTable(
                name: "PMSSyncHistories");

            migrationBuilder.DropTable(
                name: "PrivacyActionHistories");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "RoomAssignments");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "SmsHistories");

            migrationBuilder.DropTable(
                name: "StaffBehaviors");

            migrationBuilder.DropTable(
                name: "SupplierCosts");

            migrationBuilder.DropTable(
                name: "WhatsAppHistories");

            migrationBuilder.DropTable(
                name: "Itineraries");

            migrationBuilder.DropTable(
                name: "JournalEntries");

            migrationBuilder.DropTable(
                name: "OTAIntegrations");

            migrationBuilder.DropTable(
                name: "ServicePackages");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "PMSIntegrations");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "RestaurantReservations");

            migrationBuilder.DropTable(
                name: "Suppliers");

            migrationBuilder.DropTable(
                name: "CityTours");

            migrationBuilder.DropTable(
                name: "YachtTours");

            migrationBuilder.DropTable(
                name: "Transfers");

            migrationBuilder.DropTable(
                name: "Tours");

            migrationBuilder.DropTable(
                name: "Airports");

            migrationBuilder.DropTable(
                name: "Personnels");

            migrationBuilder.DropTable(
                name: "Restaurants");

            migrationBuilder.DropTable(
                name: "Vehicles");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Guests");

            migrationBuilder.DropTable(
                name: "GuestPreferences");

            migrationBuilder.DropTable(
                name: "Hotels");
        }
    }
}

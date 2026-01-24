using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuestFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestDateOfBirth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                table: "Personnels",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TwoFactorRecoveryCodes",
                table: "Personnels",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TwoFactorSecret",
                table: "Personnels",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TwoFactorSetupDate",
                table: "Personnels",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "JournalLines",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccountCode",
                table: "JournalLines",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "JournalLines",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "JournalLines",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "JournalEntries",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "JournalEntries",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "JournalEntries",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsReversed",
                table: "JournalEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PostedBy",
                table: "JournalEntries",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PostedByPersonnelId",
                table: "JournalEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PostedDate",
                table: "JournalEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReversedBy",
                table: "JournalEntries",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReversedByJournalEntryId",
                table: "JournalEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReversedByPersonnelId",
                table: "JournalEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReversedDate",
                table: "JournalEntries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Guests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAnonymized",
                table: "Guests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PreferencesId",
                table: "Guests",
                type: "int",
                nullable: true);

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
                    table.ForeignKey(
                        name: "FK_GuestBehaviors_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    table.ForeignKey(
                        name: "FK_GuestPreferences_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_CreatedByPersonnelId",
                table: "JournalEntries",
                column: "CreatedByPersonnelId");

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
                name: "IX_Guests_PreferencesId",
                table: "Guests",
                column: "PreferencesId");

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
                name: "IX_GuestPreferences_GuestId",
                table: "GuestPreferences",
                column: "GuestId",
                unique: true);

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
                name: "IX_Permissions_Code",
                table: "Permissions",
                column: "Code",
                unique: true);

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
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleName_PermissionId",
                table: "RolePermissions",
                columns: new[] { "RoleName", "PermissionId" },
                unique: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Guests_GuestPreferences_PreferencesId",
                table: "Guests",
                column: "PreferencesId",
                principalTable: "GuestPreferences",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntries_Personnels_CreatedByPersonnelId",
                table: "JournalEntries",
                column: "CreatedByPersonnelId",
                principalTable: "Personnels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntries_Personnels_PostedByPersonnelId",
                table: "JournalEntries",
                column: "PostedByPersonnelId",
                principalTable: "Personnels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntries_Personnels_ReversedByPersonnelId",
                table: "JournalEntries",
                column: "ReversedByPersonnelId",
                principalTable: "Personnels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Guests_GuestPreferences_PreferencesId",
                table: "Guests");

            migrationBuilder.DropForeignKey(
                name: "FK_JournalEntries_Personnels_CreatedByPersonnelId",
                table: "JournalEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_JournalEntries_Personnels_PostedByPersonnelId",
                table: "JournalEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_JournalEntries_Personnels_ReversedByPersonnelId",
                table: "JournalEntries");

            migrationBuilder.DropTable(
                name: "FeatureFlags");

            migrationBuilder.DropTable(
                name: "GuestBehaviors");

            migrationBuilder.DropTable(
                name: "GuestPreferences");

            migrationBuilder.DropTable(
                name: "GuestStaffInteractions");

            migrationBuilder.DropTable(
                name: "LoginAttempts");

            migrationBuilder.DropTable(
                name: "NotificationRules");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OTAWebhookLogs");

            migrationBuilder.DropTable(
                name: "PrivacyActionHistories");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "StaffBehaviors");

            migrationBuilder.DropTable(
                name: "WhatsAppHistories");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_CreatedByPersonnelId",
                table: "JournalEntries");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_PostedByPersonnelId",
                table: "JournalEntries");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_PostingDate",
                table: "JournalEntries");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_ReversedByJournalEntryId",
                table: "JournalEntries");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_ReversedByPersonnelId",
                table: "JournalEntries");

            migrationBuilder.DropIndex(
                name: "IX_Guests_PreferencesId",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                table: "Personnels");

            migrationBuilder.DropColumn(
                name: "TwoFactorRecoveryCodes",
                table: "Personnels");

            migrationBuilder.DropColumn(
                name: "TwoFactorSecret",
                table: "Personnels");

            migrationBuilder.DropColumn(
                name: "TwoFactorSetupDate",
                table: "Personnels");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "JournalLines");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "JournalLines");

            migrationBuilder.DropColumn(
                name: "IsReversed",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "PostedBy",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "PostedByPersonnelId",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "PostedDate",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "ReversedBy",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "ReversedByJournalEntryId",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "ReversedByPersonnelId",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "ReversedDate",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "IsAnonymized",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "PreferencesId",
                table: "Guests");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "JournalLines",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccountCode",
                table: "JournalLines",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "JournalEntries",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "JournalEntries",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldDefaultValue: "USD");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                table: "JournalEntries",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}

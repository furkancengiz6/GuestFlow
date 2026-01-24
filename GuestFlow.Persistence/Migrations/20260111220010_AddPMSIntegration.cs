using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuestFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPMSIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PMSGuestMappings");

            migrationBuilder.DropTable(
                name: "PMSReservationMappings");

            migrationBuilder.DropTable(
                name: "PMSSyncHistories");

            migrationBuilder.DropTable(
                name: "PMSIntegrations");
        }
    }
}

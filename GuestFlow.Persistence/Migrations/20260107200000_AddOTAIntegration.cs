using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuestFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOTAIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OTAIntegrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProviderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProviderCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ApiEndpoint = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApiSecret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TokenExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    WebhookUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastSyncStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastSyncDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SyncErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTAIntegrations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OTAHotelMappings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OTAIntegrationId = table.Column<int>(type: "int", nullable: false),
                    HotelId = table.Column<int>(type: "int", nullable: false),
                    OTARoomTypeId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OTARoomTypeName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GuestFlowRoomType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PriceMultiplier = table.Column<decimal>(type: "decimal(5,2)", nullable: true, defaultValue: 1.0m),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
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
                    OTARoomTypeId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    UpdateStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
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
                name: "OTAReservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OTAIntegrationId = table.Column<int>(type: "int", nullable: false),
                    OTAReservationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OTAHotelId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OTARoomTypeId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CheckInDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOutDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuestCount = table.Column<int>(type: "int", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    GuestName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    GuestEmail = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    GuestPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OTACreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OTALastModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuestFlowReservationId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
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

            // Indexes for performance
            migrationBuilder.CreateIndex(
                name: "IX_OTAHotelMappings_HotelId",
                table: "OTAHotelMappings",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_OTAHotelMappings_OTAIntegrationId",
                table: "OTAHotelMappings",
                column: "OTAIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_OTAHotelMappings_OTARoomTypeId",
                table: "OTAHotelMappings",
                column: "OTARoomTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_OTAPriceUpdates_HotelId",
                table: "OTAPriceUpdates",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_OTAPriceUpdates_OTAIntegrationId",
                table: "OTAPriceUpdates",
                column: "OTAIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_OTAPriceUpdates_Date_HotelId",
                table: "OTAPriceUpdates",
                columns: new[] { "Date", "HotelId" });

            migrationBuilder.CreateIndex(
                name: "IX_OTAReservations_OTAIntegrationId",
                table: "OTAReservations",
                column: "OTAIntegrationId");

            migrationBuilder.CreateIndex(
                name: "IX_OTAReservations_OTAReservationId",
                table: "OTAReservations",
                column: "OTAReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_OTAReservations_CheckInDate_CheckOutDate",
                table: "OTAReservations",
                columns: new[] { "CheckInDate", "CheckOutDate" });

            migrationBuilder.CreateIndex(
                name: "IX_OTAReservations_Status",
                table: "OTAReservations",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OTAHotelMappings");

            migrationBuilder.DropTable(
                name: "OTAPriceUpdates");

            migrationBuilder.DropTable(
                name: "OTAReservations");

            migrationBuilder.DropTable(
                name: "OTAIntegrations");
        }
    }
}
using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuestFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InvoiceRealityImplementation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_CityTours_CityTourId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Transfers_TransferId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_YachtTours_YachtTourId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_TransferId",
                table: "Invoices");

            migrationBuilder.RenameColumn(
                name: "YachtTourId",
                table: "Invoices",
                newName: "YachtTourEntityId");

            migrationBuilder.RenameColumn(
                name: "TransferId",
                table: "Invoices",
                newName: "UpdatedByPersonnelId");

            migrationBuilder.RenameColumn(
                name: "CityTourId",
                table: "Invoices",
                newName: "TransferEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Invoices_YachtTourId",
                table: "Invoices",
                newName: "IX_Invoices_YachtTourEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Invoices_CityTourId",
                table: "Invoices",
                newName: "IX_Invoices_TransferEntityId");

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "YachtTours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceInfoPdfUrl",
                table: "YachtTours",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "YachtTours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "YachtTours",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "Vehicles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "Vehicles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Vehicles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "Transfers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceInfoPdfUrl",
                table: "Transfers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "Transfers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Transfers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "Tours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "Tours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Tours",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "SmsHistories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "SmsHistories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "SmsHistories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "Settings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "Settings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Settings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "ServicePackages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "ServicePackages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "ServicePackages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "Restaurants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "Restaurants",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Restaurants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "RestaurantReservations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "RestaurantReservations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "RestaurantReservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Reservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "RefreshTokens",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "RefreshTokens",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "RefreshTokens",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "Personnels",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "Personnels",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Personnels",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransferEntityId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "ItineraryItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "ItineraryItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "ItineraryItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "Itineraries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "Itineraries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Itineraries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CityTourEntityId",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPdfGenerated",
                table: "Invoices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LockedByPersonnelId",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PdfGeneratedDate",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Invoices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Invoices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "Hotels",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "Hotels",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Hotels",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "GuestYachtTours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "GuestYachtTours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "GuestYachtTours",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "Guests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "Guests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Guests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "GuestCityTours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "GuestCityTours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "GuestCityTours",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "EmailTemplates",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "EmailTemplates",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "EmailTemplates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "EmailQueues",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "EmailQueues",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "EmailQueues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "EmailHistories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "EmailHistories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "EmailHistories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "DailyRevenues",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "DailyRevenues",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "DailyRevenues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "DailyNotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "DailyNotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "DailyNotes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "CityTours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceInfoPdfUrl",
                table: "CityTours",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "CityTours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "CityTours",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "Cities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "Cities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Cities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "Airports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedByPersonnelId",
                table: "Airports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Airports",
                type: "datetime2",
                nullable: true);

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
                name: "InvoiceItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                name: "RoomAssignmentEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    RoomNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_RoomAssignmentEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomAssignmentEntity_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedByPersonnelId", "CreatedDate", "UpdatedByPersonnelId", "UpdatedDate" },
                values: new object[] { null, new DateTime(2026, 1, 4, 13, 39, 43, 363, DateTimeKind.Utc).AddTicks(4260), null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TransferEntityId",
                table: "Payments",
                column: "TransferEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CityTourEntityId",
                table: "Invoices",
                column: "CityTourEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_LockedByPersonnelId",
                table: "Invoices",
                column: "LockedByPersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestRoomHistoryEntity_AssignedByPersonnelId",
                table: "GuestRoomHistoryEntity",
                column: "AssignedByPersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestRoomHistoryEntity_GuestId",
                table: "GuestRoomHistoryEntity",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_InvoiceId",
                table: "InvoiceItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomAssignmentEntity_GuestId",
                table: "RoomAssignmentEntity",
                column: "GuestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_CityTours_CityTourEntityId",
                table: "Invoices",
                column: "CityTourEntityId",
                principalTable: "CityTours",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Personnels_LockedByPersonnelId",
                table: "Invoices",
                column: "LockedByPersonnelId",
                principalTable: "Personnels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Transfers_TransferEntityId",
                table: "Invoices",
                column: "TransferEntityId",
                principalTable: "Transfers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_YachtTours_YachtTourEntityId",
                table: "Invoices",
                column: "YachtTourEntityId",
                principalTable: "YachtTours",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Transfers_TransferEntityId",
                table: "Payments",
                column: "TransferEntityId",
                principalTable: "Transfers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_CityTours_CityTourEntityId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Personnels_LockedByPersonnelId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Transfers_TransferEntityId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_YachtTours_YachtTourEntityId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Transfers_TransferEntityId",
                table: "Payments");

            migrationBuilder.DropTable(
                name: "GuestRoomHistoryEntity");

            migrationBuilder.DropTable(
                name: "InvoiceItems");

            migrationBuilder.DropTable(
                name: "RoomAssignmentEntity");

            migrationBuilder.DropIndex(
                name: "IX_Payments_TransferEntityId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_CityTourEntityId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_LockedByPersonnelId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "ServiceInfoPdfUrl",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "ServiceInfoPdfUrl",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "SmsHistories");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "SmsHistories");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "SmsHistories");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "ServicePackages");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "ServicePackages");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "ServicePackages");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "RestaurantReservations");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "RestaurantReservations");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "RestaurantReservations");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "Personnels");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "Personnels");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Personnels");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TransferEntityId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "ItineraryItems");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "ItineraryItems");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "ItineraryItems");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "Itineraries");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "Itineraries");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Itineraries");

            migrationBuilder.DropColumn(
                name: "CityTourEntityId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IsPdfGenerated",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "LockedByPersonnelId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PdfGeneratedDate",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "Hotels");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "Hotels");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Hotels");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "GuestYachtTours");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "GuestYachtTours");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "GuestYachtTours");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "GuestCityTours");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "GuestCityTours");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "GuestCityTours");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "EmailTemplates");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "EmailTemplates");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "EmailTemplates");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "EmailQueues");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "EmailQueues");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "EmailQueues");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "EmailHistories");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "EmailHistories");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "EmailHistories");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "DailyRevenues");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "DailyRevenues");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "DailyRevenues");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "DailyNotes");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "DailyNotes");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "DailyNotes");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "ServiceInfoPdfUrl",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "Airports");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "Airports");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Airports");

            migrationBuilder.RenameColumn(
                name: "YachtTourEntityId",
                table: "Invoices",
                newName: "YachtTourId");

            migrationBuilder.RenameColumn(
                name: "UpdatedByPersonnelId",
                table: "Invoices",
                newName: "TransferId");

            migrationBuilder.RenameColumn(
                name: "TransferEntityId",
                table: "Invoices",
                newName: "CityTourId");

            migrationBuilder.RenameIndex(
                name: "IX_Invoices_YachtTourEntityId",
                table: "Invoices",
                newName: "IX_Invoices_YachtTourId");

            migrationBuilder.RenameIndex(
                name: "IX_Invoices_TransferEntityId",
                table: "Invoices",
                newName: "IX_Invoices_CityTourId");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 4, 15, 31, 4, 281, DateTimeKind.Local).AddTicks(8262));

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_TransferId",
                table: "Invoices",
                column: "TransferId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_CityTours_CityTourId",
                table: "Invoices",
                column: "CityTourId",
                principalTable: "CityTours",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Transfers_TransferId",
                table: "Invoices",
                column: "TransferId",
                principalTable: "Transfers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_YachtTours_YachtTourId",
                table: "Invoices",
                column: "YachtTourId",
                principalTable: "YachtTours",
                principalColumn: "Id");
        }
    }
}

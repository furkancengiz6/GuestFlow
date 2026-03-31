using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuestFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CheckForPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CityTours_Cities_CityId",
                table: "CityTours");

            migrationBuilder.DropForeignKey(
                name: "FK_CityTours_Personnels_PersonnelId",
                table: "CityTours");

            migrationBuilder.DropForeignKey(
                name: "FK_CityTours_Vehicles_VehicleId",
                table: "CityTours");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Airports_AirportId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Guests_GuestId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Hotels_HotelEntityId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Hotels_HotelEntityId1",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Personnels_PersonnelId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Restaurants_RestaurantEntityId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Restaurants_RestaurantEntityId1",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Vehicles_VehicleId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_YachtTours_Cities_CityId",
                table: "YachtTours");

            migrationBuilder.DropForeignKey(
                name: "FK_YachtTours_Personnels_PersonnelId",
                table: "YachtTours");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_DriverId_TransferDate",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_Status",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_Status_TransferDate",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_TransferDate",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_VehicleId_TransferDate",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_OTAWebhookLogs_IsDeadLetter",
                table: "OTAWebhookLogs");

            migrationBuilder.DropIndex(
                name: "IX_OTAWebhookLogs_NextRetryAt",
                table: "OTAWebhookLogs");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_InvoiceId",
                table: "JournalEntries");

            migrationBuilder.DropIndex(
                name: "IX_GuestYachtTours_GuestId_YachtTourId",
                table: "GuestYachtTours");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "GuestYachtTours");

            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "GuestCityTours");

            migrationBuilder.DropColumn(
                name: "UpdatedByPersonnelId",
                table: "GuestCityTours");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "GuestCityTours");

            migrationBuilder.RenameColumn(
                name: "RestaurantEntityId1",
                table: "Transfers",
                newName: "PickupRestaurantId");

            migrationBuilder.RenameColumn(
                name: "RestaurantEntityId",
                table: "Transfers",
                newName: "PickupHotelId");

            migrationBuilder.RenameColumn(
                name: "HotelEntityId1",
                table: "Transfers",
                newName: "DropoffRestaurantId");

            migrationBuilder.RenameColumn(
                name: "HotelEntityId",
                table: "Transfers",
                newName: "DropoffHotelId");

            migrationBuilder.RenameIndex(
                name: "IX_Transfers_RestaurantEntityId1",
                table: "Transfers",
                newName: "IX_Transfers_PickupRestaurantId");

            migrationBuilder.RenameIndex(
                name: "IX_Transfers_RestaurantEntityId",
                table: "Transfers",
                newName: "IX_Transfers_PickupHotelId");

            migrationBuilder.RenameIndex(
                name: "IX_Transfers_HotelEntityId1",
                table: "Transfers",
                newName: "IX_Transfers_DropoffRestaurantId");

            migrationBuilder.RenameIndex(
                name: "IX_Transfers_HotelEntityId",
                table: "Transfers",
                newName: "IX_Transfers_DropoffHotelId");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Restaurants",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OperatingHours",
                table: "Restaurants",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Restaurants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CuisineType",
                table: "Restaurants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Capacity",
                table: "Restaurants",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AveragePricePerPerson",
                table: "Restaurants",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Restaurants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVip",
                table: "Restaurants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Restaurants",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Restaurants",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MenuUrl",
                table: "Restaurants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Rating",
                table: "Restaurants",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Restaurants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RestaurantEntityId",
                table: "RestaurantReservations",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SmokingPreference",
                table: "GuestPreferences",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PreferredLanguage",
                table: "GuestPreferences",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Interests",
                table: "GuestPreferences",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BedPreference",
                table: "GuestPreferences",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ActivityPreferences",
                table: "GuestPreferences",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_VehicleId",
                table: "Transfers",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_CuisineType",
                table: "Restaurants",
                column: "CuisineType");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_IsVip",
                table: "Restaurants",
                column: "IsVip");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantReservations_RestaurantEntityId",
                table: "RestaurantReservations",
                column: "RestaurantEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_OTAWebhookLogs_IsDeadLetter",
                table: "OTAWebhookLogs",
                column: "IsDeadLetter",
                filter: "IsDeadLetter = 1");

            migrationBuilder.CreateIndex(
                name: "IX_OTAWebhookLogs_NextRetryAt",
                table: "OTAWebhookLogs",
                column: "NextRetryAt",
                filter: "NextRetryAt IS NOT NULL AND Status = 'Failed'");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_InvoiceId",
                table: "JournalEntries",
                column: "InvoiceId",
                unique: true,
                filter: "InvoiceId IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_CityTours_Cities_CityId",
                table: "CityTours",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CityTours_Personnels_PersonnelId",
                table: "CityTours",
                column: "PersonnelId",
                principalTable: "Personnels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CityTours_Vehicles_VehicleId",
                table: "CityTours",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_RestaurantReservations_Restaurants_RestaurantEntityId",
                table: "RestaurantReservations",
                column: "RestaurantEntityId",
                principalTable: "Restaurants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Airports_AirportId",
                table: "Transfers",
                column: "AirportId",
                principalTable: "Airports",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Guests_GuestId",
                table: "Transfers",
                column: "GuestId",
                principalTable: "Guests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Hotels_DropoffHotelId",
                table: "Transfers",
                column: "DropoffHotelId",
                principalTable: "Hotels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Hotels_PickupHotelId",
                table: "Transfers",
                column: "PickupHotelId",
                principalTable: "Hotels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Personnels_PersonnelId",
                table: "Transfers",
                column: "PersonnelId",
                principalTable: "Personnels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Restaurants_DropoffRestaurantId",
                table: "Transfers",
                column: "DropoffRestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Restaurants_PickupRestaurantId",
                table: "Transfers",
                column: "PickupRestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Vehicles_VehicleId",
                table: "Transfers",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_YachtTours_Cities_CityId",
                table: "YachtTours",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_YachtTours_Personnels_PersonnelId",
                table: "YachtTours",
                column: "PersonnelId",
                principalTable: "Personnels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CityTours_Cities_CityId",
                table: "CityTours");

            migrationBuilder.DropForeignKey(
                name: "FK_CityTours_Personnels_PersonnelId",
                table: "CityTours");

            migrationBuilder.DropForeignKey(
                name: "FK_CityTours_Vehicles_VehicleId",
                table: "CityTours");

            migrationBuilder.DropForeignKey(
                name: "FK_RestaurantReservations_Restaurants_RestaurantEntityId",
                table: "RestaurantReservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Airports_AirportId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Guests_GuestId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Hotels_DropoffHotelId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Hotels_PickupHotelId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Personnels_PersonnelId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Restaurants_DropoffRestaurantId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Restaurants_PickupRestaurantId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Vehicles_VehicleId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_YachtTours_Cities_CityId",
                table: "YachtTours");

            migrationBuilder.DropForeignKey(
                name: "FK_YachtTours_Personnels_PersonnelId",
                table: "YachtTours");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_VehicleId",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Restaurants_CuisineType",
                table: "Restaurants");

            migrationBuilder.DropIndex(
                name: "IX_Restaurants_IsVip",
                table: "Restaurants");

            migrationBuilder.DropIndex(
                name: "IX_RestaurantReservations_RestaurantEntityId",
                table: "RestaurantReservations");

            migrationBuilder.DropIndex(
                name: "IX_OTAWebhookLogs_IsDeadLetter",
                table: "OTAWebhookLogs");

            migrationBuilder.DropIndex(
                name: "IX_OTAWebhookLogs_NextRetryAt",
                table: "OTAWebhookLogs");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_InvoiceId",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "AveragePricePerPerson",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "IsVip",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "MenuUrl",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Restaurants");

            migrationBuilder.DropColumn(
                name: "RestaurantEntityId",
                table: "RestaurantReservations");

            migrationBuilder.RenameColumn(
                name: "PickupRestaurantId",
                table: "Transfers",
                newName: "RestaurantEntityId1");

            migrationBuilder.RenameColumn(
                name: "PickupHotelId",
                table: "Transfers",
                newName: "RestaurantEntityId");

            migrationBuilder.RenameColumn(
                name: "DropoffRestaurantId",
                table: "Transfers",
                newName: "HotelEntityId1");

            migrationBuilder.RenameColumn(
                name: "DropoffHotelId",
                table: "Transfers",
                newName: "HotelEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Transfers_PickupRestaurantId",
                table: "Transfers",
                newName: "IX_Transfers_RestaurantEntityId1");

            migrationBuilder.RenameIndex(
                name: "IX_Transfers_PickupHotelId",
                table: "Transfers",
                newName: "IX_Transfers_RestaurantEntityId");

            migrationBuilder.RenameIndex(
                name: "IX_Transfers_DropoffRestaurantId",
                table: "Transfers",
                newName: "IX_Transfers_HotelEntityId1");

            migrationBuilder.RenameIndex(
                name: "IX_Transfers_DropoffHotelId",
                table: "Transfers",
                newName: "IX_Transfers_HotelEntityId");

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                table: "Restaurants",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "OperatingHours",
                table: "Restaurants",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Restaurants",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CuisineType",
                table: "Restaurants",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "Capacity",
                table: "Restaurants",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Restaurants",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "GuestYachtTours",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "SmokingPreference",
                table: "GuestPreferences",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PreferredLanguage",
                table: "GuestPreferences",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Interests",
                table: "GuestPreferences",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BedPreference",
                table: "GuestPreferences",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ActivityPreferences",
                table: "GuestPreferences",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_DriverId_TransferDate",
                table: "Transfers",
                columns: new[] { "DriverId", "TransferDate" });

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
                name: "IX_Transfers_VehicleId_TransferDate",
                table: "Transfers",
                columns: new[] { "VehicleId", "TransferDate" });

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
                name: "IX_JournalEntries_InvoiceId",
                table: "JournalEntries",
                column: "InvoiceId",
                unique: true,
                filter: "[InvoiceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_GuestYachtTours_GuestId_YachtTourId",
                table: "GuestYachtTours",
                columns: new[] { "GuestId", "YachtTourId" });

            migrationBuilder.AddForeignKey(
                name: "FK_CityTours_Cities_CityId",
                table: "CityTours",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CityTours_Personnels_PersonnelId",
                table: "CityTours",
                column: "PersonnelId",
                principalTable: "Personnels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CityTours_Vehicles_VehicleId",
                table: "CityTours",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Airports_AirportId",
                table: "Transfers",
                column: "AirportId",
                principalTable: "Airports",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Guests_GuestId",
                table: "Transfers",
                column: "GuestId",
                principalTable: "Guests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Hotels_HotelEntityId",
                table: "Transfers",
                column: "HotelEntityId",
                principalTable: "Hotels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Hotels_HotelEntityId1",
                table: "Transfers",
                column: "HotelEntityId1",
                principalTable: "Hotels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Personnels_PersonnelId",
                table: "Transfers",
                column: "PersonnelId",
                principalTable: "Personnels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Restaurants_RestaurantEntityId",
                table: "Transfers",
                column: "RestaurantEntityId",
                principalTable: "Restaurants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Restaurants_RestaurantEntityId1",
                table: "Transfers",
                column: "RestaurantEntityId1",
                principalTable: "Restaurants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Vehicles_VehicleId",
                table: "Transfers",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_YachtTours_Cities_CityId",
                table: "YachtTours",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YachtTours_Personnels_PersonnelId",
                table: "YachtTours",
                column: "PersonnelId",
                principalTable: "Personnels",
                principalColumn: "Id");
        }
    }
}

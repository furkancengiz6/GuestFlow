using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuestFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateServiceEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Hotels_HotelId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Restaurants_RestaurantId",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_HotelId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "IsPaymentReceived",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "PaymentNote",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "CaptainPhone",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "IsPaymentReceived",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "SupplierPaymentStatus",
                table: "CityTours");

            migrationBuilder.RenameColumn(
                name: "SupplierPaymentStatus",
                table: "Transfers",
                newName: "SecondaryContactPhone");

            migrationBuilder.RenameColumn(
                name: "SupplierPaymentDate",
                table: "Transfers",
                newName: "PickupConfirmationTime");

            migrationBuilder.RenameColumn(
                name: "RestaurantId",
                table: "Transfers",
                newName: "RestaurantEntityId1");

            migrationBuilder.RenameColumn(
                name: "PaymentMethod",
                table: "Transfers",
                newName: "HotelEntityId1");

            migrationBuilder.RenameColumn(
                name: "HotelId",
                table: "Transfers",
                newName: "DriverId");

            migrationBuilder.RenameIndex(
                name: "IX_Transfers_RestaurantId",
                table: "Transfers",
                newName: "IX_Transfers_RestaurantEntityId1");

            migrationBuilder.RenameColumn(
                name: "SupplierPaymentDate",
                table: "CityTours",
                newName: "TourConfirmationTime");

            migrationBuilder.RenameColumn(
                name: "PaymentNote",
                table: "CityTours",
                newName: "SpecialEquipment");

            migrationBuilder.AddColumn<DateTime>(
                name: "DropoffConfirmationTime",
                table: "Transfers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryContactPhone",
                table: "Transfers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssistantGuideId",
                table: "CityTours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "PickupTime",
                table: "CityTours",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TourGuideId",
                table: "CityTours",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 5, 8, 57, 8, 869, DateTimeKind.Utc).AddTicks(8022));

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_HotelEntityId1",
                table: "Transfers",
                column: "HotelEntityId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Hotels_HotelEntityId1",
                table: "Transfers",
                column: "HotelEntityId1",
                principalTable: "Hotels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Restaurants_RestaurantEntityId1",
                table: "Transfers",
                column: "RestaurantEntityId1",
                principalTable: "Restaurants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Hotels_HotelEntityId1",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Restaurants_RestaurantEntityId1",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_HotelEntityId1",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "DropoffConfirmationTime",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "PrimaryContactPhone",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "AssistantGuideId",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "PickupTime",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "TourGuideId",
                table: "CityTours");

            migrationBuilder.RenameColumn(
                name: "SecondaryContactPhone",
                table: "Transfers",
                newName: "SupplierPaymentStatus");

            migrationBuilder.RenameColumn(
                name: "RestaurantEntityId1",
                table: "Transfers",
                newName: "RestaurantId");

            migrationBuilder.RenameColumn(
                name: "PickupConfirmationTime",
                table: "Transfers",
                newName: "SupplierPaymentDate");

            migrationBuilder.RenameColumn(
                name: "HotelEntityId1",
                table: "Transfers",
                newName: "PaymentMethod");

            migrationBuilder.RenameColumn(
                name: "DriverId",
                table: "Transfers",
                newName: "HotelId");

            migrationBuilder.RenameIndex(
                name: "IX_Transfers_RestaurantEntityId1",
                table: "Transfers",
                newName: "IX_Transfers_RestaurantId");

            migrationBuilder.RenameColumn(
                name: "TourConfirmationTime",
                table: "CityTours",
                newName: "SupplierPaymentDate");

            migrationBuilder.RenameColumn(
                name: "SpecialEquipment",
                table: "CityTours",
                newName: "PaymentNote");

            migrationBuilder.AddColumn<bool>(
                name: "IsPaymentReceived",
                table: "Transfers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentNote",
                table: "Transfers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CaptainPhone",
                table: "CityTours",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaymentReceived",
                table: "CityTours",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SupplierPaymentStatus",
                table: "CityTours",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 4, 23, 21, 27, 220, DateTimeKind.Utc).AddTicks(7668));

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_HotelId",
                table: "Transfers",
                column: "HotelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Hotels_HotelId",
                table: "Transfers",
                column: "HotelId",
                principalTable: "Hotels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Restaurants_RestaurantId",
                table: "Transfers",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

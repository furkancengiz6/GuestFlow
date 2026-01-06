using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuestFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConciergeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPaymentReceived",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "PaymentNote",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "SupplierPaymentStatus",
                table: "YachtTours");

            migrationBuilder.RenameColumn(
                name: "SupplierPaymentDate",
                table: "YachtTours",
                newName: "WeatherCheckTime");

            migrationBuilder.AddColumn<int>(
                name: "CaptainId",
                table: "YachtTours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CoastGuardInspectionDate",
                table: "YachtTours",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FuelLevelCheck",
                table: "YachtTours",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LifeGuardCertified",
                table: "YachtTours",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "MarinaPickupTime",
                table: "YachtTours",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SafetyBriefingTime",
                table: "YachtTours",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YachtId",
                table: "YachtTours",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 5, 10, 18, 26, 781, DateTimeKind.Utc).AddTicks(156));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaptainId",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "CoastGuardInspectionDate",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "FuelLevelCheck",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "LifeGuardCertified",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "MarinaPickupTime",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "SafetyBriefingTime",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "YachtId",
                table: "YachtTours");

            migrationBuilder.RenameColumn(
                name: "WeatherCheckTime",
                table: "YachtTours",
                newName: "SupplierPaymentDate");

            migrationBuilder.AddColumn<bool>(
                name: "IsPaymentReceived",
                table: "YachtTours",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentNote",
                table: "YachtTours",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierPaymentStatus",
                table: "YachtTours",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 5, 8, 57, 8, 869, DateTimeKind.Utc).AddTicks(8022));
        }
    }
}

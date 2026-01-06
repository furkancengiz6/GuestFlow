using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuestFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierAndPaymentTrackingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CaptainPhone",
                table: "YachtTours",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

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
                name: "PierAddress",
                table: "YachtTours",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SupplierCost",
                table: "YachtTours",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierCurrency",
                table: "YachtTours",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierInvoiceNumber",
                table: "YachtTours",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierName",
                table: "YachtTours",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SupplierPaymentDate",
                table: "YachtTours",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierPaymentStatus",
                table: "YachtTours",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TourCategory",
                table: "YachtTours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SupplierCost",
                table: "Transfers",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierCurrency",
                table: "Transfers",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierInvoiceNumber",
                table: "Transfers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierName",
                table: "Transfers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SupplierPaymentDate",
                table: "Transfers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierPaymentStatus",
                table: "Transfers",
                type: "nvarchar(20)",
                maxLength: 20,
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
                name: "PaymentNote",
                table: "CityTours",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SupplierCost",
                table: "CityTours",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierCurrency",
                table: "CityTours",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierInvoiceNumber",
                table: "CityTours",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierName",
                table: "CityTours",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SupplierPaymentDate",
                table: "CityTours",
                type: "datetime2",
                nullable: true);

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
                value: new DateTime(2025, 12, 15, 15, 38, 10, 747, DateTimeKind.Local).AddTicks(4875));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaptainPhone",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "IsPaymentReceived",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "PaymentNote",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "PierAddress",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "SupplierCost",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "SupplierCurrency",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "SupplierInvoiceNumber",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "SupplierName",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "SupplierPaymentDate",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "SupplierPaymentStatus",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "TourCategory",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "SupplierCost",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "SupplierCurrency",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "SupplierInvoiceNumber",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "SupplierName",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "SupplierPaymentDate",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "SupplierPaymentStatus",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "CaptainPhone",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "IsPaymentReceived",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "PaymentNote",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "SupplierCost",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "SupplierCurrency",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "SupplierInvoiceNumber",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "SupplierName",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "SupplierPaymentDate",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "SupplierPaymentStatus",
                table: "CityTours");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 12, 13, 12, 39, 10, 595, DateTimeKind.Local).AddTicks(2385));
        }
    }
}

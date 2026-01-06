using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuestFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmergencyContactFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                table: "YachtTours",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPhone",
                table: "YachtTours",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactRelation",
                table: "YachtTours",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                table: "Guests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPhone",
                table: "Guests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactRelation",
                table: "Guests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactRelation",
                table: "CityTours",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 4, 23, 21, 27, 220, DateTimeKind.Utc).AddTicks(7668));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPhone",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "EmergencyContactRelation",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPhone",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "EmergencyContactRelation",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "EmergencyContactRelation",
                table: "CityTours");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 4, 22, 36, 11, 355, DateTimeKind.Utc).AddTicks(462));
        }
    }
}

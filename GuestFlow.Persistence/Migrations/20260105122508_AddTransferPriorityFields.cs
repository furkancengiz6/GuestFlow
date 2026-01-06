using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuestFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTransferPriorityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVip",
                table: "Transfers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LuggageCount",
                table: "Transfers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "Transfers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReturnTransferId",
                table: "Transfers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransportMode",
                table: "Transfers",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 5, 12, 25, 6, 982, DateTimeKind.Utc).AddTicks(8359));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsVip",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "LuggageCount",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "ReturnTransferId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "TransportMode",
                table: "Transfers");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 5, 10, 18, 26, 781, DateTimeKind.Utc).AddTicks(156));
        }
    }
}

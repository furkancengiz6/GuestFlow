using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuestFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddJournalEntryInvoiceIdUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InvoiceId",
                table: "JournalEntries",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 10, 22, 13, 20, 544, DateTimeKind.Utc).AddTicks(848));

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_InvoiceId",
                table: "JournalEntries",
                column: "InvoiceId",
                unique: true,
                filter: "[InvoiceId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_InvoiceId",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "JournalEntries");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 7, 20, 34, 58, 854, DateTimeKind.Utc).AddTicks(1405));
        }
    }
}

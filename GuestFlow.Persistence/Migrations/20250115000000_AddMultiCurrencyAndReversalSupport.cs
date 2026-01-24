using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuestFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiCurrencyAndReversalSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add PostedBy and PostedDate to JournalEntries
            migrationBuilder.AddColumn<string>(
                name: "PostedBy",
                table: "JournalEntries",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PostedDate",
                table: "JournalEntries",
                type: "datetime2",
                nullable: true);

            // Add Currency and ExchangeRate to JournalLines
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "JournalLines",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "JournalLines",
                type: "decimal(18,6)",
                precision: 18,
                scale: 6,
                nullable: true);

            // Add Reversal tracking fields to JournalEntries
            migrationBuilder.AddColumn<bool>(
                name: "IsReversed",
                table: "JournalEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ReversedByJournalEntryId",
                table: "JournalEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReversedBy",
                table: "JournalEntries",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReversedDate",
                table: "JournalEntries",
                type: "datetime2",
                nullable: true);

            // Create index for reversal lookups
            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_ReversedByJournalEntryId",
                table: "JournalEntries",
                column: "ReversedByJournalEntryId",
                filter: "[ReversedByJournalEntryId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop index
            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_ReversedByJournalEntryId",
                table: "JournalEntries");

            // Remove Reversal tracking fields
            migrationBuilder.DropColumn(
                name: "IsReversed",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "ReversedByJournalEntryId",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "ReversedBy",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "ReversedDate",
                table: "JournalEntries");

            // Remove Currency and ExchangeRate from JournalLines
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "JournalLines");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "JournalLines");

            // Remove PostedBy and PostedDate from JournalEntries
            migrationBuilder.DropColumn(
                name: "PostedBy",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "PostedDate",
                table: "JournalEntries");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuestFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddJournalEntryPersonnelAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add PersonnelId foreign keys to JournalEntries for hybrid audit approach
            // ID for referential integrity and joins, Snapshot (CreatedBy/PostedBy/ReversedBy) for historical accuracy
            
            migrationBuilder.AddColumn<int>(
                name: "CreatedByPersonnelId",
                table: "JournalEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PostedByPersonnelId",
                table: "JournalEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReversedByPersonnelId",
                table: "JournalEntries",
                type: "int",
                nullable: true);

            // Create foreign key relationships with SetNull on delete (snapshot remains)
            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_CreatedByPersonnelId",
                table: "JournalEntries",
                column: "CreatedByPersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_PostedByPersonnelId",
                table: "JournalEntries",
                column: "PostedByPersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_ReversedByPersonnelId",
                table: "JournalEntries",
                column: "ReversedByPersonnelId");

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntries_Personnel_CreatedByPersonnelId",
                table: "JournalEntries",
                column: "CreatedByPersonnelId",
                principalTable: "Personnel",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntries_Personnel_PostedByPersonnelId",
                table: "JournalEntries",
                column: "PostedByPersonnelId",
                principalTable: "Personnel",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_JournalEntries_Personnel_ReversedByPersonnelId",
                table: "JournalEntries",
                column: "ReversedByPersonnelId",
                principalTable: "Personnel",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop foreign keys
            migrationBuilder.DropForeignKey(
                name: "FK_JournalEntries_Personnel_CreatedByPersonnelId",
                table: "JournalEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_JournalEntries_Personnel_PostedByPersonnelId",
                table: "JournalEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_JournalEntries_Personnel_ReversedByPersonnelId",
                table: "JournalEntries");

            // Drop indexes
            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_CreatedByPersonnelId",
                table: "JournalEntries");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_PostedByPersonnelId",
                table: "JournalEntries");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_ReversedByPersonnelId",
                table: "JournalEntries");

            // Remove columns
            migrationBuilder.DropColumn(
                name: "CreatedByPersonnelId",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "PostedByPersonnelId",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "ReversedByPersonnelId",
                table: "JournalEntries");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuestFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixPaymentDataIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix invalid CollectedByPersonnelId references in Payments table
            // Set to NULL if the personnel doesn't exist, or to a valid personnel ID
            migrationBuilder.Sql(@"
                UPDATE p
                SET p.CollectedByPersonnelId = (
                    SELECT TOP 1 Id
                    FROM Personnels
                    WHERE IsDeleted = 0
                    ORDER BY Id
                )
                FROM Payments p
                WHERE p.CollectedByPersonnelId IS NOT NULL
                AND NOT EXISTS (
                    SELECT 1 FROM Personnels pers
                    WHERE pers.Id = p.CollectedByPersonnelId
                    AND pers.IsDeleted = 0
                )
            ");

            // For payments with NULL CollectedByPersonnelId, set to the first available personnel
            migrationBuilder.Sql(@"
                UPDATE p
                SET p.CollectedByPersonnelId = (
                    SELECT TOP 1 Id
                    FROM Personnels
                    WHERE IsDeleted = 0
                    ORDER BY Id
                )
                FROM Payments p
                WHERE p.CollectedByPersonnelId IS NULL
            ");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 4, 14, 39, 57, 408, DateTimeKind.Utc).AddTicks(1431));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 4, 14, 37, 48, 54, DateTimeKind.Utc).AddTicks(8640));
        }
    }
}

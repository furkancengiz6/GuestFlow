using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuestFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PaymentAlignmentWithProductDecisions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Invoices_InvoiceId",
                table: "Payments");

            migrationBuilder.AlterColumn<int>(
                name: "InvoiceId",
                table: "Payments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CityTourId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CollectedByPersonnelId",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TransferId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YachtTourId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CityTourRevenue",
                table: "DailyRevenues",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "DailyRevenues",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "GeneralRevenue",
                table: "DailyRevenues",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NetRevenue",
                table: "DailyRevenues",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PaymentCount",
                table: "DailyRevenues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "RefundedAmount",
                table: "DailyRevenues",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TransferRevenue",
                table: "DailyRevenues",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "YachtTourRevenue",
                table: "DailyRevenues",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            // Fix invalid CollectedByPersonnelId references in Payments table
            // Set to a valid personnel ID before adding the FK constraint
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
                OR NOT EXISTS (
                    SELECT 1 FROM Personnels pers
                    WHERE pers.Id = p.CollectedByPersonnelId
                    AND pers.IsDeleted = 0
                )
            ");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 4, 15, 31, 4, 281, DateTimeKind.Local).AddTicks(8262));

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CityTourId",
                table: "Payments",
                column: "CityTourId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CollectedByPersonnelId",
                table: "Payments",
                column: "CollectedByPersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Currency",
                table: "Payments",
                column: "Currency");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentDate",
                table: "Payments",
                column: "PaymentDate");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status",
                table: "Payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TransferId",
                table: "Payments",
                column: "TransferId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_YachtTourId",
                table: "Payments",
                column: "YachtTourId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyRevenues_Date_Currency",
                table: "DailyRevenues",
                columns: new[] { "Date", "Currency" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_CityTours_CityTourId",
                table: "Payments",
                column: "CityTourId",
                principalTable: "CityTours",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Invoices_InvoiceId",
                table: "Payments",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Personnels_CollectedByPersonnelId",
                table: "Payments",
                column: "CollectedByPersonnelId",
                principalTable: "Personnels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Transfers_TransferId",
                table: "Payments",
                column: "TransferId",
                principalTable: "Transfers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_YachtTours_YachtTourId",
                table: "Payments",
                column: "YachtTourId",
                principalTable: "YachtTours",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_CityTours_CityTourId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Invoices_InvoiceId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Personnels_CollectedByPersonnelId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Transfers_TransferId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_YachtTours_YachtTourId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_CityTourId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_CollectedByPersonnelId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_Currency",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_PaymentDate",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_Status",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_TransferId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_YachtTourId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_DailyRevenues_Date_Currency",
                table: "DailyRevenues");

            migrationBuilder.DropColumn(
                name: "CityTourId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CollectedByPersonnelId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TransferId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "YachtTourId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "CityTourRevenue",
                table: "DailyRevenues");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "DailyRevenues");

            migrationBuilder.DropColumn(
                name: "GeneralRevenue",
                table: "DailyRevenues");

            migrationBuilder.DropColumn(
                name: "NetRevenue",
                table: "DailyRevenues");

            migrationBuilder.DropColumn(
                name: "PaymentCount",
                table: "DailyRevenues");

            migrationBuilder.DropColumn(
                name: "RefundedAmount",
                table: "DailyRevenues");

            migrationBuilder.DropColumn(
                name: "TransferRevenue",
                table: "DailyRevenues");

            migrationBuilder.DropColumn(
                name: "YachtTourRevenue",
                table: "DailyRevenues");

            migrationBuilder.AlterColumn<int>(
                name: "InvoiceId",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 12, 15, 15, 38, 10, 747, DateTimeKind.Local).AddTicks(4875));

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Invoices_InvoiceId",
                table: "Payments",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuestFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RoomDateContextStabilization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoomAssignmentEntity_Guests_GuestId",
                table: "RoomAssignmentEntity");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoomAssignmentEntity",
                table: "RoomAssignmentEntity");

            migrationBuilder.RenameTable(
                name: "RoomAssignmentEntity",
                newName: "RoomAssignments");

            migrationBuilder.RenameColumn(
                name: "AssignedDate",
                table: "RoomAssignments",
                newName: "StartDate");

            migrationBuilder.RenameIndex(
                name: "IX_RoomAssignmentEntity_GuestId",
                table: "RoomAssignments",
                newName: "IX_RoomAssignments_GuestId");

            migrationBuilder.AddColumn<int>(
                name: "HotelId",
                table: "RoomAssignments",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoomAssignments",
                table: "RoomAssignments",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 4, 14, 37, 48, 54, DateTimeKind.Utc).AddTicks(8640));

            migrationBuilder.CreateIndex(
                name: "IX_RoomAssignments_HotelId",
                table: "RoomAssignments",
                column: "HotelId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoomAssignments_Guests_GuestId",
                table: "RoomAssignments",
                column: "GuestId",
                principalTable: "Guests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomAssignments_Hotels_HotelId",
                table: "RoomAssignments",
                column: "HotelId",
                principalTable: "Hotels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoomAssignments_Guests_GuestId",
                table: "RoomAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomAssignments_Hotels_HotelId",
                table: "RoomAssignments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RoomAssignments",
                table: "RoomAssignments");

            migrationBuilder.DropIndex(
                name: "IX_RoomAssignments_HotelId",
                table: "RoomAssignments");

            migrationBuilder.DropColumn(
                name: "HotelId",
                table: "RoomAssignments");

            migrationBuilder.RenameTable(
                name: "RoomAssignments",
                newName: "RoomAssignmentEntity");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "RoomAssignmentEntity",
                newName: "AssignedDate");

            migrationBuilder.RenameIndex(
                name: "IX_RoomAssignments_GuestId",
                table: "RoomAssignmentEntity",
                newName: "IX_RoomAssignmentEntity_GuestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RoomAssignmentEntity",
                table: "RoomAssignmentEntity",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 4, 13, 39, 43, 363, DateTimeKind.Utc).AddTicks(4260));

            migrationBuilder.AddForeignKey(
                name: "FK_RoomAssignmentEntity_Guests_GuestId",
                table: "RoomAssignmentEntity",
                column: "GuestId",
                principalTable: "Guests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

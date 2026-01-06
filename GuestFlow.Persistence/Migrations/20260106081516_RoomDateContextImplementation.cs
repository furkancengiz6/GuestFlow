using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuestFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RoomDateContextImplementation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transfers_VehicleId",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_CityTours_VehicleId",
                table: "CityTours");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "YachtTours",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "WeatherDependent",
                table: "YachtTours",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVipGroup",
                table: "CityTours",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "CityTours",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 6, 8, 15, 15, 699, DateTimeKind.Utc).AddTicks(5917));

            migrationBuilder.CreateIndex(
                name: "IX_YachtTours_CaptainId_TourDate",
                table: "YachtTours",
                columns: new[] { "CaptainId", "TourDate" });

            migrationBuilder.CreateIndex(
                name: "IX_YachtTours_Status",
                table: "YachtTours",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_YachtTours_Status_TourDate",
                table: "YachtTours",
                columns: new[] { "Status", "TourDate" });

            migrationBuilder.CreateIndex(
                name: "IX_YachtTours_TourDate",
                table: "YachtTours",
                column: "TourDate");

            migrationBuilder.CreateIndex(
                name: "IX_YachtTours_TourDate_Status_IsDeleted",
                table: "YachtTours",
                columns: new[] { "TourDate", "Status", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_YachtTours_WeatherDependent",
                table: "YachtTours",
                column: "WeatherDependent");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_DriverId_TransferDate",
                table: "Transfers",
                columns: new[] { "DriverId", "TransferDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_IsVip",
                table: "Transfers",
                column: "IsVip");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_Priority",
                table: "Transfers",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_Status",
                table: "Transfers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_Status_TransferDate",
                table: "Transfers",
                columns: new[] { "Status", "TransferDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_TransferDate",
                table: "Transfers",
                column: "TransferDate");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_TransferDate_DriverId_Status",
                table: "Transfers",
                columns: new[] { "TransferDate", "DriverId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_TransferDate_Status_IsDeleted",
                table: "Transfers",
                columns: new[] { "TransferDate", "Status", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_TransferDate_VehicleId_Status",
                table: "Transfers",
                columns: new[] { "TransferDate", "VehicleId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_TransportMode",
                table: "Transfers",
                column: "TransportMode");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_VehicleId_TransferDate",
                table: "Transfers",
                columns: new[] { "VehicleId", "TransferDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CityTours_IsVipGroup",
                table: "CityTours",
                column: "IsVipGroup");

            migrationBuilder.CreateIndex(
                name: "IX_CityTours_Status",
                table: "CityTours",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CityTours_Status_TourDate",
                table: "CityTours",
                columns: new[] { "Status", "TourDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CityTours_TourDate",
                table: "CityTours",
                column: "TourDate");

            migrationBuilder.CreateIndex(
                name: "IX_CityTours_TourDate_Status_IsDeleted",
                table: "CityTours",
                columns: new[] { "TourDate", "Status", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_CityTours_TourGuideId_TourDate",
                table: "CityTours",
                columns: new[] { "TourGuideId", "TourDate" });

            migrationBuilder.CreateIndex(
                name: "IX_CityTours_VehicleId_TourDate",
                table: "CityTours",
                columns: new[] { "VehicleId", "TourDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_YachtTours_CaptainId_TourDate",
                table: "YachtTours");

            migrationBuilder.DropIndex(
                name: "IX_YachtTours_Status",
                table: "YachtTours");

            migrationBuilder.DropIndex(
                name: "IX_YachtTours_Status_TourDate",
                table: "YachtTours");

            migrationBuilder.DropIndex(
                name: "IX_YachtTours_TourDate",
                table: "YachtTours");

            migrationBuilder.DropIndex(
                name: "IX_YachtTours_TourDate_Status_IsDeleted",
                table: "YachtTours");

            migrationBuilder.DropIndex(
                name: "IX_YachtTours_WeatherDependent",
                table: "YachtTours");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_DriverId_TransferDate",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_IsVip",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_Priority",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_Status",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_Status_TransferDate",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_TransferDate",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_TransferDate_DriverId_Status",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_TransferDate_Status_IsDeleted",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_TransferDate_VehicleId_Status",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_TransportMode",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_VehicleId_TransferDate",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_CityTours_IsVipGroup",
                table: "CityTours");

            migrationBuilder.DropIndex(
                name: "IX_CityTours_Status",
                table: "CityTours");

            migrationBuilder.DropIndex(
                name: "IX_CityTours_Status_TourDate",
                table: "CityTours");

            migrationBuilder.DropIndex(
                name: "IX_CityTours_TourDate",
                table: "CityTours");

            migrationBuilder.DropIndex(
                name: "IX_CityTours_TourDate_Status_IsDeleted",
                table: "CityTours");

            migrationBuilder.DropIndex(
                name: "IX_CityTours_TourGuideId_TourDate",
                table: "CityTours");

            migrationBuilder.DropIndex(
                name: "IX_CityTours_VehicleId_TourDate",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "WeatherDependent",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "IsVipGroup",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "CityTours");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 5, 12, 25, 6, 982, DateTimeKind.Utc).AddTicks(8359));

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_VehicleId",
                table: "Transfers",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_CityTours_VehicleId",
                table: "CityTours",
                column: "VehicleId");
        }
    }
}

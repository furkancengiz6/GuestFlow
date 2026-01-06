using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuestFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNewFieldsToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CityTours_Personnels_PersonnelId",
                table: "CityTours");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Airports_AirportId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Personnels_PersonnelId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Vehicles_VehicleId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_YachtTours_Personnels_PersonnelId",
                table: "YachtTours");

            migrationBuilder.AlterColumn<string>(
                name: "YachtName",
                table: "YachtTours",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<int>(
                name: "PersonnelId",
                table: "YachtTours",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "YachtTours",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AddColumn<string>(
                name: "DropoffPier",
                table: "YachtTours",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "YachtTours",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupPier",
                table: "YachtTours",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "YachtTours",
                type: "time",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "VehicleId",
                table: "Transfers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Transfers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "PickupCityId",
                table: "Transfers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PersonnelId",
                table: "Transfers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DropoffCityId",
                table: "Transfers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Transfers",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<int>(
                name: "AirportId",
                table: "Transfers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "DriverName",
                table: "Transfers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalDriverName",
                table: "Transfers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalDriverPhone",
                table: "Transfers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalVehiclePlate",
                table: "Transfers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaymentReceived",
                table: "Transfers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "Transfers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentNote",
                table: "Transfers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Invoices",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Guests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Guests",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckInDate",
                table: "Guests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckOutDate",
                table: "Guests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RoomNumber",
                table: "Guests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PersonnelId",
                table: "CityTours",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "DriverName",
                table: "CityTours",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "CityTours",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalDriverName",
                table: "CityTours",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalDriverPhone",
                table: "CityTours",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalVehiclePlate",
                table: "CityTours",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuideName",
                table: "CityTours",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuidePhone",
                table: "CityTours",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "CityTours",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TourId",
                table: "CityTours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VehicleId",
                table: "CityTours",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Tours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CityEntityId = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tours_Cities_CityEntityId",
                        column: x => x.CityEntityId,
                        principalTable: "Cities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Tours_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 12, 12, 15, 57, 49, 853, DateTimeKind.Local).AddTicks(2772));

            migrationBuilder.CreateIndex(
                name: "IX_CityTours_TourId",
                table: "CityTours",
                column: "TourId");

            migrationBuilder.CreateIndex(
                name: "IX_CityTours_VehicleId",
                table: "CityTours",
                column: "VehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Tours_CityEntityId",
                table: "Tours",
                column: "CityEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Tours_CityId",
                table: "Tours",
                column: "CityId");

            migrationBuilder.AddForeignKey(
                name: "FK_CityTours_Personnels_PersonnelId",
                table: "CityTours",
                column: "PersonnelId",
                principalTable: "Personnels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CityTours_Tours_TourId",
                table: "CityTours",
                column: "TourId",
                principalTable: "Tours",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CityTours_Vehicles_VehicleId",
                table: "CityTours",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Airports_AirportId",
                table: "Transfers",
                column: "AirportId",
                principalTable: "Airports",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Personnels_PersonnelId",
                table: "Transfers",
                column: "PersonnelId",
                principalTable: "Personnels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Vehicles_VehicleId",
                table: "Transfers",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_YachtTours_Personnels_PersonnelId",
                table: "YachtTours",
                column: "PersonnelId",
                principalTable: "Personnels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CityTours_Personnels_PersonnelId",
                table: "CityTours");

            migrationBuilder.DropForeignKey(
                name: "FK_CityTours_Tours_TourId",
                table: "CityTours");

            migrationBuilder.DropForeignKey(
                name: "FK_CityTours_Vehicles_VehicleId",
                table: "CityTours");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Airports_AirportId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Personnels_PersonnelId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Vehicles_VehicleId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_YachtTours_Personnels_PersonnelId",
                table: "YachtTours");

            migrationBuilder.DropTable(
                name: "Tours");

            migrationBuilder.DropIndex(
                name: "IX_CityTours_TourId",
                table: "CityTours");

            migrationBuilder.DropIndex(
                name: "IX_CityTours_VehicleId",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "DropoffPier",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "PickupPier",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "DriverName",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "ExternalDriverName",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "ExternalDriverPhone",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "ExternalVehiclePlate",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "IsPaymentReceived",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "PaymentNote",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "CheckInDate",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "CheckOutDate",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "RoomNumber",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "DriverName",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "ExternalDriverName",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "ExternalDriverPhone",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "ExternalVehiclePlate",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "GuideName",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "GuidePhone",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "TourId",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "VehicleId",
                table: "CityTours");

            migrationBuilder.AlterColumn<string>(
                name: "YachtName",
                table: "YachtTours",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PersonnelId",
                table: "YachtTours",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "YachtTours",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "VehicleId",
                table: "Transfers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Transfers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PickupCityId",
                table: "Transfers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PersonnelId",
                table: "Transfers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DropoffCityId",
                table: "Transfers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Transfers",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(3)",
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AirportId",
                table: "Transfers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Invoices",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Guests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Guests",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<int>(
                name: "PersonnelId",
                table: "CityTours",
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
                value: new DateTime(2025, 12, 6, 14, 17, 20, 746, DateTimeKind.Local).AddTicks(7987));

            migrationBuilder.AddForeignKey(
                name: "FK_CityTours_Personnels_PersonnelId",
                table: "CityTours",
                column: "PersonnelId",
                principalTable: "Personnels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Airports_AirportId",
                table: "Transfers",
                column: "AirportId",
                principalTable: "Airports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Personnels_PersonnelId",
                table: "Transfers",
                column: "PersonnelId",
                principalTable: "Personnels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Vehicles_VehicleId",
                table: "Transfers",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_YachtTours_Personnels_PersonnelId",
                table: "YachtTours",
                column: "PersonnelId",
                principalTable: "Personnels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

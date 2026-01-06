using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuestFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHotelsRestaurantsItinerariesAndPackages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tours_Cities_CityEntityId",
                table: "Tours");

            migrationBuilder.DropIndex(
                name: "IX_Tours_CityEntityId",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "CityEntityId",
                table: "Tours");

            migrationBuilder.AddColumn<int>(
                name: "PickupHotelId",
                table: "YachtTours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HotelEntityId",
                table: "Transfers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HotelId",
                table: "Transfers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RestaurantEntityId",
                table: "Transfers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RestaurantId",
                table: "Transfers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TransferType",
                table: "Transfers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HotelId",
                table: "Guests",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TourId",
                table: "CityTours",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "DropoffLocation",
                table: "CityTours",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PickupHotelId",
                table: "CityTours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupLocation",
                table: "CityTours",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Hotels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HotelName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    StarRating = table.Column<int>(type: "int", maxLength: 1, nullable: true),
                    CheckInTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    CheckOutTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    RoomTypes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Amenities = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hotels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hotels_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Itineraries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    PersonnelId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TotalCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ItineraryNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Itineraries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Itineraries_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Itineraries_Personnels_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Personnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Restaurants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RestaurantName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CityId = table.Column<int>(type: "int", nullable: false),
                    CuisineType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: true),
                    OperatingHours = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReservationRequired = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Restaurants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Restaurants_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServicePackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PackageName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PackageType = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    FinalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PackageContent = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePackages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItineraryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItineraryId = table.Column<int>(type: "int", nullable: false),
                    ItemType = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    ScheduledDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItineraryItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItineraryItems_Itineraries_ItineraryId",
                        column: x => x.ItineraryId,
                        principalTable: "Itineraries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RestaurantReservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RestaurantId = table.Column<int>(type: "int", nullable: false),
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    PersonnelId = table.Column<int>(type: "int", nullable: false),
                    ReservationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReservationTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    NumberOfGuests = table.Column<int>(type: "int", nullable: false),
                    TableNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SpecialRequests = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ConfirmationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TransferId = table.Column<int>(type: "int", nullable: true),
                    ReturnTransferId = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantReservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestaurantReservations_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestaurantReservations_Personnels_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Personnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestaurantReservations_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RestaurantReservations_Transfers_ReturnTransferId",
                        column: x => x.ReturnTransferId,
                        principalTable: "Transfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_RestaurantReservations_Transfers_TransferId",
                        column: x => x.TransferId,
                        principalTable: "Transfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "PackageCityTours",
                columns: table => new
                {
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    CityTourId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageCityTours", x => new { x.PackageId, x.CityTourId });
                    table.ForeignKey(
                        name: "FK_PackageCityTours_CityTours_CityTourId",
                        column: x => x.CityTourId,
                        principalTable: "CityTours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackageCityTours_ServicePackages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "ServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageTransfers",
                columns: table => new
                {
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    TransferId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageTransfers", x => new { x.PackageId, x.TransferId });
                    table.ForeignKey(
                        name: "FK_PackageTransfers_ServicePackages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "ServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PackageTransfers_Transfers_TransferId",
                        column: x => x.TransferId,
                        principalTable: "Transfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PackageYachtTours",
                columns: table => new
                {
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    YachtTourId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageYachtTours", x => new { x.PackageId, x.YachtTourId });
                    table.ForeignKey(
                        name: "FK_PackageYachtTours_ServicePackages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "ServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PackageYachtTours_YachtTours_YachtTourId",
                        column: x => x.YachtTourId,
                        principalTable: "YachtTours",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PackageRestaurantReservations",
                columns: table => new
                {
                    PackageId = table.Column<int>(type: "int", nullable: false),
                    RestaurantReservationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageRestaurantReservations", x => new { x.PackageId, x.RestaurantReservationId });
                    table.ForeignKey(
                        name: "FK_PackageRestaurantReservations_RestaurantReservations_RestaurantReservationId",
                        column: x => x.RestaurantReservationId,
                        principalTable: "RestaurantReservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PackageRestaurantReservations_ServicePackages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "ServicePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 12, 13, 12, 39, 10, 595, DateTimeKind.Local).AddTicks(2385));

            migrationBuilder.CreateIndex(
                name: "IX_YachtTours_PickupHotelId",
                table: "YachtTours",
                column: "PickupHotelId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_HotelEntityId",
                table: "Transfers",
                column: "HotelEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_HotelId",
                table: "Transfers",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_RestaurantEntityId",
                table: "Transfers",
                column: "RestaurantEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_RestaurantId",
                table: "Transfers",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_Guests_HotelId",
                table: "Guests",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_CityTours_PickupHotelId",
                table: "CityTours",
                column: "PickupHotelId");

            migrationBuilder.CreateIndex(
                name: "IX_Hotels_CityId",
                table: "Hotels",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Hotels_HotelName",
                table: "Hotels",
                column: "HotelName");

            migrationBuilder.CreateIndex(
                name: "IX_Itineraries_EndDate",
                table: "Itineraries",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Itineraries_GuestId",
                table: "Itineraries",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_Itineraries_ItineraryNumber",
                table: "Itineraries",
                column: "ItineraryNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Itineraries_PersonnelId",
                table: "Itineraries",
                column: "PersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_Itineraries_StartDate",
                table: "Itineraries",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_ItineraryItems_ItineraryId",
                table: "ItineraryItems",
                column: "ItineraryId");

            migrationBuilder.CreateIndex(
                name: "IX_ItineraryItems_ItineraryId_Order",
                table: "ItineraryItems",
                columns: new[] { "ItineraryId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_ItineraryItems_ScheduledDateTime",
                table: "ItineraryItems",
                column: "ScheduledDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_PackageCityTours_CityTourId",
                table: "PackageCityTours",
                column: "CityTourId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageRestaurantReservations_RestaurantReservationId",
                table: "PackageRestaurantReservations",
                column: "RestaurantReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageTransfers_TransferId",
                table: "PackageTransfers",
                column: "TransferId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageYachtTours_YachtTourId",
                table: "PackageYachtTours",
                column: "YachtTourId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantReservations_ConfirmationNumber",
                table: "RestaurantReservations",
                column: "ConfirmationNumber");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantReservations_GuestId",
                table: "RestaurantReservations",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantReservations_PersonnelId",
                table: "RestaurantReservations",
                column: "PersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantReservations_ReservationDate",
                table: "RestaurantReservations",
                column: "ReservationDate");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantReservations_RestaurantId",
                table: "RestaurantReservations",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantReservations_ReturnTransferId",
                table: "RestaurantReservations",
                column: "ReturnTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantReservations_TransferId",
                table: "RestaurantReservations",
                column: "TransferId");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_CityId",
                table: "Restaurants",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_RestaurantName",
                table: "Restaurants",
                column: "RestaurantName");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePackages_PackageName",
                table: "ServicePackages",
                column: "PackageName");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePackages_PackageType",
                table: "ServicePackages",
                column: "PackageType");

            migrationBuilder.AddForeignKey(
                name: "FK_CityTours_Hotels_PickupHotelId",
                table: "CityTours",
                column: "PickupHotelId",
                principalTable: "Hotels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Guests_Hotels_HotelId",
                table: "Guests",
                column: "HotelId",
                principalTable: "Hotels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Hotels_HotelEntityId",
                table: "Transfers",
                column: "HotelEntityId",
                principalTable: "Hotels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Hotels_HotelId",
                table: "Transfers",
                column: "HotelId",
                principalTable: "Hotels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Restaurants_RestaurantEntityId",
                table: "Transfers",
                column: "RestaurantEntityId",
                principalTable: "Restaurants",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Restaurants_RestaurantId",
                table: "Transfers",
                column: "RestaurantId",
                principalTable: "Restaurants",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_YachtTours_Hotels_PickupHotelId",
                table: "YachtTours",
                column: "PickupHotelId",
                principalTable: "Hotels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CityTours_Hotels_PickupHotelId",
                table: "CityTours");

            migrationBuilder.DropForeignKey(
                name: "FK_Guests_Hotels_HotelId",
                table: "Guests");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Hotels_HotelEntityId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Hotels_HotelId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Restaurants_RestaurantEntityId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Restaurants_RestaurantId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_YachtTours_Hotels_PickupHotelId",
                table: "YachtTours");

            migrationBuilder.DropTable(
                name: "Hotels");

            migrationBuilder.DropTable(
                name: "ItineraryItems");

            migrationBuilder.DropTable(
                name: "PackageCityTours");

            migrationBuilder.DropTable(
                name: "PackageRestaurantReservations");

            migrationBuilder.DropTable(
                name: "PackageTransfers");

            migrationBuilder.DropTable(
                name: "PackageYachtTours");

            migrationBuilder.DropTable(
                name: "Itineraries");

            migrationBuilder.DropTable(
                name: "RestaurantReservations");

            migrationBuilder.DropTable(
                name: "ServicePackages");

            migrationBuilder.DropTable(
                name: "Restaurants");

            migrationBuilder.DropIndex(
                name: "IX_YachtTours_PickupHotelId",
                table: "YachtTours");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_HotelEntityId",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_HotelId",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_RestaurantEntityId",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_RestaurantId",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Guests_HotelId",
                table: "Guests");

            migrationBuilder.DropIndex(
                name: "IX_CityTours_PickupHotelId",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "PickupHotelId",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "HotelEntityId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "HotelId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "RestaurantEntityId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "RestaurantId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "TransferType",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "HotelId",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "DropoffLocation",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "PickupHotelId",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "PickupLocation",
                table: "CityTours");

            migrationBuilder.AddColumn<int>(
                name: "CityEntityId",
                table: "Tours",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TourId",
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
                value: new DateTime(2025, 12, 12, 15, 57, 49, 853, DateTimeKind.Local).AddTicks(2772));

            migrationBuilder.CreateIndex(
                name: "IX_Tours_CityEntityId",
                table: "Tours",
                column: "CityEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tours_Cities_CityEntityId",
                table: "Tours",
                column: "CityEntityId",
                principalTable: "Cities",
                principalColumn: "Id");
        }
    }
}

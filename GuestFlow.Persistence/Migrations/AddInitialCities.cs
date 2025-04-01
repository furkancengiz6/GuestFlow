using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuestFlow.Persistence.Migrations
{
    public partial class AddInitialCities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "CityName", "Country", "CreatedDate", "IsDeleted" },
                values: new object[,]
                {
                    { 1, "Istanbul", "Turkey", DateTime.UtcNow, false },
                    { 2, "Antalya", "Turkey", DateTime.UtcNow, false },
                    { 3, "Izmir", "Turkey", DateTime.UtcNow, false }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "Id",
                keyValues: new object[] { 1, 2, 3 });
        }
    }
}
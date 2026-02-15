using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuestFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestPreferencesIdForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "InfluenceScore",
                table: "Guests",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "SustainabilityScore",
                table: "Guests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "GuestIntelligenceActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsAutomatic = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ExecutionDetails = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Confidence = table.Column<double>(type: "float", nullable: false),
                    ExecutionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestIntelligenceActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuestIntelligenceActions_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SustainabilityActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<int>(type: "int", nullable: false),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImpactScore = table.Column<int>(type: "int", nullable: false),
                    GuestEntityId = table.Column<int>(type: "int", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SustainabilityActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SustainabilityActions_Guests_GuestEntityId",
                        column: x => x.GuestEntityId,
                        principalTable: "Guests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SustainabilityActions_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SustainabilityRewards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    RequiredScore = table.Column<int>(type: "int", nullable: false),
                    IsClaimed = table.Column<bool>(type: "bit", nullable: false),
                    ClaimedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RewardCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    GuestEntityId = table.Column<int>(type: "int", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SustainabilityRewards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SustainabilityRewards_Guests_GuestEntityId",
                        column: x => x.GuestEntityId,
                        principalTable: "Guests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SustainabilityRewards_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuestIntelligenceActions_ExecutionDate",
                table: "GuestIntelligenceActions",
                column: "ExecutionDate");

            migrationBuilder.CreateIndex(
                name: "IX_GuestIntelligenceActions_GuestId",
                table: "GuestIntelligenceActions",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestIntelligenceActions_Status",
                table: "GuestIntelligenceActions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SustainabilityActions_GuestEntityId",
                table: "SustainabilityActions",
                column: "GuestEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_SustainabilityActions_GuestId",
                table: "SustainabilityActions",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_SustainabilityRewards_GuestEntityId",
                table: "SustainabilityRewards",
                column: "GuestEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_SustainabilityRewards_GuestId",
                table: "SustainabilityRewards",
                column: "GuestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuestIntelligenceActions");

            migrationBuilder.DropTable(
                name: "SustainabilityActions");

            migrationBuilder.DropTable(
                name: "SustainabilityRewards");

            migrationBuilder.DropColumn(
                name: "InfluenceScore",
                table: "Guests");

            migrationBuilder.DropColumn(
                name: "SustainabilityScore",
                table: "Guests");
        }
    }
}

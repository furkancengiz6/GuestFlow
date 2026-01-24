using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuestFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIntelligenceLayerBehavioralData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GuestBehaviors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    BehaviorType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BehaviorValue = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    BehaviorDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeOfDay = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DayOfWeek = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Season = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SentimentScore = table.Column<double>(type: "float", nullable: true),
                    SatisfactionScore = table.Column<double>(type: "float", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RelatedEntityId = table.Column<int>(type: "int", nullable: true),
                    SyncedToGraph = table.Column<bool>(type: "bit", nullable: false),
                    GraphSyncDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestBehaviors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuestBehaviors_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StaffBehaviors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    BehaviorType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BehaviorValue = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    BehaviorDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuestId = table.Column<int>(type: "int", nullable: true),
                    ServiceId = table.Column<int>(type: "int", nullable: true),
                    ServiceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SuccessScore = table.Column<double>(type: "float", nullable: true),
                    GuestSatisfaction = table.Column<double>(type: "float", nullable: true),
                    ResponseTimeMinutes = table.Column<int>(type: "int", nullable: true),
                    PreferenceLearned = table.Column<bool>(type: "bit", nullable: false),
                    ProblemSolved = table.Column<bool>(type: "bit", nullable: false),
                    SyncedToGraph = table.Column<bool>(type: "bit", nullable: false),
                    GraphSyncDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffBehaviors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffBehaviors_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_StaffBehaviors_Personnels_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Personnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GuestStaffInteractions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GuestId = table.Column<int>(type: "int", nullable: false),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    InteractionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InteractionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: true),
                    SentimentScore = table.Column<double>(type: "float", nullable: true),
                    SatisfactionScore = table.Column<double>(type: "float", nullable: true),
                    Context = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ServiceId = table.Column<int>(type: "int", nullable: true),
                    ServiceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RelationshipStrength = table.Column<double>(type: "float", nullable: true),
                    SyncedToGraph = table.Column<bool>(type: "bit", nullable: false),
                    GraphSyncDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedByPersonnelId = table.Column<int>(type: "int", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestStaffInteractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuestStaffInteractions_Guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "Guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GuestStaffInteractions_Personnels_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Personnels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuestBehaviors_GuestId",
                table: "GuestBehaviors",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestBehaviors_GuestId_BehaviorType",
                table: "GuestBehaviors",
                columns: new[] { "GuestId", "BehaviorType" });

            migrationBuilder.CreateIndex(
                name: "IX_GuestBehaviors_GuestId_BehaviorDate",
                table: "GuestBehaviors",
                columns: new[] { "GuestId", "BehaviorDate" });

            migrationBuilder.CreateIndex(
                name: "IX_GuestBehaviors_GuestId_SyncedToGraph",
                table: "GuestBehaviors",
                columns: new[] { "GuestId", "SyncedToGraph" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffBehaviors_StaffId",
                table: "StaffBehaviors",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffBehaviors_StaffId_BehaviorType",
                table: "StaffBehaviors",
                columns: new[] { "StaffId", "BehaviorType" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffBehaviors_StaffId_BehaviorDate",
                table: "StaffBehaviors",
                columns: new[] { "StaffId", "BehaviorDate" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffBehaviors_StaffId_GuestId",
                table: "StaffBehaviors",
                columns: new[] { "StaffId", "GuestId" });

            migrationBuilder.CreateIndex(
                name: "IX_StaffBehaviors_StaffId_SyncedToGraph",
                table: "StaffBehaviors",
                columns: new[] { "StaffId", "SyncedToGraph" });

            migrationBuilder.CreateIndex(
                name: "IX_GuestStaffInteractions_GuestId",
                table: "GuestStaffInteractions",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestStaffInteractions_StaffId",
                table: "GuestStaffInteractions",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestStaffInteractions_GuestId_StaffId",
                table: "GuestStaffInteractions",
                columns: new[] { "GuestId", "StaffId" });

            migrationBuilder.CreateIndex(
                name: "IX_GuestStaffInteractions_GuestId_InteractionDate",
                table: "GuestStaffInteractions",
                columns: new[] { "GuestId", "InteractionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_GuestStaffInteractions_GuestId_StaffId_InteractionDate",
                table: "GuestStaffInteractions",
                columns: new[] { "GuestId", "StaffId", "InteractionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_GuestStaffInteractions_GuestId_SyncedToGraph",
                table: "GuestStaffInteractions",
                columns: new[] { "GuestId", "SyncedToGraph" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuestBehaviors");

            migrationBuilder.DropTable(
                name: "StaffBehaviors");

            migrationBuilder.DropTable(
                name: "GuestStaffInteractions");
        }
    }
}

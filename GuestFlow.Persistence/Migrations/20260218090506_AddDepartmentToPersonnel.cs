using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuestFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentToPersonnel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Personnels",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GuestEntityId",
                table: "GuestStaffInteractions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GuestEntityId",
                table: "GuestReviews",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GuestEntityId",
                table: "GuestBehaviors",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuestStaffInteractions_GuestEntityId",
                table: "GuestStaffInteractions",
                column: "GuestEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestReviews_GuestEntityId",
                table: "GuestReviews",
                column: "GuestEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestBehaviors_GuestEntityId",
                table: "GuestBehaviors",
                column: "GuestEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_GuestBehaviors_Guests_GuestEntityId",
                table: "GuestBehaviors",
                column: "GuestEntityId",
                principalTable: "Guests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GuestReviews_Guests_GuestEntityId",
                table: "GuestReviews",
                column: "GuestEntityId",
                principalTable: "Guests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GuestStaffInteractions_Guests_GuestEntityId",
                table: "GuestStaffInteractions",
                column: "GuestEntityId",
                principalTable: "Guests",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuestBehaviors_Guests_GuestEntityId",
                table: "GuestBehaviors");

            migrationBuilder.DropForeignKey(
                name: "FK_GuestReviews_Guests_GuestEntityId",
                table: "GuestReviews");

            migrationBuilder.DropForeignKey(
                name: "FK_GuestStaffInteractions_Guests_GuestEntityId",
                table: "GuestStaffInteractions");

            migrationBuilder.DropIndex(
                name: "IX_GuestStaffInteractions_GuestEntityId",
                table: "GuestStaffInteractions");

            migrationBuilder.DropIndex(
                name: "IX_GuestReviews_GuestEntityId",
                table: "GuestReviews");

            migrationBuilder.DropIndex(
                name: "IX_GuestBehaviors_GuestEntityId",
                table: "GuestBehaviors");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Personnels");

            migrationBuilder.DropColumn(
                name: "GuestEntityId",
                table: "GuestStaffInteractions");

            migrationBuilder.DropColumn(
                name: "GuestEntityId",
                table: "GuestReviews");

            migrationBuilder.DropColumn(
                name: "GuestEntityId",
                table: "GuestBehaviors");
        }
    }
}

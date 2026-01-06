using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GuestFlow.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingConciergeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlcoholPolicy",
                table: "YachtTours",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BeverageType",
                table: "YachtTours",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CaptainExperience",
                table: "YachtTours",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChildCount",
                table: "YachtTours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CoastGuardApproved",
                table: "YachtTours",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConciergeInternalNotes",
                table: "YachtTours",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CrewSize",
                table: "YachtTours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyEquipment",
                table: "YachtTours",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "FoodBeverageIncluded",
                table: "YachtTours",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FuelRange",
                table: "YachtTours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GroupLeaderName",
                table: "YachtTours",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GroupLeaderPhone",
                table: "YachtTours",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InfantCount",
                table: "YachtTours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LifeJacketCount",
                table: "YachtTours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LifeJacketsProvided",
                table: "YachtTours",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MarinaContactName",
                table: "YachtTours",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MarinaContactPhone",
                table: "YachtTours",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MedicalConditions",
                table: "YachtTours",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MusicSystem",
                table: "YachtTours",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SafetyEquipmentCheck",
                table: "YachtTours",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SwimmingProficiency",
                table: "YachtTours",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WaterSportsEquipment",
                table: "YachtTours",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WeatherBackupPlan",
                table: "YachtTours",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YachtCapacity",
                table: "YachtTours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "YachtLicenceRequired",
                table: "YachtTours",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "YachtType",
                table: "YachtTours",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccessibilityRequirements",
                table: "Transfers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChildCount",
                table: "Transfers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConciergeInternalNotes",
                table: "Transfers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPersonName",
                table: "Transfers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPhone",
                table: "Transfers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GroupSize",
                table: "Transfers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestLanguage",
                table: "Transfers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuestVisibleNotes",
                table: "Transfers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InfantCount",
                table: "Transfers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MeetingPointDetails",
                table: "Transfers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "PickupTime",
                table: "Transfers",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "ServiceStartTime",
                table: "Transfers",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialHandlingNotes",
                table: "Transfers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierContactPhone",
                table: "Transfers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierEmergencyContact",
                table: "Transfers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccessibilityNeeds",
                table: "CityTours",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AdultCount",
                table: "CityTours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BackupGuideName",
                table: "CityTours",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BackupGuidePhone",
                table: "CityTours",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChildCount",
                table: "CityTours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConciergeInternalNotes",
                table: "CityTours",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DietaryRequirements",
                table: "CityTours",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactName",
                table: "CityTours",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContactPhone",
                table: "CityTours",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GroupLeaderName",
                table: "CityTours",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GroupLeaderPhone",
                table: "CityTours",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuideLanguages",
                table: "CityTours",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InfantCount",
                table: "CityTours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaximumParticipantCount",
                table: "CityTours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MeetingPersonName",
                table: "CityTours",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MeetingPointDetails",
                table: "CityTours",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinimumParticipantCount",
                table: "CityTours",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PhotographyAllowed",
                table: "CityTours",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TourDifficultyLevel",
                table: "CityTours",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WeatherDependent",
                table: "CityTours",
                type: "bit",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 4, 22, 36, 11, 355, DateTimeKind.Utc).AddTicks(462));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlcoholPolicy",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "BeverageType",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "CaptainExperience",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "ChildCount",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "CoastGuardApproved",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "ConciergeInternalNotes",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "CrewSize",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "EmergencyEquipment",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "FoodBeverageIncluded",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "FuelRange",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "GroupLeaderName",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "GroupLeaderPhone",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "InfantCount",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "LifeJacketCount",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "LifeJacketsProvided",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "MarinaContactName",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "MarinaContactPhone",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "MedicalConditions",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "MusicSystem",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "SafetyEquipmentCheck",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "SwimmingProficiency",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "WaterSportsEquipment",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "WeatherBackupPlan",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "YachtCapacity",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "YachtLicenceRequired",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "YachtType",
                table: "YachtTours");

            migrationBuilder.DropColumn(
                name: "AccessibilityRequirements",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "ChildCount",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "ConciergeInternalNotes",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "ContactPersonName",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPhone",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "GroupSize",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "GuestLanguage",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "GuestVisibleNotes",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "InfantCount",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "MeetingPointDetails",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "PickupTime",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "ServiceStartTime",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "SpecialHandlingNotes",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "SupplierContactPhone",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "SupplierEmergencyContact",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "AccessibilityNeeds",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "AdultCount",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "BackupGuideName",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "BackupGuidePhone",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "ChildCount",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "ConciergeInternalNotes",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "DietaryRequirements",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "EmergencyContactName",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "EmergencyContactPhone",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "GroupLeaderName",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "GroupLeaderPhone",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "GuideLanguages",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "InfantCount",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "MaximumParticipantCount",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "MeetingPersonName",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "MeetingPointDetails",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "MinimumParticipantCount",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "PhotographyAllowed",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "TourDifficultyLevel",
                table: "CityTours");

            migrationBuilder.DropColumn(
                name: "WeatherDependent",
                table: "CityTours");

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2026, 1, 4, 14, 37, 48, 54, DateTimeKind.Utc).AddTicks(8640));
        }
    }
}

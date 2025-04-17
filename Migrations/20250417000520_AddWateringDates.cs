using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlantAppServer.Migrations
{
    /// <inheritdoc />
    public partial class AddWateringDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "LastWatered",
                table: "UserPlants",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "NextWatering",
                table: "UserPlants",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LightRequirement",
                table: "Plants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WaterRequirement",
                table: "Plants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastWatered",
                table: "UserPlants");

            migrationBuilder.DropColumn(
                name: "NextWatering",
                table: "UserPlants");

            migrationBuilder.DropColumn(
                name: "LightRequirement",
                table: "Plants");

            migrationBuilder.DropColumn(
                name: "WaterRequirement",
                table: "Plants");
        }
    }
}

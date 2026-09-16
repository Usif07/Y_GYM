using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Y_Gem.Migrations
{
    /// <inheritdoc />
    public partial class AddCoachModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "coachSpecialty",
                table: "Coaches",
                newName: "Specialty");

            migrationBuilder.AddColumn<decimal>(
                name: "Salary",
                table: "Coaches",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "HireDate",
                table: "Coaches",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1900, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Salary",
                table: "Coaches");

            migrationBuilder.DropColumn(
                name: "HireDate",
                table: "Coaches");

            migrationBuilder.RenameColumn(
                name: "Specialty",
                table: "Coaches",
                newName: "coachSpecialty");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Y_GYM.Migrations
{
    /// <inheritdoc />
    public partial class AddMembershipVisitTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RemainingVisits",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalVisits",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalVisits",
                table: "MembershipPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RemainingVisits",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "TotalVisits",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "TotalVisits",
                table: "MembershipPlans");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Y_GYM.Migrations
{
    /// <inheritdoc />
    public partial class MakeMemberPlanOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members_MembershipPlans_MembershipPlanId",
                table: "Members");

            migrationBuilder.AlterColumn<int>(
                name: "MembershipPlanId",
                table: "Members",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Members_MembershipPlans_MembershipPlanId",
                table: "Members",
                column: "MembershipPlanId",
                principalTable: "MembershipPlans",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Members_MembershipPlans_MembershipPlanId",
                table: "Members");

            migrationBuilder.AlterColumn<int>(
                name: "MembershipPlanId",
                table: "Members",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Members_MembershipPlans_MembershipPlanId",
                table: "Members",
                column: "MembershipPlanId",
                principalTable: "MembershipPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

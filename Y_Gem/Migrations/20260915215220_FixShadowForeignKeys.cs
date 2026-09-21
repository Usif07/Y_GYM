using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Y_GYM.Migrations
{
    /// <inheritdoc />
    public partial class FixShadowForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_ClassSchedules_ClassScheduleId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassSchedules_Classes_ClassId",
                table: "ClassSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_MembershipPlans_MembershipPlansId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_MembershipPlansId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_ClassSchedules_ClassId",
                table: "ClassSchedules");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ClassScheduleId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "MembershipPlansId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "ClassScheduleId",
                table: "Bookings");

            migrationBuilder.AddColumn<int>(
                name: "ClasseId",
                table: "ClassSchedules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PlanId",
                table: "Subscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSchedules_ClasseId",
                table: "ClassSchedules",
                column: "ClasseId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ScheduleId",
                table: "Bookings",
                column: "ScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_ClassSchedules_ScheduleId",
                table: "Bookings",
                column: "ScheduleId",
                principalTable: "ClassSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassSchedules_Classes_ClasseId",
                table: "ClassSchedules",
                column: "ClasseId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_MembershipPlans_PlanId",
                table: "Subscriptions",
                column: "PlanId",
                principalTable: "MembershipPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_ClassSchedules_ScheduleId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassSchedules_Classes_ClasseId",
                table: "ClassSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_MembershipPlans_PlanId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_PlanId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_ClassSchedules_ClasseId",
                table: "ClassSchedules");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_ScheduleId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ClasseId",
                table: "ClassSchedules");

            migrationBuilder.AddColumn<int>(
                name: "MembershipPlansId",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ClassScheduleId",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_MembershipPlansId",
                table: "Subscriptions",
                column: "MembershipPlansId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSchedules_ClassId",
                table: "ClassSchedules",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_ClassScheduleId",
                table: "Bookings",
                column: "ClassScheduleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_ClassSchedules_ClassScheduleId",
                table: "Bookings",
                column: "ClassScheduleId",
                principalTable: "ClassSchedules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassSchedules_Classes_ClassId",
                table: "ClassSchedules",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_MembershipPlans_MembershipPlansId",
                table: "Subscriptions",
                column: "MembershipPlansId",
                principalTable: "MembershipPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

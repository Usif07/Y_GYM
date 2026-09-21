using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Y_GYM.Migrations
{
    /// <inheritdoc />
    public partial class FixCoachForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassSchedules_AspNetUsers_CoachId1",
                table: "ClassSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassSchedules_Classes_ClasseId",
                table: "ClassSchedules");

            migrationBuilder.DropIndex(
                name: "IX_ClassSchedules_ClasseId",
                table: "ClassSchedules");

            migrationBuilder.DropColumn(
                name: "ClasseId",
                table: "ClassSchedules");

            migrationBuilder.AlterColumn<string>(
                name: "CoachId1",
                table: "ClassSchedules",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassSchedules_ClassId",
                table: "ClassSchedules",
                column: "ClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassSchedules_AspNetUsers_CoachId1",
                table: "ClassSchedules",
                column: "CoachId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassSchedules_Classes_ClassId",
                table: "ClassSchedules",
                column: "ClassId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassSchedules_AspNetUsers_CoachId1",
                table: "ClassSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_ClassSchedules_Classes_ClassId",
                table: "ClassSchedules");

            migrationBuilder.DropIndex(
                name: "IX_ClassSchedules_ClassId",
                table: "ClassSchedules");

            migrationBuilder.AlterColumn<string>(
                name: "CoachId1",
                table: "ClassSchedules",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "ClasseId",
                table: "ClassSchedules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ClassSchedules_ClasseId",
                table: "ClassSchedules",
                column: "ClasseId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassSchedules_AspNetUsers_CoachId1",
                table: "ClassSchedules",
                column: "CoachId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassSchedules_Classes_ClasseId",
                table: "ClassSchedules",
                column: "ClasseId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

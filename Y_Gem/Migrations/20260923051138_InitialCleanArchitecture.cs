using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Y_GYM.Migrations
{
    /// <inheritdoc />
    public partial class InitialCleanArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassSchedules_AspNetUsers_CoachId1",
                table: "ClassSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_DietPlans_AspNetUsers_CoachId1",
                table: "DietPlans");

            migrationBuilder.DropIndex(
                name: "IX_DietPlans_CoachId1",
                table: "DietPlans");

            migrationBuilder.DropIndex(
                name: "IX_ClassSchedules_CoachId1",
                table: "ClassSchedules");

            migrationBuilder.DropColumn(
                name: "CoachId1",
                table: "DietPlans");

            migrationBuilder.DropColumn(
                name: "CoachId1",
                table: "ClassSchedules");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "coachSpecialty",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "Coaches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CoachSpecialty = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coaches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Coaches_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DietPlans_CoachId",
                table: "DietPlans",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSchedules_CoachId",
                table: "ClassSchedules",
                column: "CoachId");

            migrationBuilder.CreateIndex(
                name: "IX_Coaches_UserId",
                table: "Coaches",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ClassSchedules_Coaches_CoachId",
                table: "ClassSchedules",
                column: "CoachId",
                principalTable: "Coaches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DietPlans_Coaches_CoachId",
                table: "DietPlans",
                column: "CoachId",
                principalTable: "Coaches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassSchedules_Coaches_CoachId",
                table: "ClassSchedules");

            migrationBuilder.DropForeignKey(
                name: "FK_DietPlans_Coaches_CoachId",
                table: "DietPlans");

            migrationBuilder.DropTable(
                name: "Coaches");

            migrationBuilder.DropIndex(
                name: "IX_DietPlans_CoachId",
                table: "DietPlans");

            migrationBuilder.DropIndex(
                name: "IX_ClassSchedules_CoachId",
                table: "ClassSchedules");

            migrationBuilder.AddColumn<string>(
                name: "CoachId1",
                table: "DietPlans",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoachId1",
                table: "ClassSchedules",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "coachSpecialty",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DietPlans_CoachId1",
                table: "DietPlans",
                column: "CoachId1");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSchedules_CoachId1",
                table: "ClassSchedules",
                column: "CoachId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassSchedules_AspNetUsers_CoachId1",
                table: "ClassSchedules",
                column: "CoachId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DietPlans_AspNetUsers_CoachId1",
                table: "DietPlans",
                column: "CoachId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}

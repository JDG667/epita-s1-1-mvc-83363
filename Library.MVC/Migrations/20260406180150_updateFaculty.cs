using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.MVC.Migrations
{
    /// <inheritdoc />
    public partial class updateFaculty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTutor",
                table: "FacultyProfiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "FacultyProfileId",
                table: "Courses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_FacultyProfileId",
                table: "Courses",
                column: "FacultyProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_FacultyProfiles_FacultyProfileId",
                table: "Courses",
                column: "FacultyProfileId",
                principalTable: "FacultyProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_FacultyProfiles_FacultyProfileId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_FacultyProfileId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "IsTutor",
                table: "FacultyProfiles");

            migrationBuilder.DropColumn(
                name: "FacultyProfileId",
                table: "Courses");
        }
    }
}

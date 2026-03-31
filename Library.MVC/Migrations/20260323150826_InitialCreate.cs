using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.MVC.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InspectionId1",
                table: "FollowUps",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FollowUps_InspectionId1",
                table: "FollowUps",
                column: "InspectionId1");

            migrationBuilder.AddForeignKey(
                name: "FK_FollowUps_Inspections_InspectionId1",
                table: "FollowUps",
                column: "InspectionId1",
                principalTable: "Inspections",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FollowUps_Inspections_InspectionId1",
                table: "FollowUps");

            migrationBuilder.DropIndex(
                name: "IX_FollowUps_InspectionId1",
                table: "FollowUps");

            migrationBuilder.DropColumn(
                name: "InspectionId1",
                table: "FollowUps");
        }
    }
}

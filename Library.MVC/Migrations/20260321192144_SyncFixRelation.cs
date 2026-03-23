using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.MVC.Migrations
{
    /// <inheritdoc />
    public partial class SyncFixRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PremisesId1",
                table: "Inspections",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_PremisesId1",
                table: "Inspections",
                column: "PremisesId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Inspections_Premises_PremisesId1",
                table: "Inspections",
                column: "PremisesId1",
                principalTable: "Premises",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inspections_Premises_PremisesId1",
                table: "Inspections");

            migrationBuilder.DropIndex(
                name: "IX_Inspections_PremisesId1",
                table: "Inspections");

            migrationBuilder.DropColumn(
                name: "PremisesId1",
                table: "Inspections");
        }
    }
}

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzubiLog.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260617193000_AddTrainerOwnership")]
    public partial class AddTrainerOwnership : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Trainers",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Trainers_UserId_Name",
                table: "Trainers",
                columns: new[] { "UserId", "Name" });

            migrationBuilder.AddForeignKey(
                name: "FK_Trainers_AspNetUsers_UserId",
                table: "Trainers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trainers_AspNetUsers_UserId",
                table: "Trainers");

            migrationBuilder.DropIndex(
                name: "IX_Trainers_UserId_Name",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Trainers");
        }
    }
}

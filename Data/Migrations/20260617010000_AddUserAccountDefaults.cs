using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzubiLog.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260617010000_AddUserAccountDefaults")]
    public partial class AddUserAccountDefaults : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AnnualVacationDays",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.AddColumn<double>(
                name: "WeeklyTargetHours",
                table: "AspNetUsers",
                type: "REAL",
                nullable: false,
                defaultValue: 40.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnnualVacationDays",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "WeeklyTargetHours",
                table: "AspNetUsers");
        }
    }
}

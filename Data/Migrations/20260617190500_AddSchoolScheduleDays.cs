using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzubiLog.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260617190500_AddSchoolScheduleDays")]
    public partial class AddSchoolScheduleDays : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SchoolScheduleDays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    DayOfWeek = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SubjectsText = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolScheduleDays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolScheduleDays_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolScheduleDays_UserId_DayOfWeek",
                table: "SchoolScheduleDays",
                columns: new[] { "UserId", "DayOfWeek" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchoolScheduleDays");
        }
    }
}

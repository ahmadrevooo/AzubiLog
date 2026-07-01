using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzubiLog.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesAndClassTimetable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 40,
                nullable: false,
                defaultValue: "Azubi");

            migrationBuilder.CreateTable(
                name: "ClassTimetableEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    School = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    ClassName = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    DayOfWeek = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    SubjectsText = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassTimetableEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassTimetableEntries_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimetableCancellations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClassTimetableEntryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimetableCancellations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimetableCancellations_ClassTimetableEntries_ClassTimetableEntryId",
                        column: x => x.ClassTimetableEntryId,
                        principalTable: "ClassTimetableEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassTimetableEntries_CreatedByUserId",
                table: "ClassTimetableEntries",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassTimetableEntries_School_ClassName_DayOfWeek",
                table: "ClassTimetableEntries",
                columns: new[] { "School", "ClassName", "DayOfWeek" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimetableCancellations_ClassTimetableEntryId_Date",
                table: "TimetableCancellations",
                columns: new[] { "ClassTimetableEntryId", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TimetableCancellations");

            migrationBuilder.DropTable(
                name: "ClassTimetableEntries");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "AspNetUsers");
        }
    }
}

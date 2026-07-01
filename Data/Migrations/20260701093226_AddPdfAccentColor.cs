using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzubiLog.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPdfAccentColor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PdfAccentColor",
                table: "AspNetUsers",
                type: "TEXT",
                maxLength: 7,
                nullable: false,
                defaultValue: "#2563eb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PdfAccentColor",
                table: "AspNetUsers");
        }
    }
}

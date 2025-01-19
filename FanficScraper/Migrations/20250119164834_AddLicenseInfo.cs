using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FanficScraper.Migrations
{
    /// <inheritdoc />
    public partial class AddLicenseInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorId",
                table: "Stories",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "License",
                table: "Stories",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorId",
                table: "Stories");

            migrationBuilder.DropColumn(
                name: "License",
                table: "Stories");
        }
    }
}

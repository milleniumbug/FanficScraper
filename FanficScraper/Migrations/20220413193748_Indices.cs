using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FanficScraper.Migrations
{
    public partial class Indices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Stories_FileName",
                table: "Stories",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_Stories_LastUpdated",
                table: "Stories",
                column: "LastUpdated");

            migrationBuilder.CreateIndex(
                name: "IX_Stories_StoryName",
                table: "Stories",
                column: "StoryName");

            migrationBuilder.CreateIndex(
                name: "IX_Stories_StoryUpdated",
                table: "Stories",
                column: "StoryUpdated");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stories_FileName",
                table: "Stories");

            migrationBuilder.DropIndex(
                name: "IX_Stories_LastUpdated",
                table: "Stories");

            migrationBuilder.DropIndex(
                name: "IX_Stories_StoryName",
                table: "Stories");

            migrationBuilder.DropIndex(
                name: "IX_Stories_StoryUpdated",
                table: "Stories");
        }
    }
}

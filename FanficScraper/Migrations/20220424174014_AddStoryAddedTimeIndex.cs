using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FanficScraper.Migrations
{
    public partial class AddStoryAddedTimeIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Stories_StoryAdded",
                table: "Stories",
                column: "StoryAdded");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stories_StoryAdded",
                table: "Stories");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FanficScraper.Migrations
{
    public partial class AddStoryAddedTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StoryAdded",
                table: "Stories",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StoryAdded",
                table: "Stories");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FanficScraper.Migrations
{
    public partial class AddStoryData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StoryData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Category = table.Column<string>(type: "TEXT", nullable: true),
                    Characters = table.Column<string>(type: "TEXT", nullable: true),
                    Genre = table.Column<string>(type: "TEXT", nullable: true),
                    Relationships = table.Column<string>(type: "TEXT", nullable: true),
                    Rating = table.Column<string>(type: "TEXT", nullable: true),
                    Warnings = table.Column<string>(type: "TEXT", nullable: true),
                    DescriptionParagraphs = table.Column<string>(type: "TEXT", nullable: true),
                    NumChapters = table.Column<int>(type: "INTEGER", nullable: true),
                    NumWords = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoryData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StoryData_Stories_StoryId",
                        column: x => x.StoryId,
                        principalTable: "Stories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StoryData_StoryId",
                table: "StoryData",
                column: "StoryId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StoryData");
        }
    }
}

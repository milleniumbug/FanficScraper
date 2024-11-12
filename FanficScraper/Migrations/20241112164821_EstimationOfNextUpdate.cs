using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FanficScraper.Migrations
{
    /// <inheritdoc />
    public partial class EstimationOfNextUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NextUpdateIn",
                table: "Stories",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stories_NextUpdateIn",
                table: "Stories",
                column: "NextUpdateIn");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Stories_NextUpdateIn",
                table: "Stories");

            migrationBuilder.DropColumn(
                name: "NextUpdateIn",
                table: "Stories");
        }
    }
}

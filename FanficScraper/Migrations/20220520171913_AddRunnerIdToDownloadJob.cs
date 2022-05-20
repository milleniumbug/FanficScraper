using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FanficScraper.Migrations
{
    public partial class AddRunnerIdToDownloadJob : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RunnerId",
                table: "DownloadJobs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DownloadJobs_AddedDate",
                table: "DownloadJobs",
                column: "AddedDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DownloadJobs_AddedDate",
                table: "DownloadJobs");

            migrationBuilder.DropColumn(
                name: "RunnerId",
                table: "DownloadJobs");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CricPulse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMVPFieldsToMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MVPPlayerName",
                table: "Matches",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MVPScore",
                table: "Matches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MVPUserId",
                table: "Matches",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MVPPlayerName",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "MVPScore",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "MVPUserId",
                table: "Matches");
        }
    }
}

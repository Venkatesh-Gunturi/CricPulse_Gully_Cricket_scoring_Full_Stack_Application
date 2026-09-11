using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CricPulse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchStartedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "Matches",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "Matches");
        }
    }
}

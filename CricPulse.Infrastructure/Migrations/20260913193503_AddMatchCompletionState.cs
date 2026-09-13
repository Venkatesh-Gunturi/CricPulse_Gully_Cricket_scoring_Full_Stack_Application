using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CricPulse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchCompletionState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletionDeadline",
                table: "Matches",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Result",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionDeadline",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Result",
                table: "Matches");
        }
    }
}

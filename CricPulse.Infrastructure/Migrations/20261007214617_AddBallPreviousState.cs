using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CricPulse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBallPreviousState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PreviousBowlerMatchPlayerId",
                table: "Balls",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "PreviousInningsStatus",
                table: "Balls",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PreviousLegalBalls",
                table: "Balls",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "PreviousNonStrikerMatchPlayerId",
                table: "Balls",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PreviousStrikerMatchPlayerId",
                table: "Balls",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "PreviousTotalRuns",
                table: "Balls",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PreviousWickets",
                table: "Balls",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviousBowlerMatchPlayerId",
                table: "Balls");

            migrationBuilder.DropColumn(
                name: "PreviousInningsStatus",
                table: "Balls");

            migrationBuilder.DropColumn(
                name: "PreviousLegalBalls",
                table: "Balls");

            migrationBuilder.DropColumn(
                name: "PreviousNonStrikerMatchPlayerId",
                table: "Balls");

            migrationBuilder.DropColumn(
                name: "PreviousStrikerMatchPlayerId",
                table: "Balls");

            migrationBuilder.DropColumn(
                name: "PreviousTotalRuns",
                table: "Balls");

            migrationBuilder.DropColumn(
                name: "PreviousWickets",
                table: "Balls");
        }
    }
}

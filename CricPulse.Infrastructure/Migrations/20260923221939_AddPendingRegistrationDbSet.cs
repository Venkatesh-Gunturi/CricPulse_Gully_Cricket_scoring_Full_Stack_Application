using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CricPulse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingRegistrationDbSet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DidBattersCross",
                table: "Wickets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DismissedPlayerWasStriker",
                table: "Wickets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "StumpedByMatchPlayerId",
                table: "Wickets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Matches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFreeHit",
                table: "Innings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "FreeHitAfterDelivery",
                table: "Balls",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notation",
                table: "Balls",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PreviousBowlerMatchPlayerId",
                table: "Balls",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PreviousCompletionDeadline",
                table: "Balls",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PreviousFreeHit",
                table: "Balls",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PreviousInningsStatus",
                table: "Balls",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PreviousLegalBalls",
                table: "Balls",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PreviousMatchResult",
                table: "Balls",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousMatchStatus",
                table: "Balls",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreviousNonStrikerMatchPlayerId",
                table: "Balls",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PreviousStrikerMatchPlayerId",
                table: "Balls",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.CreateIndex(
                name: "IX_Wickets_StumpedByMatchPlayerId",
                table: "Wickets",
                column: "StumpedByMatchPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_UserId",
                table: "Matches",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Users_UserId",
                table: "Matches",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Wickets_MatchPlayers_StumpedByMatchPlayerId",
                table: "Wickets",
                column: "StumpedByMatchPlayerId",
                principalTable: "MatchPlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Users_UserId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Wickets_MatchPlayers_StumpedByMatchPlayerId",
                table: "Wickets");

            migrationBuilder.DropIndex(
                name: "IX_Wickets_StumpedByMatchPlayerId",
                table: "Wickets");

            migrationBuilder.DropIndex(
                name: "IX_Matches_UserId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "DidBattersCross",
                table: "Wickets");

            migrationBuilder.DropColumn(
                name: "DismissedPlayerWasStriker",
                table: "Wickets");

            migrationBuilder.DropColumn(
                name: "StumpedByMatchPlayerId",
                table: "Wickets");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "IsFreeHit",
                table: "Innings");

            migrationBuilder.DropColumn(
                name: "FreeHitAfterDelivery",
                table: "Balls");

            migrationBuilder.DropColumn(
                name: "Notation",
                table: "Balls");

            migrationBuilder.DropColumn(
                name: "PreviousBowlerMatchPlayerId",
                table: "Balls");

            migrationBuilder.DropColumn(
                name: "PreviousCompletionDeadline",
                table: "Balls");

            migrationBuilder.DropColumn(
                name: "PreviousFreeHit",
                table: "Balls");

            migrationBuilder.DropColumn(
                name: "PreviousInningsStatus",
                table: "Balls");

            migrationBuilder.DropColumn(
                name: "PreviousLegalBalls",
                table: "Balls");

            migrationBuilder.DropColumn(
                name: "PreviousMatchResult",
                table: "Balls");

            migrationBuilder.DropColumn(
                name: "PreviousMatchStatus",
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

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CricPulse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTossFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ball_Innings_InningsId",
                table: "Ball");

            migrationBuilder.DropForeignKey(
                name: "FK_Ball_MatchPlayer_BowlerMatchPlayerId",
                table: "Ball");

            migrationBuilder.DropForeignKey(
                name: "FK_Ball_MatchPlayer_NonStrikerMatchPlayerId",
                table: "Ball");

            migrationBuilder.DropForeignKey(
                name: "FK_Ball_MatchPlayer_StrikerMatchPlayerId",
                table: "Ball");

            migrationBuilder.DropForeignKey(
                name: "FK_Innings_MatchPlayer_CurrentBowlerMatchPlayerId",
                table: "Innings");

            migrationBuilder.DropForeignKey(
                name: "FK_Innings_MatchPlayer_NonStrikerMatchPlayerId",
                table: "Innings");

            migrationBuilder.DropForeignKey(
                name: "FK_Innings_MatchPlayer_StrikerMatchPlayerId",
                table: "Innings");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchPlayer_Matches_MatchId",
                table: "MatchPlayer");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchPlayer_Players_PlayerId",
                table: "MatchPlayer");

            migrationBuilder.DropForeignKey(
                name: "FK_Wicket_Ball_BallId",
                table: "Wicket");

            migrationBuilder.DropForeignKey(
                name: "FK_Wicket_MatchPlayer_CaughtByMatchPlayerId",
                table: "Wicket");

            migrationBuilder.DropForeignKey(
                name: "FK_Wicket_MatchPlayer_DismissedMatchPlayerId",
                table: "Wicket");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Wicket",
                table: "Wicket");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MatchPlayer",
                table: "MatchPlayer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Ball",
                table: "Ball");

            migrationBuilder.RenameTable(
                name: "Wicket",
                newName: "Wickets");

            migrationBuilder.RenameTable(
                name: "MatchPlayer",
                newName: "MatchPlayers");

            migrationBuilder.RenameTable(
                name: "Ball",
                newName: "Balls");

            migrationBuilder.RenameIndex(
                name: "IX_Wicket_DismissedMatchPlayerId",
                table: "Wickets",
                newName: "IX_Wickets_DismissedMatchPlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_Wicket_CaughtByMatchPlayerId",
                table: "Wickets",
                newName: "IX_Wickets_CaughtByMatchPlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_Wicket_BallId",
                table: "Wickets",
                newName: "IX_Wickets_BallId");

            migrationBuilder.RenameIndex(
                name: "IX_MatchPlayer_PlayerId",
                table: "MatchPlayers",
                newName: "IX_MatchPlayers_PlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_MatchPlayer_MatchId_PlayerId",
                table: "MatchPlayers",
                newName: "IX_MatchPlayers_MatchId_PlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_Ball_StrikerMatchPlayerId",
                table: "Balls",
                newName: "IX_Balls_StrikerMatchPlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_Ball_NonStrikerMatchPlayerId",
                table: "Balls",
                newName: "IX_Balls_NonStrikerMatchPlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_Ball_InningsId",
                table: "Balls",
                newName: "IX_Balls_InningsId");

            migrationBuilder.RenameIndex(
                name: "IX_Ball_BowlerMatchPlayerId",
                table: "Balls",
                newName: "IX_Balls_BowlerMatchPlayerId");

            migrationBuilder.AddColumn<string>(
                name: "BattingFirstTeam",
                table: "Matches",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TossDecision",
                table: "Matches",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TossWinnerTeam",
                table: "Matches",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Wickets",
                table: "Wickets",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MatchPlayers",
                table: "MatchPlayers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Balls",
                table: "Balls",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Balls_Innings_InningsId",
                table: "Balls",
                column: "InningsId",
                principalTable: "Innings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Balls_MatchPlayers_BowlerMatchPlayerId",
                table: "Balls",
                column: "BowlerMatchPlayerId",
                principalTable: "MatchPlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Balls_MatchPlayers_NonStrikerMatchPlayerId",
                table: "Balls",
                column: "NonStrikerMatchPlayerId",
                principalTable: "MatchPlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Balls_MatchPlayers_StrikerMatchPlayerId",
                table: "Balls",
                column: "StrikerMatchPlayerId",
                principalTable: "MatchPlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Innings_MatchPlayers_CurrentBowlerMatchPlayerId",
                table: "Innings",
                column: "CurrentBowlerMatchPlayerId",
                principalTable: "MatchPlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Innings_MatchPlayers_NonStrikerMatchPlayerId",
                table: "Innings",
                column: "NonStrikerMatchPlayerId",
                principalTable: "MatchPlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Innings_MatchPlayers_StrikerMatchPlayerId",
                table: "Innings",
                column: "StrikerMatchPlayerId",
                principalTable: "MatchPlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchPlayers_Matches_MatchId",
                table: "MatchPlayers",
                column: "MatchId",
                principalTable: "Matches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchPlayers_Players_PlayerId",
                table: "MatchPlayers",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Wickets_Balls_BallId",
                table: "Wickets",
                column: "BallId",
                principalTable: "Balls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Wickets_MatchPlayers_CaughtByMatchPlayerId",
                table: "Wickets",
                column: "CaughtByMatchPlayerId",
                principalTable: "MatchPlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Wickets_MatchPlayers_DismissedMatchPlayerId",
                table: "Wickets",
                column: "DismissedMatchPlayerId",
                principalTable: "MatchPlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Balls_Innings_InningsId",
                table: "Balls");

            migrationBuilder.DropForeignKey(
                name: "FK_Balls_MatchPlayers_BowlerMatchPlayerId",
                table: "Balls");

            migrationBuilder.DropForeignKey(
                name: "FK_Balls_MatchPlayers_NonStrikerMatchPlayerId",
                table: "Balls");

            migrationBuilder.DropForeignKey(
                name: "FK_Balls_MatchPlayers_StrikerMatchPlayerId",
                table: "Balls");

            migrationBuilder.DropForeignKey(
                name: "FK_Innings_MatchPlayers_CurrentBowlerMatchPlayerId",
                table: "Innings");

            migrationBuilder.DropForeignKey(
                name: "FK_Innings_MatchPlayers_NonStrikerMatchPlayerId",
                table: "Innings");

            migrationBuilder.DropForeignKey(
                name: "FK_Innings_MatchPlayers_StrikerMatchPlayerId",
                table: "Innings");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchPlayers_Matches_MatchId",
                table: "MatchPlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_MatchPlayers_Players_PlayerId",
                table: "MatchPlayers");

            migrationBuilder.DropForeignKey(
                name: "FK_Wickets_Balls_BallId",
                table: "Wickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Wickets_MatchPlayers_CaughtByMatchPlayerId",
                table: "Wickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Wickets_MatchPlayers_DismissedMatchPlayerId",
                table: "Wickets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Wickets",
                table: "Wickets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MatchPlayers",
                table: "MatchPlayers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Balls",
                table: "Balls");

            migrationBuilder.DropColumn(
                name: "BattingFirstTeam",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TossDecision",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TossWinnerTeam",
                table: "Matches");

            migrationBuilder.RenameTable(
                name: "Wickets",
                newName: "Wicket");

            migrationBuilder.RenameTable(
                name: "MatchPlayers",
                newName: "MatchPlayer");

            migrationBuilder.RenameTable(
                name: "Balls",
                newName: "Ball");

            migrationBuilder.RenameIndex(
                name: "IX_Wickets_DismissedMatchPlayerId",
                table: "Wicket",
                newName: "IX_Wicket_DismissedMatchPlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_Wickets_CaughtByMatchPlayerId",
                table: "Wicket",
                newName: "IX_Wicket_CaughtByMatchPlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_Wickets_BallId",
                table: "Wicket",
                newName: "IX_Wicket_BallId");

            migrationBuilder.RenameIndex(
                name: "IX_MatchPlayers_PlayerId",
                table: "MatchPlayer",
                newName: "IX_MatchPlayer_PlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_MatchPlayers_MatchId_PlayerId",
                table: "MatchPlayer",
                newName: "IX_MatchPlayer_MatchId_PlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_Balls_StrikerMatchPlayerId",
                table: "Ball",
                newName: "IX_Ball_StrikerMatchPlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_Balls_NonStrikerMatchPlayerId",
                table: "Ball",
                newName: "IX_Ball_NonStrikerMatchPlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_Balls_InningsId",
                table: "Ball",
                newName: "IX_Ball_InningsId");

            migrationBuilder.RenameIndex(
                name: "IX_Balls_BowlerMatchPlayerId",
                table: "Ball",
                newName: "IX_Ball_BowlerMatchPlayerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Wicket",
                table: "Wicket",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MatchPlayer",
                table: "MatchPlayer",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Ball",
                table: "Ball",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Ball_Innings_InningsId",
                table: "Ball",
                column: "InningsId",
                principalTable: "Innings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ball_MatchPlayer_BowlerMatchPlayerId",
                table: "Ball",
                column: "BowlerMatchPlayerId",
                principalTable: "MatchPlayer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ball_MatchPlayer_NonStrikerMatchPlayerId",
                table: "Ball",
                column: "NonStrikerMatchPlayerId",
                principalTable: "MatchPlayer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ball_MatchPlayer_StrikerMatchPlayerId",
                table: "Ball",
                column: "StrikerMatchPlayerId",
                principalTable: "MatchPlayer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Innings_MatchPlayer_CurrentBowlerMatchPlayerId",
                table: "Innings",
                column: "CurrentBowlerMatchPlayerId",
                principalTable: "MatchPlayer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Innings_MatchPlayer_NonStrikerMatchPlayerId",
                table: "Innings",
                column: "NonStrikerMatchPlayerId",
                principalTable: "MatchPlayer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Innings_MatchPlayer_StrikerMatchPlayerId",
                table: "Innings",
                column: "StrikerMatchPlayerId",
                principalTable: "MatchPlayer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchPlayer_Matches_MatchId",
                table: "MatchPlayer",
                column: "MatchId",
                principalTable: "Matches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MatchPlayer_Players_PlayerId",
                table: "MatchPlayer",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Wicket_Ball_BallId",
                table: "Wicket",
                column: "BallId",
                principalTable: "Ball",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Wicket_MatchPlayer_CaughtByMatchPlayerId",
                table: "Wicket",
                column: "CaughtByMatchPlayerId",
                principalTable: "MatchPlayer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Wicket_MatchPlayer_DismissedMatchPlayerId",
                table: "Wicket",
                column: "DismissedMatchPlayerId",
                principalTable: "MatchPlayer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

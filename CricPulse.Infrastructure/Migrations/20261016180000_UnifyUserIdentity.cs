using System;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CricPulse.Infrastructure.Migrations;

public partial class UnifyUserIdentity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(name: "DateOfBirth", table: "Users", type: "datetime2", nullable: true);
        migrationBuilder.AddColumn<string>(name: "Gender", table: "Users", type: "nvarchar(30)", maxLength: 30, nullable: true);
        migrationBuilder.AddColumn<string>(name: "State", table: "Users", type: "nvarchar(100)", maxLength: 100, nullable: true);
        migrationBuilder.AddColumn<string>(name: "PinCode", table: "Users", type: "nvarchar(10)", maxLength: 10, nullable: true);
        migrationBuilder.AddColumn<string>(name: "BattingStyle", table: "Users", type: "nvarchar(50)", maxLength: 50, nullable: true);
        migrationBuilder.AddColumn<string>(name: "BowlingStyle", table: "Users", type: "nvarchar(50)", maxLength: 50, nullable: true);
        migrationBuilder.AddColumn<string>(name: "PlayerRole", table: "Users", type: "nvarchar(50)", maxLength: 50, nullable: true);

        // Copy profile data before dropping the Players table.
        migrationBuilder.Sql("""
            UPDATE u
            SET u.DateOfBirth = p.DateOfBirth,
                u.Gender = p.Gender,
                u.State = p.State,
                u.PinCode = p.PinCode,
                u.BattingStyle = p.BattingStyle,
                u.BowlingStyle = p.BowlingStyle,
                u.PlayerRole = p.PlayerRole
            FROM dbo.Users AS u
            INNER JOIN dbo.Players AS p ON p.UserId = u.Id;
            """);

        migrationBuilder.DropForeignKey(name: "FK_Matches_Users_UmpireUserId", table: "Matches");
        migrationBuilder.DropForeignKey(name: "FK_MatchPlayers_Players_PlayerUserId", table: "MatchPlayers");
        migrationBuilder.DropForeignKey(name: "FK_PlayerStatistics_Players_PlayerUserId", table: "PlayerStatistics");
        migrationBuilder.DropForeignKey(name: "FK_UmpireStatistics_Umpires_UmpireUserId", table: "UmpireStatistics");

        migrationBuilder.DropIndex(name: "IX_Matches_UmpireUserId", table: "Matches");
        migrationBuilder.DropIndex(name: "IX_MatchPlayers_PlayerUserId", table: "MatchPlayers");
        migrationBuilder.DropIndex(name: "IX_MatchPlayers_MatchId_PlayerUserId", table: "MatchPlayers");

        migrationBuilder.RenameColumn(name: "UmpireUserId", table: "Matches", newName: "CreatedByUserId");
        migrationBuilder.RenameColumn(name: "PlayerUserId", table: "MatchPlayers", newName: "UserId");
        migrationBuilder.RenameColumn(name: "PlayerUserId", table: "PlayerStatistics", newName: "UserId");
        migrationBuilder.RenameColumn(name: "UmpireUserId", table: "UmpireStatistics", newName: "UserId");

        migrationBuilder.DropTable(name: "Players");
        migrationBuilder.DropTable(name: "Umpires");

        migrationBuilder.CreateIndex(name: "IX_Matches_CreatedByUserId", table: "Matches", column: "CreatedByUserId");
        migrationBuilder.CreateIndex(name: "IX_MatchPlayers_UserId", table: "MatchPlayers", column: "UserId");
        migrationBuilder.CreateIndex(name: "IX_MatchPlayers_MatchId_UserId", table: "MatchPlayers", columns: new[] { "MatchId", "UserId" }, unique: true);

        migrationBuilder.AddForeignKey(name: "FK_Matches_Users_CreatedByUserId", table: "Matches", column: "CreatedByUserId", principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey(name: "FK_MatchPlayers_Users_UserId", table: "MatchPlayers", column: "UserId", principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey(name: "FK_PlayerStatistics_Users_UserId", table: "PlayerStatistics", column: "UserId", principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
        migrationBuilder.AddForeignKey(name: "FK_UmpireStatistics_Users_UserId", table: "UmpireStatistics", column: "UserId", principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(name: "FK_Matches_Users_CreatedByUserId", table: "Matches");
        migrationBuilder.DropForeignKey(name: "FK_MatchPlayers_Users_UserId", table: "MatchPlayers");
        migrationBuilder.DropForeignKey(name: "FK_PlayerStatistics_Users_UserId", table: "PlayerStatistics");
        migrationBuilder.DropForeignKey(name: "FK_UmpireStatistics_Users_UserId", table: "UmpireStatistics");
        migrationBuilder.DropIndex(name: "IX_Matches_CreatedByUserId", table: "Matches");
        migrationBuilder.DropIndex(name: "IX_MatchPlayers_UserId", table: "MatchPlayers");
        migrationBuilder.DropIndex(name: "IX_MatchPlayers_MatchId_UserId", table: "MatchPlayers");

        migrationBuilder.RenameColumn(name: "CreatedByUserId", table: "Matches", newName: "UmpireUserId");
        migrationBuilder.RenameColumn(name: "UserId", table: "MatchPlayers", newName: "PlayerUserId");
        migrationBuilder.RenameColumn(name: "UserId", table: "PlayerStatistics", newName: "PlayerUserId");
        migrationBuilder.RenameColumn(name: "UserId", table: "UmpireStatistics", newName: "UmpireUserId");

        migrationBuilder.CreateTable(
            name: "Players",
            columns: table => new
            {
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                Gender = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                PinCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                BattingStyle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                BowlingStyle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                PlayerRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                PlayerNumber = table.Column<int>(type: "int", nullable: false).Annotation("SqlServer:Identity", "1, 1")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Players", x => x.UserId);
                table.ForeignKey(name: "FK_Players_Users_UserId", column: x => x.UserId, principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });
        migrationBuilder.CreateTable(
            name: "Umpires",
            columns: table => new { UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false) },
            constraints: table =>
            {
                table.PrimaryKey("PK_Umpires", x => x.UserId);
                table.ForeignKey(name: "FK_Umpires_Users_UserId", column: x => x.UserId, principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.Sql("""
            INSERT INTO dbo.Players (UserId, DateOfBirth, Gender, State, PinCode, BattingStyle, BowlingStyle, PlayerRole)
            SELECT Id, DateOfBirth, Gender, State, PinCode, BattingStyle, BowlingStyle, PlayerRole FROM dbo.Users;
            INSERT INTO dbo.Umpires (UserId)
            SELECT UmpireUserId FROM dbo.UmpireStatistics
            UNION
            SELECT UmpireUserId FROM dbo.Matches;
            """);

        migrationBuilder.CreateIndex(name: "IX_Players_PlayerNumber", table: "Players", column: "PlayerNumber", unique: true);
        migrationBuilder.CreateIndex(name: "IX_Matches_UmpireUserId", table: "Matches", column: "UmpireUserId");
        migrationBuilder.CreateIndex(name: "IX_MatchPlayers_PlayerUserId", table: "MatchPlayers", column: "PlayerUserId");
        migrationBuilder.CreateIndex(name: "IX_MatchPlayers_MatchId_PlayerUserId", table: "MatchPlayers", columns: new[] { "MatchId", "PlayerUserId" }, unique: true);

        migrationBuilder.AddForeignKey(name: "FK_Matches_Users_UmpireUserId", table: "Matches", column: "UmpireUserId", principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey(name: "FK_MatchPlayers_Players_PlayerUserId", table: "MatchPlayers", column: "PlayerUserId", principalTable: "Players", principalColumn: "UserId", onDelete: ReferentialAction.Restrict);
        migrationBuilder.AddForeignKey(name: "FK_PlayerStatistics_Players_PlayerUserId", table: "PlayerStatistics", column: "PlayerUserId", principalTable: "Players", principalColumn: "UserId", onDelete: ReferentialAction.Cascade);
        migrationBuilder.AddForeignKey(name: "FK_UmpireStatistics_Umpires_UmpireUserId", table: "UmpireStatistics", column: "UmpireUserId", principalTable: "Umpires", principalColumn: "UserId", onDelete: ReferentialAction.Cascade);

        migrationBuilder.DropColumn(name: "DateOfBirth", table: "Users");
        migrationBuilder.DropColumn(name: "Gender", table: "Users");
        migrationBuilder.DropColumn(name: "State", table: "Users");
        migrationBuilder.DropColumn(name: "PinCode", table: "Users");
        migrationBuilder.DropColumn(name: "BattingStyle", table: "Users");
        migrationBuilder.DropColumn(name: "BowlingStyle", table: "Users");
        migrationBuilder.DropColumn(name: "PlayerRole", table: "Users");
    }
}

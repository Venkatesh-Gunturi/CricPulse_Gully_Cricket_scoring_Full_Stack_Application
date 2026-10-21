using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CricPulse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ProfileImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UmpireUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TeamAName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TeamBName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LocationAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PinCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    ActualStartLatitude = table.Column<double>(type: "float", nullable: true),
                    ActualStartLongitude = table.Column<double>(type: "float", nullable: true),
                    ActualStartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CancellationReasonType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matches_Users_UmpireUserId",
                        column: x => x.UmpireUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                    PlayerRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Players_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Umpires",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Umpires", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Umpires_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Innings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InningsNumber = table.Column<int>(type: "int", nullable: false),
                    BattingTeam = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BowlingTeam = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TotalRuns = table.Column<int>(type: "int", nullable: false),
                    Wickets = table.Column<int>(type: "int", nullable: false),
                    LegalBalls = table.Column<int>(type: "int", nullable: false),
                    TotalOvers = table.Column<int>(type: "int", nullable: false),
                    TargetRuns = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CurrentStrikerMatchPlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CurrentNonStrikerMatchPlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CurrentBowlerMatchPlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Innings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Innings_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchPlayers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Team = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsCaptain = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchPlayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchPlayers_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchPlayers_Players_PlayerUserId",
                        column: x => x.PlayerUserId,
                        principalTable: "Players",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerStatistics",
                columns: table => new
                {
                    PlayerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Matches = table.Column<int>(type: "int", nullable: false),
                    Innings = table.Column<int>(type: "int", nullable: false),
                    Runs = table.Column<int>(type: "int", nullable: false),
                    BallsFaced = table.Column<int>(type: "int", nullable: false),
                    HighestScore = table.Column<int>(type: "int", nullable: false),
                    NotOuts = table.Column<int>(type: "int", nullable: false),
                    Fours = table.Column<int>(type: "int", nullable: false),
                    Sixes = table.Column<int>(type: "int", nullable: false),
                    Fifties = table.Column<int>(type: "int", nullable: false),
                    Hundreds = table.Column<int>(type: "int", nullable: false),
                    BowlingInnings = table.Column<int>(type: "int", nullable: false),
                    BallsBowled = table.Column<int>(type: "int", nullable: false),
                    RunsConceded = table.Column<int>(type: "int", nullable: false),
                    Wickets = table.Column<int>(type: "int", nullable: false),
                    Maidens = table.Column<int>(type: "int", nullable: false),
                    ThreeWicketHauls = table.Column<int>(type: "int", nullable: false),
                    FiveWicketHauls = table.Column<int>(type: "int", nullable: false),
                    BestBowling = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Catches = table.Column<int>(type: "int", nullable: false),
                    RunOuts = table.Column<int>(type: "int", nullable: false),
                    Stumpings = table.Column<int>(type: "int", nullable: false),
                    MVPCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerStatistics", x => x.PlayerUserId);
                    table.ForeignKey(
                        name: "FK_PlayerStatistics_Players_PlayerUserId",
                        column: x => x.PlayerUserId,
                        principalTable: "Players",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UmpireStatistics",
                columns: table => new
                {
                    UmpireUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchesCreated = table.Column<int>(type: "int", nullable: false),
                    MatchesSuccessfullyOrganized = table.Column<int>(type: "int", nullable: false),
                    MatchesFinished = table.Column<int>(type: "int", nullable: false),
                    MatchesCancelled = table.Column<int>(type: "int", nullable: false),
                    MatchesManaged = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UmpireStatistics", x => x.UmpireUserId);
                    table.ForeignKey(
                        name: "FK_UmpireStatistics_Umpires_UmpireUserId",
                        column: x => x.UmpireUserId,
                        principalTable: "Umpires",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Balls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InningsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OverNumber = table.Column<int>(type: "int", nullable: false),
                    BallNumber = table.Column<int>(type: "int", nullable: false),
                    StrikerMatchPlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NonStrikerMatchPlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BowlerMatchPlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Runs = table.Column<int>(type: "int", nullable: false),
                    BatterRuns = table.Column<int>(type: "int", nullable: false),
                    IsLegalDelivery = table.Column<bool>(type: "bit", nullable: false),
                    ExtraType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ExtraRuns = table.Column<int>(type: "int", nullable: false),
                    Notation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Balls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Balls_Innings_InningsId",
                        column: x => x.InningsId,
                        principalTable: "Innings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Balls_MatchPlayers_BowlerMatchPlayerId",
                        column: x => x.BowlerMatchPlayerId,
                        principalTable: "MatchPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Balls_MatchPlayers_NonStrikerMatchPlayerId",
                        column: x => x.NonStrikerMatchPlayerId,
                        principalTable: "MatchPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Balls_MatchPlayers_StrikerMatchPlayerId",
                        column: x => x.StrikerMatchPlayerId,
                        principalTable: "MatchPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Wickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BallId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DismissedMatchPlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WicketType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CaughtByMatchPlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RunOutByMatchPlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StumpedByMatchPlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RunsCompleted = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wickets_Balls_BallId",
                        column: x => x.BallId,
                        principalTable: "Balls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Wickets_MatchPlayers_CaughtByMatchPlayerId",
                        column: x => x.CaughtByMatchPlayerId,
                        principalTable: "MatchPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Wickets_MatchPlayers_DismissedMatchPlayerId",
                        column: x => x.DismissedMatchPlayerId,
                        principalTable: "MatchPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Wickets_MatchPlayers_RunOutByMatchPlayerId",
                        column: x => x.RunOutByMatchPlayerId,
                        principalTable: "MatchPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Wickets_MatchPlayers_StumpedByMatchPlayerId",
                        column: x => x.StumpedByMatchPlayerId,
                        principalTable: "MatchPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Balls_BowlerMatchPlayerId",
                table: "Balls",
                column: "BowlerMatchPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Balls_InningsId_OverNumber_BallNumber",
                table: "Balls",
                columns: new[] { "InningsId", "OverNumber", "BallNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_Balls_NonStrikerMatchPlayerId",
                table: "Balls",
                column: "NonStrikerMatchPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Balls_StrikerMatchPlayerId",
                table: "Balls",
                column: "StrikerMatchPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Innings_MatchId_InningsNumber",
                table: "Innings",
                columns: new[] { "MatchId", "InningsNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_Latitude_Longitude",
                table: "Matches",
                columns: new[] { "Latitude", "Longitude" });

            migrationBuilder.CreateIndex(
                name: "IX_Matches_ScheduledAt",
                table: "Matches",
                column: "ScheduledAt");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_State",
                table: "Matches",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_Status",
                table: "Matches",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_UmpireUserId",
                table: "Matches",
                column: "UmpireUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchPlayers_MatchId_PlayerUserId",
                table: "MatchPlayers",
                columns: new[] { "MatchId", "PlayerUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchPlayers_MatchId_Team",
                table: "MatchPlayers",
                columns: new[] { "MatchId", "Team" });

            migrationBuilder.CreateIndex(
                name: "IX_MatchPlayers_PlayerUserId",
                table: "MatchPlayers",
                column: "PlayerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_MobileNumber",
                table: "Users",
                column: "MobileNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wickets_BallId",
                table: "Wickets",
                column: "BallId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wickets_CaughtByMatchPlayerId",
                table: "Wickets",
                column: "CaughtByMatchPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Wickets_DismissedMatchPlayerId",
                table: "Wickets",
                column: "DismissedMatchPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Wickets_RunOutByMatchPlayerId",
                table: "Wickets",
                column: "RunOutByMatchPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Wickets_StumpedByMatchPlayerId",
                table: "Wickets",
                column: "StumpedByMatchPlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerStatistics");

            migrationBuilder.DropTable(
                name: "UmpireStatistics");

            migrationBuilder.DropTable(
                name: "Wickets");

            migrationBuilder.DropTable(
                name: "Umpires");

            migrationBuilder.DropTable(
                name: "Balls");

            migrationBuilder.DropTable(
                name: "Innings");

            migrationBuilder.DropTable(
                name: "MatchPlayers");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}

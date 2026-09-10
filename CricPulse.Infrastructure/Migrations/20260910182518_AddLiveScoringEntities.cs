using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CricPulse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLiveScoringEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Innings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchId = table.Column<int>(type: "int", nullable: false),
                    InningsNumber = table.Column<int>(type: "int", nullable: false),
                    BattingTeam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BowlingTeam = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StrikerMatchPlayerId = table.Column<int>(type: "int", nullable: false),
                    NonStrikerMatchPlayerId = table.Column<int>(type: "int", nullable: false),
                    CurrentBowlerMatchPlayerId = table.Column<int>(type: "int", nullable: true),
                    TotalRuns = table.Column<int>(type: "int", nullable: false),
                    Wickets = table.Column<int>(type: "int", nullable: false),
                    LegalBalls = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Innings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Innings_MatchPlayer_CurrentBowlerMatchPlayerId",
                        column: x => x.CurrentBowlerMatchPlayerId,
                        principalTable: "MatchPlayer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Innings_MatchPlayer_NonStrikerMatchPlayerId",
                        column: x => x.NonStrikerMatchPlayerId,
                        principalTable: "MatchPlayer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Innings_MatchPlayer_StrikerMatchPlayerId",
                        column: x => x.StrikerMatchPlayerId,
                        principalTable: "MatchPlayer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Innings_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ball",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InningsId = table.Column<int>(type: "int", nullable: false),
                    OverNumber = table.Column<int>(type: "int", nullable: false),
                    BallNumber = table.Column<int>(type: "int", nullable: false),
                    StrikerMatchPlayerId = table.Column<int>(type: "int", nullable: false),
                    NonStrikerMatchPlayerId = table.Column<int>(type: "int", nullable: false),
                    BowlerMatchPlayerId = table.Column<int>(type: "int", nullable: false),
                    Runs = table.Column<int>(type: "int", nullable: false),
                    IsLegalDelivery = table.Column<bool>(type: "bit", nullable: false),
                    ExtraType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExtraRuns = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ball", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ball_Innings_InningsId",
                        column: x => x.InningsId,
                        principalTable: "Innings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Ball_MatchPlayer_BowlerMatchPlayerId",
                        column: x => x.BowlerMatchPlayerId,
                        principalTable: "MatchPlayer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ball_MatchPlayer_NonStrikerMatchPlayerId",
                        column: x => x.NonStrikerMatchPlayerId,
                        principalTable: "MatchPlayer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ball_MatchPlayer_StrikerMatchPlayerId",
                        column: x => x.StrikerMatchPlayerId,
                        principalTable: "MatchPlayer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Wicket",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BallId = table.Column<int>(type: "int", nullable: false),
                    DismissedMatchPlayerId = table.Column<int>(type: "int", nullable: false),
                    WicketType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CaughtByMatchPlayerId = table.Column<int>(type: "int", nullable: true),
                    RunsCompleted = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wicket", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wicket_Ball_BallId",
                        column: x => x.BallId,
                        principalTable: "Ball",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Wicket_MatchPlayer_CaughtByMatchPlayerId",
                        column: x => x.CaughtByMatchPlayerId,
                        principalTable: "MatchPlayer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Wicket_MatchPlayer_DismissedMatchPlayerId",
                        column: x => x.DismissedMatchPlayerId,
                        principalTable: "MatchPlayer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ball_BowlerMatchPlayerId",
                table: "Ball",
                column: "BowlerMatchPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Ball_InningsId",
                table: "Ball",
                column: "InningsId");

            migrationBuilder.CreateIndex(
                name: "IX_Ball_NonStrikerMatchPlayerId",
                table: "Ball",
                column: "NonStrikerMatchPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Ball_StrikerMatchPlayerId",
                table: "Ball",
                column: "StrikerMatchPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Innings_CurrentBowlerMatchPlayerId",
                table: "Innings",
                column: "CurrentBowlerMatchPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Innings_MatchId",
                table: "Innings",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Innings_NonStrikerMatchPlayerId",
                table: "Innings",
                column: "NonStrikerMatchPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Innings_StrikerMatchPlayerId",
                table: "Innings",
                column: "StrikerMatchPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Wicket_BallId",
                table: "Wicket",
                column: "BallId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wicket_CaughtByMatchPlayerId",
                table: "Wicket",
                column: "CaughtByMatchPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Wicket_DismissedMatchPlayerId",
                table: "Wicket",
                column: "DismissedMatchPlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Wicket");

            migrationBuilder.DropTable(
                name: "Ball");

            migrationBuilder.DropTable(
                name: "Innings");
        }
    }
}

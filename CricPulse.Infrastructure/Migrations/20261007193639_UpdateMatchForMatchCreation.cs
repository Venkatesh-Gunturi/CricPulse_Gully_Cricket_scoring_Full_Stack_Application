using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CricPulse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMatchForMatchCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoUrl",
                table: "Matches");

            migrationBuilder.AddColumn<int>(
                name: "Overs",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlayersPerTeam",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TeamALogo",
                table: "Matches",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeamBLogo",
                table: "Matches",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Venue",
                table: "Matches",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Overs",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "PlayersPerTeam",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TeamALogo",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "TeamBLogo",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Venue",
                table: "Matches");

            migrationBuilder.AddColumn<string>(
                name: "LogoUrl",
                table: "Matches",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CricPulse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchResultState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResultText",
                table: "Matches",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WinnerTeam",
                table: "Matches",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PreviousCompletionPending",
                table: "Balls",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PreviousIsFreeHit",
                table: "Balls",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResultText",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "WinnerTeam",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "PreviousCompletionPending",
                table: "Balls");

            migrationBuilder.DropColumn(
                name: "PreviousIsFreeHit",
                table: "Balls");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CricPulse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConvertMatchStatusToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                    UPDATE Matches
                SET Status =
                CASE Status
                WHEN 'Scheduled' THEN '0'
                WHEN 'Live' THEN '1'
                WHEN 'Completed' THEN '2'
                WHEN 'Cancelled' THEN '3'
                ELSE '0'
                END
                """);

                migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Matches",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Matches",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}

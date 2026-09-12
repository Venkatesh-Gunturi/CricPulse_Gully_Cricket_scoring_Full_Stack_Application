using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CricPulse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingRegistrationOtpVerified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOtpVerified",
                table: "PendingRegistrations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOtpVerified",
                table: "PendingRegistrations");
        }
    }
}

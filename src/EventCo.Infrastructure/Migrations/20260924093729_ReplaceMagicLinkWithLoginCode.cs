using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventCo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceMagicLinkWithLoginCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MagicLinkTokens_TokenHash",
                table: "MagicLinkTokens");

            migrationBuilder.AddColumn<int>(
                name: "FailedAttempts",
                table: "MagicLinkTokens",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FailedAttempts",
                table: "MagicLinkTokens");

            migrationBuilder.CreateIndex(
                name: "IX_MagicLinkTokens_TokenHash",
                table: "MagicLinkTokens",
                column: "TokenHash",
                unique: true);
        }
    }
}

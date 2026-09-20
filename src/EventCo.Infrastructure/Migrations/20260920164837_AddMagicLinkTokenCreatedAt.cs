using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventCo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMagicLinkTokenCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MagicLinkTokens",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_MagicLinkTokens_Email_CreatedAt",
                table: "MagicLinkTokens",
                columns: new[] { "Email", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MagicLinkTokens_Email_CreatedAt",
                table: "MagicLinkTokens");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MagicLinkTokens");
        }
    }
}

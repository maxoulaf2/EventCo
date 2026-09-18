using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventCo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEventParticipantJoinedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JoinedAt",
                table: "EventParticipants");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "JoinedAt",
                table: "EventParticipants",
                type: "timestamp with time zone",
                nullable: true);
        }
    }
}

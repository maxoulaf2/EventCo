using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventCo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTaskCreatedByUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "EventTasks",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_EventTasks_CreatedByUserId",
                table: "EventTasks",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventTasks_Users_CreatedByUserId",
                table: "EventTasks",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventTasks_Users_CreatedByUserId",
                table: "EventTasks");

            migrationBuilder.DropIndex(
                name: "IX_EventTasks_CreatedByUserId",
                table: "EventTasks");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "EventTasks");
        }
    }
}

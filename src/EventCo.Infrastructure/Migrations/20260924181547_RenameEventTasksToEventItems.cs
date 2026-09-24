using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventCo.Infrastructure.Migrations
{
    /// <inheritdoc />
    // Réécrite à la main : EF Core, ne pouvant pas rapprocher l'ancienne entité de la nouvelle, générait un
    // DROP TABLE + CREATE TABLE, qui aurait perdu tous les articles existants. Renommages seuls, données conservées
    // (clé primaire, clés étrangères et index renommés pour rester alignés sur les conventions de nommage EF Core).
    public partial class RenameEventTasksToEventItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_EventTasks_Events_EventId", table: "EventTasks");

            migrationBuilder.DropForeignKey(name: "FK_EventTasks_Users_AssignedToUserId", table: "EventTasks");

            migrationBuilder.DropForeignKey(name: "FK_EventTasks_Users_CreatedByUserId", table: "EventTasks");

            migrationBuilder.DropPrimaryKey(name: "PK_EventTasks", table: "EventTasks");

            migrationBuilder.RenameTable(name: "EventTasks", newName: "EventItems");

            migrationBuilder.RenameIndex(name: "IX_EventTasks_AssignedToUserId", table: "EventItems", newName: "IX_EventItems_AssignedToUserId");

            migrationBuilder.RenameIndex(name: "IX_EventTasks_CreatedByUserId", table: "EventItems", newName: "IX_EventItems_CreatedByUserId");

            migrationBuilder.RenameIndex(name: "IX_EventTasks_EventId", table: "EventItems", newName: "IX_EventItems_EventId");

            migrationBuilder.AddPrimaryKey(name: "PK_EventItems", table: "EventItems", column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EventItems_Events_EventId",
                table: "EventItems",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventItems_Users_AssignedToUserId",
                table: "EventItems",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EventItems_Users_CreatedByUserId",
                table: "EventItems",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_EventItems_Events_EventId", table: "EventItems");

            migrationBuilder.DropForeignKey(name: "FK_EventItems_Users_AssignedToUserId", table: "EventItems");

            migrationBuilder.DropForeignKey(name: "FK_EventItems_Users_CreatedByUserId", table: "EventItems");

            migrationBuilder.DropPrimaryKey(name: "PK_EventItems", table: "EventItems");

            migrationBuilder.RenameIndex(name: "IX_EventItems_EventId", table: "EventItems", newName: "IX_EventTasks_EventId");

            migrationBuilder.RenameIndex(name: "IX_EventItems_CreatedByUserId", table: "EventItems", newName: "IX_EventTasks_CreatedByUserId");

            migrationBuilder.RenameIndex(name: "IX_EventItems_AssignedToUserId", table: "EventItems", newName: "IX_EventTasks_AssignedToUserId");

            migrationBuilder.RenameTable(name: "EventItems", newName: "EventTasks");

            migrationBuilder.AddPrimaryKey(name: "PK_EventTasks", table: "EventTasks", column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EventTasks_Events_EventId",
                table: "EventTasks",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventTasks_Users_AssignedToUserId",
                table: "EventTasks",
                column: "AssignedToUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_EventTasks_Users_CreatedByUserId",
                table: "EventTasks",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

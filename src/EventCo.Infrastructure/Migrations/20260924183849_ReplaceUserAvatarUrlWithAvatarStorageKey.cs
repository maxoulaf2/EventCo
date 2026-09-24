using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventCo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceUserAvatarUrlWithAvatarStorageKey : Migration
    {
        /// <inheritdoc />
        // Suppression + ajout plutôt que renommage : AvatarUrl n'a jamais été renseignée par aucun use case
        // (toujours NULL), et une URL ne serait de toute façon pas une clé de stockage valide.
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "AvatarStorageKey",
                table: "Users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarStorageKey",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Users",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);
        }
    }
}

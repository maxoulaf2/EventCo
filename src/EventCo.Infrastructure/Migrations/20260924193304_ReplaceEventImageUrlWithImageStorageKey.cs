using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventCo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceEventImageUrlWithImageStorageKey : Migration
    {
        /// <inheritdoc />
        // Suppression + ajout plutôt que renommage : une URL externe n'est pas une clé de stockage valide,
        // et une image d'événement n'est plus qu'un fichier envoyé par l'utilisateur (bucket dédié). Les
        // liens saisis jusqu'ici sont volontairement abandonnés : les événements concernés perdent leur image.
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Events");

            migrationBuilder.AddColumn<string>(
                name: "ImageStorageKey",
                table: "Events",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageStorageKey",
                table: "Events");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Events",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);
        }
    }
}

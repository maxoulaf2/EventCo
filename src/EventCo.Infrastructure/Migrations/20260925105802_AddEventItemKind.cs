using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventCo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEventItemKind : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Kind",
                table: "EventItems",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "ToBring");

            // Reprise des articles existants : seul un article créé par un participant simple et attribué
            // à lui-même devient « apporté » ; tous les autres restent « à prendre » (valeur par défaut).
            migrationBuilder.Sql("""
                UPDATE "EventItems" AS i
                SET "Kind" = 'Contribution'
                WHERE i."AssignedToUserId" = i."CreatedByUserId"
                  AND NOT EXISTS (
                      SELECT 1 FROM "EventParticipants" AS p
                      WHERE p."EventId" = i."EventId"
                        AND p."UserId" = i."CreatedByUserId"
                        AND p."Role" = 'Organizer');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Kind",
                table: "EventItems");
        }
    }
}

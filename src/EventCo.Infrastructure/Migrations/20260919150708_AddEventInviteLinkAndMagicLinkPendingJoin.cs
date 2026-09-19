using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventCo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEventInviteLinkAndMagicLinkPendingJoin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EventInviteLinkToken",
                table: "MagicLinkTokens",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InviteLinkToken",
                table: "Events",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            // Backfill : des lignes Events existent déjà (colonne ajoutée après coup), on ne peut donc pas
            // poser directement une contrainte NOT NULL + unique. md5(random()...) donne une valeur distincte
            // par ligne, suffisant ici puisque ces valeurs de secours ne sont jamais partagées (chaque
            // événement existant se voit doter d'un lien d'invitation inédit, jamais communiqué avant cette
            // migration).
            migrationBuilder.Sql(
                "UPDATE \"Events\" SET \"InviteLinkToken\" = md5(random()::text || clock_timestamp()::text || \"Id\"::text) WHERE \"InviteLinkToken\" IS NULL;");

            migrationBuilder.AlterColumn<string>(
                name: "InviteLinkToken",
                table: "Events",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_InviteLinkToken",
                table: "Events",
                column: "InviteLinkToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Events_InviteLinkToken",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "EventInviteLinkToken",
                table: "MagicLinkTokens");

            migrationBuilder.DropColumn(
                name: "InviteLinkToken",
                table: "Events");
        }
    }
}

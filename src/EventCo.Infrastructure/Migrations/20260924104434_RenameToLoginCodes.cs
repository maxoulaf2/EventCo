using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventCo.Infrastructure.Migrations
{
    /// <inheritdoc />
    // Réécrite à la main : EF Core, ne pouvant pas rapprocher l'ancienne entité de la nouvelle, générait un
    // DROP TABLE + CREATE TABLE, qui aurait perdu les codes en cours de validité et l'historique anti-spam
    // (CreatedAt, cf. TooManyLoginCodeRequestsException). Renommages seuls, données conservées.
    public partial class RenameToLoginCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(name: "PK_MagicLinkTokens", table: "MagicLinkTokens");

            migrationBuilder.RenameTable(name: "MagicLinkTokens", newName: "LoginCodes");

            migrationBuilder.RenameColumn(name: "TokenHash", table: "LoginCodes", newName: "CodeHash");

            migrationBuilder.RenameIndex(name: "IX_MagicLinkTokens_Email", table: "LoginCodes", newName: "IX_LoginCodes_Email");

            migrationBuilder.RenameIndex(name: "IX_MagicLinkTokens_Email_CreatedAt", table: "LoginCodes", newName: "IX_LoginCodes_Email_CreatedAt");

            migrationBuilder.AddPrimaryKey(name: "PK_LoginCodes", table: "LoginCodes", column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(name: "PK_LoginCodes", table: "LoginCodes");

            migrationBuilder.RenameIndex(name: "IX_LoginCodes_Email_CreatedAt", table: "LoginCodes", newName: "IX_MagicLinkTokens_Email_CreatedAt");

            migrationBuilder.RenameIndex(name: "IX_LoginCodes_Email", table: "LoginCodes", newName: "IX_MagicLinkTokens_Email");

            migrationBuilder.RenameColumn(name: "CodeHash", table: "LoginCodes", newName: "TokenHash");

            migrationBuilder.RenameTable(name: "LoginCodes", newName: "MagicLinkTokens");

            migrationBuilder.AddPrimaryKey(name: "PK_MagicLinkTokens", table: "MagicLinkTokens", column: "Id");
        }
    }
}

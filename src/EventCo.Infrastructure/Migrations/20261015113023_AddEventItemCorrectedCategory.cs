using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventCo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEventItemCorrectedCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorrectedCategory",
                table: "EventItems",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrectedCategory",
                table: "EventItems");
        }
    }
}

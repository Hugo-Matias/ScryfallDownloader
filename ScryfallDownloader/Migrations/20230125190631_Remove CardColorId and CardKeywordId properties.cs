using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScryfallDownloader.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCardColorIdandCardKeywordIdproperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardKeywordId",
                table: "CardKeywords");

            migrationBuilder.DropColumn(
                name: "CardGenerateColorId",
                table: "CardGenerateColors");

            migrationBuilder.DropColumn(
                name: "CardColorId",
                table: "CardColors");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CardKeywordId",
                table: "CardKeywords",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CardGenerateColorId",
                table: "CardGenerateColors",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CardColorId",
                table: "CardColors",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}

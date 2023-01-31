using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScryfallDownloader.Migrations
{
    /// <inheritdoc />
    public partial class ConvertDeckTagstoListofDeckTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Decks_DeckId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Tags_DeckId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "DeckId",
                table: "Tags");

            migrationBuilder.CreateTable(
                name: "DeckTag",
                columns: table => new
                {
                    DeckId = table.Column<int>(type: "INTEGER", nullable: false),
                    TagId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeckTag", x => new { x.DeckId, x.TagId });
                    table.ForeignKey(
                        name: "FK_DeckTag_Decks_DeckId",
                        column: x => x.DeckId,
                        principalTable: "Decks",
                        principalColumn: "DeckId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeckTag_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "TagId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeckTag_TagId",
                table: "DeckTag",
                column: "TagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeckTag");

            migrationBuilder.AddColumn<int>(
                name: "DeckId",
                table: "Tags",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tags_DeckId",
                table: "Tags",
                column: "DeckId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Decks_DeckId",
                table: "Tags",
                column: "DeckId",
                principalTable: "Decks",
                principalColumn: "DeckId");
        }
    }
}

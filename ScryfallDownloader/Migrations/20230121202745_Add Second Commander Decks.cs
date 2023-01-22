using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScryfallDownloader.Migrations
{
    /// <inheritdoc />
    public partial class AddSecondCommanderDecks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Commander2CardId",
                table: "Decks",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EdhrecCommanders_Name",
                table: "EdhrecCommanders",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Decks_Commander2CardId",
                table: "Decks",
                column: "Commander2CardId");

            migrationBuilder.AddForeignKey(
                name: "FK_Decks_Cards_Commander2CardId",
                table: "Decks",
                column: "Commander2CardId",
                principalTable: "Cards",
                principalColumn: "CardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Decks_Cards_Commander2CardId",
                table: "Decks");

            migrationBuilder.DropIndex(
                name: "IX_EdhrecCommanders_Name",
                table: "EdhrecCommanders");

            migrationBuilder.DropIndex(
                name: "IX_Decks_Commander2CardId",
                table: "Decks");

            migrationBuilder.DropColumn(
                name: "Commander2CardId",
                table: "Decks");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScryfallDownloader.Migrations
{
    /// <inheritdoc />
    public partial class AddCardNameSetCNconstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Cards_Name_SetId_CollectorsNumber",
                table: "Cards",
                columns: new[] { "Name", "SetId", "CollectorsNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cards_Name_SetId_CollectorsNumber",
                table: "Cards");
        }
    }
}

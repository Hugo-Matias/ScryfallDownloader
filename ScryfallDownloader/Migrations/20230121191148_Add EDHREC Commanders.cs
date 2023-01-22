using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScryfallDownloader.Migrations
{
    /// <inheritdoc />
    public partial class AddEDHRECCommanders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EdhrecCommanders",
                columns: table => new
                {
                    EdhrecCommanderId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Link = table.Column<string>(type: "TEXT", nullable: false),
                    DeckCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EdhrecCommanders", x => x.EdhrecCommanderId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EdhrecCommanders");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScryfallDownloader.Migrations
{
    /// <inheritdoc />
    public partial class AddSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    SettingId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MT8Page = table.Column<int>(type: "INTEGER", nullable: false),
                    SCGDate = table.Column<string>(type: "TEXT", nullable: false),
                    SCGDeck = table.Column<int>(type: "INTEGER", nullable: false),
                    SCGLimit = table.Column<int>(type: "INTEGER", nullable: false),
                    SCGPage = table.Column<int>(type: "INTEGER", nullable: false),
                    EDHCommander = table.Column<string>(type: "TEXT", nullable: false),
                    EDHDeck = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.SettingId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Settings");
        }
    }
}

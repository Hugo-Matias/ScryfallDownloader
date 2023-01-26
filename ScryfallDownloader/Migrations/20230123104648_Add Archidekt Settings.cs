using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScryfallDownloader.Migrations
{
    /// <inheritdoc />
    public partial class AddArchidektSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ARCHPage",
                table: "Settings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ARCHPage",
                table: "Settings");
        }
    }
}

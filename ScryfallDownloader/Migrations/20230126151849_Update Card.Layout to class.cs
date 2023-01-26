using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScryfallDownloader.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCardLayouttoclass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Layout",
                table: "Cards");

            migrationBuilder.AddColumn<int>(
                name: "LayoutId",
                table: "Cards",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Layouts",
                columns: table => new
                {
                    LayoutId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Layouts", x => x.LayoutId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cards_LayoutId",
                table: "Cards",
                column: "LayoutId");

            migrationBuilder.CreateIndex(
                name: "IX_Layouts_Name",
                table: "Layouts",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Cards_Layouts_LayoutId",
                table: "Cards",
                column: "LayoutId",
                principalTable: "Layouts",
                principalColumn: "LayoutId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cards_Layouts_LayoutId",
                table: "Cards");

            migrationBuilder.DropTable(
                name: "Layouts");

            migrationBuilder.DropIndex(
                name: "IX_Cards_LayoutId",
                table: "Cards");

            migrationBuilder.DropColumn(
                name: "LayoutId",
                table: "Cards");

            migrationBuilder.AddColumn<string>(
                name: "Layout",
                table: "Cards",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}

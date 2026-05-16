using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickStock.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAddedByToConsumableItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AddedByUserId",
                table: "ConsumableItems",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AddedByUsername",
                table: "ConsumableItems",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AddedByUserId",
                table: "ConsumableItems");

            migrationBuilder.DropColumn(
                name: "AddedByUsername",
                table: "ConsumableItems");
        }
    }
}

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickStock.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomsAndAssetLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoomId",
                table: "Itassets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    RoomId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoomName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoomFloor = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoomDescription = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.RoomId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Itassets_RoomId",
                table: "Itassets",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Itassets_Rooms_RoomId",
                table: "Itassets",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "RoomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Itassets_Rooms_RoomId",
                table: "Itassets");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Itassets_RoomId",
                table: "Itassets");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "Itassets");
        }
    }
}

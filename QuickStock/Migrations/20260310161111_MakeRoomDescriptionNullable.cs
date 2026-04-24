using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickStock.Migrations
{
    /// <inheritdoc />
    public partial class MakeRoomDescriptionNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RoomDescription",
                table: "Rooms",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomDescription",
                keyValue: null,
                column: "RoomDescription",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "RoomDescription",
                table: "Rooms",
                type: "varchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}

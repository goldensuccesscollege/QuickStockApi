using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuickStock.Migrations
{
    /// <inheritdoc />
    public partial class SeedRoomData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "RoomId", "RoomDescription", "RoomFloor", "RoomName" },
                values: new object[,]
                {
                    { 1, "General IT Office", "4th Floor", "Room 403" },
                    { 2, "Hardware Testing and Maintenance", "2nd Floor", "IT Lab" },
                    { 3, "Critical Infrastructure", "Basement", "Server Room 1" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 3);
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickStock.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminManagementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDisabled",
                table: "Rooms",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsBlocked",
                table: "AccountCampuses",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 1,
                column: "IsDisabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 2,
                column: "IsDisabled",
                value: false);

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 3,
                column: "IsDisabled",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDisabled",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "IsBlocked",
                table: "AccountCampuses");
        }
    }
}

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuickStock.Migrations
{
    /// <inheritdoc />
    public partial class AddCampusSegregation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CampusId",
                table: "Rooms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CampusId",
                table: "Itassets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CampusId",
                table: "Accounts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Campuses",
                columns: table => new
                {
                    CampusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Address = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campuses", x => x.CampusId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Campuses",
                columns: new[] { "CampusId", "Address", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Cebu City", "Main Campus", "Cebu Campus" },
                    { 2, "Metro Manila", "Luzon Branch", "Manila Campus" }
                });

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 1,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 2,
                column: "CampusId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Rooms",
                keyColumn: "RoomId",
                keyValue: 3,
                column: "CampusId",
                value: 1);
            
            migrationBuilder.Sql("UPDATE Rooms SET CampusId = 1");
            migrationBuilder.Sql("UPDATE Itassets SET CampusId = 1");
            migrationBuilder.Sql("UPDATE Accounts SET CampusId = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_CampusId",
                table: "Rooms",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_Itassets_CampusId",
                table: "Itassets",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_CampusId",
                table: "Accounts",
                column: "CampusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Campuses_CampusId",
                table: "Accounts",
                column: "CampusId",
                principalTable: "Campuses",
                principalColumn: "CampusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Itassets_Campuses_CampusId",
                table: "Itassets",
                column: "CampusId",
                principalTable: "Campuses",
                principalColumn: "CampusId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Campuses_CampusId",
                table: "Rooms",
                column: "CampusId",
                principalTable: "Campuses",
                principalColumn: "CampusId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Campuses_CampusId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Itassets_Campuses_CampusId",
                table: "Itassets");

            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Campuses_CampusId",
                table: "Rooms");

            migrationBuilder.DropTable(
                name: "Campuses");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_CampusId",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Itassets_CampusId",
                table: "Itassets");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_CampusId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "CampusId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "CampusId",
                table: "Itassets");

            migrationBuilder.DropColumn(
                name: "CampusId",
                table: "Accounts");
        }
    }
}

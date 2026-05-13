using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickStock.Migrations
{
    /// <inheritdoc />
    public partial class RefactorLibraryToItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessionNumber",
                table: "LibraryBooks");

            migrationBuilder.DropColumn(
                name: "AvailableQuantity",
                table: "LibraryBooks");

            migrationBuilder.DropColumn(
                name: "Condition",
                table: "LibraryBooks");

            migrationBuilder.DropColumn(
                name: "QRCode",
                table: "LibraryBooks");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "LibraryBooks");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "LibraryBooks");

            migrationBuilder.CreateTable(
                name: "LibraryBookItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LibrarydataId = table.Column<int>(type: "int", nullable: false),
                    AccessionNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QRCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Condition = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CampusId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryBookItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LibraryBookItems_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalTable: "Campuses",
                        principalColumn: "CampusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LibraryBookItems_LibraryBooks_LibrarydataId",
                        column: x => x.LibrarydataId,
                        principalTable: "LibraryBooks",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryBookItems_CampusId",
                table: "LibraryBookItems",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryBookItems_LibrarydataId",
                table: "LibraryBookItems",
                column: "LibrarydataId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LibraryBookItems");

            migrationBuilder.AddColumn<string>(
                name: "AccessionNumber",
                table: "LibraryBooks",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "AvailableQuantity",
                table: "LibraryBooks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Condition",
                table: "LibraryBooks",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "QRCode",
                table: "LibraryBooks",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "LibraryBooks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "LibraryBooks",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}

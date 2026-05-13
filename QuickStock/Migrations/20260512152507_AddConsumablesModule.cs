using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickStock.Migrations
{
    /// <inheritdoc />
    public partial class AddConsumablesModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanAccessConsumables",
                table: "Accounts",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ConsumableList",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Product = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateArrived = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    In = table.Column<int>(type: "int", nullable: false),
                    Out = table.Column<int>(type: "int", nullable: false),
                    CampusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumableList", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsumableList_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalTable: "Campuses",
                        principalColumn: "CampusId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ConsumableItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ItemCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateOut = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ConsumableDataId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumableItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsumableItems_ConsumableList_ConsumableDataId",
                        column: x => x.ConsumableDataId,
                        principalTable: "ConsumableList",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumableItems_ConsumableDataId",
                table: "ConsumableItems",
                column: "ConsumableDataId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumableList_CampusId",
                table: "ConsumableList",
                column: "CampusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsumableItems");

            migrationBuilder.DropTable(
                name: "ConsumableList");

            migrationBuilder.DropColumn(
                name: "CanAccessConsumables",
                table: "Accounts");
        }
    }
}

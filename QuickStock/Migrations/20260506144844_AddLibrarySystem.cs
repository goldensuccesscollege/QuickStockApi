using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickStock.Migrations
{
    /// <inheritdoc />
    public partial class AddLibrarySystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApparelList_Campuses_CampusId",
                table: "ApparelList");

            migrationBuilder.DropForeignKey(
                name: "FK_Itassets_Campuses_CampusId",
                table: "Itassets");

            migrationBuilder.DropForeignKey(
                name: "FK_Itassets_Rooms_RoomId",
                table: "Itassets");

            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Campuses_CampusId",
                table: "Rooms");

            migrationBuilder.CreateTable(
                name: "LibraryBooks",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccessionNumber = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ISBN = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    QRCode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Title = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subtitle = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Author = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CoAuthor = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Publisher = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PublicationYear = table.Column<int>(type: "int", nullable: true),
                    Edition = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CategoryId = table.Column<int>(type: "int", nullable: true),
                    Genre = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Language = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ShelfLocation = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CallNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    AvailableQuantity = table.Column<int>(type: "int", nullable: false),
                    Condition = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DateAcquired = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Source = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Donor = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CoverImage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CampusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryBooks", x => x.ItemId);
                    table.ForeignKey(
                        name: "FK_LibraryBooks_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalTable: "Campuses",
                        principalColumn: "CampusId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryBooks_CampusId",
                table: "LibraryBooks",
                column: "CampusId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApparelList_Campuses_CampusId",
                table: "ApparelList",
                column: "CampusId",
                principalTable: "Campuses",
                principalColumn: "CampusId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Itassets_Campuses_CampusId",
                table: "Itassets",
                column: "CampusId",
                principalTable: "Campuses",
                principalColumn: "CampusId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Itassets_Rooms_RoomId",
                table: "Itassets",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "RoomId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Campuses_CampusId",
                table: "Rooms",
                column: "CampusId",
                principalTable: "Campuses",
                principalColumn: "CampusId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApparelList_Campuses_CampusId",
                table: "ApparelList");

            migrationBuilder.DropForeignKey(
                name: "FK_Itassets_Campuses_CampusId",
                table: "Itassets");

            migrationBuilder.DropForeignKey(
                name: "FK_Itassets_Rooms_RoomId",
                table: "Itassets");

            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Campuses_CampusId",
                table: "Rooms");

            migrationBuilder.DropTable(
                name: "LibraryBooks");

            migrationBuilder.AddForeignKey(
                name: "FK_ApparelList_Campuses_CampusId",
                table: "ApparelList",
                column: "CampusId",
                principalTable: "Campuses",
                principalColumn: "CampusId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Itassets_Campuses_CampusId",
                table: "Itassets",
                column: "CampusId",
                principalTable: "Campuses",
                principalColumn: "CampusId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Itassets_Rooms_RoomId",
                table: "Itassets",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Campuses_CampusId",
                table: "Rooms",
                column: "CampusId",
                principalTable: "Campuses",
                principalColumn: "CampusId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

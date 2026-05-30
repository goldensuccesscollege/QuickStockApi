using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace QuickStock.Migrations
{
    /// <inheritdoc />
    public partial class AddConsumableRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConsumableRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    RequestType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    ProductName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    ProductType = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    TargetItemId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    RejectionReason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RequestorId = table.Column<string>(type: "longtext", nullable: true),
                    RequestorName = table.Column<string>(type: "longtext", nullable: true),
                    ReviewerId = table.Column<string>(type: "longtext", nullable: true),
                    ReviewerName = table.Column<string>(type: "longtext", nullable: true),
                    CampusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumableRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsumableRequest_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalTable: "Campuses",
                        principalColumn: "CampusId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsumableRequest_ConsumableUnit_TargetItemId",
                        column: x => x.TargetItemId,
                        principalTable: "ConsumableUnit",
                        principalColumn: "ID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumableRequest_CampusId",
                table: "ConsumableRequest",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumableRequest_TargetItemId",
                table: "ConsumableRequest",
                column: "TargetItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsumableRequest");
        }
    }
}

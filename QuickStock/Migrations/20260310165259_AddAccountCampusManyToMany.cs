using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickStock.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountCampusManyToMany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Campuses_CampusId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_CampusId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "CampusId",
                table: "Accounts");

            migrationBuilder.CreateTable(
                name: "AccountCampuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    CampusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountCampuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountCampuses_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccountCampuses_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalTable: "Campuses",
                        principalColumn: "CampusId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AccountCampuses_AccountId",
                table: "AccountCampuses",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountCampuses_CampusId",
                table: "AccountCampuses",
                column: "CampusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountCampuses");

            migrationBuilder.AddColumn<int>(
                name: "CampusId",
                table: "Accounts",
                type: "int",
                nullable: true);

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
        }
    }
}

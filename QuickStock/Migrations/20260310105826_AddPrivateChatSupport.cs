using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickStock.Migrations
{
    /// <inheritdoc />
    public partial class AddPrivateChatSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReceiverAccountId",
                table: "ChatMessages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ReceiverAccountId",
                table: "ChatMessages",
                column: "ReceiverAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_Accounts_ReceiverAccountId",
                table: "ChatMessages",
                column: "ReceiverAccountId",
                principalTable: "Accounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_Accounts_ReceiverAccountId",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_ReceiverAccountId",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "ReceiverAccountId",
                table: "ChatMessages");
        }
    }
}

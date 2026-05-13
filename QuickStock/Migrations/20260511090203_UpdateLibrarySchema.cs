using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickStock.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLibrarySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "LibraryBooks");

            migrationBuilder.DropColumn(
                name: "DateAcquired",
                table: "LibraryBooks");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "LibraryBooks");

            migrationBuilder.DropColumn(
                name: "PublicationYear",
                table: "LibraryBooks");

            migrationBuilder.RenameColumn(
                name: "Source",
                table: "LibraryBooks",
                newName: "SourceOfFund");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "LibraryBooks",
                newName: "Remarks");

            migrationBuilder.UpdateData(
                table: "LibraryBooks",
                keyColumn: "Author",
                keyValue: null,
                column: "Author",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Author",
                table: "LibraryBooks",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "BookNumber",
                table: "LibraryBooks",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Class",
                table: "LibraryBooks",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "CostPrice",
                table: "LibraryBooks",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateReceived",
                table: "LibraryBooks",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Pages",
                table: "LibraryBooks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Volumes",
                table: "LibraryBooks",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "LibraryBooks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookNumber",
                table: "LibraryBooks");

            migrationBuilder.DropColumn(
                name: "Class",
                table: "LibraryBooks");

            migrationBuilder.DropColumn(
                name: "CostPrice",
                table: "LibraryBooks");

            migrationBuilder.DropColumn(
                name: "DateReceived",
                table: "LibraryBooks");

            migrationBuilder.DropColumn(
                name: "Pages",
                table: "LibraryBooks");

            migrationBuilder.DropColumn(
                name: "Volumes",
                table: "LibraryBooks");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "LibraryBooks");

            migrationBuilder.RenameColumn(
                name: "SourceOfFund",
                table: "LibraryBooks",
                newName: "Source");

            migrationBuilder.RenameColumn(
                name: "Remarks",
                table: "LibraryBooks",
                newName: "Description");

            migrationBuilder.AlterColumn<string>(
                name: "Author",
                table: "LibraryBooks",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "LibraryBooks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateAcquired",
                table: "LibraryBooks",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "LibraryBooks",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PublicationYear",
                table: "LibraryBooks",
                type: "int",
                nullable: true);
        }
    }
}

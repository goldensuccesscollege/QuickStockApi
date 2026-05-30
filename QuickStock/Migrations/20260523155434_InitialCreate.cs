using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuickStock.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Username = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    AcceptTerms = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Role = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    VerificationTokens = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    Verified = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ResetToken = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    ResetTokenExpires = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PasswordReset = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Status = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    CanAccessITAssets = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CanAccessApparel = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CanAccessLibrary = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CanAccessFurniture = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CanAccessConsumables = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Action = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    EntityName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Details = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UserId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Username = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    CampusId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Campuses",
                columns: table => new
                {
                    CampusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Address = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campuses", x => x.CampusId);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "StoredImages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    FileName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Data = table.Column<byte[]>(type: "longblob", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredImages", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Birthday = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Address = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    PhoneNumber = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    ImageProfilePath = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    Created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Updated = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Profiles_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AccountCampuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    CampusId = table.Column<int>(type: "int", nullable: false),
                    IsBlocked = table.Column<bool>(type: "tinyint(1)", nullable: false)
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
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ApparelList",
                columns: table => new
                {
                    Apparel_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Apparel_Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Sex = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Size = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Grade_Level = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Quality_In_Stock = table.Column<int>(type: "int", nullable: false),
                    Reorder_level = table.Column<int>(type: "int", nullable: false),
                    Date_Purchased = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Unit_Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Supplier_Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Remarks = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    Location = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    CampusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApparelList", x => x.Apparel_ID);
                    table.ForeignKey(
                        name: "FK_ApparelList_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalTable: "Campuses",
                        principalColumn: "CampusId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ConsumableUnit",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ProductName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    Count = table.Column<int>(type: "int", nullable: true),
                    DateArrive = table.Column<DateTime>(type: "datetime", nullable: true),
                    CampusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumableUnit", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ConsumableUnit_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalTable: "Campuses",
                        principalColumn: "CampusId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LibraryBooks",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    BookNumber = table.Column<string>(type: "longtext", nullable: false),
                    Title = table.Column<string>(type: "longtext", nullable: false),
                    Subtitle = table.Column<string>(type: "longtext", nullable: true),
                    Author = table.Column<string>(type: "longtext", nullable: false),
                    CoAuthor = table.Column<string>(type: "longtext", nullable: true),
                    Class = table.Column<string>(type: "longtext", nullable: true),
                    Publisher = table.Column<string>(type: "longtext", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Edition = table.Column<string>(type: "longtext", nullable: true),
                    Volumes = table.Column<string>(type: "longtext", nullable: false),
                    Pages = table.Column<int>(type: "int", nullable: false),
                    ISBN = table.Column<string>(type: "longtext", nullable: true),
                    DateReceived = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CostPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SourceOfFund = table.Column<string>(type: "longtext", nullable: true),
                    Donor = table.Column<string>(type: "longtext", nullable: true),
                    AcquisitionType = table.Column<string>(type: "longtext", nullable: true),
                    Remarks = table.Column<string>(type: "longtext", nullable: true),
                    Genre = table.Column<string>(type: "longtext", nullable: true),
                    Language = table.Column<string>(type: "longtext", nullable: true),
                    ShelfLocation = table.Column<string>(type: "longtext", nullable: true),
                    CallNumber = table.Column<string>(type: "longtext", nullable: true),
                    CoverImage = table.Column<string>(type: "longtext", nullable: true),
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
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    RoomId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    RoomName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    RoomFloor = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    CampusId = table.Column<int>(type: "int", nullable: false),
                    RoomDescription = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true),
                    IsDisabled = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.RoomId);
                    table.ForeignKey(
                        name: "FK_Rooms_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalTable: "Campuses",
                        principalColumn: "CampusId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ApparelItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    AppareldataId = table.Column<int>(type: "int", nullable: false),
                    Apparel_Number = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    CampusId = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastModified = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApparelItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApparelItems_ApparelList_AppareldataId",
                        column: x => x.AppareldataId,
                        principalTable: "ApparelList",
                        principalColumn: "Apparel_ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApparelItems_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalTable: "Campuses",
                        principalColumn: "CampusId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LibraryBookItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    LibrarydataId = table.Column<int>(type: "int", nullable: false),
                    AccessionNumber = table.Column<string>(type: "longtext", nullable: false),
                    QRCode = table.Column<string>(type: "longtext", nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: false),
                    Condition = table.Column<string>(type: "longtext", nullable: true),
                    DateAcquired = table.Column<DateTime>(type: "datetime(6)", nullable: true),
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
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Furnitures",
                columns: table => new
                {
                    Item_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Item_Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Item_number = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Brand = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Location = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    DateAdded = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Condition = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    Qrcode = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    Item_count = table.Column<int>(type: "int", nullable: false),
                    RoomId = table.Column<int>(type: "int", nullable: true),
                    CampusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Furnitures", x => x.Item_ID);
                    table.ForeignKey(
                        name: "FK_Furnitures_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalTable: "Campuses",
                        principalColumn: "CampusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Furnitures_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Itassets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    SerialNumber = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Brand = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    DateAdded = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Qrcode = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    Type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    CampusId = table.Column<int>(type: "int", nullable: false),
                    RoomId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Itassets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Itassets_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalTable: "Campuses",
                        principalColumn: "CampusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Itassets_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Campuses",
                columns: new[] { "CampusId", "Address", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Cebu City", "Main Campus", "GSC-Cebu" },
                    { 2, "Metro Manila", "Luzon Branch", "GSC-Manila" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountCampuses_AccountId",
                table: "AccountCampuses",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountCampuses_CampusId",
                table: "AccountCampuses",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_ApparelItems_AppareldataId",
                table: "ApparelItems",
                column: "AppareldataId");

            migrationBuilder.CreateIndex(
                name: "IX_ApparelItems_CampusId",
                table: "ApparelItems",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_ApparelList_CampusId",
                table: "ApparelList",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumableUnit_CampusId",
                table: "ConsumableUnit",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_Furnitures_CampusId",
                table: "Furnitures",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_Furnitures_RoomId",
                table: "Furnitures",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Itassets_CampusId",
                table: "Itassets",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_Itassets_RoomId",
                table: "Itassets",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryBookItems_CampusId",
                table: "LibraryBookItems",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryBookItems_LibrarydataId",
                table: "LibraryBookItems",
                column: "LibrarydataId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryBooks_CampusId",
                table: "LibraryBooks",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_Profiles_AccountId",
                table: "Profiles",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_CampusId",
                table: "Rooms",
                column: "CampusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountCampuses");

            migrationBuilder.DropTable(
                name: "ApparelItems");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "ConsumableUnit");

            migrationBuilder.DropTable(
                name: "Furnitures");

            migrationBuilder.DropTable(
                name: "Itassets");

            migrationBuilder.DropTable(
                name: "LibraryBookItems");

            migrationBuilder.DropTable(
                name: "Profiles");

            migrationBuilder.DropTable(
                name: "StoredImages");

            migrationBuilder.DropTable(
                name: "ApparelList");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "LibraryBooks");

            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "Campuses");
        }
    }
}

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace notip_server.Migrations
{
    /// <inheritdoc />
    public partial class add_table_friend : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupUser_User",
                table: "GroupUser");

            migrationBuilder.CreateTable(
                name: "Friend",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SenderCode = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReceiverCode = table.Column<string>(type: "varchar(32)", unicode: false, maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friend", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupUser_User_UserCode",
                table: "GroupUser",
                column: "UserCode",
                principalTable: "User",
                principalColumn: "Code",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupUser_User_UserCode",
                table: "GroupUser");

            migrationBuilder.DropTable(
                name: "Friend");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupUser_User",
                table: "GroupUser",
                column: "UserCode",
                principalTable: "User",
                principalColumn: "Code");
        }
    }
}

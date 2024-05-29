using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramBusinessTripBot.WebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddChatOrChannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChatOrChannelId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChatOrChannel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChatOrChannelId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatOrChannel", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_ChatOrChannelId",
                table: "Users",
                column: "ChatOrChannelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_ChatOrChannel_ChatOrChannelId",
                table: "Users",
                column: "ChatOrChannelId",
                principalTable: "ChatOrChannel",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_ChatOrChannel_ChatOrChannelId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "ChatOrChannel");

            migrationBuilder.DropIndex(
                name: "IX_Users_ChatOrChannelId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ChatOrChannelId",
                table: "Users");
        }
    }
}

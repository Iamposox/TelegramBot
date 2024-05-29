using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramBusinessTripBot.WebApp.Migrations
{
    /// <inheritdoc />
    public partial class changeUserToChannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_ChatOrChannels_ChatOrChannelId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ChatOrChannelId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ChatOrChannelId",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "UsersChatOrChannel",
                columns: table => new
                {
                    ChatOrChannelId = table.Column<int>(type: "int", nullable: false),
                    UsersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersChatOrChannel", x => new { x.ChatOrChannelId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_UsersChatOrChannel_ChatOrChannel",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsersChatOrChannel_Users",
                        column: x => x.ChatOrChannelId,
                        principalTable: "ChatOrChannels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsersChatOrChannel_UsersId",
                table: "UsersChatOrChannel",
                column: "UsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UsersChatOrChannel");

            migrationBuilder.AddColumn<int>(
                name: "ChatOrChannelId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ChatOrChannelId",
                table: "Users",
                column: "ChatOrChannelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_ChatOrChannels_ChatOrChannelId",
                table: "Users",
                column: "ChatOrChannelId",
                principalTable: "ChatOrChannels",
                principalColumn: "Id");
        }
    }
}

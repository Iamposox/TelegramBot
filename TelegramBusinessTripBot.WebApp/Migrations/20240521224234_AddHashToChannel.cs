using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramBusinessTripBot.WebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddHashToChannel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_ChatOrChannel_ChatOrChannelId",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatOrChannel",
                table: "ChatOrChannel");

            migrationBuilder.RenameTable(
                name: "ChatOrChannel",
                newName: "ChatOrChannels");

            migrationBuilder.AddColumn<long>(
                name: "Hash",
                table: "ChatOrChannels",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatOrChannels",
                table: "ChatOrChannels",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_ChatOrChannels_ChatOrChannelId",
                table: "Users",
                column: "ChatOrChannelId",
                principalTable: "ChatOrChannels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_ChatOrChannels_ChatOrChannelId",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatOrChannels",
                table: "ChatOrChannels");

            migrationBuilder.DropColumn(
                name: "Hash",
                table: "ChatOrChannels");

            migrationBuilder.RenameTable(
                name: "ChatOrChannels",
                newName: "ChatOrChannel");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatOrChannel",
                table: "ChatOrChannel",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_ChatOrChannel_ChatOrChannelId",
                table: "Users",
                column: "ChatOrChannelId",
                principalTable: "ChatOrChannel",
                principalColumn: "Id");
        }
    }
}

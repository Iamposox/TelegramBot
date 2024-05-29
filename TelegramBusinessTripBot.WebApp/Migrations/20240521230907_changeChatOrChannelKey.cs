using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramBusinessTripBot.WebApp.Migrations
{
    /// <inheritdoc />
    public partial class changeChatOrChannelKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsersChatOrChannel_Users",
                table: "UsersChatOrChannel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsersChatOrChannel",
                table: "UsersChatOrChannel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatOrChannels",
                table: "ChatOrChannels");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ChatOrChannels");

            migrationBuilder.AlterColumn<long>(
                name: "ChatOrChannelId",
                table: "UsersChatOrChannel",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<long>(
                name: "ChatOrChannelHash",
                table: "UsersChatOrChannel",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<long>(
                name: "Hash",
                table: "ChatOrChannels",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsersChatOrChannel",
                table: "UsersChatOrChannel",
                columns: new[] { "ChatOrChannelId", "ChatOrChannelHash", "UsersUserId", "UsersUserHash" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatOrChannels",
                table: "ChatOrChannels",
                columns: new[] { "ChatOrChannelId", "Hash" });

            migrationBuilder.AddForeignKey(
                name: "FK_UsersChatOrChannel_Users",
                table: "UsersChatOrChannel",
                columns: new[] { "ChatOrChannelId", "ChatOrChannelHash" },
                principalTable: "ChatOrChannels",
                principalColumns: new[] { "ChatOrChannelId", "Hash" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsersChatOrChannel_Users",
                table: "UsersChatOrChannel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsersChatOrChannel",
                table: "UsersChatOrChannel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatOrChannels",
                table: "ChatOrChannels");

            migrationBuilder.DropColumn(
                name: "ChatOrChannelHash",
                table: "UsersChatOrChannel");

            migrationBuilder.AlterColumn<int>(
                name: "ChatOrChannelId",
                table: "UsersChatOrChannel",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "Hash",
                table: "ChatOrChannels",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ChatOrChannels",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsersChatOrChannel",
                table: "UsersChatOrChannel",
                columns: new[] { "ChatOrChannelId", "UsersUserId", "UsersUserHash" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatOrChannels",
                table: "ChatOrChannels",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UsersChatOrChannel_Users",
                table: "UsersChatOrChannel",
                column: "ChatOrChannelId",
                principalTable: "ChatOrChannels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

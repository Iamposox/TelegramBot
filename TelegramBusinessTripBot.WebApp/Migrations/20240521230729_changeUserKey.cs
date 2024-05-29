using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramBusinessTripBot.WebApp.Migrations
{
    /// <inheritdoc />
    public partial class changeUserKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsersChatOrChannel_ChatOrChannel",
                table: "UsersChatOrChannel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsersChatOrChannel",
                table: "UsersChatOrChannel");

            migrationBuilder.DropIndex(
                name: "IX_UsersChatOrChannel_UsersId",
                table: "UsersChatOrChannel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UsersId",
                table: "UsersChatOrChannel");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Users");

            migrationBuilder.AddColumn<long>(
                name: "UsersUserId",
                table: "UsersChatOrChannel",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "UsersUserHash",
                table: "UsersChatOrChannel",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "UserHash",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsersChatOrChannel",
                table: "UsersChatOrChannel",
                columns: new[] { "ChatOrChannelId", "UsersUserId", "UsersUserHash" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                columns: new[] { "UserId", "UserHash" });

            migrationBuilder.CreateIndex(
                name: "IX_UsersChatOrChannel_UsersUserId_UsersUserHash",
                table: "UsersChatOrChannel",
                columns: new[] { "UsersUserId", "UsersUserHash" });

            migrationBuilder.AddForeignKey(
                name: "FK_UsersChatOrChannel_ChatOrChannel",
                table: "UsersChatOrChannel",
                columns: new[] { "UsersUserId", "UsersUserHash" },
                principalTable: "Users",
                principalColumns: new[] { "UserId", "UserHash" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsersChatOrChannel_ChatOrChannel",
                table: "UsersChatOrChannel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UsersChatOrChannel",
                table: "UsersChatOrChannel");

            migrationBuilder.DropIndex(
                name: "IX_UsersChatOrChannel_UsersUserId_UsersUserHash",
                table: "UsersChatOrChannel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UsersUserId",
                table: "UsersChatOrChannel");

            migrationBuilder.DropColumn(
                name: "UsersUserHash",
                table: "UsersChatOrChannel");

            migrationBuilder.AddColumn<int>(
                name: "UsersId",
                table: "UsersChatOrChannel",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<long>(
                name: "UserHash",
                table: "Users",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "Users",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UsersChatOrChannel",
                table: "UsersChatOrChannel",
                columns: new[] { "ChatOrChannelId", "UsersId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UsersChatOrChannel_UsersId",
                table: "UsersChatOrChannel",
                column: "UsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_UsersChatOrChannel_ChatOrChannel",
                table: "UsersChatOrChannel",
                column: "UsersId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

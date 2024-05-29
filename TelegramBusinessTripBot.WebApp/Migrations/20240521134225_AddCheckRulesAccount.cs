using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TelegramBusinessTripBot.WebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckRulesAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AdminAccount",
                table: "Users",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TrevelAccount",
                table: "Users",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminAccount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TrevelAccount",
                table: "Users");
        }
    }
}

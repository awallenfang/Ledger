using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class updateRankForGlobalSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Messages",
                table: "XpGuildUsers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Cooldown",
                table: "XpGuildSettings",
                type: "bigint",
                nullable: false,
                defaultValue: 60L);

            migrationBuilder.AddColumn<int>(
                name: "ExpMax",
                table: "XpGuildSettings",
                type: "integer",
                nullable: false,
                defaultValue: 25);

            migrationBuilder.AddColumn<int>(
                name: "ExpMin",
                table: "XpGuildSettings",
                type: "integer",
                nullable: false,
                defaultValue: 15);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Messages",
                table: "XpGuildUsers");

            migrationBuilder.DropColumn(
                name: "Cooldown",
                table: "XpGuildSettings");

            migrationBuilder.DropColumn(
                name: "ExpMax",
                table: "XpGuildSettings");

            migrationBuilder.DropColumn(
                name: "ExpMin",
                table: "XpGuildSettings");
        }
    }
}

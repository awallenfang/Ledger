using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class updateGuildUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_XpGuildUserSettings_Guilds_GuildId",
                table: "XpGuildUserSettings");

            migrationBuilder.DropForeignKey(
                name: "FK_XpGuildUserSettings_Users_UserId",
                table: "XpGuildUserSettings");

            migrationBuilder.DropIndex(
                name: "IX_XpGuildUserSettings_GuildId",
                table: "XpGuildUserSettings");

            migrationBuilder.DropColumn(
                name: "GuildId",
                table: "XpGuildUserSettings");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "XpGuildUserSettings",
                newName: "GuildUserId");

            migrationBuilder.RenameIndex(
                name: "IX_XpGuildUserSettings_UserId",
                table: "XpGuildUserSettings",
                newName: "IX_XpGuildUserSettings_GuildUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_XpGuildUserSettings_GuildUsers_GuildUserId",
                table: "XpGuildUserSettings",
                column: "GuildUserId",
                principalTable: "GuildUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_XpGuildUserSettings_GuildUsers_GuildUserId",
                table: "XpGuildUserSettings");

            migrationBuilder.RenameColumn(
                name: "GuildUserId",
                table: "XpGuildUserSettings",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_XpGuildUserSettings_GuildUserId",
                table: "XpGuildUserSettings",
                newName: "IX_XpGuildUserSettings_UserId");

            migrationBuilder.AddColumn<long>(
                name: "GuildId",
                table: "XpGuildUserSettings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_XpGuildUserSettings_GuildId",
                table: "XpGuildUserSettings",
                column: "GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_XpGuildUserSettings_Guilds_GuildId",
                table: "XpGuildUserSettings",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "GuildId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_XpGuildUserSettings_Users_UserId",
                table: "XpGuildUserSettings",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

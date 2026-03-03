using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class leaderboardUsers3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildUser_Guilds_GuildId",
                table: "GuildUser");

            migrationBuilder.DropForeignKey(
                name: "FK_XpGuildUsers_GuildUser_UserId",
                table: "XpGuildUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildUser",
                table: "GuildUser");

            migrationBuilder.RenameTable(
                name: "GuildUser",
                newName: "GuildUsers");

            migrationBuilder.RenameIndex(
                name: "IX_GuildUser_GuildId",
                table: "GuildUsers",
                newName: "IX_GuildUsers_GuildId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildUsers",
                table: "GuildUsers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GuildUsers_Guilds_GuildId",
                table: "GuildUsers",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_XpGuildUsers_GuildUsers_UserId",
                table: "XpGuildUsers",
                column: "UserId",
                principalTable: "GuildUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildUsers_Guilds_GuildId",
                table: "GuildUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_XpGuildUsers_GuildUsers_UserId",
                table: "XpGuildUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GuildUsers",
                table: "GuildUsers");

            migrationBuilder.RenameTable(
                name: "GuildUsers",
                newName: "GuildUser");

            migrationBuilder.RenameIndex(
                name: "IX_GuildUsers_GuildId",
                table: "GuildUser",
                newName: "IX_GuildUser_GuildId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GuildUser",
                table: "GuildUser",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GuildUser_Guilds_GuildId",
                table: "GuildUser",
                column: "GuildId",
                principalTable: "Guilds",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_XpGuildUsers_GuildUser_UserId",
                table: "XpGuildUsers",
                column: "UserId",
                principalTable: "GuildUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

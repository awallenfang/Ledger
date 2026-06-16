using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToXpGuildUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_XpGuildUserSettings_GuildUserId",
                table: "XpGuildUserSettings");

            migrationBuilder.Sql(@"
                DELETE FROM ""XpGuildUserSettings"" 
                WHERE ""Id"" NOT IN (
                    SELECT MAX(""Id"") FROM ""XpGuildUserSettings"" 
                    GROUP BY ""GuildUserId""
                );");

            migrationBuilder.CreateIndex(
                name: "IX_XpGuildUserSettings_GuildUserId",
                table: "XpGuildUserSettings",
                column: "GuildUserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_XpGuildUserSettings_GuildUserId",
                table: "XpGuildUserSettings");

            migrationBuilder.CreateIndex(
                name: "IX_XpGuildUserSettings_GuildUserId",
                table: "XpGuildUserSettings",
                column: "GuildUserId");
        }
    }
}

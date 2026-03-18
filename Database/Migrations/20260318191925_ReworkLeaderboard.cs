using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class ReworkLeaderboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_XpGuildUsers_GuildUsers_UserId",
                table: "XpGuildUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_XpGuildUsers",
                table: "XpGuildUsers");

            migrationBuilder.DropIndex(
                name: "IX_XpGuildUsers_UserId",
                table: "XpGuildUsers");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "GuildUsers");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "XpGuildUsers",
                newName: "GuildUserId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Guilds",
                newName: "GuildId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_XpGuildUsers",
                table: "XpGuildUsers",
                column: "GuildUserId");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "XpGuildUserSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    GuildId = table.Column<long>(type: "bigint", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XpGuildUserSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_XpGuildUserSettings_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_XpGuildUserSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "XpUserSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XpUserSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_XpUserSettings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuildUsers_UserId",
                table: "GuildUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_XpGuildUserSettings_GuildId",
                table: "XpGuildUserSettings",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_XpGuildUserSettings_UserId",
                table: "XpGuildUserSettings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_XpUserSettings_UserId",
                table: "XpUserSettings",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GuildUsers_Users_UserId",
                table: "GuildUsers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_XpGuildUsers_GuildUsers_GuildUserId",
                table: "XpGuildUsers",
                column: "GuildUserId",
                principalTable: "GuildUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildUsers_Users_UserId",
                table: "GuildUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_XpGuildUsers_GuildUsers_GuildUserId",
                table: "XpGuildUsers");

            migrationBuilder.DropTable(
                name: "XpGuildUserSettings");

            migrationBuilder.DropTable(
                name: "XpUserSettings");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_XpGuildUsers",
                table: "XpGuildUsers");

            migrationBuilder.DropIndex(
                name: "IX_GuildUsers_UserId",
                table: "GuildUsers");

            migrationBuilder.RenameColumn(
                name: "GuildUserId",
                table: "XpGuildUsers",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "GuildId",
                table: "Guilds",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "GuildUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_XpGuildUsers",
                table: "XpGuildUsers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_XpGuildUsers_UserId",
                table: "XpGuildUsers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_XpGuildUsers_GuildUsers_UserId",
                table: "XpGuildUsers",
                column: "UserId",
                principalTable: "GuildUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

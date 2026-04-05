using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class updateRankForGlobalSettings2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ExpMin",
                table: "XpGuildSettings",
                type: "integer",
                nullable: false,
                defaultValue: 15,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "ExpMax",
                table: "XpGuildSettings",
                type: "integer",
                nullable: false,
                defaultValue: 25,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<long>(
                name: "Cooldown",
                table: "XpGuildSettings",
                type: "bigint",
                nullable: false,
                defaultValue: 60L,
                oldClrType: typeof(long),
                oldType: "bigint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ExpMin",
                table: "XpGuildSettings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 15);

            migrationBuilder.AlterColumn<int>(
                name: "ExpMax",
                table: "XpGuildSettings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 25);

            migrationBuilder.AlterColumn<long>(
                name: "Cooldown",
                table: "XpGuildSettings",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldDefaultValue: 60L);
        }
    }
}

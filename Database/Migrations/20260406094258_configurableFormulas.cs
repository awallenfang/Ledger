using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Database.Migrations
{
    /// <inheritdoc />
    public partial class configurableFormulas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Formula",
                table: "XpGuildSettings",
                type: "text",
                nullable: false,
                defaultValue: "Polynomial");

            migrationBuilder.AddColumn<double>(
                name: "FormulaBase",
                table: "XpGuildSettings",
                type: "double precision",
                nullable: false,
                defaultValue: 100.0);

            migrationBuilder.AddColumn<double>(
                name: "FormulaExponent",
                table: "XpGuildSettings",
                type: "double precision",
                nullable: false,
                defaultValue: 2.0);

            migrationBuilder.AddColumn<double>(
                name: "FormulaMultiplier",
                table: "XpGuildSettings",
                type: "double precision",
                nullable: false,
                defaultValue: 1.5);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Formula",
                table: "XpGuildSettings");

            migrationBuilder.DropColumn(
                name: "FormulaBase",
                table: "XpGuildSettings");

            migrationBuilder.DropColumn(
                name: "FormulaExponent",
                table: "XpGuildSettings");

            migrationBuilder.DropColumn(
                name: "FormulaMultiplier",
                table: "XpGuildSettings");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aikido.Migrations
{
    /// <inheritdoc />
    public partial class IDKOneMoreFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SchoolClass",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SchoolClass",
                table: "Users");
        }
    }
}

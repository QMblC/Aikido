using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aikido.Migrations
{
    /// <inheritdoc />
    public partial class FixedGroups2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "Groups",
                newName: "CoachId");

            migrationBuilder.AlterColumn<string>(
                name: "AgeGroup",
                table: "Groups",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CoachId",
                table: "Groups",
                newName: "CreatorId");

            migrationBuilder.AlterColumn<int>(
                name: "AgeGroup",
                table: "Groups",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aikido.Migrations
{
    /// <inheritdoc />
    public partial class FixSeminars2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Seminars",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "ResultStatus",
                table: "SeminarMembers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResultStatus",
                table: "SeminarMembers");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Seminars",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}

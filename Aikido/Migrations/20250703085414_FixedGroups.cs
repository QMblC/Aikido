using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aikido.Migrations
{
    /// <inheritdoc />
    public partial class FixedGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Groups",
                newName: "CreatorId");

            migrationBuilder.AddColumn<List<long>>(
                name: "UserIds",
                table: "Groups",
                type: "bigint[]",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserIds",
                table: "Groups");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "Groups",
                newName: "UserId");
        }
    }
}

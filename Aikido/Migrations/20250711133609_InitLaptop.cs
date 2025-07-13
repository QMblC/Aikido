using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Aikido.Migrations
{
    /// <inheritdoc />
    public partial class InitLaptop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "ExclusionDates");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "ExclusionDates");

            migrationBuilder.CreateTable(
                name: "SeminarMembers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    GradeAttestation = table.Column<string>(type: "text", nullable: false),
                    GradeConfirmationStatus = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeminarMembers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Seminars",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: true),
                    PriceSeminarInRubles = table.Column<decimal>(type: "numeric", nullable: false),
                    PriceAnnualFeeRubles = table.Column<decimal>(type: "numeric", nullable: false),
                    PriceBudoPassportRubles = table.Column<decimal>(type: "numeric", nullable: false),
                    PriceAttestation5to2KyuInRubles = table.Column<decimal>(type: "numeric", nullable: false),
                    PriceAttestation1KyuInRubles = table.Column<decimal>(type: "numeric", nullable: false),
                    PriceAttestationBlackBeltInRubles = table.Column<decimal>(type: "numeric", nullable: false),
                    FinalStatementFile = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seminars", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeminarMembers");

            migrationBuilder.DropTable(
                name: "Seminars");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "ExclusionDates",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "ExclusionDates",
                type: "interval",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}

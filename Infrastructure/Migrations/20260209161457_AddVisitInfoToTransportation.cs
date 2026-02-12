using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraceWay.AccountingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitInfoToTransportation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProgramDayNumber",
                table: "triptransportations",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisitName",
                table: "triptransportations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProgramDayNumber",
                table: "triptransportations");

            migrationBuilder.DropColumn(
                name: "VisitName",
                table: "triptransportations");
        }
    }
}

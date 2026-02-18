using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraceWay.AccountingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDriverTipFromAccommodation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "drivertip",
                table: "tripaccommodations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "drivertip",
                table: "tripaccommodations",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}

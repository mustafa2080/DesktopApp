using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraceWay.AccountingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddGuideCostAndDriverTipToTrips : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // إضافة عمود guidecost
            migrationBuilder.AddColumn<decimal>(
                name: "guidecost",
                table: "trips",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            // إضافة عمود drivertip
            migrationBuilder.AddColumn<decimal>(
                name: "drivertip",
                table: "trips",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "guidecost",
                table: "trips");

            migrationBuilder.DropColumn(
                name: "drivertip",
                table: "trips");
        }
    }
}

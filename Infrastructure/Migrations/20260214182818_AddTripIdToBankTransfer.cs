using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraceWay.AccountingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTripIdToBankTransfer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TripId",
                table: "banktransfers",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_banktransfers_TripId",
                table: "banktransfers",
                column: "TripId");

            migrationBuilder.AddForeignKey(
                name: "FK_banktransfers_trips_TripId",
                table: "banktransfers",
                column: "TripId",
                principalTable: "trips",
                principalColumn: "tripid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_banktransfers_trips_TripId",
                table: "banktransfers");

            migrationBuilder.DropIndex(
                name: "IX_banktransfers_TripId",
                table: "banktransfers");

            migrationBuilder.DropColumn(
                name: "TripId",
                table: "banktransfers");
        }
    }
}

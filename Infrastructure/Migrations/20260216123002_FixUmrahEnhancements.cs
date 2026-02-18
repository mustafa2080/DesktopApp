using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraceWay.AccountingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUmrahEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "driverphone",
                table: "triptransportations");

            migrationBuilder.DropColumn(
                name: "suppliername",
                table: "triptransportations");

            migrationBuilder.DropColumn(
                name: "vehiclemodel",
                table: "triptransportations");

            migrationBuilder.RenameColumn(
                name: "supervisorexpenses",
                table: "umrahpackages",
                newName: "supervisorexpensessar");

            migrationBuilder.AddColumn<int>(
                name: "busescount",
                table: "umrahpackages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "buspricessar",
                table: "umrahpackages",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "giftsprice",
                table: "umrahpackages",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "otherexpenses",
                table: "umrahpackages",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "otherexpensesnotes",
                table: "umrahpackages",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "profitmarginegp",
                table: "umrahpackages",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "busescount",
                table: "umrahpackages");

            migrationBuilder.DropColumn(
                name: "buspricessar",
                table: "umrahpackages");

            migrationBuilder.DropColumn(
                name: "giftsprice",
                table: "umrahpackages");

            migrationBuilder.DropColumn(
                name: "otherexpenses",
                table: "umrahpackages");

            migrationBuilder.DropColumn(
                name: "otherexpensesnotes",
                table: "umrahpackages");

            migrationBuilder.DropColumn(
                name: "profitmarginegp",
                table: "umrahpackages");

            migrationBuilder.RenameColumn(
                name: "supervisorexpensessar",
                table: "umrahpackages",
                newName: "supervisorexpenses");

            migrationBuilder.AddColumn<string>(
                name: "driverphone",
                table: "triptransportations",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "suppliername",
                table: "triptransportations",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "vehiclemodel",
                table: "triptransportations",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}

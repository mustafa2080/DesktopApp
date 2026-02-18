using Microsoft.EntityFrameworkCore.Migrations;

namespace GraceWay.AccountingSystem.Infrastructure.Migrations
{
    public partial class AddUmrahEnhancements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // إضافة الأعمدة الجديدة
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

            // تغيير اسم العمود القديم
            migrationBuilder.RenameColumn(
                name: "supervisorexpenses",
                table: "umrahpackages",
                newName: "supervisorexpensessar");
        }

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
        }
    }
}

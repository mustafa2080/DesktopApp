using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraceWay.AccountingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencySupportToCashTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // إضافة أعمدة العملات للـ CashTransaction
            migrationBuilder.AddColumn<string>(
                name: "transactioncurrency",
                table: "CashTransactions",
                type: "TEXT",
                nullable: false,
                defaultValue: "EGP");

            migrationBuilder.AddColumn<decimal>(
                name: "exchangerateused",
                table: "CashTransactions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "originalamount",
                table: "CashTransactions",
                type: "TEXT",
                nullable: true);
            
            // تحديث البيانات الموجودة
            migrationBuilder.Sql(
                "UPDATE CashTransactions SET transactioncurrency = 'EGP' WHERE transactioncurrency IS NULL OR transactioncurrency = ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "transactioncurrency",
                table: "CashTransactions");

            migrationBuilder.DropColumn(
                name: "exchangerateused",
                table: "CashTransactions");

            migrationBuilder.DropColumn(
                name: "originalamount",
                table: "CashTransactions");
        }
    }
}

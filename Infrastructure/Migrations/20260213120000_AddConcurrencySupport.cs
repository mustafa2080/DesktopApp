using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraceWay.AccountingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConcurrencySupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add xmin system column for Optimistic Concurrency Control
            // PostgreSQL has built-in xmin column that tracks row versions
            // We'll use it for concurrency control
            
            // Tables that need concurrency control
            var tables = new[]
            {
                "CashBoxes",
                "CashTransactions",
                "SalesInvoices",
                "PurchaseInvoices",
                "Trips",
                "TripBookings",
                "UmrahPackages",
                "UmrahTrips",
                "FlightBookings",
                "Customers",
                "Suppliers",
                "Accounts",
                "JournalEntries"
            };

            // PostgreSQL's xmin is automatically available
            // No need to add explicit column
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nothing to do - xmin is system column
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraceWay.AccountingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTrackingToCashBoxAndTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatorUserId",
                table: "cashtransactions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "cashtransactions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdaterUserId",
                table: "cashtransactions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatorUserId",
                table: "cashboxes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "cashboxes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdaterUserId",
                table: "cashboxes",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_cashtransactions_CreatorUserId",
                table: "cashtransactions",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_cashtransactions_UpdaterUserId",
                table: "cashtransactions",
                column: "UpdaterUserId");

            migrationBuilder.CreateIndex(
                name: "IX_cashboxes_CreatorUserId",
                table: "cashboxes",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_cashboxes_UpdaterUserId",
                table: "cashboxes",
                column: "UpdaterUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_cashboxes_users_CreatorUserId",
                table: "cashboxes",
                column: "CreatorUserId",
                principalTable: "users",
                principalColumn: "userid");

            migrationBuilder.AddForeignKey(
                name: "FK_cashboxes_users_UpdaterUserId",
                table: "cashboxes",
                column: "UpdaterUserId",
                principalTable: "users",
                principalColumn: "userid");

            migrationBuilder.AddForeignKey(
                name: "FK_cashtransactions_users_CreatorUserId",
                table: "cashtransactions",
                column: "CreatorUserId",
                principalTable: "users",
                principalColumn: "userid");

            migrationBuilder.AddForeignKey(
                name: "FK_cashtransactions_users_UpdaterUserId",
                table: "cashtransactions",
                column: "UpdaterUserId",
                principalTable: "users",
                principalColumn: "userid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_cashboxes_users_CreatorUserId",
                table: "cashboxes");

            migrationBuilder.DropForeignKey(
                name: "FK_cashboxes_users_UpdaterUserId",
                table: "cashboxes");

            migrationBuilder.DropForeignKey(
                name: "FK_cashtransactions_users_CreatorUserId",
                table: "cashtransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_cashtransactions_users_UpdaterUserId",
                table: "cashtransactions");

            migrationBuilder.DropIndex(
                name: "IX_cashtransactions_CreatorUserId",
                table: "cashtransactions");

            migrationBuilder.DropIndex(
                name: "IX_cashtransactions_UpdaterUserId",
                table: "cashtransactions");

            migrationBuilder.DropIndex(
                name: "IX_cashboxes_CreatorUserId",
                table: "cashboxes");

            migrationBuilder.DropIndex(
                name: "IX_cashboxes_UpdaterUserId",
                table: "cashboxes");

            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "cashtransactions");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "cashtransactions");

            migrationBuilder.DropColumn(
                name: "UpdaterUserId",
                table: "cashtransactions");

            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "cashboxes");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "cashboxes");

            migrationBuilder.DropColumn(
                name: "UpdaterUserId",
                table: "cashboxes");
        }
    }
}

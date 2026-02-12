using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraceWay.AccountingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTrackingToUmrahAndFlights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_umrahtrips_users_createdby",
                table: "umrahtrips");

            migrationBuilder.RenameColumn(
                name: "createdby",
                table: "umrahtrips",
                newName: "createdbyuserid");

            migrationBuilder.RenameIndex(
                name: "IX_umrahtrips_createdby",
                table: "umrahtrips",
                newName: "IX_umrahtrips_createdbyuserid");

            // إضافة العمود بدون قيمة افتراضية أولاً (nullable)
            migrationBuilder.AddColumn<int>(
                name: "createdbyuserid",
                table: "flightbookings",
                type: "integer",
                nullable: true);

            // تحديث السجلات الموجودة: استخدام أول admin أو أول user
            migrationBuilder.Sql(@"
                UPDATE flightbookings 
                SET createdbyuserid = (
                    SELECT userid 
                    FROM users 
                    WHERE roleid = (SELECT roleid FROM roles WHERE rolename IN ('admin', 'مدير') LIMIT 1)
                    LIMIT 1
                )
                WHERE createdbyuserid IS NULL;
                
                -- إذا لم يوجد admin، استخدم أول user
                UPDATE flightbookings 
                SET createdbyuserid = (SELECT userid FROM users ORDER BY userid LIMIT 1)
                WHERE createdbyuserid IS NULL;
            ");

            // الآن نجعل العمود required
            migrationBuilder.AlterColumn<int>(
                name: "createdbyuserid",
                table: "flightbookings",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_flightbookings_createdbyuserid",
                table: "flightbookings",
                column: "createdbyuserid");

            migrationBuilder.AddForeignKey(
                name: "FK_flightbookings_users_createdbyuserid",
                table: "flightbookings",
                column: "createdbyuserid",
                principalTable: "users",
                principalColumn: "userid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_umrahtrips_users_createdbyuserid",
                table: "umrahtrips",
                column: "createdbyuserid",
                principalTable: "users",
                principalColumn: "userid",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_flightbookings_users_createdbyuserid",
                table: "flightbookings");

            migrationBuilder.DropForeignKey(
                name: "FK_umrahtrips_users_createdbyuserid",
                table: "umrahtrips");

            migrationBuilder.DropIndex(
                name: "IX_flightbookings_createdbyuserid",
                table: "flightbookings");

            migrationBuilder.DropColumn(
                name: "createdbyuserid",
                table: "flightbookings");

            migrationBuilder.RenameColumn(
                name: "createdbyuserid",
                table: "umrahtrips",
                newName: "createdby");

            migrationBuilder.RenameIndex(
                name: "IX_umrahtrips_createdbyuserid",
                table: "umrahtrips",
                newName: "IX_umrahtrips_createdby");

            migrationBuilder.AddForeignKey(
                name: "FK_umrahtrips_users_createdby",
                table: "umrahtrips",
                column: "createdby",
                principalTable: "users",
                principalColumn: "userid",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

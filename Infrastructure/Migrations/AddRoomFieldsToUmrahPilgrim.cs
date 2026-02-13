using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraceWay.AccountingSystem.Migrations
{
    /// <summary>
    /// إضافة حقول نوع الغرفة ورقم الغرفة المشتركة للمعتمرين
    /// </summary>
    public partial class AddRoomFieldsToUmrahPilgrim : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // إضافة عمود نوع الغرفة (بحروف صغيرة للتوافق مع PostgreSQL)
            migrationBuilder.AddColumn<int>(
                name: "roomtype",
                table: "umrahpilgrims",
                type: "integer",
                nullable: true);

            // إضافة عمود رقم الغرفة المشتركة
            migrationBuilder.AddColumn<string>(
                name: "sharedroomnumber",
                table: "umrahpilgrims",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            // إنشاء فهرس على رقم الغرفة المشتركة لتسريع الاستعلامات
            migrationBuilder.CreateIndex(
                name: "ix_umrahpilgrims_sharedroomnumber",
                table: "umrahpilgrims",
                column: "sharedroomnumber");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // حذف الفهرس
            migrationBuilder.DropIndex(
                name: "ix_umrahpilgrims_sharedroomnumber",
                table: "umrahpilgrims");

            // حذف الأعمدة
            migrationBuilder.DropColumn(
                name: "roomtype",
                table: "umrahpilgrims");

            migrationBuilder.DropColumn(
                name: "sharedroomnumber",
                table: "umrahpilgrims");
        }
    }
}

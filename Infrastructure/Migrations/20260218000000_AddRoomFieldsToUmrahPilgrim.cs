using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GraceWay.AccountingSystem.Infrastructure.Migrations
{
    /// <summary>
    /// إضافة حقول نوع الغرفة ورقم الغرفة المشتركة للمعتمرين
    /// </summary>
    public partial class AddRoomFieldsToUmrahPilgrim : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // إضافة عمود نوع الغرفة بحماية "IF NOT EXISTS" من خلال SQL مباشر
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'umrahpilgrims' AND column_name = 'roomtype'
                    ) THEN
                        ALTER TABLE umrahpilgrims ADD COLUMN roomtype integer;
                    END IF;
                    
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns 
                        WHERE table_name = 'umrahpilgrims' AND column_name = 'sharedroomnumber'
                    ) THEN
                        ALTER TABLE umrahpilgrims ADD COLUMN sharedroomnumber character varying(20);
                    END IF;
                END $$;
            ");

            // إنشاء فهرس بحماية IF NOT EXISTS
            migrationBuilder.Sql(@"
                CREATE INDEX IF NOT EXISTS ix_umrahpilgrims_sharedroomnumber 
                ON umrahpilgrims(sharedroomnumber);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS ix_umrahpilgrims_sharedroomnumber;");
            
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM information_schema.columns 
                               WHERE table_name = 'umrahpilgrims' AND column_name = 'sharedroomnumber') THEN
                        ALTER TABLE umrahpilgrims DROP COLUMN sharedroomnumber;
                    END IF;
                    IF EXISTS (SELECT 1 FROM information_schema.columns 
                               WHERE table_name = 'umrahpilgrims' AND column_name = 'roomtype') THEN
                        ALTER TABLE umrahpilgrims DROP COLUMN roomtype;
                    END IF;
                END $$;
            ");
        }
    }
}

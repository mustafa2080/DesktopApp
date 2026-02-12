using Npgsql;
using BCrypt.Net;

namespace GraceWay.AccountingSystem.Utilities;

/// <summary>
/// أداة لإنشاء مستخدم تجريبي للاختبار
/// Username: admin
/// Password: admin123
/// </summary>
public static class CreateTestUser
{
    public static void CreateDefaultUser(string connectionString)
    {
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();

        // إضافة دور المدير
        var roleQuery = @"
            INSERT INTO roles (rolename, description) 
            VALUES ('مدير النظام', 'صلاحيات كاملة على النظام')
            ON CONFLICT DO NOTHING;
            
            SELECT roleid FROM roles WHERE rolename = 'مدير النظام';
        ";

        int roleId;
        using (var cmd = new NpgsqlCommand(roleQuery, conn))
        {
            var result = cmd.ExecuteScalar();
            roleId = result != null ? Convert.ToInt32(result) : 1;
        }

        // تشفير كلمة المرور
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("admin123");

        // إضافة المستخدم
        var userQuery = @"
            INSERT INTO users (username, passwordhash, fullname, email, phone, roleid, isactive, createdat, updatedat)
            VALUES (@username, @passwordhash, @fullname, @email, @phone, @roleid, @isactive, @createdat, @updatedat)
            ON CONFLICT (username) DO UPDATE 
            SET passwordhash = @passwordhash, fullname = @fullname, email = @email, 
                phone = @phone, roleid = @roleid, isactive = @isactive, updatedat = @updatedat;
        ";

        using (var cmd = new NpgsqlCommand(userQuery, conn))
        {
            cmd.Parameters.AddWithValue("@username", "admin");
            cmd.Parameters.AddWithValue("@passwordhash", passwordHash);
            cmd.Parameters.AddWithValue("@fullname", "المدير العام");
            cmd.Parameters.AddWithValue("@email", "admin@graceway.com");
            cmd.Parameters.AddWithValue("@phone", "01000000000");
            cmd.Parameters.AddWithValue("@roleid", roleId);
            cmd.Parameters.AddWithValue("@isactive", true);
            cmd.Parameters.AddWithValue("@createdat", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@updatedat", DateTime.UtcNow);

            cmd.ExecuteNonQuery();
        }

        Console.WriteLine("✅ تم إنشاء المستخدم التجريبي بنجاح");
        Console.WriteLine("Username: admin");
        Console.WriteLine("Password: admin123");
    }
}

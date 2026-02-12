using System;
using BCrypt.Net;
using Npgsql;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("Setting up admin user...");
        Console.WriteLine();

        string connectionString = "Host=localhost;Port=5432;Database=graceway_accounting;Username=postgres;Password=123456";
        string username = "admin";
        string password = "admin";
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        Console.WriteLine($"Username: {username}");
        Console.WriteLine($"Password: {password}");
        Console.WriteLine($"Hash: {passwordHash}");
        Console.WriteLine();

        try
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();
            Console.WriteLine("Connected to database successfully!");

            // Check if admin user exists
            bool adminExists = false;
            using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM users WHERE username = @username", conn))
            {
                cmd.Parameters.AddWithValue("username", username);
                adminExists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }

            if (adminExists)
            {
                // Update existing admin user password
                using (var cmd = new NpgsqlCommand("UPDATE users SET passwordhash = @passwordHash WHERE username = @username", conn))
                {
                    cmd.Parameters.AddWithValue("username", username);
                    cmd.Parameters.AddWithValue("passwordHash", passwordHash);
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Admin user password updated successfully!");
                }
            }
            else
            {
                // Insert new admin user
                string insertSql = @"
                    INSERT INTO users 
                    (username, passwordhash, fullname, email, roleid, isactive, createdat)
                    VALUES 
                    (@username, @passwordHash, @fullName, @email, @roleId, @isActive, @createdAt)";

                using (var cmd = new NpgsqlCommand(insertSql, conn))
                {
                    cmd.Parameters.AddWithValue("username", username);
                    cmd.Parameters.AddWithValue("passwordHash", passwordHash);
                    cmd.Parameters.AddWithValue("fullName", "المدير العام");
                    cmd.Parameters.AddWithValue("email", "admin@graceway.com");
                    cmd.Parameters.AddWithValue("roleId", 1);
                    cmd.Parameters.AddWithValue("isActive", true);
                    cmd.Parameters.AddWithValue("createdAt", DateTime.Now);

                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Admin user created successfully!");
                }
            }

            // Verify
            using (var cmd = new NpgsqlCommand("SELECT userid, username, fullname, isactive FROM users WHERE username = @username", conn))
            {
                cmd.Parameters.AddWithValue("username", username);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    Console.WriteLine();
                    Console.WriteLine("Verification:");
                    Console.WriteLine($"  UserId: {reader["userid"]}");
                    Console.WriteLine($"  Username: {reader["username"]}");
                    Console.WriteLine($"  FullName: {reader["fullname"]}");
                    Console.WriteLine($"  IsActive: {reader["isactive"]}");
                }
            }

            Console.WriteLine();
            Console.WriteLine("✓ Admin user setup completed!");
            Console.WriteLine();
            Console.WriteLine("You can now login with:");
            Console.WriteLine($"  Username: {username}");
            Console.WriteLine($"  Password: {password}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }

        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        try { Console.ReadKey(); } catch { }
    }
}

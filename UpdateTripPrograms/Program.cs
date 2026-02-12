using System;
using Npgsql;

class Program
{
    static void Main()
    {
        string connString = "Host=localhost;Port=5432;Database=graceway_accounting;Username=postgres;Password=123456;Timeout=60";
        
        Console.WriteLine("=== تحديث بيانات TripPrograms ===\n");
        
        try
        {
            using var connection = new NpgsqlConnection(connString);
            connection.Open();
            Console.WriteLine("✅ تم الاتصال بقاعدة البيانات\n");
            
            // Update DayDate based on StartDate and DayNumber
            Console.WriteLine("--- تحديث التواريخ في البرنامج اليومي ---");
            using (var command = new NpgsqlCommand(
                @"UPDATE tripprograms tp
                  SET daydate = t.startdate + (tp.daynumber - 1) * INTERVAL '1 day'
                  FROM trips t
                  WHERE tp.tripid = t.tripid", 
                connection))
            {
                int updated = command.ExecuteNonQuery();
                Console.WriteLine($"✅ تم تحديث {updated} سجل");
            }
            
            // Verify the update
            Console.WriteLine("\n--- التحقق من التحديث ---");
            using (var command = new NpgsqlCommand(
                @"SELECT tp.tripprogramid, tp.tripid, tp.daynumber, tp.daydate, t.startdate
                  FROM tripprograms tp
                  JOIN trips t ON tp.tripid = t.tripid
                  ORDER BY tp.tripid, tp.daynumber
                  LIMIT 10", 
                connection))
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine($"  Trip {reader[1]}, Day {reader[2]}: DayDate = {reader[3]}, StartDate = {reader[4]}");
                }
            }
            
            Console.WriteLine("\n✅ تم الانتهاء من التحديث");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ خطأ: {ex.Message}");
        }
    }
}

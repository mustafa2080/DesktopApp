# حل مشكلة عمود المستخدم الفاضي في قسم الطيران

## المشكلة:
عمود "المستخدم" ظاهر لكن **فاضي** - مافيش اسم اليوزر بيظهر!

## السبب المحتمل:
الـ `.Include(f => f.CreatedByUser)` مش بيشتغل صح، أو الـ `CreatedByUser` navigation property بترجع `null`.

## الحل السريع (Option 1): استخدام Join مباشر

في ملف `FlightBookingService.cs` السطر 18-80، استبدل الـ `GetAllFlightBookingsAsync` بالكود ده:

```csharp
public async Task<List<FlightBooking>> GetAllFlightBookingsAsync(string? searchTerm = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null)
{
    try
    {
        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
            return new List<FlightBooking>();

        bool isAdmin = await IsAdminAsync(currentUser.UserId);
        
        Console.WriteLine($"🔍 GetAllFlightBookingsAsync: User={currentUser.Username}, IsAdmin={isAdmin}");

        // ✅ استخدام Query Syntax مع Join صريح
        var query = from fb in _context.FlightBookings
                    join u in _context.Users on fb.CreatedByUserId equals u.UserId
                    select new { FlightBooking = fb, User = u };

        // فلترة حسب اليوزر
        if (!isAdmin)
        {
            query = query.Where(x => x.FlightBooking.CreatedByUserId == currentUser.UserId);
            Console.WriteLine($"🔹 Filtering by CreatedByUserId={currentUser.UserId}");
        }
        else
        {
            Console.WriteLine($"✅ Admin mode: Showing all flight bookings");
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(x => 
                x.FlightBooking.BookingNumber.Contains(searchTerm) ||
                x.FlightBooking.ClientName.Contains(searchTerm) ||
                x.FlightBooking.Supplier.Contains(searchTerm) ||
                x.FlightBooking.TicketNumbers.Contains(searchTerm) ||
                x.FlightBooking.MobileNumber.Contains(searchTerm)
            );
        }

        // Apply status filter
        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.FlightBooking.TicketStatus == status);
        }

        // Apply date range filter
        if (fromDate.HasValue)
        {
            query = query.Where(x => x.FlightBooking.TravelDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.FlightBooking.TravelDate <= toDate.Value);
        }

        var results = await query
            .OrderByDescending(x => x.FlightBooking.IssuanceDate)
            .ToListAsync();

        // ✅ ربط الـ User بالـ FlightBooking يدوياً
        var bookings = results.Select(x => {
            x.FlightBooking.CreatedByUser = x.User;
            return x.FlightBooking;
        }).ToList();
        
        Console.WriteLine($"✅ Found {bookings.Count} flight bookings");
        
        return bookings;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error in GetAllFlightBookingsAsync: {ex.Message}");
        throw new Exception($"خطأ في جلب حجوزات الطيران: {ex.Message}", ex);
    }
}
```

## الحل البديل (Option 2): AsSplitQuery

لو Option 1 مانفعش، جرب تضيف `.AsSplitQuery()`:

```csharp
var query = _context.FlightBookings
    .AsSplitQuery()  // ⭐ أضف ده
    .Include(f => f.CreatedByUser)
    .AsQueryable();
```

## الحل الأخير (Option 3): تأكد من الـ Foreign Key في الـ Database

شغل الأمر ده في PostgreSQL:

```sql
SELECT 
    tc.constraint_name, 
    tc.table_name, 
    kcu.column_name, 
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name 
FROM information_schema.table_constraints AS tc 
JOIN information_schema.key_column_usage AS kcu
  ON tc.constraint_name = kcu.constraint_name
JOIN information_schema.constraint_column_usage AS ccu
  ON ccu.constraint_name = tc.constraint_name
WHERE tc.constraint_type = 'FOREIGN KEY' 
  AND tc.table_name='flightbookings';
```

لو مافيش foreign key، أضيفه:

```sql
ALTER TABLE flightbookings 
ADD CONSTRAINT fk_flightbookings_users 
FOREIGN KEY (createdbyuserid) 
REFERENCES users(userid);
```

## الاختبار:
بعد أي تعديل:
1. أعد تشغيل البرنامج
2. افتح قسم الطيران
3. **المفروض عمود "المستخدم" يظهر فيه الأسماء** ✅

لو لسه فاضي، بعتلي screenshot من الـ Console output! 🔍

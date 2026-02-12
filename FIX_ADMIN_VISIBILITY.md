=== FIX SUMMARY ===

## المشكلة:
الأدمن مش بيشوف حجوزات الطيران/الرحلات/العمرة اللي بيعملها المستخدمين التانيين.

## السبب:
الـ `IsAdminAsync` function كانت بتدور على role اسمه "admin" أو "مدير"  
لكن اسم الـ role في الـ database هو: **"Administrator"** ❌

## الحل:
1. ✅ تم إضافة ServiceHelpers.cs مع IsAdminAsync مشتركة
2. ✅ تم تعديل FlightBookingService ليستخدم الـ helper
3. ⏳ يجب تعديل UmrahService (الملف مقفول حالياً)
4. ⏳ يجب إضافة filtering للـ TripService

## الخطوات المطلوبة:

### 1. تعديل UmrahService.cs (آخر 10 أسطر):
```csharp
// استبدل الـ IsAdminAsync function بـ:
private Task<bool> IsAdminAsync(int userId)
{
    return ServiceHelpers.IsAdminAsync(_context, userId);
}
```

### 2. تعديل TripService.cs - إضافة filtering في GetAllTripsAsync:

في السطر 528، استبدل:
```csharp
public async Task<List<Trip>> GetAllTripsAsync(bool includeDetails = false)
{
    var query = _context.Trips.AsQueryable();
```

بـ:
```csharp
public async Task<List<Trip>> GetAllTripsAsync(bool includeDetails = false)
{
    // ✅ الحصول على اليوزر الحالي من AuthService
    var authService = _context.GetService<IAuthService>();
    var currentUser = authService?.CurrentUser;
    
    if (currentUser == null)
        return new List<Trip>();

    // ✅ التحقق من الأدمن
    bool isAdmin = await ServiceHelpers.IsAdminAsync(_context, currentUser.UserId);
    
    Console.WriteLine($"🔍 GetAllTripsAsync: User={currentUser.Username}, IsAdmin={isAdmin}");
    
    var query = _context.Trips.AsQueryable();
    
    // ✅ فلترة حسب اليوزر (إذا لم يكن أدمن)
    if (!isAdmin)
    {
        query = query.Where(t => t.CreatedBy == currentUser.UserId);
        Console.WriteLine($"🔹 Filtering by CreatedBy={currentUser.UserId}");
    }
    else
    {
        Console.WriteLine($"✅ Admin mode: Showing all trips");
    }
```

**ملحوظة مهمة**: الـ TripService مش بياخد IAuthService في الـ constructor!  
لازم نضيفه أول حاجة.

### 3. تعديل TripService constructor:

في أول الـ file، استبدل:
```csharp
public class TripService : ITripService
{
    private readonly AppDbContext _context;
    private readonly IAuditService? _auditService;
    
    public TripService(AppDbContext context, IAuditService? auditService = null)
    {
        _context = context;
        _auditService = auditService;
    }
```

بـ:
```csharp
public class TripService : ITripService
{
    private readonly AppDbContext _context;
    private readonly IAuditService? _auditService;
    private readonly IAuthService _authService;
    
    public TripService(AppDbContext context, IAuthService authService, IAuditService? auditService = null)
    {
        _context = context;
        _authService = authService;
        _auditService = auditService;
    }
```

### 4. في GetAllTripsAsync استخدم _authService مباشرة:
```csharp
public async Task<List<Trip>> GetAllTripsAsync(bool includeDetails = false)
{
    var currentUser = _authService.CurrentUser;
    
    if (currentUser == null)
        return new List<Trip>();

    bool isAdmin = await ServiceHelpers.IsAdminAsync(_context, currentUser.UserId);
    
    Console.WriteLine($"🔍 GetAllTripsAsync: User={currentUser.Username}, IsAdmin={isAdmin}");
    
    var query = _context.Trips.AsQueryable();
    
    if (!isAdmin)
    {
        query = query.Where(t => t.CreatedBy == currentUser.UserId);
        Console.WriteLine($"🔹 Filtering by CreatedBy={currentUser.UserId}");
    }
    else
    {
        Console.WriteLine($"✅ Admin mode: Showing all trips");
    }
    
    // باقي الكود كما هو...
```

## الاختبار:
بعد التعديلات:
1. سجل الدخول كـ admin (username: admin)
2. افتح قسم الطيران - المفروض تشوف الـ 6 حجوزات
3. افتح قسم الرحلات - المفروض تشوف الـ 3 رحلات
4. افتح قسم العمرة - المفروض تشوف كل حزم العمرة

## التأكد من النجاح:
شوف الـ Console Output - المفروض تشوف:
```
✅ Admin mode: Showing all flight bookings
✅ Found 6 flight bookings

✅ Admin mode: Showing all trips  
✅ Found 3 trips

✅ Admin mode: Showing all packages
✅ Found X packages
```

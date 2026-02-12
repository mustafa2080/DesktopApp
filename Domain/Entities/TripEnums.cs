namespace GraceWay.AccountingSystem.Domain.Entities;

/// <summary>
/// أنواع الرحلات
/// </summary>
public enum TripType
{
    Umrah = 1,              // عمرة
    DomesticTourism = 2,    // سياحة داخلية
    InternationalTourism = 3, // سياحة خارجية
    Hajj = 4,               // حج
    Religious = 5,          // رحلات دينية
    Educational = 6         // رحلات تعليمية
}

/// <summary>
/// حالة الرحلة
/// </summary>
public enum TripStatus
{
    Draft = 1,          // مسودة
    Unconfirmed = 2,    // غير مؤكدة
    Confirmed = 3,      // مؤكدة
    InProgress = 4,     // جاري التنفيذ
    Completed = 5,      // مكتملة
    Cancelled = 6       // ملغاة
}

/// <summary>
/// أنواع وسائل النقل
/// </summary>
public enum TransportationType
{
    Bus = 1,        // أتوبيس
    MiniBus = 2,    // ميني باص
    Coaster = 3,    // كوستر
    HiAce = 4,      // هاي أس
    Car = 5,        // ملاكي
    Plane = 6,      // طائرة
    Train = 7       // قطار
}

/// <summary>
/// أنواع الإقامة
/// </summary>
public enum AccommodationType
{
    Hotel = 1,          // فندق
    NileCruise = 2,     // نايل كروز
    Resort = 3,         // منتجع
    Apartment = 4,      // شقة فندقية
    Hostel = 5          // بيت شباب
}

/// <summary>
/// تصنيف الفنادق
/// </summary>
public enum HotelRating
{
    OneStar = 1,
    TwoStars = 2,
    ThreeStars = 3,
    FourStars = 4,
    FiveStars = 5
}

/// <summary>
/// أنواع الغرف
/// </summary>
public enum RoomType
{
    Single = 1,     // فردي
    Double = 2,     // مزدوج
    Triple = 3,     // ثلاثي
    Quad = 4,       // رباعي
    Quint = 5,      // خماسي
    Suite = 6       // جناح
}

/// <summary>
/// مستوى النايل كروز
/// </summary>
public enum CruiseLevel
{
    Standard = 1,       // عادي
    FourStars = 2,      // 4 نجوم
    FiveStars = 3,      // 5 نجوم
    Luxury = 4,         // فاخر
    DeluxeLuxury = 5    // فاخر جداً
}

/// <summary>
/// حالة الدفع
/// </summary>
public enum PaymentStatus
{
    NotPaid = 1,        // لم يدفع
    PartiallyPaid = 2,  // دفع جزئي
    FullyPaid = 3,      // مدفوع بالكامل
    Refunded = 4        // مسترد
}

/// <summary>
/// حالة الحجز
/// </summary>
public enum BookingStatus
{
    Pending = 1,    // معلق
    Confirmed = 2,  // مؤكد
    Cancelled = 3,  // ملغي
    Completed = 4   // مكتمل
}
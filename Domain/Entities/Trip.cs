using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

/// <summary>
/// الرحلة الرئيسية
/// </summary>
public class Trip
{
    [Key]
    public int TripId { get; set; }
    
    /// <summary>
    /// رقم الرحلة (TR-2025-001)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string TripNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// كود الرحلة (رمز فريد)
    /// </summary>
    [MaxLength(50)]
    public string? TripCode { get; set; }
    
    /// <summary>
    /// اسم الرحلة
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string TripName { get; set; } = string.Empty;
    
    /// <summary>
    /// الوجهة
    /// </summary>
    [Required]
    [MaxLength(200)]
    [Column("destination")]
    public string Destination { get; set; } = string.Empty;
    
    /// <summary>
    /// نوع الرحلة
    /// </summary>
    [Column("triptype")]
    public TripType TripType { get; set; }
    
    /// <summary>
    /// وصف الرحلة
    /// </summary>
    [Column("description")]
    public string? Description { get; set; }
    
    /// <summary>
    /// تاريخ بدء الرحلة
    /// </summary>
    [Column("startdate")]
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// تاريخ انتهاء الرحلة
    /// </summary>
    [Column("enddate")]
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// عدد الأيام (محسوب تلقائياً)
    /// </summary>
    [NotMapped]
    public int TotalDays => (EndDate - StartDate).Days + 1;
    
    // ══════════════════════════════════════
    // الأعداد والسعة
    // ══════════════════════════════════════
    
    /// <summary>
    /// الطاقة الاستيعابية للرحلة
    /// </summary>
    [Column("totalcapacity")]
    public int TotalCapacity { get; set; }
    
    /// <summary>
    /// عدد البالغين (Adult)
    /// </summary>
    [Column("adultcount")]
    public int AdultCount { get; set; } = 0;
    
    /// <summary>
    /// عدد الأطفال (Child)
    /// </summary>
    [Column("childcount")]
    public int ChildCount { get; set; } = 0;
    
    /// <summary>
    /// تكلفة المرشد السياحي
    /// </summary>
    [Column("guidecost")]
    public decimal GuideCost { get; set; } = 0;
    
    /// <summary>
    /// إكرامية السائق
    /// </summary>
    [Column("drivertip")]
    public decimal DriverTip { get; set; } = 0;
    
    /// <summary>
    /// عدد الأماكن المحجوزة
    /// </summary>
    [Column("bookedseats")]
    public int BookedSeats { get; set; } = 0;
    
    /// <summary>
    /// الأماكن المتاحة (محسوب تلقائياً)
    /// </summary>
    [NotMapped]
    public int AvailableSeats => TotalCapacity - BookedSeats;
    
    /// <summary>
    /// نسبة الإشغال %
    /// </summary>
    [NotMapped]
    public decimal OccupancyRate => TotalCapacity > 0 ? 
        ((decimal)BookedSeats / TotalCapacity * 100) : 0;
    
    // ══════════════════════════════════════
    // المالية والأسعار
    // ══════════════════════════════════════
    
    /// <summary>
    /// سعر البيع للفرد الواحد (بالعملة المختارة)
    /// </summary>
    [Column("sellingpriceperperson")]
    public decimal SellingPricePerPerson { get; set; }
    
    /// <summary>
    /// سعر البيع للفرد بالجنيه المصري (محسوب)
    /// </summary>
    [NotMapped]
    public decimal SellingPricePerPersonInEGP => SellingPricePerPerson * ExchangeRate;
    
    /// <summary>
    /// إجمالي الإيرادات المتوقعة بناءً على الأماكن المحجوزة (بالجنيه المصري)
    /// </summary>
    [NotMapped]
    public decimal ExpectedRevenue => BookedSeats * SellingPricePerPersonInEGP;
    
    /// <summary>
    /// إجمالي الإيرادات المتوقعة بناءً على الطاقة الكاملة (بالجنيه المصري)
    /// </summary>
    [NotMapped]
    public decimal TotalRevenue => TotalCapacity * SellingPricePerPersonInEGP;
    
    /// <summary>
    /// التكلفة الإجمالية (تحسب من جميع المصاريف)
    /// </summary>
    [Column("totalcost")]
    public decimal TotalCost { get; set; }
    
    /// <summary>
    /// صافي الربح المتوقع (بناءً على الطاقة الكاملة)
    /// </summary>
    [NotMapped]
    public decimal NetProfit => TotalRevenue - TotalCost;
    
    /// <summary>
    /// صافي الربح الفعلي (بناءً على الأماكن المحجوزة)
    /// </summary>
    [NotMapped]
    public decimal ActualProfit => ExpectedRevenue - TotalCost;
    
    /// <summary>
    /// هامش الربح % (محسوب من الربح الفعلي)
    /// </summary>
    [NotMapped]
    public decimal ProfitMargin => TotalRevenue > 0 ? 
        (NetProfit / TotalRevenue * 100) : 0;
    
    /// <summary>
    /// هامش الربح المطلوب % (يستخدم في حساب السعر)
    /// </summary>
    [Column("profitmarginpercent")]
    public decimal ProfitMarginPercent { get; set; } = 20m; // القيمة الافتراضية 20%
    
    // ══════════════════════════════════════
    // العملة
    // ══════════════════════════════════════
    
    /// <summary>
    /// معرف العملة
    /// </summary>
    [Column("currencyid")]
    public int CurrencyId { get; set; }
    
    /// <summary>
    /// سعر الصرف
    /// </summary>
    [Column("exchangerate")]
    public decimal ExchangeRate { get; set; } = 1.0m;
    
    // ══════════════════════════════════════
    // الحالة والنشر
    // ══════════════════════════════════════
    
    /// <summary>
    /// حالة الرحلة
    /// </summary>
    [Column("status")]
    public TripStatus Status { get; set; } = TripStatus.Draft;
    
    /// <summary>
    /// هل الرحلة مقفولة للتعديل من قسم الرحلات؟
    /// </summary>
    [Column("islockedfortrips")]
    public bool IsLockedForTrips { get; set; } = false;
    
    /// <summary>
    /// هل الرحلة منشورة ومتاحة للحجز؟
    /// </summary>
    [Column("ispublished")]
    public bool IsPublished { get; set; } = false;
    
    /// <summary>
    /// هل الرحلة نشطة؟
    /// </summary>
    [Column("isactive")]
    public bool IsActive { get; set; } = true;
    
    // ══════════════════════════════════════
    // التتبع والصلاحيات
    // ══════════════════════════════════════
    
    /// <summary>
    /// معرف منشئ الرحلة
    /// </summary>
    [Column("createdby")]
    public int CreatedBy { get; set; }
    
    /// <summary>
    /// تاريخ الإنشاء
    /// </summary>
    [Column("createdat", TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// معرف آخر من قام بالتعديل
    /// </summary>
    [Column("updatedby")]
    public int? UpdatedBy { get; set; }
    
    /// <summary>
    /// تاريخ آخر تعديل
    /// </summary>
    [Column("updatedat", TypeName = "timestamp with time zone")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // ══════════════════════════════════════
    // العلاقات (Navigation Properties)
    // ══════════════════════════════════════
    
    public Currency Currency { get; set; } = null!;
    public User Creator { get; set; } = null!;
    public User? Updater { get; set; }
    
    /// <summary>
    /// برنامج الرحلة اليومي
    /// </summary>
    public ICollection<TripProgram> Programs { get; set; } = new List<TripProgram>();
    
    /// <summary>
    /// وسائل النقل
    /// </summary>
    public ICollection<TripTransportation> Transportation { get; set; } = new List<TripTransportation>();
    
    /// <summary>
    /// أماكن الإقامة
    /// </summary>
    public ICollection<TripAccommodation> Accommodations { get; set; } = new List<TripAccommodation>();
    
    /// <summary>
    /// المرشدون السياحيون
    /// </summary>
    public ICollection<TripGuide> Guides { get; set; } = new List<TripGuide>();
    
    /// <summary>
    /// الرحلات الاختيارية
    /// </summary>
    public ICollection<TripOptionalTour> OptionalTours { get; set; } = new List<TripOptionalTour>();
    
    /// <summary>
    /// المصاريف الأخرى
    /// </summary>
    public ICollection<TripExpense> Expenses { get; set; } = new List<TripExpense>();
    
    /// <summary>
    /// حجوزات الرحلة
    /// </summary>
    public ICollection<TripBooking> Bookings { get; set; } = new List<TripBooking>();
    
    /// <summary>
    /// موردو الرحلة (فنادق، نقل، طيران، إلخ)
    /// </summary>
    public ICollection<TripSupplier> TripSuppliers { get; set; } = new List<TripSupplier>();
    
    /// <summary>
    /// الحجوزات العامة المرتبطة بالرحلة (من نظام الحجوزات)
    /// </summary>
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    
    // ══════════════════════════════════════
    // دوال مساعدة
    // ══════════════════════════════════════
    
    /// <summary>
    /// حساب التكلفة الإجمالية من جميع المصادر
    /// </summary>
    public void CalculateTotalCost()
    {
        // ════════════════════════════════════════════════════════════════
        // 📊 حساب تكلفة البرنامج اليومي (المزارات + سعر المرشد فقط)
        // ════════════════════════════════════════════════════════════════
        decimal programCost = Programs.Sum(p => 
            (p.VisitsCost * p.ParticipantsCount) +  // تكلفة المزارات
            p.GuideCost                              // سعر المرشد في اليوم
        );
        
        // ════════════════════════════════════════════════════════════════
        // 🚗 حساب تكلفة النقل (المواصلات لكل مزار)
        // ════════════════════════════════════════════════════════════════
        // التكلفة الإجمالية = (عدد المركبات × التكلفة للمركبة) + إكرامية التور ليدر + إكرامية السواق
        decimal transportationCost = Transportation.Sum(t => t.TotalCost);
        
        // ════════════════════════════════════════════════════════════════
        // 🎯 حساب تكلفة الرحلات الاختيارية (التكلفة فقط، مش الإيرادات)
        // ════════════════════════════════════════════════════════════════
        decimal optionalToursCost = OptionalTours.Sum(o => o.TotalCost);
        
        // ════════════════════════════════════════════════════════════════
        // 💰 إجمالي التكلفة
        // ════════════════════════════════════════════════════════════════
        TotalCost = 
            programCost +                           // ✅ البرنامج اليومي (المزارات + المرشد)
            transportationCost +                    // ✅ النقل (المواصلات + الإكراميات)
            Accommodations.Sum(a => a.TotalCost) +  // ✅ الإقامة (شاملة إكرامية المرشد والسواق)
            Guides.Sum(g => g.TotalCost) +          // ✅ المرشدين الرئيسيين
            optionalToursCost +                     // ✅ الرحلات الاختيارية
            Expenses.Sum(e => e.Amount);            // ✅ المصاريف الأخرى
        
        // ملاحظة: 
        // - تكلفة النقل دلوقتي منفصلة ومرتبطة بكل مزار
        // - كل مزار له سعر مواصلة خاص بيه
        // - الإكراميات (التور ليدر والسواق) محسوبة في تكلفة النقل
    }
    
    /// <summary>
    /// هل الرحلة ممتلئة؟
    /// </summary>
    public bool IsFull() => BookedSeats >= TotalCapacity;
    
    /// <summary>
    /// هل يمكن الحجز؟
    /// </summary>
    public bool CanBook() => IsPublished && IsActive && !IsFull() && 
                             (Status == TripStatus.Confirmed || Status == TripStatus.InProgress) && 
                             StartDate > DateTime.UtcNow;
    
    /// <summary>
    /// هل يمكن التعديل من قسم الرحلات؟
    /// </summary>
    public bool CanEditFromTrips() => !IsLockedForTrips;
    
    /// <summary>
    /// قفل الرحلة للتعديل من قسم الرحلات (يستخدم من قسم الحجوزات)
    /// </summary>
    public void LockForTrips() => IsLockedForTrips = true;
    
    /// <summary>
    /// فتح الرحلة للتعديل من قسم الرحلات (يستخدم من قسم الحجوزات)
    /// </summary>
    public void UnlockForTrips() => IsLockedForTrips = false;
}

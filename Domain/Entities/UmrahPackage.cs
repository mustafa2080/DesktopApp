using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

/// <summary>
/// حزمة العمرة
/// </summary>
[Table("umrahpackages")]
public class UmrahPackage
{
    [Key]
    public int UmrahPackageId { get; set; }
    
    /// <summary>
    /// رقم الحزمة (UMR-2025-001)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string PackageNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// التاريخ
    /// </summary>
    [Required]
    public DateTime Date { get; set; }
    
    /// <summary>
    /// اسم الرحلة
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string TripName { get; set; } = string.Empty;
    
    /// <summary>
    /// عدد الأفراد
    /// </summary>
    [Required]
    public int NumberOfPersons { get; set; }
    
    /// <summary>
    /// نوع الغرفة
    /// </summary>
    [Required]
    [Column("roomtype")]
    public RoomType RoomType { get; set; }
    
    // ══════════════════════════════════════
    // فندق مكة
    // ══════════════════════════════════════
    
    /// <summary>
    /// فندق مكة
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string MakkahHotel { get; set; } = string.Empty;
    
    /// <summary>
    /// عدد ليالي مكة
    /// </summary>
    [Required]
    public int MakkahNights { get; set; }
    
    // ══════════════════════════════════════
    // فندق المدينة
    // ══════════════════════════════════════
    
    /// <summary>
    /// فندق المدينة
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string MadinahHotel { get; set; } = string.Empty;
    
    /// <summary>
    /// عدد ليالي المدينة
    /// </summary>
    [Required]
    public int MadinahNights { get; set; }
    
    // ══════════════════════════════════════
    // وسيلة السفر
    // ══════════════════════════════════════
    
    /// <summary>
    /// وسيلة السفر
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string TransportMethod { get; set; } = string.Empty;
    
    // ══════════════════════════════════════
    // الأسعار والتكاليف
    // ══════════════════════════════════════
    
    /// <summary>
    /// سعر البيع (بالجنيه المصري)
    /// </summary>
    [Required]
    public decimal SellingPrice { get; set; }
    
    /// <summary>
    /// سعر التأشيرة (بالريال السعودي)
    /// </summary>
    [Required]
    public decimal VisaPriceSAR { get; set; }
    
    /// <summary>
    /// سعر صرف الريال السعودي
    /// </summary>
    [Required]
    public decimal SARExchangeRate { get; set; }
    
    /// <summary>
    /// سعر التأشيرة بالجنيه المصري (محسوب)
    /// </summary>
    public decimal VisaPriceEGP => VisaPriceSAR * SARExchangeRate;
    
    /// <summary>
    /// توتال الإقامة (بالجنيه المصري)
    /// </summary>
    [Required]
    public decimal AccommodationTotal { get; set; }
    
    /// <summary>
    /// سعر الباركود (بالجنيه المصري)
    /// </summary>
    public decimal BarcodePrice { get; set; }
    
    /// <summary>
    /// سعر الطيران (بالجنيه المصري)
    /// </summary>
    public decimal FlightPrice { get; set; }
    
    /// <summary>
    /// سعر القطار السريع (بالريال السعودي)
    /// </summary>
    public decimal FastTrainPriceSAR { get; set; }
    
    /// <summary>
    /// سعر القطار السريع بالجنيه المصري (محسوب)
    /// </summary>
    public decimal FastTrainPriceEGP => FastTrainPriceSAR * SARExchangeRate;
    
    // ══════════════════════════════════════
    // الوسيط والمشرف
    // ══════════════════════════════════════
    
    /// <summary>
    /// اسم الوسيط
    /// </summary>
    [MaxLength(200)]
    public string? BrokerName { get; set; }
    
    /// <summary>
    /// اسم المشرف
    /// </summary>
    [MaxLength(200)]
    public string? SupervisorName { get; set; }
    
    /// <summary>
    /// العمولة (بالجنيه المصري)
    /// </summary>
    public decimal Commission { get; set; }
    
    /// <summary>
    /// مصاريف المشرف (بالجنيه المصري)
    /// </summary>
    public decimal SupervisorExpenses { get; set; }
    
    // ══════════════════════════════════════
    // الحسابات المالية
    // ══════════════════════════════════════
    
    /// <summary>
    /// إجمالي التكاليف (محسوب)
    /// </summary>
    public decimal TotalCosts => 
        VisaPriceEGP + 
        AccommodationTotal + 
        BarcodePrice + 
        FlightPrice + 
        FastTrainPriceEGP + 
        Commission + 
        SupervisorExpenses;
    
    /// <summary>
    /// إجمالي الإيرادات (محسوب)
    /// </summary>
    public decimal TotalRevenue => SellingPrice * NumberOfPersons;
    
    /// <summary>
    /// صافي الربح (محسوب)
    /// </summary>
    public decimal NetProfit => TotalRevenue - (TotalCosts * NumberOfPersons);
    
    /// <summary>
    /// هامش الربح %
    /// </summary>
    public decimal ProfitMargin => TotalRevenue > 0 ? 
        (NetProfit / TotalRevenue * 100) : 0;
    
    /// <summary>
    /// صافي الربح للفرد (محسوب)
    /// </summary>
    public decimal NetProfitPerPerson => NumberOfPersons > 0 ? 
        NetProfit / NumberOfPersons : 0;
    
    // ══════════════════════════════════════
    // الحالة والتتبع
    // ══════════════════════════════════════
    
    /// <summary>
    /// الحالة
    /// </summary>
    public PackageStatus Status { get; set; } = PackageStatus.Draft;
    
    /// <summary>
    /// هل الحزمة نشطة؟
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// ملاحظات
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// معرف منشئ الحزمة
    /// </summary>
    public int CreatedBy { get; set; }
    
    /// <summary>
    /// تاريخ الإنشاء
    /// </summary>
    [Column(TypeName = "timestamp with time zone")]

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// معرف آخر من قام بالتعديل
    /// </summary>
    public int? UpdatedBy { get; set; }
    
    /// <summary>
    /// تاريخ آخر تعديل
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // ══════════════════════════════════════
    // العلاقات
    // ══════════════════════════════════════
    
    [ForeignKey("CreatedBy")]
    public User Creator { get; set; } = null!;
    
    [ForeignKey("UpdatedBy")]
    public User? Updater { get; set; }
    
    /// <summary>
    /// قائمة المعتمرين
    /// </summary>
    public ICollection<UmrahPilgrim> Pilgrims { get; set; } = new List<UmrahPilgrim>();
    
    /// <summary>
    /// قائمة الدفعات
    /// </summary>
    public ICollection<UmrahPayment> Payments { get; set; } = new List<UmrahPayment>();
    
    // ══════════════════════════════════════
    // دوال مساعدة
    // ══════════════════════════════════════
    
    /// <summary>
    /// عرض نوع الغرفة
    /// </summary>
    public string GetRoomTypeDisplay()
    {
        return RoomType switch
        {
            RoomType.Single => "مفردة",
            RoomType.Double => "ثنائي",
            RoomType.Triple => "ثلاثي",
            RoomType.Quad => "رباعي",
            RoomType.Quint => "خماسي",
            _ => "غير محدد"
        };
    }
    
    /// <summary>
    /// عرض الحالة
    /// </summary>
    public string GetStatusDisplay()
    {
        return Status switch
        {
            PackageStatus.Draft => "مسودة",
            PackageStatus.Confirmed => "مؤكد",
            PackageStatus.InProgress => "قيد التنفيذ",
            PackageStatus.Completed => "مكتمل",
            PackageStatus.Cancelled => "ملغي",
            _ => "غير محدد"
        };
    }
    
    /// <summary>
    /// إجمالي الليالي
    /// </summary>
    public int TotalNights => MakkahNights + MadinahNights;
}

/// <summary>
/// حالة الحزمة
/// </summary>
public enum PackageStatus
{
    Draft = 1,          // مسودة
    Confirmed = 2,      // مؤكد
    InProgress = 3,     // قيد التنفيذ
    Completed = 4,      // مكتمل
    Cancelled = 5       // ملغي
}

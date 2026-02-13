using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

/// <summary>
/// رحلة العمرة (الرحلة الأساسية)
/// </summary>
public class UmrahTrip
{
    [Key]
    public int UmrahTripId { get; set; }
    
    /// <summary>
    /// رقم الرحلة (UMR-2025-001)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string TripNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// اسم الرحلة (مثال: رحلة رمضان 2025)
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string TripName { get; set; } = string.Empty;
    
    /// <summary>
    /// تاريخ بداية الرحلة
    /// </summary>
    [Required]
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// تاريخ نهاية الرحلة
    /// </summary>
    [Required]
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// عدد المعتمرين في الرحلة
    /// </summary>
    [Required]
    public int TotalPilgrims { get; set; }
    
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
    /// وسيلة السفر (طيران، باص، قطار)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string TransportMethod { get; set; } = string.Empty;
    
    // ══════════════════════════════════════
    // الأسعار والتكاليف (للفرد الواحد)
    // ══════════════════════════════════════
    
    /// <summary>
    /// سعر الحزمة للفرد (بالجنيه المصري)
    /// </summary>
    [Required]
    public decimal PricePerPerson { get; set; }
    
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
    /// سعر التأشيرة بالجنيه (محسوب)
    /// </summary>
    public decimal VisaPriceEGP => VisaPriceSAR * SARExchangeRate;
    
    /// <summary>
    /// تكلفة الإقامة للفرد (بالجنيه)
    /// </summary>
    public decimal AccommodationCost { get; set; }
    
    /// <summary>
    /// سعر الباركود للفرد (بالجنيه)
    /// </summary>
    public decimal BarcodeCost { get; set; }
    
    /// <summary>
    /// سعر الطيران للفرد (بالجنيه)
    /// </summary>
    public decimal FlightCost { get; set; }
    
    /// <summary>
    /// سعر القطار السريع للفرد (بالريال السعودي)
    /// </summary>
    public decimal FastTrainPriceSAR { get; set; }
    
    /// <summary>
    /// سعر القطار السريع بالجنيه (محسوب)
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
    /// عمولة الوسيط (للفرد)
    /// </summary>
    public decimal BrokerCommission { get; set; }
    
    /// <summary>
    /// مصاريف المشرف (إجمالي للرحلة)
    /// </summary>
    public decimal SupervisorExpenses { get; set; }
    
    // ══════════════════════════════════════
    // الحسابات المالية المحسوبة
    // ══════════════════════════════════════
    
    /// <summary>
    /// إجمالي التكلفة للفرد (محسوب)
    /// </summary>
    public decimal TotalCostPerPerson => 
        VisaPriceEGP + 
        AccommodationCost + 
        BarcodeCost + 
        FlightCost + 
        FastTrainPriceEGP + 
        BrokerCommission;
    
    /// <summary>
    /// إجمالي تكلفة الرحلة (محسوب)
    /// </summary>
    public decimal TotalTripCost => (TotalCostPerPerson * TotalPilgrims) + SupervisorExpenses;
    
    /// <summary>
    /// إجمالي إيرادات الرحلة المتوقعة (محسوب)
    /// </summary>
    public decimal ExpectedRevenue => PricePerPerson * TotalPilgrims;
    
    /// <summary>
    /// صافي الربح المتوقع (محسوب)
    /// </summary>
    public decimal ExpectedProfit => ExpectedRevenue - TotalTripCost;
    
    // ══════════════════════════════════════
    // الحالة والتتبع
    // ══════════════════════════════════════
    
    /// <summary>
    /// حالة الرحلة
    /// </summary>
    public UmrahTripStatus Status { get; set; } = UmrahTripStatus.Draft;
    
    /// <summary>
    /// هل الرحلة نشطة؟
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// ملاحظات
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// معرف منشئ الرحلة (اليوزر اللي عمل الملف)
    /// </summary>
    [Required]
    public int CreatedByUserId { get; set; }
    
    /// <summary>
    /// تاريخ الإنشاء
    /// </summary>
    [Required]
    [Column(TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// معرف آخر من قام بالتعديل
    /// </summary>
    public int? UpdatedBy { get; set; }
    
    /// <summary>
    /// تاريخ آخر تعديل
    /// </summary>
    [Column("UpdatedAt", TypeName = "timestamp with time zone")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // ══════════════════════════════════════
    // العلاقات
    // ══════════════════════════════════════
    
    /// <summary>
    /// قائمة المعتمرين في هذه الرحلة
    /// </summary>
    public ICollection<UmrahPilgrim> Pilgrims { get; set; } = new List<UmrahPilgrim>();
    
    /// <summary>
    /// اليوزر اللي أنشأ الرحلة
    /// </summary>
    [ForeignKey("CreatedByUserId")]
    public User CreatedByUser { get; set; } = null!;
    
    [ForeignKey("UpdatedBy")]
    public User? Updater { get; set; }
}

/// <summary>
/// حالة رحلة العمرة
/// </summary>
public enum UmrahTripStatus
{
    Draft = 1,          // مسودة
    Confirmed = 2,      // مؤكدة
    InProgress = 3,     // جارية
    Completed = 4,      // مكتملة
    Cancelled = 5       // ملغاة
}

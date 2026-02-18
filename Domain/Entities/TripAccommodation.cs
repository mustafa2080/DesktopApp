using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

/// <summary>
/// أماكن الإقامة في الرحلة
/// </summary>
public class TripAccommodation
{
    [Key]
    public int TripAccommodationId { get; set; }
    
    /// <summary>
    /// معرف الرحلة
    /// </summary>
    public int TripId { get; set; }
    
    /// <summary>
    /// نوع الإقامة (فندق، نايل كروز، منتجع، إلخ)
    /// </summary>
    public AccommodationType Type { get; set; }
    
    /// <summary>
    /// اسم الفندق/الكروز/المنتجع
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string HotelName { get; set; } = string.Empty;
    
    /// <summary>
    /// تصنيف الفندق (نجوم)
    /// </summary>
    public HotelRating? Rating { get; set; }
    
    /// <summary>
    /// مستوى النايل كروز (للكروز فقط)
    /// </summary>
    [Column("CruiseLevel")]
    public CruiseLevel? CruiseLevel { get; set; }
    
    /// <summary>
    /// الموقع/المدينة
    /// </summary>
    [MaxLength(200)]
    public string? Location { get; set; }
    
    /// <summary>
    /// عدد الغرف
    /// </summary>
    public int NumberOfRooms { get; set; }
    
    /// <summary>
    /// نوع الغرفة
    /// </summary>
    [Column("roomtype")]
    public RoomType RoomType { get; set; }
    
    /// <summary>
    /// عدد الليالي
    /// </summary>
    public int NumberOfNights { get; set; }
    
    /// <summary>
    /// عدد الأفراد المستخدمين لهذه الإقامة
    /// </summary>
    [Column("participantscount")]
    public int ParticipantsCount { get; set; }
    
    /// <summary>
    /// تاريخ تسجيل الدخول
    /// </summary>
    public DateTime CheckInDate { get; set; }
    
    /// <summary>
    /// تاريخ تسجيل الخروج
    /// </summary>
    public DateTime CheckOutDate { get; set; }
    
    /// <summary>
    /// التكلفة للغرفة الواحدة في الليلة
    /// </summary>
    public decimal CostPerRoomPerNight { get; set; }
    
    /// <summary>
    /// العملة المستخدمة (جنيه مصري، دولار، جنيه استرليني)
    /// </summary>
    [MaxLength(50)]
    [Column("currency")]
    public string? Currency { get; set; }
    
    /// <summary>
    /// سعر الصرف للتحويل للجنيه المصري
    /// </summary>
    [Column("exchangerate")]
    public decimal ExchangeRate { get; set; } = 1.0m;
    
    /// <summary>
    /// تكلفة إقامة المرشد السياحي
    /// </summary>
    [Column("guidecost")]
    public decimal GuideCost { get; set; }
    
    /// <summary>
    /// التكلفة الإجمالية (غرف + مرشد)
    /// </summary>
    public decimal TotalCost => (NumberOfRooms * NumberOfNights * CostPerRoomPerNight) + GuideCost;
    
    /// <summary>
    /// نظام الوجبات (إفطار فقط، نصف إقامة، إقامة كاملة)
    /// </summary>
    [MaxLength(100)]
    public string? MealPlan { get; set; }
    
    /// <summary>
    /// ملاحظات
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// معرف المورد (الفندق/الكروز/المنتجع)
    /// </summary>
    public int? SupplierId { get; set; }
    
    /// <summary>
    /// معرف فاتورة الشراء المرتبطة بالإقامة
    /// </summary>
    public int? PurchaseInvoiceId { get; set; }
    
    // العلاقات
    public Trip Trip { get; set; } = null!;
    public Supplier? Supplier { get; set; }
    public PurchaseInvoice? PurchaseInvoice { get; set; }
}
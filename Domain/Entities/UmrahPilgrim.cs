using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

/// <summary>
/// المعتمر (كل فرد في الرحلة)
/// </summary>
[Table("umrahpilgrims")]
public class UmrahPilgrim
{
    [Key]
    public int UmrahPilgrimId { get; set; }
    
    /// <summary>
    /// رقم المعتمر (UMP-2025-001)
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string PilgrimNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// الحزمة التابع لها
    /// </summary>
    [Required]
    public int UmrahPackageId { get; set; }
    
    /// <summary>
    /// اسم المعتمر بالكامل
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;
    
    /// <summary>
    /// رقم الهاتف
    /// </summary>
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// رقم الهوية/الجواز
    /// </summary>
    [MaxLength(50)]
    public string? IdentityNumber { get; set; }
    
    /// <summary>
    /// العمر
    /// </summary>
    public int? Age { get; set; }
    
    /// <summary>
    /// نوع الغرفة الخاص بالمعتمر
    /// </summary>
    [Column("roomtype")]
    public RoomType? RoomType { get; set; }
    
    /// <summary>
    /// رقم الغرفة المشتركة (لربط المعتمرين في نفس الغرفة)
    /// مثال: "R001" - جميع المعتمرين بنفس رقم الغرفة يشتركون في غرفة واحدة
    /// </summary>
    [MaxLength(20)]
    public string? SharedRoomNumber { get; set; }
    
    // ══════════════════════════════════════
    // المدفوعات
    // ══════════════════════════════════════
    
    /// <summary>
    /// سعر الحزمة للمعتمر (قد يختلف عن السعر العام)
    /// </summary>
    [Required]
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// المبلغ المدفوع
    /// </summary>
    public decimal PaidAmount { get; set; }
    
    /// <summary>
    /// المبلغ المتبقي (محسوب)
    /// </summary>
    [NotMapped]
    public decimal RemainingAmount => TotalAmount - PaidAmount;
    
    /// <summary>
    /// حالة الدفع (محسوبة)
    /// </summary>
    [NotMapped]
    public UmrahPaymentStatus PaymentStatus
    {
        get
        {
            if (PaidAmount == 0) return UmrahPaymentStatus.NotPaid;
            if (PaidAmount >= TotalAmount) return UmrahPaymentStatus.FullyPaid;
            return UmrahPaymentStatus.PartiallyPaid;
        }
    }
    
    /// <summary>
    /// نسبة الدفع % (محسوبة)
    /// </summary>
    [NotMapped]
    public decimal PaymentPercentage => TotalAmount > 0 ? (PaidAmount / TotalAmount * 100) : 0;
    
    // ══════════════════════════════════════
    // الحالة
    // ══════════════════════════════════════
    
    /// <summary>
    /// حالة المعتمر
    /// </summary>
    public PilgrimStatus Status { get; set; } = PilgrimStatus.Registered;
    
    /// <summary>
    /// ملاحظات خاصة بالمعتمر
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// تاريخ التسجيل
    /// </summary>
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// معرف من قام بالتسجيل
    /// </summary>
    public int CreatedBy { get; set; }
    
    /// <summary>
    /// تاريخ آخر تعديل
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // ══════════════════════════════════════
    // العلاقات
    // ══════════════════════════════════════
    
    /// <summary>
    /// الحزمة التابع لها
    /// </summary>
    [ForeignKey(nameof(UmrahPackageId))]
    public UmrahPackage Package { get; set; } = null!;
    
    /// <summary>
    /// قائمة الدفعات
    /// </summary>
    public ICollection<UmrahPayment> Payments { get; set; } = new List<UmrahPayment>();
    
    public User Creator { get; set; } = null!;
}

/// <summary>
/// حالة الدفع للعمرة
/// </summary>
public enum UmrahPaymentStatus
{
    NotPaid = 1,        // غير مدفوع
    PartiallyPaid = 2,  // مدفوع جزئياً
    FullyPaid = 3       // مدفوع بالكامل
}

/// <summary>
/// حالة المعتمر
/// </summary>
public enum PilgrimStatus
{
    Registered = 1,     // مسجل
    Confirmed = 2,      // مؤكد
    Travelling = 3,     // في الرحلة
    Completed = 4,      // أكمل الرحلة
    Cancelled = 5       // ملغي
}

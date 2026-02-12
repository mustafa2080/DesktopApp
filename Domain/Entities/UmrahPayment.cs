using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

/// <summary>
/// دفعة معتمر
/// </summary>
public class UmrahPayment
{
    [Key]
    public int UmrahPaymentId { get; set; }
    
    /// <summary>
    /// رقم الدفعة
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string PaymentNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// المعتمر
    /// </summary>
    [Required]
    public int UmrahPilgrimId { get; set; }
    
    /// <summary>
    /// تاريخ الدفعة
    /// </summary>
    [Required]
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// المبلغ المدفوع
    /// </summary>
    [Required]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// طريقة الدفع
    /// </summary>
    [Required]
    public PaymentMethod PaymentMethod { get; set; }
    
    /// <summary>
    /// رقم المرجع (للشيكات والتحويلات)
    /// </summary>
    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }
    
    /// <summary>
    /// ملاحظات
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// معرف من قام بتسجيل الدفعة
    /// </summary>
    public int CreatedBy { get; set; }
    
    /// <summary>
    /// تاريخ التسجيل
    /// </summary>
    [Column(TypeName = "timestamp with time zone")]

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // ══════════════════════════════════════
    // العلاقات
    // ══════════════════════════════════════
    
    [ForeignKey(nameof(UmrahPilgrimId))]
    public UmrahPilgrim Pilgrim { get; set; } = null!;
    
    public User Creator { get; set; } = null!;
}

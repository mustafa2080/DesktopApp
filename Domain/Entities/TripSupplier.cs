using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

/// <summary>
/// ربط الرحلات بالموردين (فنادق، نقل، طيران، إلخ)
/// </summary>
public class TripSupplier
{
    [Key]
    public int TripSupplierId { get; set; }
    
    /// <summary>
    /// معرف الرحلة
    /// </summary>
    public int TripId { get; set; }
    
    /// <summary>
    /// معرف المورد
    /// </summary>
    public int SupplierId { get; set; }
    
    /// <summary>
    /// دور المورد في الرحلة (فندق، طيران، نقل، إلخ)
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string SupplierRole { get; set; } = string.Empty; // "فندق", "طيران", "نقل", "مطاعم", "أخرى"
    
    /// <summary>
    /// التكلفة الإجمالية من هذا المورد
    /// </summary>
    public decimal TotalCost { get; set; }
    
    /// <summary>
    /// معرف فاتورة الشراء المرتبطة (إذا وجدت)
    /// </summary>
    public int? PurchaseInvoiceId { get; set; }
    
    /// <summary>
    /// حالة الدفع
    /// </summary>
    [MaxLength(50)]
    public string PaymentStatus { get; set; } = "Pending"; // Pending, Partial, Paid
    
    /// <summary>
    /// المبلغ المدفوع
    /// </summary>
    public decimal PaidAmount { get; set; }
    
    /// <summary>
    /// المبلغ المتبقي
    /// </summary>
    public decimal RemainingAmount => TotalCost - PaidAmount;
    
    /// <summary>
    /// ملاحظات
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// تاريخ الإنشاء
    /// </summary>
    [Column(TypeName = "timestamp with time zone")]

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // العلاقات
    public Trip Trip { get; set; } = null!;
    public Supplier Supplier { get; set; } = null!;
    public PurchaseInvoice? PurchaseInvoice { get; set; }
}

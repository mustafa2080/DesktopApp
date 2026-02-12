using System.ComponentModel.DataAnnotations;

namespace GraceWay.AccountingSystem.Domain.Entities;

/// <summary>
/// المصاريف الأخرى للرحلة
/// </summary>
public class TripExpense
{
    [Key]
    public int TripExpenseId { get; set; }
    
    public int TripId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string ExpenseType { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public decimal Amount { get; set; }
    
    public DateTime ExpenseDate { get; set; }
    
    [MaxLength(50)]
    public string? PaymentMethod { get; set; }
    
    [MaxLength(50)]
    public string? ReceiptNumber { get; set; }
    
    public string? Notes { get; set; }
    
    /// <summary>
    /// معرف المورد (إذا كانت المصروفات مرتبطة بمورد)
    /// </summary>
    public int? SupplierId { get; set; }
    
    /// <summary>
    /// معرف فاتورة الشراء المرتبطة بهذا المصروف
    /// </summary>
    public int? PurchaseInvoiceId { get; set; }
    
    public Trip Trip { get; set; } = null!;
    public Supplier? Supplier { get; set; }
    public PurchaseInvoice? PurchaseInvoice { get; set; }
}

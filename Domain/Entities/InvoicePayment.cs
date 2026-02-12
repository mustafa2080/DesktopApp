using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

/// <summary>
/// دفعات الفواتير - Invoice Payments
/// </summary>
[Table("invoicepayments")]
public class InvoicePayment
{
    [Key]
    [Column("paymentid")]
    public int PaymentId { get; set; }
    
    [Column("salesinvoiceid")]
    public int? SalesInvoiceId { get; set; }
    
    [Column("purchaseinvoiceid")]
    public int? PurchaseInvoiceId { get; set; }
    
    [Required]
    [Column("cashboxid")]
    public int CashBoxId { get; set; }
    
    [Column("cashtransactionid")]
    public int? CashTransactionId { get; set; }
    
    [Column("amount")]
    public decimal Amount { get; set; }
    
    [Column("paymentdate")]
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    
    [MaxLength(20)]
    [Column("paymentmethod")]
    public string PaymentMethod { get; set; } = "Cash";
    
    [MaxLength(100)]
    [Column("referencenumber")]
    public string? ReferenceNumber { get; set; }
    
    [Column("notes")]
    public string? Notes { get; set; }
    
    [Column("createdby")]
    public int? CreatedBy { get; set; }
    
    [Column("CreatedAt", TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation Properties
    [NotMapped]
    public SalesInvoice? SalesInvoice { get; set; }
    [NotMapped]
    public PurchaseInvoice? PurchaseInvoice { get; set; }
    [NotMapped]
    public CashBox? CashBox { get; set; }
    [NotMapped]
    public CashTransaction? CashTransaction { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

[Table("salesinvoices")]
public class SalesInvoice
{
    [Key]
    [Column("salesinvoiceid")]
    public int SalesInvoiceId { get; set; }
    
    [Column("invoicenumber")]
    public string InvoiceNumber { get; set; } = string.Empty;
    
    [Column("invoicedate")]
    public DateTime InvoiceDate { get; set; }
    
    [Column("customerid")]
    public int CustomerId { get; set; }
    
    [Column("reservationid")]
    public int? ReservationId { get; set; }
    
    [Column("subtotal")]
    public decimal SubTotal { get; set; }
    
    [Column("taxrate")]
    public decimal TaxRate { get; set; }
    
    [Column("taxamount")]
    public decimal TaxAmount { get; set; }
    
    [Column("totalamount")]
    public decimal TotalAmount { get; set; }
    
    [Column("paidamount")]
    public decimal PaidAmount { get; set; }
    
    [NotMapped]
    public decimal RemainingAmount => TotalAmount - PaidAmount;
    
    [Column("currencyid")]
    public int? CurrencyId { get; set; }
    
    [Column("exchangerate")]
    public decimal ExchangeRate { get; set; } = 1.000000m;
    
    [Column("status")]
    public string Status { get; set; } = "Unpaid";
    
    [Column("journalentryid")]
    public int? JournalEntryId { get; set; }
    
    [Column("cashboxid")]
    public int? CashBoxId { get; set; }
    
    [Column("notes")]
    public string? Notes { get; set; }
    
    [Column("createdby")]
    public int? CreatedBy { get; set; }
    
    [Column("CreatedAt", TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [NotMapped]
    public Customer? Customer { get; set; }
    [NotMapped]
    public CashBox? CashBox { get; set; }
    [NotMapped]
    public Reservation? Reservation { get; set; }
    [NotMapped]
    public Currency? Currency { get; set; }
    [NotMapped]
    public JournalEntry? JournalEntry { get; set; }
    [NotMapped]
    public ICollection<SalesInvoiceItem> Items { get; set; } = new List<SalesInvoiceItem>();
    [NotMapped]
    public ICollection<InvoicePayment> Payments { get; set; } = new List<InvoicePayment>();
    [NotMapped]
    public ICollection<TripBooking> TripBookings { get; set; } = new List<TripBooking>();
}

[Table("salesinvoiceitems")]
public class SalesInvoiceItem
{
    [Key]
    [Column("itemid")]
    public int ItemId { get; set; }
    
    [Column("salesinvoiceid")]
    public int SalesInvoiceId { get; set; }
    
    [Column("description")]
    public string Description { get; set; } = string.Empty;
    
    [Column("quantity")]
    public decimal Quantity { get; set; } = 1;
    
    [Column("unitprice")]
    public decimal UnitPrice { get; set; }
    
    [NotMapped]
    public decimal TotalPrice => Quantity * UnitPrice;

    // Navigation
    [NotMapped]
    public SalesInvoice? SalesInvoice { get; set; }
}

[Table("purchaseinvoices")]
public class PurchaseInvoice
{
    [Key]
    [Column("purchaseinvoiceid")]
    public int PurchaseInvoiceId { get; set; }
    
    [Column("invoicenumber")]
    public string InvoiceNumber { get; set; } = string.Empty;
    
    [Column("invoicedate")]
    public DateTime InvoiceDate { get; set; }
    
    [Column("supplierid")]
    public int SupplierId { get; set; }
    
    [Column("reservationid")]
    public int? ReservationId { get; set; }
    
    [Column("subtotal")]
    public decimal SubTotal { get; set; }
    
    [Column("taxrate")]
    public decimal TaxRate { get; set; }
    
    [Column("taxamount")]
    public decimal TaxAmount { get; set; }
    
    [Column("totalamount")]
    public decimal TotalAmount { get; set; }
    
    [Column("paidamount")]
    public decimal PaidAmount { get; set; }
    
    [NotMapped]
    public decimal RemainingAmount => TotalAmount - PaidAmount;
    
    [Column("currencyid")]
    public int? CurrencyId { get; set; }
    
    [Column("exchangerate")]
    public decimal ExchangeRate { get; set; } = 1.000000m;
    
    [Column("status")]
    public string Status { get; set; } = "Unpaid";
    
    [Column("journalentryid")]
    public int? JournalEntryId { get; set; }
    
    [Column("cashboxid")]
    public int? CashBoxId { get; set; }
    
    [Column("notes")]
    public string? Notes { get; set; }
    
    [Column("createdby")]
    public int? CreatedBy { get; set; }
    
    [Column("CreatedAt", TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [NotMapped]
    public Supplier? Supplier { get; set; }
    [NotMapped]
    public CashBox? CashBox { get; set; }
    [NotMapped]
    public Reservation? Reservation { get; set; }
    [NotMapped]
    public Currency? Currency { get; set; }
    [NotMapped]
    public JournalEntry? JournalEntry { get; set; }
    [NotMapped]
    public ICollection<PurchaseInvoiceItem> Items { get; set; } = new List<PurchaseInvoiceItem>();
    [NotMapped]
    public ICollection<InvoicePayment> Payments { get; set; } = new List<InvoicePayment>();
    
    // علاقات الرحلات
    [NotMapped]
    public ICollection<TripExpense> TripExpenses { get; set; } = new List<TripExpense>();
    [NotMapped]
    public ICollection<TripAccommodation> TripAccommodations { get; set; } = new List<TripAccommodation>();
    [NotMapped]
    public ICollection<TripTransportation> TripTransportations { get; set; } = new List<TripTransportation>();
    [NotMapped]
    public ICollection<TripSupplier> TripSuppliers { get; set; } = new List<TripSupplier>();
}

[Table("purchaseinvoiceitems")]
public class PurchaseInvoiceItem
{
    [Key]
    [Column("itemid")]
    public int ItemId { get; set; }
    
    [Column("purchaseinvoiceid")]
    public int PurchaseInvoiceId { get; set; }
    
    [Column("description")]
    public string Description { get; set; } = string.Empty;
    
    [Column("quantity")]
    public decimal Quantity { get; set; } = 1;
    
    [Column("unitprice")]
    public decimal UnitPrice { get; set; }
    
    [NotMapped]
    public decimal TotalPrice => Quantity * UnitPrice;

    // Navigation
    [NotMapped]
    public PurchaseInvoice? PurchaseInvoice { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

[Table("suppliers")]
public class Supplier
{
    [Key]
    [Column("supplierid")]
    public int SupplierId { get; set; }
    
    [Column("suppliercode")]
    public string SupplierCode { get; set; } = string.Empty;
    
    [Column("suppliername")]
    public string SupplierName { get; set; } = string.Empty;
    
    [Column("suppliernameen")]
    public string? SupplierNameEn { get; set; }
    
    [Column("phone")]
    public string? Phone { get; set; }
    
    [Column("mobile")]
    public string? Mobile { get; set; }
    
    [Column("email")]
    public string? Email { get; set; }
    
    [Column("address")]
    public string? Address { get; set; }
    
    [Column("city")]
    public string? City { get; set; }
    
    [Column("country")]
    public string? Country { get; set; }
    
    [Column("taxnumber")]
    public string? TaxNumber { get; set; }
    
    [Column("creditlimit")]
    public decimal CreditLimit { get; set; }
    
    [Column("paymenttermdays")]
    public int PaymentTermDays { get; set; }
    
    [Column("accountid")]
    public int? AccountId { get; set; }
    
    [Column("openingbalance")]
    public decimal OpeningBalance { get; set; }
    
    [Column("currentbalance")]
    public decimal CurrentBalance { get; set; }
    
    [Column("isactive")]
    public bool IsActive { get; set; } = true;
    
    [Column("notes")]
    public string? Notes { get; set; }
    
    [Column("CreatedAt", TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [NotMapped]
    public Account? Account { get; set; }
    [NotMapped]
    public ICollection<PurchaseInvoice> PurchaseInvoices { get; set; } = new List<PurchaseInvoice>();
    [NotMapped]
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    
    // علاقات الرحلات
    [NotMapped]
    public ICollection<TripAccommodation> TripAccommodations { get; set; } = new List<TripAccommodation>();
    [NotMapped]
    public ICollection<TripTransportation> TripTransportations { get; set; } = new List<TripTransportation>();
    [NotMapped]
    public ICollection<TripExpense> TripExpenses { get; set; } = new List<TripExpense>();
    [NotMapped]
    public ICollection<TripSupplier> TripSuppliers { get; set; } = new List<TripSupplier>();
}

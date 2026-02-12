using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

[Table("customers")]
public class Customer
{
    [Key]
    [Column("customerid")]
    public int CustomerId { get; set; }
    
    [Column("customercode")]
    public string CustomerCode { get; set; } = string.Empty;
    
    [Column("customername")]
    public string CustomerName { get; set; } = string.Empty;
    
    [Column("customernameen")]
    public string? CustomerNameEn { get; set; }
    
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
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    [NotMapped]
    public ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();
    [NotMapped]
    public ICollection<TripBooking> TripBookings { get; set; } = new List<TripBooking>();
}

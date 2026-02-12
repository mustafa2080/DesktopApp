using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

public class ServiceType
{
    [Key]
    public int ServiceTypeId { get; set; }
    public string ServiceTypeName { get; set; } = string.Empty;
    public string? ServiceTypeNameEn { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}

public class Reservation
{
    [Key]
    public int ReservationId { get; set; }
    public string ReservationNumber { get; set; } = string.Empty;
    public DateTime ReservationDate { get; set; }
    public int CustomerId { get; set; }
    public int ServiceTypeId { get; set; }
    public string? ServiceDescription { get; set; }
    public DateTime? TravelDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public int NumberOfPeople { get; set; } = 1;
    public decimal SellingPrice { get; set; }
    public decimal CostPrice { get; set; }
    public decimal ProfitAmount => SellingPrice - CostPrice;
    public decimal ProfitPercentage => CostPrice > 0 ? ((SellingPrice - CostPrice) / CostPrice * 100) : 0;
    public int? CurrencyId { get; set; }
    public decimal ExchangeRate { get; set; } = 1.000000m;
    public string Status { get; set; } = "Draft"; // Draft, Confirmed, Cancelled, Completed
    public int? SupplierId { get; set; }
    public decimal SupplierCost { get; set; }
    public string? Notes { get; set; }
    public int? CreatedBy { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Cash Box Integration
    public int? CashBoxId { get; set; }
    public int? CashTransactionId { get; set; }
    
    // Trip Integration (Optional - ربط مع الرحلات)
    public int? TripId { get; set; }

    // Navigation
    public Customer? Customer { get; set; }
    public ServiceType? ServiceType { get; set; }
    public Currency? Currency { get; set; }
    public Supplier? Supplier { get; set; }
    public User? Creator { get; set; }
    public CashBox? CashBox { get; set; }
    public CashTransaction? CashTransaction { get; set; }
    public Trip? Trip { get; set; }
}

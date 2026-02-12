using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

/// <summary>
/// العملات - Currencies Entity
/// </summary>
public class Currency
{
    [Key]
    public int CurrencyId { get; set; }
    
    [Required]
    [MaxLength(3)]
    [Column("currencycode")]
    public string Code { get; set; } = string.Empty; // USD, EUR, EGP
    
    [Required]
    [MaxLength(50)]
    [Column("currencyname")]
    public string Name { get; set; } = string.Empty; // Dollar, Euro, Egyptian Pound
    
    [MaxLength(10)]
    public string? Symbol { get; set; } // $, €, £E
    
    public decimal ExchangeRate { get; set; } = 1.000000m; // Exchange rate to base currency
    
    public bool IsBaseCurrency { get; set; } = false;
    
    public bool IsActive { get; set; } = true;
    
    [Column(TypeName = "timestamp with time zone")]

    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [Column(TypeName = "timestamp with time zone")]

    public DateTime? UpdatedAt { get; set; }
}

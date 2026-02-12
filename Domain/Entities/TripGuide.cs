using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

/// <summary>
/// المرشدون السياحيون في الرحلة
/// </summary>
public class TripGuide
{
    [Key]
    public int TripGuideId { get; set; }
    
    public int TripId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string GuideName { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string? Phone { get; set; }
    
    [MaxLength(100)]
    public string? Email { get; set; }
    
    [MaxLength(100)]
    public string? Languages { get; set; }
    
    public decimal BaseFee { get; set; }
    public decimal CommissionPercentage { get; set; }
    public decimal CommissionAmount { get; set; }
    
    [Column("drivertip")]
    public decimal DriverTip { get; set; }
    
    /// <summary>
    /// التكلفة الإجمالية (الراتب الأساسي + العمولة + إكرامية السواق)
    /// </summary>
    public decimal TotalCost => BaseFee + CommissionAmount + DriverTip;
    
    public string? Notes { get; set; }
    
    public Trip Trip { get; set; } = null!;
}

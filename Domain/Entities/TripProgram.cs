using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

/// <summary>
/// البرنامج اليومي للرحلة
/// </summary>
public class TripProgram
{
    [Key]
    public int TripProgramId { get; set; }
    
    /// <summary>
    /// معرف الرحلة
    /// </summary>
    public int TripId { get; set; }
    
    /// <summary>
    /// رقم اليوم (1، 2، 3، ...)
    /// </summary>
    public int DayNumber { get; set; }
    
    /// <summary>
    /// عنوان اليوم
    /// </summary>
    [MaxLength(200)]
    [Column("daytitle")]
    public string DayTitle { get; set; } = string.Empty;
    
    /// <summary>
    /// تاريخ اليوم
    /// </summary>
    public DateTime DayDate { get; set; }
    
    /// <summary>
    /// الأنشطة في هذا اليوم
    /// </summary>
    [Column("activities")]
    public string Activities { get; set; } = string.Empty;
    
    /// <summary>
    /// المزارات في هذا اليوم
    /// </summary>
    public string Visits { get; set; } = string.Empty;
    
    /// <summary>
    /// الوجبات المتضمنة
    /// </summary>
    [MaxLength(100)]
    [Column("mealsincluded")]
    public string MealsIncluded { get; set; } = string.Empty;
    
    /// <summary>
    /// سعر المزارات
    /// </summary>
    public decimal VisitsCost { get; set; }
    
    /// <summary>
    /// سعر المرشد لهذا اليوم
    /// </summary>
    [Column("guidecost")]
    public decimal GuideCost { get; set; }
    
    /// <summary>
    /// عدد الأفراد
    /// </summary>
    [Column("participantscount")]
    public int ParticipantsCount { get; set; }
    
    /// <summary>
    /// إكرامية السائق
    /// </summary>
    [Column("drivertip")]
    public decimal DriverTip { get; set; }
    
    /// <summary>
    /// نوع الحجز (Adult/Child)
    /// </summary>
    [MaxLength(10)]
    public string BookingType { get; set; } = "Adult";
    
    /// <summary>
    /// ملاحظات إضافية
    /// </summary>
    public string? Notes { get; set; }
    
    // العلاقات
    public Trip Trip { get; set; } = null!;
}

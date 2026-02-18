using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

/// <summary>
/// وسائل النقل في الرحلة
/// </summary>
public class TripTransportation
{
    [Key]
    public int TripTransportationId { get; set; }
    
    /// <summary>
    /// معرف الرحلة
    /// </summary>
    public int TripId { get; set; }
    
    /// <summary>
    /// نوع وسيلة النقل
    /// </summary>
    public TransportationType Type { get; set; }
    
    /// <summary>
    /// عدد السيارات/الوسائل
    /// </summary>
    public int NumberOfVehicles { get; set; } = 1;
    
    /// <summary>
    /// عدد المقاعد لكل وسيلة
    /// </summary>
    public int SeatsPerVehicle { get; set; }
    
    /// <summary>
    /// إجمالي المقاعد
    /// </summary>
    public int TotalSeats => NumberOfVehicles * SeatsPerVehicle;
    
    /// <summary>
    /// التكلفة للوسيلة الواحدة
    /// </summary>
    public decimal CostPerVehicle { get; set; }
    
    /// <summary>
    /// التكلفة الإجمالية (تكلفة المركبات + إكرامية التور ليدر + إكرامية السواق)
    /// </summary>
    public decimal TotalCost => (NumberOfVehicles * CostPerVehicle) + TourLeaderTip + DriverTip;
    
    /// <summary>
    /// اسم السائق
    /// </summary>
    [MaxLength(100)]
    public string? DriverName { get; set; }
    
    /// <summary>
    /// تاريخ النقل
    /// </summary>
    public DateTime? TransportDate { get; set; }
    
    /// <summary>
    /// مسار النقل (من - إلى)
    /// </summary>
    [MaxLength(300)]
    public string? Route { get; set; }
    
    /// <summary>
    /// عدد الأفراد المستخدمين لهذا النقل
    /// </summary>
    [Column("participantscount")]
    public int ParticipantsCount { get; set; }
    
    /// <summary>
    /// السعر للفرد الواحد
    /// </summary>
    public decimal CostPerPerson => ParticipantsCount > 0 ? CostPerVehicle / ParticipantsCount : 0;
    
    /// <summary>
    /// إكرامية التور ليدر
    /// </summary>
    [Column("tourleadertip")]
    public decimal TourLeaderTip { get; set; }
    
    /// <summary>
    /// إكرامية السواق
    /// </summary>
    [Column("drivertip")]
    public decimal DriverTip { get; set; }
    
    /// <summary>
    /// ملاحظات
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// اسم المزار المرتبط بهذا النقل (اختياري - للربط مع البرنامج اليومي)
    /// </summary>
    [MaxLength(200)]
    public string? VisitName { get; set; }
    
    /// <summary>
    /// رقم اليوم في البرنامج (اختياري - للربط مع البرنامج اليومي)
    /// </summary>
    public int? ProgramDayNumber { get; set; }
    
    /// <summary>
    /// معرف المورد (شركة النقل)
    /// </summary>
    public int? SupplierId { get; set; }
    
    /// <summary>
    /// معرف فاتورة الشراء المرتبطة بالنقل
    /// </summary>
    public int? PurchaseInvoiceId { get; set; }
    
    // العلاقات
    public Trip Trip { get; set; } = null!;
    public Supplier? Supplier { get; set; }
    public PurchaseInvoice? PurchaseInvoice { get; set; }
}

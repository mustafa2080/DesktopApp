using System.ComponentModel.DataAnnotations;

namespace GraceWay.AccountingSystem.Domain.Entities;

/// <summary>
/// حجوزات الرحلات الاختيارية
/// يربط بين TripBooking و TripOptionalTour
/// </summary>
public class TripOptionalTourBooking
{
    [Key]
    public int TripOptionalTourBookingId { get; set; }
    
    /// <summary>
    /// معرف الحجز الرئيسي
    /// </summary>
    public int TripBookingId { get; set; }
    
    /// <summary>
    /// معرف الرحلة الاختيارية
    /// </summary>
    public int TripOptionalTourId { get; set; }
    
    /// <summary>
    /// عدد المشاركين في هذه الرحلة الاختيارية
    /// </summary>
    public int NumberOfParticipants { get; set; }
    
    /// <summary>
    /// سعر الفرد الواحد
    /// </summary>
    public decimal PricePerPerson { get; set; }
    
    /// <summary>
    /// إجمالي المبلغ
    /// </summary>
    public decimal TotalAmount => NumberOfParticipants * PricePerPerson;
    
    /// <summary>
    /// تاريخ الحجز
    /// </summary>
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// ملاحظات
    /// </summary>
    public string? Notes { get; set; }
    
    // العلاقات
    public TripBooking TripBooking { get; set; } = null!;
    public TripOptionalTour OptionalTour { get; set; } = null!;
}

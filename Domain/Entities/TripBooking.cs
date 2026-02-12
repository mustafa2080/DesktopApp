using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

/// <summary>
/// حجوزات العملاء في الرحلة
/// </summary>
public class TripBooking
{
    [Key]
    public int TripBookingId { get; set; }
    
    public int TripId { get; set; }
    public int CustomerId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string BookingNumber { get; set; } = string.Empty;
    
    public DateTime BookingDate { get; set; } = DateTime.UtcNow;
    
    // تفاصيل الحجز
    public int NumberOfPersons { get; set; }
    public decimal PricePerPerson { get; set; }
    
    // ملحوظة: TotalAmount و RemainingAmount محسوبة في قاعدة البيانات لكن يمكن حسابها ديناميكياً
    public decimal TotalAmount => NumberOfPersons * PricePerPerson;
    
    // المدفوعات
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount => TotalAmount - PaidAmount;
    public PaymentStatus PaymentStatus { get; set; }
    
    // الحالة
    public BookingStatus Status { get; set; }
    
    public string? SpecialRequests { get; set; }
    public string? Notes { get; set; }
    
    public int CreatedBy { get; set; }    [Column(TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // العلاقات
    public Trip Trip { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public User Creator { get; set; } = null!;
    public ICollection<TripBookingPayment> Payments { get; set; } = new List<TripBookingPayment>();
    
    // ربط مع الفواتير
    public int? SalesInvoiceId { get; set; }
    public SalesInvoice? SalesInvoice { get; set; }
    
    // ربط مع الرحلات الاختيارية
    public ICollection<TripOptionalTourBooking> OptionalTourBookings { get; set; } = new List<TripOptionalTourBooking>();
}

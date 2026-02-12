using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

/// <summary>
/// مدفوعات الحجز - مرتبطة بالخزنة
/// </summary>
public class TripBookingPayment
{
    [Key]
    public int TripBookingPaymentId { get; set; }
    
    public int TripBookingId { get; set; }
    
    // المبلغ
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    
    // طريقة الدفع
    public PaymentMethod PaymentMethod { get; set; }
    
    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }
    
    // الربط بالخزنة 💰
    public int CashBoxId { get; set; }
    public int? CashTransactionId { get; set; }
    
    // عمولة إنستا باي
    public decimal? InstaPayCommission { get; set; }
    public decimal NetAmount => PaymentMethod == PaymentMethod.InstaPay ? 
        Amount - (InstaPayCommission ?? 0) : Amount;
    
    public string? Notes { get; set; }
    public int CreatedBy { get; set; }    [Column(TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // العلاقات
    public TripBooking Booking { get; set; } = null!;
    public CashBox CashBox { get; set; } = null!;
    public CashTransaction? CashTransaction { get; set; }
    public User Creator { get; set; } = null!;
}

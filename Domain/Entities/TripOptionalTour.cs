using System.ComponentModel.DataAnnotations;

namespace GraceWay.AccountingSystem.Domain.Entities;

/// <summary>
/// الرحلات الاختيارية (Optional Tours)
/// </summary>
public class TripOptionalTour
{
    [Key]
    public int TripOptionalTourId { get; set; }
    
    public int TripId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string TourName { get; set; } = string.Empty; // نوع الرحلة
    
    public string? Description { get; set; }
    
    public int Duration { get; set; } // بالساعات
    
    // الأسعار
    public decimal SellingPrice { get; set; } // سعر البيع للفرد
    public decimal PurchasePrice { get; set; } // سعر الشراء للفرد
    public decimal Profit => SellingPrice - PurchasePrice; // الربح للفرد
    
    // العمولات
    public decimal GuideCommission { get; set; } // عمولة المرشد (إجمالي)
    public decimal SalesRepCommission { get; set; } // عمولة المندوب (إجمالي)
    
    // المشتركون
    public int ParticipantsCount { get; set; } // عدد الأفراد
    
    // الحسابات
    public decimal TotalRevenue => ParticipantsCount * SellingPrice; // إجمالي الإيراد
    public decimal TotalPurchaseCost => ParticipantsCount * PurchasePrice; // إجمالي تكلفة الشراء
    public decimal TotalCost => TotalPurchaseCost + GuideCommission + SalesRepCommission; // إجمالي التكلفة
    public decimal NetProfit => TotalRevenue - TotalCost; // صافي الربح
    
    // العلاقات
    public Trip Trip { get; set; } = null!;
    public ICollection<TripOptionalTourBooking> Bookings { get; set; } = new List<TripOptionalTourBooking>();
}

using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

/// <summary>
/// كيان الحركة المالية - يمثل إيراد أو مصروف في الخزنة
/// </summary>
public class CashTransaction
{
    public int Id { get; set; }
    
    /// <summary>
    /// رقم السند (تلقائي)
    /// </summary>
    public string VoucherNumber { get; set; } = string.Empty;
    
    /// <summary>
    /// نوع الحركة: إيراد أو مصروف
    /// </summary>
    public TransactionType Type { get; set; }
    
    /// <summary>
    /// معرف الخزنة
    /// </summary>
    public int CashBoxId { get; set; }
    
    /// <summary>
    /// الخزنة المرتبطة
    /// </summary>
    public virtual CashBox CashBox { get; set; } = null!;
    
    /// <summary>
    /// المبلغ بالعملة الأساسية للخزنة
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// العملة المستخدمة في هذه الحركة (EGP, USD, EUR, GBP, SAR)
    /// </summary>
    public string TransactionCurrency { get; set; } = "EGP";
    
    /// <summary>
    /// سعر الصرف المستخدم وقت الحركة
    /// </summary>
    public decimal? ExchangeRateUsed { get; set; }
    
    /// <summary>
    /// المبلغ الأصلي بالعملة الأجنبية (قبل التحويل)
    /// </summary>
    public decimal? OriginalAmount { get; set; }
    
    /// <summary>
    /// التاريخ
    /// </summary>
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// الشهر (1-12)
    /// </summary>
    public int Month { get; set; }
    
    /// <summary>
    /// السنة
    /// </summary>
    public int Year { get; set; }
    
    /// <summary>
    /// التصنيف (مثل: رواتب، إيجار، مبيعات، إلخ)
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// الوصف
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// اسم الطرف الآخر (عميل/مورد)
    /// </summary>
    public string? PartyName { get; set; }
    
    /// <summary>
    /// طريقة الدفع
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }
    
    /// <summary>
    /// عمولة إنستا باي (إن وجدت)
    /// </summary>
    public decimal? InstaPayCommission { get; set; }
    
    /// <summary>
    /// رقم المرجع (رقم شيك، رقم تحويل، إلخ)
    /// </summary>
    public string? ReferenceNumber { get; set; }
    
    /// <summary>
    /// ملاحظات
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// الرصيد قبل الحركة
    /// </summary>
    public decimal BalanceBefore { get; set; }
    
    /// <summary>
    /// الرصيد بعد الحركة
    /// </summary>
    public decimal BalanceAfter { get; set; }
    
    /// <summary>
    /// المستخدم الذي أدخل الحركة
    /// </summary>
    public int CreatedBy { get; set; }
    
    /// <summary>
    /// المستخدم الذي قام بآخر تعديل
    /// </summary>
    public int? UpdatedBy { get; set; }
    
    /// <summary>
    /// بيانات المستخدم الذي أدخل الحركة
    /// </summary>
    public virtual User? Creator { get; set; }
    
    /// <summary>
    /// بيانات المستخدم الذي قام بآخر تعديل
    /// </summary>
    public virtual User? Updater { get; set; }
    
    /// <summary>
    /// تاريخ الإنشاء
    /// </summary>
    [Column(TypeName = "timestamp with time zone")]

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// تاريخ آخر تحديث
    /// </summary>
    [Column("updatedat", TypeName = "timestamp with time zone")]
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// محذوف؟ (Soft Delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// معرف الحجز المرتبط (إن وجد)
    /// </summary>
    public int? ReservationId { get; set; }
    
    /// <summary>
    /// الحجز المرتبط
    /// </summary>
    public virtual Reservation? Reservation { get; set; }
}

/// <summary>
/// نوع الحركة المالية
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// إيراد (قبض)
    /// </summary>
    Income = 1,
    
    /// <summary>
    /// مصروف (صرف)
    /// </summary>
    Expense = 2
}

/// <summary>
/// طريقة الدفع
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// نقدي
    /// </summary>
    Cash = 1,
    
    /// <summary>
    /// شيك
    /// </summary>
    Cheque = 2,
    
    /// <summary>
    /// تحويل بنكي
    /// </summary>
    BankTransfer = 3,
    
    /// <summary>
    /// بطاقة ائتمان
    /// </summary>
    CreditCard = 4,
    
    /// <summary>
    /// إنستا باي
    /// </summary>
    InstaPay = 5,
    
    /// <summary>
    /// آخر
    /// </summary>
    Other = 6,

    /// <summary>
    /// بطاقة - Card (alias for CreditCard)
    /// </summary>
    Card = 4,

    /// <summary>
    /// فيزا - Visa (alias for CreditCard)
    /// </summary>
    Visa = 4
}

/// <summary>
/// العملات المدعومة في النظام
/// </summary>
public enum CurrencyType
{
    /// <summary>
    /// جنيه مصري
    /// </summary>
    EGP = 1,
    
    /// <summary>
    /// دولار أمريكي
    /// </summary>
    USD = 2,
    
    /// <summary>
    /// يورو
    /// </summary>
    EUR = 3,
    
    /// <summary>
    /// جنيه إسترليني
    /// </summary>
    GBP = 4,
    
    /// <summary>
    /// ريال سعودي
    /// </summary>
    SAR = 5
}

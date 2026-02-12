using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

/// <summary>
/// كيان الخزنة - يمثل الخزنة النقدية أو الحساب البنكي
/// </summary>
public class CashBox
{
    public int Id { get; set; }
    
    /// <summary>
    /// كود الخزنة (مثل: CB001)
    /// </summary>
    public string CashBoxCode { get; set; } = string.Empty;
    
    /// <summary>
    /// اسم الخزنة أو البنك
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// نوع الحساب: خزنة أو بنك (CashBox أو BankAccount)
    /// </summary>
    public string Type { get; set; } = "CashBox";
    
    /// <summary>
    /// رقم الحساب البنكي (إن وجد)
    /// </summary>
    public string? AccountNumber { get; set; }
    
    /// <summary>
    /// IBAN للحساب البنكي
    /// </summary>
    public string? Iban { get; set; }
    
    /// <summary>
    /// معرف الحساب المحاسبي المرتبط
    /// </summary>
    public int? AccountId { get; set; }
    
    /// <summary>
    /// اسم البنك (للحسابات البنكية)
    /// </summary>
    public string? BankName { get; set; }
    
    /// <summary>
    /// الرصيد الافتتاحي
    /// </summary>
    public decimal OpeningBalance { get; set; }
    
    /// <summary>
    /// الرصيد الحالي
    /// </summary>
    public decimal CurrentBalance { get; set; }
    
    /// <summary>
    /// العملة الأساسية للخزنة
    /// </summary>
    public string Currency { get; set; } = "EGP";
    
    /// <summary>
    /// سعر الصرف مقابل الجنيه المصري (للعملات الأجنبية)
    /// </summary>
    public decimal? ExchangeRate { get; set; }
    
    /// <summary>
    /// نشطة أم لا
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// محذوفة أم لا (Soft Delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    
    /// <summary>
    /// ملاحظات
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// تاريخ الإنشاء
    /// </summary>
    [Column(TypeName = "timestamp with time zone")]

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// تاريخ آخر تحديث
    /// </summary>
    [Column(TypeName = "timestamp with time zone")]

    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// المستخدم الذي أنشأ الخزنة
    /// </summary>
    public int CreatedBy { get; set; }
    
    /// <summary>
    /// المستخدم الذي قام بآخر تحديث
    /// </summary>
    public int? UpdatedBy { get; set; }
    
    /// <summary>
    /// بيانات المستخدم الذي أنشأ الخزنة
    /// </summary>
    public virtual User? Creator { get; set; }
    
    /// <summary>
    /// بيانات المستخدم الذي قام بآخر تحديث
    /// </summary>
    public virtual User? Updater { get; set; }
    
    /// <summary>
    /// الحركات المالية المرتبطة بالخزنة
    /// </summary>
    public virtual ICollection<CashTransaction> Transactions { get; set; } = new List<CashTransaction>();
}

/// <summary>
/// نوع الخزنة
/// </summary>
public enum CashBoxType
{
    /// <summary>
    /// خزنة نقدية
    /// </summary>
    CashBox = 1,
    
    /// <summary>
    /// حساب بنكي
    /// </summary>
    BankAccount = 2
}

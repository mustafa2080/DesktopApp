using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

public class InvoiceSetting
{
    public int Id { get; set; }
    
    // إعدادات الضريبة
    public bool EnableTax { get; set; } = true;
    public decimal DefaultTaxRate { get; set; } = 14.0m; // الضريبة الافتراضية في مصر 14%
    public string? TaxLabel { get; set; } = "ضريبة القيمة المضافة";
    
    // إعدادات الترقيم التلقائي
    public bool AutoNumbering { get; set; } = true;
    public string? SalesInvoicePrefix { get; set; } = "SI";
    public string? PurchaseInvoicePrefix { get; set; } = "PI";
    public int NextSalesInvoiceNumber { get; set; } = 1;
    public int NextPurchaseInvoiceNumber { get; set; } = 1;
    public int InvoiceNumberLength { get; set; } = 6; // SI-000001
    
    // نصوص الفاتورة
    public string? InvoiceFooterText { get; set; }
    public string? PaymentTerms { get; set; }
    public string? BankDetails { get; set; }
    public string? NotesTemplate { get; set; }
    
    // إعدادات العرض
    public bool ShowCompanyLogo { get; set; } = true;
    public bool ShowTaxNumber { get; set; } = true;
    public bool ShowBankDetails { get; set; } = true;
    public bool ShowPaymentTerms { get; set; } = true;
    
    // إعدادات الطباعة
    public string? PaperSize { get; set; } = "A4"; // A4, A5, Letter
    public bool PrintInColor { get; set; } = true;
    public bool PrintDuplicateCopy { get; set; } = false;
    
    // معلومات النظام
    [Column("createddate")]

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedDate { get; set; }
    public int? LastModifiedByUserId { get; set; }
    
    // علاقات
    public User? LastModifiedByUser { get; set; }
}

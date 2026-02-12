using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

/// <summary>
/// كشف حساب المورد - Supplier Statement
/// </summary>
public class SupplierStatement
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string SupplierCode { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public List<SupplierStatementLine> Transactions { get; set; } = new();
}

/// <summary>
/// سطر في كشف حساب المورد
/// </summary>
public class SupplierStatementLine
{
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty; // فاتورة شراء، سداد، رصيد افتتاحي
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Debit { get; set; }  // مدين (لنا عليه - سداد)
    public decimal Credit { get; set; } // دائن (له علينا - فاتورة شراء)
    public decimal Balance { get; set; } // الرصيد الجاري
    public string Notes { get; set; } = string.Empty;
    
    // للربط بالمصدر الأصلي
    public int? PurchaseInvoiceId { get; set; }
    public int? PaymentId { get; set; }
}

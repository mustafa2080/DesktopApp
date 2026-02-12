using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

public class JournalEntry
{
    [Key]
    public int JournalEntryId { get; set; }
    public string EntryNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public string EntryType { get; set; } = string.Empty;
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public string? Description { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public bool IsPosted { get; set; }
    public DateTime? PostedAt { get; set; }
    public int? CreatedBy { get; set; }    [Column(TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
}

public class JournalEntryLine
{
    [Key]
    public int LineId { get; set; }
    public int JournalEntryId { get; set; }
    public int AccountId { get; set; }
    public string? Description { get; set; }
    public decimal DebitAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public int LineOrder { get; set; }

    // Navigation
    public JournalEntry? JournalEntry { get; set; }
    public Account? Account { get; set; }
}

public class Payment
{
    [Key]
    public int PaymentId { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public DateTime PaymentDate { get; set; }
    public string PaymentType { get; set; } = string.Empty; // Receipt, Payment
    public int CashBoxId { get; set; }
    public decimal Amount { get; set; }
    public int? CurrencyId { get; set; }
    public decimal ExchangeRate { get; set; } = 1.000000m;
    public string? PaymentMethod { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public string? CheckNumber { get; set; }
    public DateTime? CheckDate { get; set; }
    public string? BankName { get; set; }
    public int? JournalEntryId { get; set; }
    public string? Notes { get; set; }
    public int? CreatedBy { get; set; }    [Column(TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public CashBox? CashBox { get; set; }
    public Currency? Currency { get; set; }
}

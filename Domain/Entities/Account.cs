using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

[Table("accounts")]
public class Account
{
    [Key]
    [Column("accountid")]
    public int AccountId { get; set; }
    
    [Column("accountcode")]
    public string AccountCode { get; set; } = string.Empty;
    
    [Column("accountname")]
    public string AccountName { get; set; } = string.Empty;
    
    [Column("accountnameen")]
    public string? AccountNameEn { get; set; }
    
    [Column("parentaccountid")]
    public int? ParentAccountId { get; set; }
    
    [Column("accounttype")]
    public string AccountType { get; set; } = string.Empty; // Asset, Liability, Equity, Revenue, Expense
    
    [Column("level")]
    public int Level { get; set; }
    
    [Column("isparent")]
    public bool IsParent { get; set; }
    
    [Column("isactive")]
    public bool IsActive { get; set; } = true;
    
    [Column("openingbalance")]
    public decimal OpeningBalance { get; set; }
    
    [Column("currentbalance")]
    public decimal CurrentBalance { get; set; }
    
    [Column("notes")]
    public string? Notes { get; set; }    [Column(TypeName = "timestamp with time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    [NotMapped]
    public Account? ParentAccount { get; set; }
    [NotMapped]
    public ICollection<Account> ChildAccounts { get; set; } = new List<Account>();
}

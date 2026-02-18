using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities
{
    public class BankTransfer
    {
        public int Id { get; set; }
        public int? SourceBankAccountId { get; set; }
        public int? SourceCashBoxId { get; set; }
        public int? DestinationBankAccountId { get; set; }
        public int? DestinationCashBoxId { get; set; }
        public decimal Amount { get; set; }
        public string TransferType { get; set; } = string.Empty; // BankToBank, BankToCash, CashToBank
        public DateTime TransferDate { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
        public string? ReferenceNumber { get; set; }
        
        // Link to Trip for Fawateerk payments
        public int? TripId { get; set; }
        
        [Column("createdby")]
        public int CreatedBy { get; set; }
        
        [Column("createddate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual BankAccount? SourceBankAccount { get; set; }
        public virtual BankAccount? DestinationBankAccount { get; set; }
        public virtual CashBox? SourceCashBox { get; set; }
        public virtual CashBox? DestinationCashBox { get; set; }
        public virtual Trip? Trip { get; set; }
    }
}

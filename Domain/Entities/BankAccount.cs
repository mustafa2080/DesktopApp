using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities
{
    public class BankAccount
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "اسم البنك مطلوب")]
        [MaxLength(200, ErrorMessage = "اسم البنك يجب ألا يتجاوز 200 حرف")]
        public string BankName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "رقم الحساب مطلوب")]
        [MaxLength(100, ErrorMessage = "رقم الحساب يجب ألا يتجاوز 100 حرف")]
        public string AccountNumber { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string AccountType { get; set; } = string.Empty; // جاري، توفير، إلخ
        
        [Range(0, double.MaxValue, ErrorMessage = "الرصيد يجب أن يكون قيمة موجبة")]
        public decimal Balance { get; set; }
        
        [MaxLength(10)]
        public string Currency { get; set; } = "EGP";
        
        [MaxLength(200)]
        public string? Branch { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public string? Notes { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        
        public int CreatedBy { get; set; }
        
        public DateTime? ModifiedDate { get; set; }
        
        public int? ModifiedBy { get; set; }
    }
}

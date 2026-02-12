using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

public class CompanySetting
{
    public int Id { get; set; }
    
    // معلومات الشركة الأساسية
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyNameEnglish { get; set; } = string.Empty;
    public string? LogoPath { get; set; }
    
    // بيانات الاتصال
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; } = "مصر";
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    
    // البيانات الضريبية
    public string? TaxRegistrationNumber { get; set; }
    public string? CommercialRegistrationNumber { get; set; }
    
    // معلومات إضافية
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankIBAN { get; set; }
    
    // معلومات النظام
    [Column("createddate")]

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastModifiedDate { get; set; }
    public int? LastModifiedByUserId { get; set; }
    
    // علاقات
    public User? LastModifiedByUser { get; set; }
}

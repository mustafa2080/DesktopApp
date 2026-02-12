using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities;

public class AuditLog
{
    [Key]
    public int AuditLogId { get; set; }
    
    // Who did it
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    
    // What was done
    public AuditAction Action { get; set; }
    public string EntityType { get; set; } = string.Empty; // Trip, Umrah, Flight, Customer, etc.
    public int? EntityId { get; set; }
    public string EntityName { get; set; } = string.Empty; // اسم العنصر للعرض
    
    // Details
    public string Description { get; set; } = string.Empty;
    public string? OldValues { get; set; } // JSON للقيم القديمة
    public string? NewValues { get; set; } // JSON للقيم الجديدة
    
    // When
    public DateTime Timestamp { get; set; } = DateTime.Now;
    
    // Where (optional)
    public string? IpAddress { get; set; }
    public string? MachineName { get; set; }
    
    // Navigation
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}

public enum AuditAction
{
    Create,     // إضافة
    Update,     // تعديل
    Delete,     // حذف
    View,       // عرض (اختياري)
    Export,     // تصدير
    Print,      // طباعة
    Login,      // تسجيل دخول
    Logout,     // تسجيل خروج
    Approve,    // موافقة
    Reject,     // رفض
    Cancel      // إلغاء
}

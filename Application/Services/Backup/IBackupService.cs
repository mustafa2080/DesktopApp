using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Application.Services.Backup;

public interface IBackupService
{
    /// <summary>
    /// إنشاء نسخة احتياطية من قاعدة البيانات
    /// </summary>
    Task<BackupResult> CreateBackupAsync(string? backupPath = null, string? description = null);
    
    /// <summary>
    /// استعادة قاعدة البيانات من نسخة احتياطية
    /// </summary>
    Task<RestoreResult> RestoreBackupAsync(string backupFilePath);
    
    /// <summary>
    /// الحصول على قائمة النسخ الاحتياطية المتاحة
    /// </summary>
    Task<List<DatabaseBackup>> GetBackupHistoryAsync();
    
    /// <summary>
    /// حذف نسخة احتياطية قديمة
    /// </summary>
    Task<bool> DeleteBackupAsync(int backupId);
    
    /// <summary>
    /// إنشاء نسخة احتياطية تلقائية (scheduled)
    /// </summary>
    Task<BackupResult> CreateAutoBackupAsync();
    
    /// <summary>
    /// التحقق من صحة ملف النسخة الاحتياطية
    /// </summary>
    Task<bool> ValidateBackupFileAsync(string backupFilePath);
}

public class BackupResult
{
    public bool Success { get; set; }
    public required string Message { get; set; }
    public string? BackupFilePath { get; set; }
    public long FileSizeBytes { get; set; }
    public DateTime BackupDate { get; set; }
}

public class RestoreResult
{
    public bool Success { get; set; }
    public required string Message { get; set; }
    public DateTime RestoreDate { get; set; }
}

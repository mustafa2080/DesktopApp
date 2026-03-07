using GraceWay.AccountingSystem.Application.Services;
using GraceWay.AccountingSystem.Application.Services.Backup;

namespace GraceWay.AccountingSystem.Application.Services;

// ══════════════════════════════════════════════════════════════
// نموذج الإشعار
// ══════════════════════════════════════════════════════════════
public enum NotificationType { Warning, Danger, Info, Success }

public class AppNotification
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string Module { get; set; } = string.Empty;   // cashbox / invoice / trip / umrah
    public string ActionKey { get; set; } = string.Empty; // للانتقال للقسم
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsRead { get; set; } = false;
}

// ══════════════════════════════════════════════════════════════
// Interface
// ══════════════════════════════════════════════════════════════
public interface INotificationService
{
    event EventHandler<List<AppNotification>>? NotificationsRefreshed;
    Task<List<AppNotification>> GetAllAsync();
    Task RefreshAsync();
    void MarkAllRead();
    int UnreadCount { get; }
}

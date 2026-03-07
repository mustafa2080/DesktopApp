using GraceWay.AccountingSystem.Infrastructure.Logging;
using Microsoft.Toolkit.Uwp.Notifications;

namespace GraceWay.AccountingSystem.Application.Services;

/// <summary>
/// خدمة إشعارات Windows Toast
/// بتظهر إشعارات في ركن الشاشة حتى لو البرنامج مصغّر أو مخفي
/// Singleton — تعيش طول عمر التطبيق
/// </summary>
public class WindowsToastService : IDisposable
{
    private const string AppId = "GraceWay.AccountingSystem";
    private bool _initialized  = false;

    // ══════════════════════════════════════════════════
    // Initialize — يُستدعى مرة واحدة في Program.cs
    // ══════════════════════════════════════════════════
    public void Initialize()
    {
        try
        {
            ToastNotificationManagerCompat.OnActivated += OnToastActivated;
            _initialized = true;
            AppLogger.Info("WindowsToastService: initialized.");
        }
        catch (Exception ex)
        {
            AppLogger.Warning($"WindowsToastService: init failed (non-fatal): {ex.Message}");
        }
    }

    // ══════════════════════════════════════════════════
    // Public send methods
    // ══════════════════════════════════════════════════

    public void ShowInfo(string title, string body, string? detail = null)
        => Send(title, body, detail, ToastKind.Info);

    public void ShowSuccess(string title, string body, string? detail = null)
        => Send(title, body, detail, ToastKind.Success);

    public void ShowWarning(string title, string body, string? detail = null)
        => Send(title, body, detail, ToastKind.Warning);

    public void ShowDanger(string title, string body, string? detail = null)
        => Send(title, body, detail, ToastKind.Danger);

    // ── إشعارات جاهزة للسيناريوهات المتكررة ──────────

    public void NotifyBackupSuccess(string filePath, string size)
        => Send("تم النسخ الاحتياطي",
                "تم حفظ نسخة احتياطية بنجاح",
                $"{size} — {filePath}", ToastKind.Success);

    public void NotifyBackupFailed(string reason)
        => Send("فشل النسخ الاحتياطي", reason, null, ToastKind.Danger);

    public void NotifyTripAlert(string tripName, int daysLeft)
        => Send(daysLeft == 0 ? "رحلة اليوم" : $"رحلة بعد {daysLeft} يوم",
                tripName,
                "افتح قسم الرحلات للتفاصيل",
                daysLeft <= 1 ? ToastKind.Danger : ToastKind.Warning);

    public void NotifyLowBalance(string cashBoxName, decimal balance)
        => Send("رصيد منخفض",
                $"خزنة {cashBoxName}",
                $"الرصيد الحالي: {balance:N2} ج.م",
                ToastKind.Warning);

    public void NotifyOverdueInvoices(int count, decimal total)
        => Send($"{count} فاتورة متأخرة",
                $"اجمالي مستحق: {total:N2} ج.م",
                "افتح قسم الفواتير للمراجعة",
                ToastKind.Danger);

    public void NotifyUmrahAlert(string packageName, int daysLeft)
        => Send(daysLeft == 0 ? "حزمة عمرة اليوم" : $"عمرة بعد {daysLeft} يوم",
                packageName,
                "افتح قسم العمرة للتفاصيل",
                daysLeft <= 2 ? ToastKind.Danger : ToastKind.Warning);

    // ══════════════════════════════════════════════════
    // Core builder
    // ══════════════════════════════════════════════════
    private void Send(string title, string body, string? detail, ToastKind kind)
    {
        if (!_initialized) return;

        try
        {
            var builder = new ToastContentBuilder()
                .AddText(title)
                .AddText(body)
                .SetToastScenario(ToastScenario.Default)
                .AddAudio(new ToastAudio
                {
                    Src  = new Uri(GetSound(kind)),
                    Loop = false,
                })
                .AddButton(new ToastButton()
                    .SetContent("فتح البرنامج")
                    .AddArgument("action", "bring_to_front"));

            if (!string.IsNullOrWhiteSpace(detail))
                builder.AddText(detail);

            var iconPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources", "Icons", "ico.ico");

            if (File.Exists(iconPath))
                builder.AddAppLogoOverride(
                    new Uri("file:///" + iconPath.Replace('\\', '/')),
                    ToastGenericAppLogoCrop.Circle);

            builder.Show(t =>
            {
                t.Tag            = kind.ToString();
                t.Group          = AppId;
                t.ExpirationTime = DateTimeOffset.Now.AddSeconds(10);
            });

            AppLogger.Info($"Toast [{kind}]: {title}");
        }
        catch (Exception ex)
        {
            AppLogger.Warning($"Toast send failed: {ex.Message}");
        }
    }

    // ══════════════════════════════════════════════════
    // User clicked the toast — bring app to front
    // ══════════════════════════════════════════════════
    private static void OnToastActivated(ToastNotificationActivatedEventArgsCompat e)
    {
        try
        {
            var args = ToastArguments.Parse(e.Argument);
            if (args.TryGetValue("action", out var action) && action == "bring_to_front")
                BringAppToFront();
        }
        catch { }
    }

    private static void BringAppToFront()
    {
        var form = System.Windows.Forms.Application.OpenForms
            .OfType<System.Windows.Forms.Form>()
            .FirstOrDefault();
        if (form == null) return;

        void Restore()
        {
            form.WindowState = System.Windows.Forms.FormWindowState.Normal;
            form.Show();
            form.BringToFront();
            form.Activate();
        }

        if (form.InvokeRequired) form.BeginInvoke((Action)Restore);
        else Restore();
    }

    // ══════════════════════════════════════════════════
    // Helpers
    // ══════════════════════════════════════════════════
    private static string GetSound(ToastKind kind) => kind switch
    {
        ToastKind.Danger  => "ms-winsoundevent:Notification.Looping.Alarm2",
        ToastKind.Warning => "ms-winsoundevent:Notification.Default",
        _                 => "ms-winsoundevent:Notification.Default",
    };

    public void Dispose()
    {
        try { ToastNotificationManagerCompat.History.Clear(); } catch { }
    }
}

public enum ToastKind { Info, Success, Warning, Danger }

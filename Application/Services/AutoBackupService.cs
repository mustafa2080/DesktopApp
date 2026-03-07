using GraceWay.AccountingSystem.Application.Services.Backup;
using GraceWay.AccountingSystem.Infrastructure.Logging;

namespace GraceWay.AccountingSystem.Application.Services;

/// <summary>
/// خدمة جدولة النسخ الاحتياطية التلقائية
/// Singleton — تعيش طول عمر التطبيق
/// </summary>
public class AutoBackupService : IDisposable
{
    private readonly IBackupService _backupService;
    private System.Windows.Forms.Timer? _timer;
    private AutoBackupSettings _settings;
    private DateTime _lastBackupTime = DateTime.MinValue;

    public event EventHandler<string>? BackupCompleted;  // (message)
    public event EventHandler<string>? BackupFailed;

    public AutoBackupSettings Settings => _settings;

    public AutoBackupService(IBackupService backupService)
    {
        _backupService = backupService;
        _settings      = AutoBackupSettings.Load();
    }

    // ── بدء الجدولة ────────────────────────────────────────────
    public void Start()
    {
        if (!_settings.Enabled) return;
        _timer?.Dispose();
        _timer = new System.Windows.Forms.Timer
        {
            Interval = 60_000 // فحص كل دقيقة
        };
        _timer.Tick += async (_, _) => await CheckAndBackupAsync();
        _timer.Start();
        AppLogger.Info($"AutoBackup started — interval: {_settings.IntervalHours}h, path: {_settings.BackupPath}");
    }

    public void Stop()
    {
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
    }

    public void ApplySettings(AutoBackupSettings settings)
    {
        _settings = settings;
        settings.Save();
        Stop();
        if (settings.Enabled) Start();
    }

    // ── منطق الفحص ─────────────────────────────────────────────
    private async Task CheckAndBackupAsync()
    {
        if (!_settings.Enabled) return;

        var now          = DateTime.Now;
        bool timeReached = (now - _lastBackupTime).TotalHours >= _settings.IntervalHours;

        // تحقق من وقت محدد في اليوم إن كان المستخدم اختار ذلك
        if (_settings.UseScheduledTime)
        {
            var scheduled = now.Date.Add(_settings.ScheduledTime);
            bool inWindow = now >= scheduled && now < scheduled.AddMinutes(2);
            if (!inWindow) return;
            if (_lastBackupTime.Date == now.Date) return; // مرة واحدة في اليوم
        }
        else if (!timeReached) return;

        await RunBackupAsync();
    }

    public async Task RunBackupAsync()
    {
        try
        {
            AppLogger.Info("AutoBackup: starting...");
            var result = await _backupService.CreateAutoBackupAsync();

            if (result.Success)
            {
                _lastBackupTime = DateTime.Now;
                string msg = $"✅ نسخة احتياطية تلقائية — {DateTime.Now:dd/MM HH:mm} — {FormatSize(result.FileSizeBytes)}";
                AppLogger.Info($"AutoBackup success: {result.BackupFilePath}");
                BackupCompleted?.Invoke(this, msg);
                await CleanOldBackupsAsync();
            }
            else
            {
                AppLogger.Warning($"AutoBackup failed: {result.Message}");
                BackupFailed?.Invoke(this, result.Message);
            }
        }
        catch (Exception ex)
        {
            AppLogger.Error("AutoBackup exception", ex);
            BackupFailed?.Invoke(this, ex.Message);
        }
    }

    // ── حذف النسخ القديمة ──────────────────────────────────────
    private async Task CleanOldBackupsAsync()
    {
        if (_settings.MaxBackupsToKeep <= 0) return;
        try
        {
            var all = await _backupService.GetBackupHistoryAsync();
            var autoBackups = all
                .Where(b => b.IsAutoBackup)
                .OrderByDescending(b => b.BackupDate)
                .ToList();

            var toDelete = autoBackups.Skip(_settings.MaxBackupsToKeep).ToList();
            foreach (var old in toDelete)
            {
                await _backupService.DeleteBackupAsync(old.BackupId);
                AppLogger.Info($"AutoBackup: deleted old backup #{old.BackupId} ({old.BackupFileName})");
            }
        }
        catch (Exception ex)
        {
            AppLogger.Warning($"AutoBackup cleanup error: {ex.Message}");
        }
    }

    private static string FormatSize(long bytes)
    {
        if (bytes >= 1_048_576) return $"{bytes / 1_048_576.0:N1} MB";
        if (bytes >= 1024)      return $"{bytes / 1024.0:N0} KB";
        return $"{bytes} B";
    }

    public void Dispose() { Stop(); }
}

// ══════════════════════════════════════════════════════════════
// إعدادات النسخ التلقائي — تُحفظ في ملف JSON
// ══════════════════════════════════════════════════════════════
public class AutoBackupSettings
{
    public bool    Enabled          { get; set; } = true;
    public int     IntervalHours    { get; set; } = 24;
    public bool    UseScheduledTime { get; set; } = true;
    public TimeSpan ScheduledTime   { get; set; } = new TimeSpan(23, 0, 0); // 11 PM
    public string  BackupPath       { get; set; } = DefaultPath();
    public int     MaxBackupsToKeep { get; set; } = 7;

    private static readonly string ConfigFile = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "GraceWay", "autobackup.json");

    public static AutoBackupSettings Load()
    {
        try
        {
            if (File.Exists(ConfigFile))
            {
                var json = File.ReadAllText(ConfigFile);
                return System.Text.Json.JsonSerializer.Deserialize<AutoBackupSettings>(json)
                       ?? new AutoBackupSettings();
            }
        }
        catch { }
        return new AutoBackupSettings();
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigFile)!);
            var json = System.Text.Json.JsonSerializer.Serialize(this,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFile, json);
        }
        catch { }
    }

    private static string DefaultPath() => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "GraceWay Accounting", "Backups");
}

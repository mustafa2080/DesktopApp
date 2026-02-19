namespace GraceWay.AccountingSystem.Infrastructure.Logging;

/// <summary>
/// نظام تسجيل الأحداث - يكتب في ملفات يومية مع Rotation تلقائي
/// Thread-safe - آمن للاستخدام من أي Thread
/// </summary>
public static class AppLogger
{
    private static readonly object _lock = new();
    private static string _logDirectory = string.Empty;
    private static LogLevel _minimumLevel = LogLevel.Info;
    private static bool _initialized = false;

    public static void Initialize(LogLevel minimumLevel = LogLevel.Info)
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _logDirectory = Path.Combine(appData, "GraceWay", "AccountingSystem", "Logs");
        Directory.CreateDirectory(_logDirectory);
        _minimumLevel = minimumLevel;
        _initialized = true;

        // حذف ملفات اللوج القديمة (أكثر من 30 يوم)
        CleanOldLogs();

        Info("=== Logger Initialized ===");
        Info($"Log directory: {_logDirectory}");
    }

    public static void Info(string message) => Write(LogLevel.Info, message, null);
    public static void Warning(string message) => Write(LogLevel.Warning, message, null);
    public static void Error(string message, Exception? ex = null) => Write(LogLevel.Error, message, ex);
    public static void Fatal(string message, Exception? ex = null) => Write(LogLevel.Fatal, message, ex);
    public static void Debug(string message) => Write(LogLevel.Debug, message, null);

    private static void Write(LogLevel level, string message, Exception? ex)
    {
        if (level < _minimumLevel) return;

        // لو مش initialized، نكتب في debug output فقط
        if (!_initialized)
        {
            System.Diagnostics.Debug.WriteLine($"[{level}] {message}");
            return;
        }

        try
        {
            var logEntry = FormatEntry(level, message, ex);

            lock (_lock)
            {
                var logFile = GetCurrentLogFile();
                File.AppendAllText(logFile, logEntry + Environment.NewLine);
            }

            // Error و Fatal يطلعوا في Debug Output كمان
            if (level >= LogLevel.Error)
                System.Diagnostics.Debug.WriteLine(logEntry);
        }
        catch
        {
            // Logger لازم ميطلعش exception - silent fail
        }
    }

    private static string FormatEntry(LogLevel level, string message, Exception? ex)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var levelStr = level.ToString().ToUpper().PadRight(7);
        var thread = System.Threading.Thread.CurrentThread.ManagedThreadId;
        var entry = $"[{timestamp}] [{levelStr}] [T:{thread:D3}] {message}";

        if (ex != null)
        {
            entry += Environment.NewLine + $"  Exception: {ex.GetType().FullName}: {ex.Message}";
            if (ex.InnerException != null)
                entry += Environment.NewLine + $"  Inner: {ex.InnerException.Message}";
            entry += Environment.NewLine + $"  StackTrace: {ex.StackTrace}";
        }

        return entry;
    }

    private static string GetCurrentLogFile()
    {
        var fileName = $"graceway_{DateTime.Now:yyyy-MM-dd}.log";
        return Path.Combine(_logDirectory, fileName);
    }

    private static void CleanOldLogs()
    {
        try
        {
            var cutoff = DateTime.Now.AddDays(-30);
            var logFiles = Directory.GetFiles(_logDirectory, "graceway_*.log");
            foreach (var file in logFiles)
            {
                if (File.GetCreationTime(file) < cutoff)
                    File.Delete(file);
            }
        }
        catch { }
    }

    /// <summary>
    /// مسار مجلد اللوجز - للعرض في الإعدادات
    /// </summary>
    public static string LogDirectory => _logDirectory;

    /// <summary>
    /// مسار ملف اللوج الحالي
    /// </summary>
    public static string CurrentLogFile => GetCurrentLogFile();
}

public enum LogLevel
{
    Debug = 0,
    Info = 1,
    Warning = 2,
    Error = 3,
    Fatal = 4
}

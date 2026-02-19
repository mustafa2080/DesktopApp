using System.Text.Json;

namespace GraceWay.AccountingSystem.Infrastructure.Configuration;

public class AppConfiguration
{
    private static AppConfiguration? _instance;
    private string? _cachedConnectionString;

    private AppConfiguration() { }

    public static AppConfiguration Instance
    {
        get
        {
            _instance ??= new AppConfiguration();
            return _instance;
        }
    }

    /// <summary>
    /// يرجع الـ Connection String - أولوية: DPAPI Config > appsettings.json
    /// </summary>
    public string GetConnectionString()
    {
        if (_cachedConnectionString != null)
            return _cachedConnectionString;

        // 1. حاول تحميل الإعدادات المشفرة أولاً
        var secureConfig = SecureConfigManager.LoadConfig();
        if (secureConfig != null)
        {
            _cachedConnectionString = SecureConfigManager.BuildConnectionString(secureConfig);
            return _cachedConnectionString;
        }

        // 2. fallback: appsettings.json (للـ development فقط)
        _cachedConnectionString = LoadFromAppSettings();
        return _cachedConnectionString;
    }

    /// <summary>
    /// تحديث الـ Cache بعد حفظ إعدادات جديدة
    /// </summary>
    public void RefreshConnectionString()
    {
        _cachedConnectionString = null;
    }

    private static string LoadFromAppSettings()
    {
        try
        {
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            var settingsPath = Path.Combine(appPath, "appsettings.json");

            if (!File.Exists(settingsPath))
            {
                var projectRoot = Directory.GetParent(appPath)?.Parent?.Parent?.Parent?.FullName;
                if (projectRoot != null)
                    settingsPath = Path.Combine(projectRoot, "appsettings.json");
            }

            if (File.Exists(settingsPath))
            {
                var jsonString = File.ReadAllText(settingsPath);
                var document = JsonDocument.Parse(jsonString);

                if (document.RootElement.TryGetProperty("ConnectionStrings", out var connStrings)
                    && connStrings.TryGetProperty("DefaultConnection", out var defaultConn))
                {
                    var conn = defaultConn.GetString();
                    if (!string.IsNullOrEmpty(conn))
                        return conn;
                }
            }
        }
        catch { /* يكمل للـ fallback */ }

        // fallback نهائي بدون كلمة مرور - سيفشل الاتصال ويطلب الإعداد
        return "Host=localhost;Port=5432;Database=graceway_accounting;Username=postgres;Password=";
    }

    public T? GetSetting<T>(string sectionName, string key, T? defaultValue = default)
    {
        try
        {
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            var settingsPath = Path.Combine(appPath, "appsettings.json");

            if (!File.Exists(settingsPath))
            {
                var projectRoot = Directory.GetParent(appPath)?.Parent?.Parent?.Parent?.FullName;
                if (projectRoot != null)
                    settingsPath = Path.Combine(projectRoot, "appsettings.json");
            }

            if (!File.Exists(settingsPath)) return defaultValue;

            var document = JsonDocument.Parse(File.ReadAllText(settingsPath));

            if (document.RootElement.TryGetProperty(sectionName, out var section)
                && section.TryGetProperty(key, out var value))
            {
                if (typeof(T) == typeof(string)) return (T)(object)value.GetString()!;
                if (typeof(T) == typeof(int)) return (T)(object)value.GetInt32();
                if (typeof(T) == typeof(bool)) return (T)(object)value.GetBoolean();
                if (typeof(T) == typeof(decimal)) return (T)(object)value.GetDecimal();
            }
        }
        catch { }

        return defaultValue;
    }
}

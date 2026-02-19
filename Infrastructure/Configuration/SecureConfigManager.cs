using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GraceWay.AccountingSystem.Infrastructure.Configuration;

/// <summary>
/// يدير تشفير وفك تشفير إعدادات الاتصال بقاعدة البيانات
/// باستخدام Windows DPAPI - مرتبط بمستخدم الويندوز الحالي
/// </summary>
public static class SecureConfigManager
{
    private static readonly string ConfigFileName = "db.config";
    
    private static string ConfigFilePath
    {
        get
        {
            // حفظ الإعدادات في AppData\Roaming\GraceWay
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var folder = Path.Combine(appData, "GraceWay", "AccountingSystem");
            Directory.CreateDirectory(folder);
            return Path.Combine(folder, ConfigFileName);
        }
    }

    /// <summary>
    /// هل يوجد إعداد محفوظ؟
    /// </summary>
    public static bool HasSavedConfig()
    {
        return File.Exists(ConfigFilePath);
    }

    /// <summary>
    /// حفظ إعدادات الاتصال مشفرة
    /// </summary>
    public static void SaveConfig(DatabaseConfig config)
    {
        var json = JsonSerializer.Serialize(config);
        var plainBytes = Encoding.UTF8.GetBytes(json);
        
        // تشفير بـ DPAPI - CurrentUser scope
        var encryptedBytes = ProtectedData.Protect(
            plainBytes, 
            GetEntropy(), 
            DataProtectionScope.CurrentUser);
        
        File.WriteAllBytes(ConfigFilePath, encryptedBytes);
    }

    /// <summary>
    /// تحميل إعدادات الاتصال وفك تشفيرها
    /// </summary>
    public static DatabaseConfig? LoadConfig()
    {
        if (!HasSavedConfig()) return null;

        try
        {
            var encryptedBytes = File.ReadAllBytes(ConfigFilePath);
            var plainBytes = ProtectedData.Unprotect(
                encryptedBytes, 
                GetEntropy(), 
                DataProtectionScope.CurrentUser);
            
            var json = Encoding.UTF8.GetString(plainBytes);
            return JsonSerializer.Deserialize<DatabaseConfig>(json);
        }
        catch
        {
            // لو الملف فسد أو مش من نفس المستخدم، احذفه وابدأ من أول
            File.Delete(ConfigFilePath);
            return null;
        }
    }

    /// <summary>
    /// بناء Connection String من الإعدادات
    /// </summary>
    public static string BuildConnectionString(DatabaseConfig config)
    {
        return $"Host={config.Host};" +
               $"Port={config.Port};" +
               $"Database={config.DatabaseName};" +
               $"Username={config.Username};" +
               $"Password={config.Password};" +
               $"Timeout=60;" +
               $"Command Timeout=60;" +
               $"Pooling=true;" +
               $"MinPoolSize=5;" +
               $"MaxPoolSize=50;" +
               $"Connection Idle Lifetime=300;" +
               $"No Reset On Close=false";
    }

    /// <summary>
    /// حذف الإعدادات المحفوظة
    /// </summary>
    public static void DeleteConfig()
    {
        if (File.Exists(ConfigFilePath))
            File.Delete(ConfigFilePath);
    }

    // Entropy ثابتة لتمييز برنامجنا
    private static byte[] GetEntropy()
    {
        return Encoding.UTF8.GetBytes("GraceWay.AccountingSystem.v1.SecureKey@2026");
    }
}

/// <summary>
/// إعدادات الاتصال بقاعدة البيانات
/// </summary>
public class DatabaseConfig
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5432;
    public string DatabaseName { get; set; } = "graceway_accounting";
    public string Username { get; set; } = "postgres";
    public string Password { get; set; } = string.Empty;
}

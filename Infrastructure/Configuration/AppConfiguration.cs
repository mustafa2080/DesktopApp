using System.Text.Json;

namespace GraceWay.AccountingSystem.Infrastructure.Configuration;

public class AppConfiguration
{
    private static AppConfiguration? _instance;
    private readonly Dictionary<string, JsonElement> _settings;

    private AppConfiguration()
    {
        _settings = new Dictionary<string, JsonElement>();
        LoadSettings();
    }

    public static AppConfiguration Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new AppConfiguration();
            }
            return _instance;
        }
    }

    private void LoadSettings()
    {
        try
        {
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            Console.WriteLine($"App base directory: {appPath}");
            var settingsPath = Path.Combine(appPath, "appsettings.json");
            Console.WriteLine($"Looking for appsettings.json at: {settingsPath}");

            if (!File.Exists(settingsPath))
            {
                Console.WriteLine("File not found at base directory, trying parent directories...");
                // Try parent directories
                var projectRoot = Directory.GetParent(appPath)?.Parent?.Parent?.Parent?.FullName;
                if (projectRoot != null)
                {
                    settingsPath = Path.Combine(projectRoot, "appsettings.json");
                    Console.WriteLine($"Trying: {settingsPath}");
                }
            }

            if (File.Exists(settingsPath))
            {
                Console.WriteLine($"Found appsettings.json at: {settingsPath}");
                var jsonString = File.ReadAllText(settingsPath);
                Console.WriteLine($"File content length: {jsonString.Length} characters");
                var document = JsonDocument.Parse(jsonString);
                
                foreach (var property in document.RootElement.EnumerateObject())
                {
                    _settings[property.Name] = property.Value.Clone();
                    Console.WriteLine($"Loaded setting: {property.Name}");
                }
                Console.WriteLine($"Total settings loaded: {_settings.Count}");
            }
            else
            {
                Console.WriteLine("appsettings.json not found anywhere!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading settings: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
        }
    }

    public string GetConnectionString()
    {
        try
        {
            Console.WriteLine("Getting connection string from settings...");
            if (_settings.TryGetValue("ConnectionStrings", out var connStrings))
            {
                Console.WriteLine("Found ConnectionStrings section");
                if (connStrings.TryGetProperty("DefaultConnection", out var defaultConn))
                {
                    var connStr = defaultConn.GetString();
                    Console.WriteLine($"Found connection string: {connStr}");
                    return connStr ?? GetDefaultConnectionString();
                }
                else
                {
                    Console.WriteLine("DefaultConnection property not found");
                }
            }
            else
            {
                Console.WriteLine("ConnectionStrings section not found in settings");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading connection string: {ex.Message}");
            // Fall through to default
        }

        Console.WriteLine("Using default connection string");
        return GetDefaultConnectionString();
    }

    private string GetDefaultConnectionString()
    {
        // استخدام PostgreSQL
        return "Host=localhost;Port=5432;Database=graceway_accounting;Username=postgres;Password=123456";
    }

    public T? GetSetting<T>(string sectionName, string key, T? defaultValue = default)
    {
        try
        {
            if (_settings.TryGetValue(sectionName, out var section))
            {
                if (section.TryGetProperty(key, out var value))
                {
                    if (typeof(T) == typeof(string))
                        return (T)(object)value.GetString()!;
                    if (typeof(T) == typeof(int))
                        return (T)(object)value.GetInt32();
                    if (typeof(T) == typeof(bool))
                        return (T)(object)value.GetBoolean();
                    if (typeof(T) == typeof(decimal))
                        return (T)(object)value.GetDecimal();
                }
            }
        }
        catch (Exception)
        {
            // Return default value
        }

        return defaultValue;
    }
}

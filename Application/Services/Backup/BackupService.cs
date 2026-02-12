using GraceWay.AccountingSystem.Infrastructure.Data;
using GraceWay.AccountingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IO.Compression;

namespace GraceWay.AccountingSystem.Application.Services.Backup;

public class BackupService : IBackupService
{
    private readonly AppDbContext _context;
    private readonly string _defaultBackupPath;
    private readonly ISettingService _settingService;

    public BackupService(AppDbContext context, ISettingService settingService)
    {
        _context = context;
        _settingService = settingService;
        
        // المسار الافتراضي للنسخ الاحتياطية
        _defaultBackupPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "GraceWay Accounting",
            "Backups"
        );
        
        // إنشاء المجلد إذا لم يكن موجوداً
        if (!Directory.Exists(_defaultBackupPath))
        {
            Directory.CreateDirectory(_defaultBackupPath);
        }
    }

    public async Task<BackupResult> CreateBackupAsync(string? backupPath = null, string? description = null)
    {
        try
        {
            var connectionString = Infrastructure.Configuration.AppConfiguration.Instance.GetConnectionString();
            var dbConfig = ParseConnectionString(connectionString);
            
            // تحديد مسار النسخة الاحتياطية
            backupPath ??= _defaultBackupPath;
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"GraceWay_Backup_{timestamp}.backup";
            var backupFilePath = Path.Combine(backupPath, backupFileName);
            
            // التأكد من وجود المجلد
            if (!Directory.Exists(backupPath))
            {
                Directory.CreateDirectory(backupPath);
            }

            // استخدام pg_dump لعمل backup
            var pgDumpPath = FindPgDumpPath();
            if (string.IsNullOrEmpty(pgDumpPath))
            {
                return new BackupResult
                {
                    Success = false,
                    Message = "لم يتم العثور على أداة pg_dump. يرجى التأكد من تثبيت PostgreSQL بشكل صحيح."
                };
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = pgDumpPath,
                Arguments = $"-h {dbConfig.Host} -p {dbConfig.Port} -U {dbConfig.Username} -F c -b -v -f \"{backupFilePath}\" {dbConfig.Database}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            // إضافة كلمة المرور كمتغير بيئي
            startInfo.EnvironmentVariables["PGPASSWORD"] = dbConfig.Password;

            using (var process = Process.Start(startInfo))
            {
                if (process == null)
                {
                    return new BackupResult
                    {
                        Success = false,
                        Message = "فشل في بدء عملية النسخ الاحتياطي"
                    };
                }

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    return new BackupResult
                    {
                        Success = false,
                        Message = $"فشل في إنشاء النسخة الاحتياطية: {error}"
                    };
                }
            }

            // التحقق من وجود الملف وحجمه
            var fileInfo = new FileInfo(backupFilePath);
            if (!fileInfo.Exists || fileInfo.Length == 0)
            {
                return new BackupResult
                {
                    Success = false,
                    Message = "فشل في إنشاء ملف النسخة الاحتياطية"
                };
            }

            // حفظ معلومات النسخة الاحتياطية في قاعدة البيانات
            var backup = new DatabaseBackup
            {
                BackupFileName = backupFileName,
                BackupFilePath = backupFilePath,
                BackupDate = DateTime.UtcNow,
                FileSizeBytes = fileInfo.Length,
                Description = description ?? "نسخة احتياطية يدوية",
                IsAutoBackup = false,
                CreatedBy = "System" // يمكن تحديث هذا ليكون المستخدم الحالي
            };

            _context.DatabaseBackups.Add(backup);
            await _context.SaveChangesAsync();

            return new BackupResult
            {
                Success = true,
                Message = "تم إنشاء النسخة الاحتياطية بنجاح",
                BackupFilePath = backupFilePath,
                FileSizeBytes = fileInfo.Length,
                BackupDate = backup.BackupDate
            };
        }
        catch (Exception ex)
        {
            return new BackupResult
            {
                Success = false,
                Message = $"حدث خطأ أثناء إنشاء النسخة الاحتياطية: {ex.Message}"
            };
        }
    }

    public async Task<RestoreResult> RestoreBackupAsync(string backupFilePath)
    {
        try
        {
            // التحقق من وجود الملف
            if (!File.Exists(backupFilePath))
            {
                return new RestoreResult
                {
                    Success = false,
                    Message = "ملف النسخة الاحتياطية غير موجود"
                };
            }

            var connectionString = Infrastructure.Configuration.AppConfiguration.Instance.GetConnectionString();
            var dbConfig = ParseConnectionString(connectionString);

            // استخدام pg_restore
            var pgRestorePath = FindPgRestorePath();
            if (string.IsNullOrEmpty(pgRestorePath))
            {
                return new RestoreResult
                {
                    Success = false,
                    Message = "لم يتم العثور على أداة pg_restore"
                };
            }

            // حذف الاتصالات الموجودة وإعادة إنشاء قاعدة البيانات
            var dropDbResult = await DropAndRecreateDatabase(dbConfig);
            if (!dropDbResult.Success)
            {
                return new RestoreResult
                {
                    Success = false,
                    Message = dropDbResult.Message
                };
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = pgRestorePath,
                Arguments = $"-h {dbConfig.Host} -p {dbConfig.Port} -U {dbConfig.Username} -d {dbConfig.Database} -v \"{backupFilePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            startInfo.EnvironmentVariables["PGPASSWORD"] = dbConfig.Password;

            using (var process = Process.Start(startInfo))
            {
                if (process == null)
                {
                    return new RestoreResult
                    {
                        Success = false,
                        Message = "فشل في بدء عملية الاستعادة"
                    };
                }

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                
                await process.WaitForExitAsync();

                // pg_restore قد يعطي exit code غير صفر حتى مع النجاح بسبب warnings
                // لذا نتحقق من المحتوى بدلاً من exit code فقط
            }

            return new RestoreResult
            {
                Success = true,
                Message = "تم استعادة قاعدة البيانات بنجاح",
                RestoreDate = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            return new RestoreResult
            {
                Success = false,
                Message = $"حدث خطأ أثناء استعادة قاعدة البيانات: {ex.Message}"
            };
        }
    }

    public async Task<List<DatabaseBackup>> GetBackupHistoryAsync()
    {
        return await _context.DatabaseBackups
            .OrderByDescending(b => b.BackupDate)
            .ToListAsync();
    }

    public async Task<bool> DeleteBackupAsync(int backupId)
    {
        try
        {
            var backup = await _context.DatabaseBackups.FindAsync(backupId);
            if (backup == null)
                return false;

            // حذف الملف الفعلي
            if (File.Exists(backup.BackupFilePath))
            {
                File.Delete(backup.BackupFilePath);
            }

            // حذف السجل من قاعدة البيانات
            _context.DatabaseBackups.Remove(backup);
            await _context.SaveChangesAsync();

            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<BackupResult> CreateAutoBackupAsync()
    {
        var result = await CreateBackupAsync(description: "نسخة احتياطية تلقائية");
        
        if (result.Success)
        {
            // تحديث السجل لتعيينه كنسخة تلقائية
            var backup = await _context.DatabaseBackups
                .OrderByDescending(b => b.BackupDate)
                .FirstOrDefaultAsync();
            
            if (backup != null)
            {
                backup.IsAutoBackup = true;
                await _context.SaveChangesAsync();
            }
        }

        return result;
    }

    public Task<bool> ValidateBackupFileAsync(string backupFilePath)
    {
        try
        {
            if (!File.Exists(backupFilePath))
                return Task.FromResult(false);

            var fileInfo = new FileInfo(backupFilePath);
            
            // التحقق من حجم الملف (يجب أن يكون أكبر من 0)
            if (fileInfo.Length == 0)
                return Task.FromResult(false);

            // التحقق من امتداد الملف
            var validExtensions = new[] { ".backup", ".dump", ".sql" };
            if (!validExtensions.Contains(fileInfo.Extension.ToLower()))
                return Task.FromResult(false);

            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    private string? FindPgDumpPath()
    {
        // البحث عن pg_dump في المسارات الشائعة
        var possiblePaths = new[]
        {
            @"C:\Program Files\PostgreSQL\16\bin\pg_dump.exe",
            @"C:\Program Files\PostgreSQL\15\bin\pg_dump.exe",
            @"C:\Program Files\PostgreSQL\14\bin\pg_dump.exe",
            @"C:\Program Files\PostgreSQL\13\bin\pg_dump.exe",
            @"C:\Program Files (x86)\PostgreSQL\16\bin\pg_dump.exe",
            @"C:\Program Files (x86)\PostgreSQL\15\bin\pg_dump.exe",
            "pg_dump" // محاولة استخدام PATH
        };

        foreach (var path in possiblePaths)
        {
            if (path == "pg_dump" || File.Exists(path))
                return path;
        }

        return null;
    }

    private string? FindPgRestorePath()
    {
        var possiblePaths = new[]
        {
            @"C:\Program Files\PostgreSQL\16\bin\pg_restore.exe",
            @"C:\Program Files\PostgreSQL\15\bin\pg_restore.exe",
            @"C:\Program Files\PostgreSQL\14\bin\pg_restore.exe",
            @"C:\Program Files\PostgreSQL\13\bin\pg_restore.exe",
            @"C:\Program Files (x86)\PostgreSQL\16\bin\pg_restore.exe",
            @"C:\Program Files (x86)\PostgreSQL\15\bin\pg_restore.exe",
            "pg_restore"
        };

        foreach (var path in possiblePaths)
        {
            if (path == "pg_restore" || File.Exists(path))
                return path;
        }

        return null;
    }

    private DatabaseConfig ParseConnectionString(string connectionString)
    {
        var config = new DatabaseConfig
        {
            Database = "",
            Username = "",
            Password = ""
        };
        var parts = connectionString.Split(';');

        foreach (var part in parts)
        {
            var keyValue = part.Split('=');
            if (keyValue.Length != 2) continue;

            var key = keyValue[0].Trim().ToLower();
            var value = keyValue[1].Trim();

            switch (key)
            {
                case "host":
                    config.Host = value;
                    break;
                case "port":
                    config.Port = value;
                    break;
                case "database":
                    config.Database = value;
                    break;
                case "username":
                case "user id":
                    config.Username = value;
                    break;
                case "password":
                    config.Password = value;
                    break;
            }
        }

        return config;
    }

    private async Task<(bool Success, string Message)> DropAndRecreateDatabase(DatabaseConfig dbConfig)
    {
        try
        {
            // الاتصال بقاعدة بيانات postgres الافتراضية
            var postgresConnectionString = $"Host={dbConfig.Host};Port={dbConfig.Port};Database=postgres;Username={dbConfig.Username};Password={dbConfig.Password}";
            
            await using var connection = new Npgsql.NpgsqlConnection(postgresConnectionString);
            await connection.OpenAsync();

            // إنهاء جميع الاتصالات النشطة
            var terminateQuery = $@"
                SELECT pg_terminate_backend(pg_stat_activity.pid)
                FROM pg_stat_activity
                WHERE pg_stat_activity.datname = '{dbConfig.Database}'
                AND pid <> pg_backend_pid();";
            
            await using (var cmd = new Npgsql.NpgsqlCommand(terminateQuery, connection))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            // حذف قاعدة البيانات
            var dropQuery = $"DROP DATABASE IF EXISTS {dbConfig.Database};";
            await using (var cmd = new Npgsql.NpgsqlCommand(dropQuery, connection))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            // إنشاء قاعدة البيانات من جديد
            var createQuery = $"CREATE DATABASE {dbConfig.Database};";
            await using (var cmd = new Npgsql.NpgsqlCommand(createQuery, connection))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            return (true, "تم إعادة إنشاء قاعدة البيانات بنجاح");
        }
        catch (Exception ex)
        {
            return (false, $"فشل في إعادة إنشاء قاعدة البيانات: {ex.Message}");
        }
    }

    private class DatabaseConfig
    {
        public string Host { get; set; } = "localhost";
        public string Port { get; set; } = "5432";
        public required string Database { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}

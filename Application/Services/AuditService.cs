using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace GraceWay.AccountingSystem.Application.Services;

public interface IAuditService
{
    Task LogAsync(AuditAction action, string entityType, int? entityId, string entityName, string description, object? oldValues = null, object? newValues = null);
    Task<List<AuditLog>> GetLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, int? userId = null, string? entityType = null);
    Task<List<AuditLog>> GetEntityLogsAsync(string entityType, int entityId);
    Task<List<AuditLog>> GetUserLogsAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<bool> DeleteLogAsync(int auditLogId);
    Task<bool> DeleteAllLogsAsync();
}

public class AuditService : IAuditService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IAuthService _authService;

    public AuditService(IDbContextFactory<AppDbContext> contextFactory, IAuthService authService)
    {
        _contextFactory = contextFactory;
        _authService = authService;
    }

    public async Task LogAsync(
        AuditAction action,
        string entityType,
        int? entityId,
        string entityName,
        string description,
        object? oldValues = null,
        object? newValues = null)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            
            var currentUser = _authService.CurrentUser;
            if (currentUser == null)
            {
                Console.WriteLine("⚠️ AuditLog: No current user, skipping log");
                return;
            }

            var auditLog = new AuditLog
            {
                UserId = currentUser.UserId,
                Username = currentUser.Username,
                UserFullName = currentUser.FullName,
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                EntityName = entityName,
                Description = description,
                OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
                Timestamp = DateTime.Now,
                MachineName = Environment.MachineName
            };

            context.AuditLogs.Add(auditLog);
            await context.SaveChangesAsync();

            Console.WriteLine($"✓ Audit: {currentUser.FullName} - {action} - {entityType}: {entityName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ AuditLog Error: {ex.Message}");
            // Don't throw - audit logging shouldn't break the app
        }
    }

    public async Task<List<AuditLog>> GetLogsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int? userId = null,
        string? entityType = null)
    {
        using var context = _contextFactory.CreateDbContext();
        
        var query = context.AuditLogs
            .Include(a => a.User)
            .AsNoTracking()
            .AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(a => a.Timestamp >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(a => a.Timestamp <= toDate.Value);

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(a => a.EntityType == entityType);

        return await query.OrderByDescending(a => a.Timestamp).ToListAsync();
    }

    public async Task<List<AuditLog>> GetEntityLogsAsync(string entityType, int entityId)
    {
        using var context = _contextFactory.CreateDbContext();
        
        return await context.AuditLogs
            .Include(a => a.User)
            .AsNoTracking()
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetUserLogsAsync(int userId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        using var context = _contextFactory.CreateDbContext();
        
        var query = context.AuditLogs
            .AsNoTracking()
            .Where(a => a.UserId == userId);

        if (fromDate.HasValue)
            query = query.Where(a => a.Timestamp >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(a => a.Timestamp <= toDate.Value);

        return await query.OrderByDescending(a => a.Timestamp).ToListAsync();
    }

    public async Task<bool> DeleteLogAsync(int auditLogId)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            
            var log = await context.AuditLogs.FindAsync(auditLogId);
            if (log == null)
                return false;

            context.AuditLogs.Remove(log);
            await context.SaveChangesAsync();

            Console.WriteLine($"✓ Deleted AuditLog ID: {auditLogId}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ DeleteLog Error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteAllLogsAsync()
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            
            var allLogs = await context.AuditLogs.ToListAsync();
            int count = allLogs.Count;

            context.AuditLogs.RemoveRange(allLogs);
            await context.SaveChangesAsync();

            Console.WriteLine($"✓ Deleted ALL AuditLogs: {count} records removed");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ DeleteAllLogs Error: {ex.Message}");
            return false;
        }
    }
}

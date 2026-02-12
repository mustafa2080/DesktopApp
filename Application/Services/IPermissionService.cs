using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GraceWay.AccountingSystem.Application.Services;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(int userId, PermissionType permission);
    Task<bool> HasAnyPermissionAsync(int userId, params PermissionType[] permissions);
    Task<bool> HasAllPermissionsAsync(int userId, params PermissionType[] permissions);
    Task<List<PermissionType>> GetUserPermissionsAsync(int userId);
    Task<Dictionary<string, List<PermissionType>>> GetUserPermissionsByModuleAsync(int userId);
}

public class PermissionService : IPermissionService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    
    public PermissionService(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }
    
    public async Task<bool> HasPermissionAsync(int userId, PermissionType permission)
    {
        var permissions = await GetUserPermissionsAsync(userId);
        return permissions.Contains(permission);
    }
    
    public async Task<bool> HasAnyPermissionAsync(int userId, params PermissionType[] permissions)
    {
        if (permissions == null || permissions.Length == 0)
            return true;
        
        var userPermissions = await GetUserPermissionsAsync(userId);
        return permissions.Any(p => userPermissions.Contains(p));
    }
    
    public async Task<bool> HasAllPermissionsAsync(int userId, params PermissionType[] permissions)
    {
        if (permissions == null || permissions.Length == 0)
            return true;
        
        var userPermissions = await GetUserPermissionsAsync(userId);
        return permissions.All(p => userPermissions.Contains(p));
    }
    
    public async Task<List<PermissionType>> GetUserPermissionsAsync(int userId)
    {
        Console.WriteLine($"🔍 GetUserPermissionsAsync called for userId: {userId}");
        
        // ✅ ALWAYS create fresh DbContext to ensure latest data
        using var context = _contextFactory.CreateDbContext();
        
        Console.WriteLine($"📊 Loading permissions from database for user {userId} (fresh DbContext)");
        
        var user = await context.Users
            .AsNoTracking() // ✅ Don't track this query to ensure fresh data
            .Include(u => u.Role)
                .ThenInclude(r => r!.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.UserId == userId);
        
        if (user == null)
        {
            Console.WriteLine($"⚠️ User {userId} NOT FOUND in database!");
            return new List<PermissionType>();
        }
        
        if (user.Role == null)
        {
            Console.WriteLine($"⚠️ User {userId} has NO ROLE assigned!");
            return new List<PermissionType>();
        }
        
        Console.WriteLine($"✓ User {userId} found - Role: {user.Role.RoleName}");
        
        var permissions = user.Role.RolePermissions
            .Select(rp => rp.Permission!.PermissionType)
            .ToList();
        
        Console.WriteLine($"✓ Loaded {permissions.Count} permissions for user {userId}");
        Console.WriteLine($"  Permissions: {string.Join(", ", permissions.Take(5))}...");
        
        return permissions;
    }
    
    public async Task<Dictionary<string, List<PermissionType>>> GetUserPermissionsByModuleAsync(int userId)
    {
        var permissions = await GetUserPermissionsAsync(userId);
        
        // ✅ Create fresh DbContext here too
        using var context = _contextFactory.CreateDbContext();
        
        var permissionDetails = await context.Permissions
            .AsNoTracking()
            .Where(p => permissions.Contains(p.PermissionType))
            .ToListAsync();
        
        return permissionDetails
            .GroupBy(p => p.Module)
            .ToDictionary(
                g => g.Key,
                g => g.Select(p => p.PermissionType).ToList()
            );
    }
}

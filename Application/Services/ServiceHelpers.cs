using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GraceWay.AccountingSystem.Application.Services;

/// <summary>
/// Helper methods مشتركة بين الـ Services
/// </summary>
public static class ServiceHelpers
{
    /// <summary>
    /// التحقق من صلاحيات الأدمن
    /// </summary>
    public static async Task<bool> IsAdminAsync(AppDbContext context, int userId)
    {
        var user = await context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);
        
        var roleName = user?.Role?.RoleName?.ToLower();
        return roleName == "admin" || 
               roleName == "مدير" ||
               roleName == "administrator";
    }
}

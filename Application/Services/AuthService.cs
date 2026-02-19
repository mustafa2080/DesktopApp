using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace GraceWay.AccountingSystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private User? _currentUser;
    private readonly Lazy<IAuditService> _auditService;

    public AuthService(IDbContextFactory<AppDbContext> contextFactory, Lazy<IAuditService> auditService)
    {
        _contextFactory = contextFactory;
        _auditService = auditService;
    }

    // Kept for backward compatibility
    public void SetAuditService(IAuditService auditService)
    {
        // No-op: Lazy injection is used instead
    }

    public User? CurrentUser => _currentUser;

    public async Task<(bool Success, string Message, User? User)> LoginAsync(string username, string password)
    {
        try
        {
            using var context = _contextFactory.CreateDbContext();
            
            // البحث بالاسم الإنجليزي (Username) أو العربي (FullName)
            var user = await context.Users
                .Include(u => u.Role)
                .ThenInclude(r => r!.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .AsNoTracking() // Important: don't track entities in singleton service
                .FirstOrDefaultAsync(u => u.Username == username || u.FullName == username);

            if (user == null)
            {
                return (false, "اسم المستخدم أو كلمة المرور غير صحيحة", null);
            }

            if (!user.IsActive)
            {
                return (false, "هذا الحساب موقوف. يرجى الاتصال بالمسؤول", null);
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return (false, "اسم المستخدم أو كلمة المرور غير صحيحة", null);
            }

            _currentUser = user;

            // ✅ Log Login Action
            if (_auditService.Value != null)
            {
                await _auditService.Value.LogAsync(
                    AuditAction.Login,
                    "User",
                    user.UserId,
                    user.FullName,
                    $"تسجيل دخول المستخدم: {user.FullName} ({user.Role?.RoleName})"
                );
            }

            return (true, "تم تسجيل الدخول بنجاح", user);
        }
        catch (Exception ex)
        {
            return (false, $"حدث خطأ: {ex.Message}", null);
        }
    }

    public async void Logout()
    {
        if (_currentUser != null && _auditService.Value != null)
        {
            // ✅ Log Logout Action
            await _auditService.Value.LogAsync(
                AuditAction.Logout,
                "User",
                _currentUser.UserId,
                _currentUser.FullName,
                $"تسجيل خروج المستخدم: {_currentUser.FullName}"
            );
        }
        
        _currentUser = null;
    }

    public bool HasPermission(string permissionName)
    {
        if (_currentUser?.Role?.RolePermissions == null)
            return false;

        return _currentUser.Role.RolePermissions
            .Any(rp => rp.Permission?.PermissionName == permissionName);
    }

    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}

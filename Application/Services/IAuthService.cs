using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Application.Services;

public interface IAuthService
{
    User? CurrentUser { get; }
    Task<(bool Success, string Message, User? User)> LoginAsync(string username, string password);
    void Logout();
    bool HasPermission(string permissionName);
}

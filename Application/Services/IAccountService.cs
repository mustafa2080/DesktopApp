using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Application.Services;

public interface IAccountService
{
    Task<List<Account>> GetAllAccountsAsync();
    Task<List<Account>> GetRootAccountsAsync();
    Task<List<Account>> GetChildAccountsAsync(int parentId);
    Task<Account?> GetAccountByIdAsync(int accountId);
    Task<Account?> GetAccountByCodeAsync(string code);
    Task<Account> CreateAccountAsync(Account account);
    Task UpdateAccountAsync(Account account);
    Task DeleteAccountAsync(int accountId);
    Task<bool> CanDeleteAccountAsync(int accountId);
    Task<string> GenerateAccountCodeAsync(int? parentId, string accountType);
    Task<decimal> GetAccountBalanceAsync(int accountId);
    Task<List<Account>> GetAccountsByTypeAsync(string accountType);
}

using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GraceWay.AccountingSystem.Application.Services;

public class AccountService : IAccountService
{
    private readonly AppDbContext _context;

    public AccountService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Account>> GetAllAccountsAsync()
    {
        return await _context.Accounts
            .OrderBy(a => a.AccountCode)
            .ToListAsync();
    }

    public async Task<List<Account>> GetRootAccountsAsync()
    {
        return await _context.Accounts
            .Where(a => a.ParentAccountId == null)
            .OrderBy(a => a.AccountCode)
            .ToListAsync();
    }

    public async Task<List<Account>> GetChildAccountsAsync(int parentId)
    {
        return await _context.Accounts
            .Where(a => a.ParentAccountId == parentId)
            .OrderBy(a => a.AccountCode)
            .ToListAsync();
    }

    public async Task<Account?> GetAccountByIdAsync(int accountId)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountId == accountId);
    }

    public async Task<Account?> GetAccountByCodeAsync(string code)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountCode == code);
    }

    public async Task<Account> CreateAccountAsync(Account account)
    {
        // تحديد المستوى بناءً على الحساب الأب
        if (account.ParentAccountId.HasValue)
        {
            var parent = await _context.Accounts.FindAsync(account.ParentAccountId.Value);
            if (parent != null)
            {
                account.Level = parent.Level + 1;
                account.AccountType = parent.AccountType; // نفس نوع الحساب الأب
                
                // تحديث الأب ليصبح Parent
                if (!parent.IsParent)
                {
                    parent.IsParent = true;
                    _context.Accounts.Update(parent);
                }
            }
        }
        else
        {
            account.Level = 1; // حساب رئيسي
        }

        // إنشاء كود تلقائي إذا لم يتم توفيره
        if (string.IsNullOrEmpty(account.AccountCode))
        {
            account.AccountCode = await GenerateAccountCodeAsync(account.ParentAccountId, account.AccountType);
        }

        account.CreatedAt = DateTime.UtcNow;
        account.CurrentBalance = account.OpeningBalance;

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync();

        return account;
    }

    public async Task UpdateAccountAsync(Account account)
    {
        var existing = await _context.Accounts.FindAsync(account.AccountId);
        if (existing == null)
            throw new Exception("الحساب غير موجود");

        existing.AccountName = account.AccountName;
        existing.AccountNameEn = account.AccountNameEn;
        existing.IsActive = account.IsActive;
        existing.Notes = account.Notes;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAccountAsync(int accountId)
    {
        if (!await CanDeleteAccountAsync(accountId))
            throw new Exception("لا يمكن حذف هذا الحساب لوجود حسابات فرعية أو قيود محاسبية مرتبطة به");

        var account = await _context.Accounts.FindAsync(accountId);
        if (account == null)
            throw new Exception("الحساب غير موجود");

        // تحديث الحساب الأب
        if (account.ParentAccountId.HasValue)
        {
            var parent = await _context.Accounts.FindAsync(account.ParentAccountId.Value);
            if (parent != null)
            {
                var siblingCount = await _context.Accounts
                    .CountAsync(a => a.ParentAccountId == account.ParentAccountId.Value && a.AccountId != accountId);
                
                if (siblingCount == 0)
                {
                    parent.IsParent = false;
                    _context.Accounts.Update(parent);
                }
            }
        }

        _context.Accounts.Remove(account);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> CanDeleteAccountAsync(int accountId)
    {
        // تحقق من وجود حسابات فرعية
        var hasChildren = await _context.Accounts.AnyAsync(a => a.ParentAccountId == accountId);
        if (hasChildren)
            return false;

        // تحقق من وجود قيود محاسبية
        var hasJournalEntries = await _context.JournalEntryLines.AnyAsync(l => l.AccountId == accountId);
        if (hasJournalEntries)
            return false;

        return true;
    }

    public async Task<string> GenerateAccountCodeAsync(int? parentId, string accountType)
    {
        if (parentId.HasValue)
        {
            // حساب فرعي
            var parent = await _context.Accounts.FindAsync(parentId.Value);
            if (parent == null)
                throw new Exception("الحساب الأب غير موجود");

            var siblings = await _context.Accounts
                .Where(a => a.ParentAccountId == parentId.Value)
                .OrderByDescending(a => a.AccountCode)
                .FirstOrDefaultAsync();

            if (siblings != null)
            {
                // زيادة آخر رقم فرعي
                var lastCode = siblings.AccountCode;
                var parts = lastCode.Split('.');
                if (parts.Length > 0 && int.TryParse(parts[^1], out int lastNum))
                {
                    return $"{parent.AccountCode}.{lastNum + 1}";
                }
            }

            return $"{parent.AccountCode}.1";
        }
        else
        {
            // حساب رئيسي - كود بناءً على النوع
            var prefix = accountType switch
            {
                "Asset" => "1",
                "Liability" => "2",
                "Equity" => "3",
                "Revenue" => "4",
                "Expense" => "5",
                _ => "9"
            };

            var lastAccount = await _context.Accounts
                .Where(a => a.AccountCode.StartsWith(prefix) && a.ParentAccountId == null)
                .OrderByDescending(a => a.AccountCode)
                .FirstOrDefaultAsync();

            if (lastAccount != null && int.TryParse(lastAccount.AccountCode, out int lastNum))
            {
                return (lastNum + 1).ToString();
            }

            return $"{prefix}000";
        }
    }

    public async Task<decimal> GetAccountBalanceAsync(int accountId)
    {
        var account = await _context.Accounts.FindAsync(accountId);
        return account?.CurrentBalance ?? 0;
    }

    public async Task<List<Account>> GetAccountsByTypeAsync(string accountType)
    {
        return await _context.Accounts
            .Where(a => a.AccountType == accountType && a.IsActive)
            .OrderBy(a => a.AccountCode)
            .ToListAsync();
    }
}

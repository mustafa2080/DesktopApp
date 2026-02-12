using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GraceWay.AccountingSystem.Application.Services;

public class CashBoxService : ICashBoxService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly IAuthService _authService;
    private IJournalService? _journalService;

    public CashBoxService(IDbContextFactory<AppDbContext> contextFactory, IAuthService authService)
    {
        _contextFactory = contextFactory;
        _authService = authService;
    }
    
    // Property injection لتجنب Circular Dependency
    public void SetJournalService(IJournalService journalService)
    {
        _journalService = journalService;
    }

    #region CashBox Management

    public async Task<CashBox> CreateCashBoxAsync(CashBox cashBox)
    {
        using var _context = _contextFactory.CreateDbContext();
        
        var currentUser = _authService.CurrentUser;
        if (currentUser != null)
        {
            cashBox.CreatedBy = currentUser.UserId;
        }
        
        cashBox.CreatedAt = DateTime.UtcNow;
        
        _context.CashBoxes.Add(cashBox);
        await _context.SaveChangesAsync();
        return cashBox;
    }

    public async Task<CashBox?> GetCashBoxByIdAsync(int id)
    {
        using var _context = _contextFactory.CreateDbContext();
        return await _context.CashBoxes
            .Include(c => c.Creator)
            .Include(c => c.Updater)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<List<CashBox>> GetAllCashBoxesAsync()
    {
        using var _context = _contextFactory.CreateDbContext();
        return await _context.CashBoxes
            .Include(c => c.Creator)
            .Include(c => c.Updater)
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<List<CashBox>> GetActiveCashBoxesAsync()
    {
        using var _context = _contextFactory.CreateDbContext();
        return await _context.CashBoxes
            .Include(c => c.Creator)
            .Include(c => c.Updater)
            .Where(c => !c.IsDeleted && c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task UpdateCashBoxAsync(CashBox cashBox)
    {
        using var _context = _contextFactory.CreateDbContext();
        
        var currentUser = _authService.CurrentUser;
        
        // ✅ تحميل الـ entity من الداتابيز وتحديثها مباشرة لتجنب مشاكل Detached Entity
        var existing = await _context.CashBoxes
            .FirstOrDefaultAsync(c => c.Id == cashBox.Id && !c.IsDeleted);
            
        if (existing == null)
            throw new InvalidOperationException("الخزنة غير موجودة");
        
        // تحديث الحقول المطلوبة فقط
        existing.Name = cashBox.Name;
        existing.Notes = cashBox.Notes;
        existing.IsActive = cashBox.IsActive;
        
        if (currentUser != null)
            existing.UpdatedBy = currentUser.UserId;
        
        existing.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCashBoxAsync(int id)
    {
        using var _context = _contextFactory.CreateDbContext();
        
        System.Diagnostics.Debug.WriteLine($"=== DeleteCashBoxAsync ===");
        System.Diagnostics.Debug.WriteLine($"محاولة حذف الخزنة رقم: {id}");
        
        var cashBox = await _context.CashBoxes
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            
        if (cashBox == null)
        {
            System.Diagnostics.Debug.WriteLine("❌ الخزنة غير موجودة!");
            throw new InvalidOperationException("الخزنة غير موجودة");
        }
        
        System.Diagnostics.Debug.WriteLine($"✅ تم العثور على الخزنة: {cashBox.Name}");
        
        // ═══════════════════════════════════════════════════════
        // حذف جميع المعاملات المرتبطة بهذه الخزنة - HARD DELETE
        // ═══════════════════════════════════════════════════════
        var transactions = await _context.CashTransactions
            .Where(t => t.CashBoxId == id)
            .ToListAsync();
            
        System.Diagnostics.Debug.WriteLine($"عدد المعاملات المرتبطة: {transactions.Count}");
        
        if (transactions.Any())
        {
            _context.CashTransactions.RemoveRange(transactions);
            System.Diagnostics.Debug.WriteLine($"✅ تم حذف {transactions.Count} معاملة من قاعدة البيانات");
        }
        
        // ═══════════════════════════════════════════════════════
        // حذف الخزنة نفسها - HARD DELETE
        // ═══════════════════════════════════════════════════════
        _context.CashBoxes.Remove(cashBox);
        System.Diagnostics.Debug.WriteLine("✅ تم حذف الخزنة من قاعدة البيانات");
        
        var changesCount = await _context.SaveChangesAsync();
        System.Diagnostics.Debug.WriteLine($"✅ تم الحفظ بنجاح - إجمالي التغييرات: {changesCount}");
    }

    #endregion

    #region Transactions

    public async Task<CashTransaction> AddTransactionAsync(CashTransaction transaction)
    {
        if (transaction.Type == TransactionType.Income)
        {
            return await AddIncomeAsync(transaction);
        }
        else
        {
            return await AddExpenseAsync(transaction);
        }
    }

    public async Task<CashTransaction> AddIncomeAsync(CashTransaction transaction)
    {
        using var _context = _contextFactory.CreateDbContext();

        var currentUser = _authService.CurrentUser;
        if (currentUser != null)
        {
            transaction.CreatedBy = currentUser.UserId;
        }

        // Get current balance
        var cashBox = await _context.CashBoxes
            .FirstOrDefaultAsync(c => c.Id == transaction.CashBoxId && !c.IsDeleted);
        var currentBalance = cashBox?.CurrentBalance ?? 0m;
        
        transaction.Type = TransactionType.Income;
        transaction.BalanceBefore = currentBalance;
        transaction.BalanceAfter = currentBalance + transaction.Amount;
        transaction.Month = transaction.TransactionDate.Month;
        transaction.Year = transaction.TransactionDate.Year;
        transaction.CreatedAt = DateTime.UtcNow;
        
        _context.CashTransactions.Add(transaction);
        
        if (cashBox != null)
        {
            cashBox.CurrentBalance += transaction.Amount;
            _context.CashBoxes.Update(cashBox);
        }
        
        await _context.SaveChangesAsync();
        
        if (_journalService != null)
        {
            try
            {
                await _journalService.CreateCashTransactionJournalEntryAsync(transaction);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"فشل إنشاء القيد اليومي: {ex.Message}");
            }
        }
        
        return transaction;
    }

    public async Task<CashTransaction> AddExpenseAsync(CashTransaction transaction)
    {
        using var _context = _contextFactory.CreateDbContext();

        var currentUser = _authService.CurrentUser;
        if (currentUser != null)
        {
            transaction.CreatedBy = currentUser.UserId;
        }

        var cashBox = await _context.CashBoxes
            .FirstOrDefaultAsync(c => c.Id == transaction.CashBoxId && !c.IsDeleted);
        
        if (cashBox == null)
        {
            throw new InvalidOperationException("الخزنة غير موجودة");
        }
        
        var currentBalance = cashBox.CurrentBalance;
        
        // ✅ في حالة InstaPay مع عمولة: المبلغ المخصوم الفعلي = Amount + Commission
        decimal actualAmountToDeduct = transaction.Amount;
        if (transaction.PaymentMethod == PaymentMethod.InstaPay && transaction.InstaPayCommission.HasValue)
        {
            actualAmountToDeduct = transaction.Amount + transaction.InstaPayCommission.Value;
        }
        
        // منع الرصيد السالب - validation قوي
        if (currentBalance < actualAmountToDeduct)
        {
            throw new InvalidOperationException(
                $"الرصيد غير كافٍ لإتمام العملية. الرصيد الحالي: {currentBalance:N2}، المبلغ المطلوب: {actualAmountToDeduct:N2}");
        }
        
        var newBalance = currentBalance - actualAmountToDeduct;
        
        // تأكيد إضافي - منع الرصيد السالب نهائياً
        if (newBalance < 0)
        {
            throw new InvalidOperationException(
                $"لا يمكن إتمام العملية لأنها ستؤدي إلى رصيد سالب. الرصيد بعد العملية سيكون: {newBalance:N2}");
        }
        
        transaction.Type = TransactionType.Expense;
        transaction.BalanceBefore = currentBalance;
        transaction.BalanceAfter = newBalance;
        transaction.Month = transaction.TransactionDate.Month;
        transaction.Year = transaction.TransactionDate.Year;
        transaction.CreatedAt = DateTime.UtcNow;
        
        _context.CashTransactions.Add(transaction);
        
        cashBox.CurrentBalance = newBalance;
        _context.CashBoxes.Update(cashBox);
        
        await _context.SaveChangesAsync();
        
        if (_journalService != null)
        {
            try
            {
                await _journalService.CreateCashTransactionJournalEntryAsync(transaction);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"فشل إنشاء القيد اليومي: {ex.Message}");
            }
        }
        
        return transaction;
    }

    public async Task<CashTransaction?> GetTransactionByIdAsync(int transactionId)
    {
        using var _context = _contextFactory.CreateDbContext();
        return await _context.CashTransactions
            .Include(t => t.CashBox)
            .Include(t => t.Reservation)
            .Include(t => t.Creator)
            .Include(t => t.Updater)
            .FirstOrDefaultAsync(t => t.Id == transactionId && !t.IsDeleted);
    }

    public async Task UpdateTransactionAsync(CashTransaction transaction)
    {
        using var _context = _contextFactory.CreateDbContext();

        var currentUser = _authService.CurrentUser;

        System.Diagnostics.Debug.WriteLine($"=== UpdateTransactionAsync ===");
        System.Diagnostics.Debug.WriteLine($"Transaction ID: {transaction.Id}");
        System.Diagnostics.Debug.WriteLine($"Amount: {transaction.Amount}");
        System.Diagnostics.Debug.WriteLine($"Description: {transaction.Description}");

        var existingTransaction = await _context.CashTransactions
            .Include(t => t.CashBox)
            .FirstOrDefaultAsync(t => t.Id == transaction.Id && !t.IsDeleted);

        if (existingTransaction == null)
        {
            System.Diagnostics.Debug.WriteLine("❌ الحركة غير موجودة!");
            throw new InvalidOperationException("الحركة غير موجودة");
        }

        System.Diagnostics.Debug.WriteLine($"✅ تم العثور على الحركة الموجودة:");
        System.Diagnostics.Debug.WriteLine($"   - Amount القديم: {existingTransaction.Amount}");
        System.Diagnostics.Debug.WriteLine($"   - Description القديم: {existingTransaction.Description}");

        // ✅ استخدام الـ CashBox المحملة من Include بدلاً من query جديد
        var cashBox = existingTransaction.CashBox;

        if (cashBox == null)
        {
            System.Diagnostics.Debug.WriteLine("❌ الخزنة غير موجودة!");
            throw new InvalidOperationException("الخزنة غير موجودة");
        }

        System.Diagnostics.Debug.WriteLine($"✅ الخزنة: {cashBox.Name}, الرصيد الحالي: {cashBox.CurrentBalance}");

        // استرجاع الرصيد قبل التعديل (Revert old transaction effect)
        decimal oldActualAmount = existingTransaction.Amount;
        if (existingTransaction.Type == TransactionType.Expense && 
            existingTransaction.PaymentMethod == PaymentMethod.InstaPay && 
            existingTransaction.InstaPayCommission.HasValue)
        {
            // عند الإضافة: خصمنا Amount + Commission، فعند الاسترجاع نضيفهم
            oldActualAmount = existingTransaction.Amount + existingTransaction.InstaPayCommission.Value;
        }
        
        System.Diagnostics.Debug.WriteLine($"استرجاع تأثير المعاملة القديمة: {oldActualAmount}");
        
        if (existingTransaction.Type == TransactionType.Income)
            cashBox.CurrentBalance -= oldActualAmount;
        else
            cashBox.CurrentBalance += oldActualAmount;

        System.Diagnostics.Debug.WriteLine($"الرصيد بعد الاسترجاع: {cashBox.CurrentBalance}");

        var currentBalance = cashBox.CurrentBalance;
        
        // حساب المبلغ الفعلي الجديد
        decimal newActualAmount = transaction.Amount;
        if (transaction.Type == TransactionType.Expense && 
            transaction.PaymentMethod == PaymentMethod.InstaPay && 
            transaction.InstaPayCommission.HasValue)
        {
            // عند المصروف بـ InstaPay: المبلغ الفعلي = Amount + Commission (مطابق لـ AddExpenseAsync)
            newActualAmount = transaction.Amount + transaction.InstaPayCommission.Value;
        }

        System.Diagnostics.Debug.WriteLine($"المبلغ الفعلي الجديد: {newActualAmount}");

        // التحقق من الرصيد في حالة المصروف
        if (transaction.Type == TransactionType.Expense)
        {
            if (currentBalance < newActualAmount)
            {
                System.Diagnostics.Debug.WriteLine($"❌ الرصيد غير كافٍ: متاح={currentBalance}, مطلوب={newActualAmount}");
                throw new InvalidOperationException(
                    $"الرصيد غير كافٍ لإتمام العملية. الرصيد الحالي: {currentBalance:N2}، المبلغ المطلوب: {newActualAmount:N2}");
            }
            
            var newBalance = currentBalance - newActualAmount;
            
            // تأكيد إضافي - منع الرصيد السالب
            if (newBalance < 0)
            {
                System.Diagnostics.Debug.WriteLine($"❌ الرصيد سيصبح سالباً: {newBalance}");
                throw new InvalidOperationException(
                    $"لا يمكن إتمام العملية لأنها ستؤدي إلى رصيد سالب. الرصيد بعد العملية سيكون: {newBalance:N2}");
            }
        }

        System.Diagnostics.Debug.WriteLine("بدء تحديث البيانات...");

        // ✅ تحديث الـ existingTransaction بدلاً من الـ transaction الجديدة
        existingTransaction.TransactionDate = transaction.TransactionDate;
        existingTransaction.Category = transaction.Category;
        existingTransaction.Description = transaction.Description;
        existingTransaction.Amount = transaction.Amount;
        existingTransaction.Notes = transaction.Notes;
        existingTransaction.PaymentMethod = transaction.PaymentMethod;
        existingTransaction.InstaPayCommission = transaction.InstaPayCommission;
        
        System.Diagnostics.Debug.WriteLine($"✅ تم تحديث PaymentMethod: {existingTransaction.PaymentMethod}");
        
        // ✅ حفظ معرف اليوزر اللي عمل التعديل
        if (currentUser != null)
        {
            existingTransaction.UpdatedBy = currentUser.UserId;
        }
        
        System.Diagnostics.Debug.WriteLine($"✅ تم تحديث البيانات في الذاكرة:");
        System.Diagnostics.Debug.WriteLine($"   - Amount الجديد: {existingTransaction.Amount}");
        System.Diagnostics.Debug.WriteLine($"   - Description الجديد: {existingTransaction.Description}");
        System.Diagnostics.Debug.WriteLine($"   - PaymentMethod الجديد: {existingTransaction.PaymentMethod}");
        
        // تطبيق التعديل الجديد
        existingTransaction.BalanceBefore = currentBalance;
        if (existingTransaction.Type == TransactionType.Income)
        {
            existingTransaction.BalanceAfter = currentBalance + existingTransaction.Amount;
            cashBox.CurrentBalance = currentBalance + existingTransaction.Amount;
        }
        else
        {
            // في المصروفات: المبلغ الفعلي = Amount + Commission (في حالة InstaPay)
            decimal actualExpenseAmount = existingTransaction.Amount;
            if (existingTransaction.PaymentMethod == PaymentMethod.InstaPay && existingTransaction.InstaPayCommission.HasValue)
            {
                actualExpenseAmount = existingTransaction.Amount + existingTransaction.InstaPayCommission.Value;
            }
            existingTransaction.BalanceAfter = currentBalance - actualExpenseAmount;
            cashBox.CurrentBalance = currentBalance - actualExpenseAmount;
        }

        existingTransaction.Month = existingTransaction.TransactionDate.Month;
        existingTransaction.Year = existingTransaction.TransactionDate.Year;
        existingTransaction.UpdatedAt = DateTime.UtcNow;

        System.Diagnostics.Debug.WriteLine($"الرصيد الجديد للخزنة: {cashBox.CurrentBalance}");

        // ✅ استدعاء Update() صريح لضمان تسجيل التغييرات في IDbContextFactory
        _context.CashTransactions.Update(existingTransaction);
        _context.CashBoxes.Update(cashBox);

        System.Diagnostics.Debug.WriteLine("استدعاء SaveChangesAsync...");
        var changeCount = await _context.SaveChangesAsync();
        System.Diagnostics.Debug.WriteLine($"✅ تم حفظ {changeCount} تغيير في قاعدة البيانات");
        
        // ✅ تأكيد إضافي: قراءة البيانات من الداتابيز مرة أخرى
        var verifyTransaction = await _context.CashTransactions
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == transaction.Id);
        
        if (verifyTransaction != null)
        {
            System.Diagnostics.Debug.WriteLine($"✅ تأكيد من الداتابيز:");
            System.Diagnostics.Debug.WriteLine($"   - Amount في الداتابيز: {verifyTransaction.Amount}");
            System.Diagnostics.Debug.WriteLine($"   - Description في الداتابيز: {verifyTransaction.Description}");
            System.Diagnostics.Debug.WriteLine($"   - PaymentMethod في الداتابيز: {verifyTransaction.PaymentMethod}");
        }
    }

    public async Task<List<CashTransaction>> GetTransactionsByCashBoxAsync(
        int cashBoxId, DateTime? startDate = null, DateTime? endDate = null)
    {
        using var _context = _contextFactory.CreateDbContext();

        var query = _context.CashTransactions
            .AsNoTracking()
            .Include(t => t.Creator)
            .Include(t => t.Updater)
            .Where(t => t.CashBoxId == cashBoxId && !t.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);

        return await query
            .OrderBy(t => t.TransactionDate)
            .ThenBy(t => t.Id)
            .ToListAsync();
    }

    public async Task<List<CashTransaction>> GetTransactionsByMonthAsync(
        int cashBoxId, int month, int year)
    {
        using var _context = _contextFactory.CreateDbContext();

        return await _context.CashTransactions
            .AsNoTracking()
            .Include(t => t.Creator)
            .Include(t => t.Updater)
            .Where(t => t.CashBoxId == cashBoxId 
                && t.Month == month 
                && t.Year == year 
                && !t.IsDeleted)
            .OrderBy(t => t.TransactionDate)
            .ThenBy(t => t.Id)
            .ToListAsync();
    }

    public async Task DeleteTransactionAsync(int transactionId)
    {
        using var _context = _contextFactory.CreateDbContext();

        var transaction = await _context.CashTransactions
            .FirstOrDefaultAsync(t => t.Id == transactionId);
            
        if (transaction == null || transaction.IsDeleted)
        {
            throw new InvalidOperationException("الحركة غير موجودة");
        }
        
        var cashBox = await _context.CashBoxes
            .FirstOrDefaultAsync(c => c.Id == transaction.CashBoxId && !c.IsDeleted);

        if (cashBox == null)
        {
            throw new InvalidOperationException("الخزنة غير موجودة");
        }

        // حساب المبلغ الفعلي المستخدم في التعامل (في حالة InstaPay مع عمولة)
        decimal actualAmount = transaction.Amount;
        if (transaction.Type == TransactionType.Expense && 
            transaction.PaymentMethod == PaymentMethod.InstaPay && 
            transaction.InstaPayCommission.HasValue)
        {
            // في المصروفات مع InstaPay: المبلغ المخصوم الفعلي = Amount - Commission
            actualAmount = transaction.Amount - transaction.InstaPayCommission.Value;
        }

        // حساب الرصيد بعد الحذف
        decimal newBalance;
        if (transaction.Type == TransactionType.Income)
        {
            // حذف إيراد = نطرح المبلغ من الرصيد الحالي
            newBalance = cashBox.CurrentBalance - transaction.Amount;
        }
        else
        {
            // حذف مصروف = نضيف المبلغ الفعلي للرصيد الحالي
            newBalance = cashBox.CurrentBalance + actualAmount;
        }
        
        // منع الرصيد السالب عند حذف إيراد
        if (newBalance < 0)
        {
            throw new InvalidOperationException(
                $"لا يمكن حذف هذه الحركة لأن ذلك سيؤدي إلى رصيد سالب. " +
                $"الرصيد الحالي: {cashBox.CurrentBalance:N2}، " +
                $"الرصيد بعد الحذف: {newBalance:N2}");
        }
        
        // تطبيق التعديل على الرصيد
        cashBox.CurrentBalance = newBalance;
        _context.CashBoxes.Update(cashBox);
        
        // وضع علامة الحذف على المعاملة
        transaction.IsDeleted = true;
        _context.CashTransactions.Update(transaction);
        
        await _context.SaveChangesAsync();
    }
    
    public async Task<string?> GetLastVoucherNumberAsync(int cashBoxId)
    {
        using var _context = _contextFactory.CreateDbContext();

        var lastTransaction = await _context.CashTransactions
            .Where(t => t.CashBoxId == cashBoxId && !t.IsDeleted)
            .OrderByDescending(t => t.Id)
            .FirstOrDefaultAsync();
            
        return lastTransaction?.VoucherNumber;
    }

    #endregion

    #region Reports and Statistics

    public async Task<decimal> GetCurrentBalanceAsync(int cashBoxId)
    {
        using var _context = _contextFactory.CreateDbContext();

        var cashBox = await _context.CashBoxes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == cashBoxId && !c.IsDeleted);
        return cashBox?.CurrentBalance ?? 0;
    }

    public async Task<MonthlyReport> GetMonthlyReportAsync(int cashBoxId, int month, int year)
    {
        // ✅ إنشاء context جديد تماماً لضمان قراءة البيانات الحديثة
        using var _context = _contextFactory.CreateDbContext();

        try
        {
            System.Diagnostics.Debug.WriteLine($"=== GetMonthlyReportAsync ===");
            System.Diagnostics.Debug.WriteLine($"CashBoxId: {cashBoxId}, Month: {month}, Year: {year}");
            
            // ✅ استخدام AsNoTrackingWithIdentityResolution لتجنب الـ cache تماماً
            var transactions = await _context.CashTransactions
                .AsNoTrackingWithIdentityResolution()
                .Include(t => t.Creator)
                .Include(t => t.Updater)
                .Where(t => t.CashBoxId == cashBoxId 
                    && t.Month == month 
                    && t.Year == year 
                    && !t.IsDeleted)
                .OrderBy(t => t.TransactionDate)
                .ThenBy(t => t.Id)
                .ToListAsync();
            
            System.Diagnostics.Debug.WriteLine($"✅ تم تحميل {transactions.Count} معاملة من قاعدة البيانات");
            
            // طباعة أول 3 معاملات للتأكد
            for (int i = 0; i < Math.Min(3, transactions.Count); i++)
            {
                var t = transactions[i];
                System.Diagnostics.Debug.WriteLine($"   معاملة {i + 1}: ID={t.Id}, Amount={t.Amount}, Description={t.Description}");
            }
            
            var incomeTransactions = transactions.Where(t => t.Type == TransactionType.Income).ToList();
            var expenseTransactions = transactions.Where(t => t.Type == TransactionType.Expense).ToList();
            
            // ✅ الإجماليات بالجنيه المصري فقط (EGP) - لا تخلط العملات
            var totalIncome = incomeTransactions
                .Where(t => t.TransactionCurrency == "EGP" || string.IsNullOrEmpty(t.TransactionCurrency))
                .Sum(t => t.Amount);
            
            var totalExpense = expenseTransactions
                .Where(t => t.TransactionCurrency == "EGP" || string.IsNullOrEmpty(t.TransactionCurrency))
                .Sum(t => {
                    // خصم العمولة من المصروف في حالة InstaPay
                    if (t.PaymentMethod == PaymentMethod.InstaPay && t.InstaPayCommission.HasValue)
                        return t.Amount + t.InstaPayCommission.Value;
                    return t.Amount;
                });
            
            System.Diagnostics.Debug.WriteLine($"✅ Income (EGP): {incomeTransactions.Count} معاملة، الإجمالي: {totalIncome}");
            System.Diagnostics.Debug.WriteLine($"✅ Expense (EGP): {expenseTransactions.Count} معاملة، الإجمالي: {totalExpense}");
            System.Diagnostics.Debug.WriteLine($"✅ Net Profit: {totalIncome - totalExpense}");
            
            // ✅ تصنيف الإيرادات - بالجنيه فقط، مع تمييز العملات الأجنبية
            var incomeByCategory = incomeTransactions
                .GroupBy(t => t.Category)
                .Select(g => {
                    // المبالغ بالجنيه فقط للحساب
                    var egpAmount = g.Where(t => t.TransactionCurrency == "EGP" || string.IsNullOrEmpty(t.TransactionCurrency))
                                    .Sum(t => t.Amount);
                    // عرض المبالغ بالعملات الأجنبية كنص منفصل في الـ Category
                    var foreignAmounts = g.Where(t => t.TransactionCurrency != "EGP" && !string.IsNullOrEmpty(t.TransactionCurrency))
                                         .GroupBy(t => t.TransactionCurrency)
                                         .Select(fc => $"{fc.Sum(t => t.Amount):N2} {fc.Key}")
                                         .ToList();
                    
                    string categoryName = g.Key ?? "غير محدد";
                    if (foreignAmounts.Any())
                        categoryName += $" ({string.Join(" | ", foreignAmounts)})";
                    
                    return new CategorySummary
                    {
                        Category = categoryName,
                        Amount = egpAmount,
                        TransactionCount = g.Count(),
                        Percentage = totalIncome > 0 ? (egpAmount / totalIncome * 100) : 0
                    };
                })
                .OrderByDescending(c => c.Amount)
                .ToList();
            
            // ✅ تصنيف المصروفات - بالجنيه فقط
            var expenseByCategory = expenseTransactions
                .Where(t => t.TransactionCurrency == "EGP" || string.IsNullOrEmpty(t.TransactionCurrency))
                .GroupBy(t => t.Category)
                .Select(g => {
                    var egpAmount = g.Sum(t => {
                        if (t.PaymentMethod == PaymentMethod.InstaPay && t.InstaPayCommission.HasValue)
                            return t.Amount + t.InstaPayCommission.Value;
                        return t.Amount;
                    });
                    return new CategorySummary
                    {
                        Category = g.Key ?? "غير محدد",
                        Amount = egpAmount,
                        TransactionCount = g.Count(),
                        Percentage = totalExpense > 0 ? (egpAmount / totalExpense * 100) : 0
                    };
                })
                .OrderByDescending(c => c.Amount)
                .ToList();
            
            var monthNames = new[] { "يناير", "فبراير", "مارس", "إبريل", "مايو", "يونيو",
                                    "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر" };
            
            return new MonthlyReport
            {
                Month = month,
                Year = year,
                MonthName = monthNames[month - 1],
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                NetProfit = totalIncome - totalExpense,
                IncomeTransactionCount = incomeTransactions.Count,
                ExpenseTransactionCount = expenseTransactions.Count,
                IncomeByCategory = incomeByCategory,
                ExpenseByCategory = expenseByCategory,
                Transactions = transactions
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"خطأ في GetMonthlyReportAsync: {ex.Message}");
            throw new Exception($"خطأ في تحميل البيانات: {ex.Message}", ex);
        }
    }

    public async Task<YearlyReport> GetYearlyReportAsync(int cashBoxId, int year)
    {
        var monthlyReports = await GetAllMonthsReportAsync(cashBoxId, year);
        
        var totalIncome = monthlyReports.Sum(m => m.TotalIncome);
        var totalExpense = monthlyReports.Sum(m => m.TotalExpense);
        
        return new YearlyReport
        {
            Year = year,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            NetProfit = totalIncome - totalExpense,
            AverageMonthlyIncome = totalIncome / 12,
            AverageMonthlyExpense = totalExpense / 12,
            MonthlyReports = monthlyReports
        };
    }

    public async Task<List<MonthlyReport>> GetAllMonthsReportAsync(int cashBoxId, int year)
    {
        var reports = new List<MonthlyReport>();
        
        for (int month = 1; month <= 12; month++)
        {
            var report = await GetMonthlyReportAsync(cashBoxId, month, year);
            reports.Add(report);
        }
        
        return reports;
    }

    #endregion
}

using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Application.Services;

/// <summary>
/// خدمة إدارة الخزنة والبنوك
/// </summary>
public interface ICashBoxService
{
    // إدارة الخزنة
    Task<CashBox> CreateCashBoxAsync(CashBox cashBox);
    Task<CashBox?> GetCashBoxByIdAsync(int id);
    Task<List<CashBox>> GetAllCashBoxesAsync();
    Task<List<CashBox>> GetActiveCashBoxesAsync();
    Task UpdateCashBoxAsync(CashBox cashBox);
    Task DeleteCashBoxAsync(int id);
    
    // الحركات المالية
    Task<CashTransaction> AddTransactionAsync(CashTransaction transaction);
    Task<CashTransaction> AddIncomeAsync(CashTransaction transaction);
    Task<CashTransaction> AddExpenseAsync(CashTransaction transaction);
    Task<CashTransaction?> GetTransactionByIdAsync(int transactionId);
    Task UpdateTransactionAsync(CashTransaction transaction);
    Task<List<CashTransaction>> GetTransactionsByCashBoxAsync(int cashBoxId, DateTime? startDate = null, DateTime? endDate = null);
    Task<List<CashTransaction>> GetTransactionsByMonthAsync(int cashBoxId, int month, int year);
    Task DeleteTransactionAsync(int transactionId);
    Task<string?> GetLastVoucherNumberAsync(int cashBoxId);
    
    // التقارير والإحصائيات
    Task<decimal> GetCurrentBalanceAsync(int cashBoxId);
    Task<MonthlyReport> GetMonthlyReportAsync(int cashBoxId, int month, int year);
    Task<YearlyReport> GetYearlyReportAsync(int cashBoxId, int year);
    Task<List<MonthlyReport>> GetAllMonthsReportAsync(int cashBoxId, int year);
}

/// <summary>
/// تقرير شهري
/// </summary>
public class MonthlyReport
{
    public int Month { get; set; }
    public int Year { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetProfit { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public int IncomeTransactionCount { get; set; }
    public int ExpenseTransactionCount { get; set; }
    public List<CategorySummary> IncomeByCategory { get; set; } = new();
    public List<CategorySummary> ExpenseByCategory { get; set; } = new();
    public List<CashTransaction> Transactions { get; set; } = new(); // إضافة الحركات
}

/// <summary>
/// تقرير سنوي
/// </summary>
public class YearlyReport
{
    public int Year { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetProfit { get; set; }
    public decimal AverageMonthlyIncome { get; set; }
    public decimal AverageMonthlyExpense { get; set; }
    public List<MonthlyReport> MonthlyReports { get; set; } = new();
}

/// <summary>
/// ملخص حسب التصنيف
/// </summary>
public class CategorySummary
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int TransactionCount { get; set; }
    public decimal Percentage { get; set; }
}

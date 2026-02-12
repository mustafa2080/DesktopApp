using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GraceWay.AccountingSystem.Domain.Reports;

namespace GraceWay.AccountingSystem.Application.Services.Reports;

/// <summary>
/// واجهة خدمة التقارير المالية
/// </summary>
public interface IFinancialReportService
{
    /// <summary>
    /// الحصول على قائمة الدخل لفترة محددة
    /// </summary>
    Task<IncomeStatementReport> GetIncomeStatementAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// الحصول على قائمة الدخل المقارنة (فترتين)
    /// </summary>
    Task<ComparativeIncomeStatement> GetComparativeIncomeStatementAsync(
        DateTime currentStart, DateTime currentEnd,
        DateTime previousStart, DateTime previousEnd);
    
    /// <summary>
    /// الحصول على تقرير ربحية الرحلات
    /// </summary>
    Task<List<TripProfitabilityReport>> GetTripProfitabilityAsync(
        DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// الحصول على ربحية رحلة محددة
    /// </summary>
    Task<TripProfitabilityReport?> GetTripProfitabilityByIdAsync(int tripId);
}

/// <summary>
/// قائمة دخل مقارنة بين فترتين
/// </summary>
public class ComparativeIncomeStatement
{
    public IncomeStatementReport CurrentPeriod { get; set; } = new();
    public IncomeStatementReport PreviousPeriod { get; set; } = new();
    
    // نسب النمو
    public decimal RevenueGrowth => CalculateGrowth(
        CurrentPeriod.TotalRevenue,
        PreviousPeriod.TotalRevenue);
    
    public decimal GrossProfitGrowth => CalculateGrowth(
        CurrentPeriod.GrossProfit,
        PreviousPeriod.GrossProfit);
    
    public decimal NetProfitGrowth => CalculateGrowth(
        CurrentPeriod.NetProfit,
        PreviousPeriod.NetProfit);
    
    private decimal CalculateGrowth(decimal current, decimal previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return ((current - previous) / previous) * 100;
    }
}

using System;
using System.Collections.Generic;

namespace GraceWay.AccountingSystem.Domain.Reports;

/// <summary>
/// قائمة الدخل - Income Statement
/// التقرير الأساسي لقياس الربحية
/// </summary>
public class IncomeStatementReport
{
    public string Period { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    // الإيرادات
    public RevenueSection Revenue { get; set; } = new();
    
    // التكاليف المباشرة
    public DirectCostSection DirectCosts { get; set; } = new();
    
    // المصروفات التشغيلية
    public OperatingExpenseSection OperatingExpenses { get; set; } = new();
    
    // الحسابات المجمعة
    public decimal TotalRevenue => Revenue.Total;
    public decimal TotalDirectCosts => DirectCosts.Total;
    public decimal GrossProfit => TotalRevenue - TotalDirectCosts;
    public decimal GrossProfitMargin => TotalRevenue > 0 ? (GrossProfit / TotalRevenue) * 100 : 0;
    
    public decimal TotalOperatingExpenses => OperatingExpenses.Total;
    public decimal NetProfit => GrossProfit - TotalOperatingExpenses;
    public decimal NetProfitMargin => TotalRevenue > 0 ? (NetProfit / TotalRevenue) * 100 : 0;
}

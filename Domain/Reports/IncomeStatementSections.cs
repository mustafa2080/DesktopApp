namespace GraceWay.AccountingSystem.Domain.Reports;

/// <summary>
/// قسم الإيرادات في قائمة الدخل
/// </summary>
public class RevenueSection
{
    public decimal TripRevenue { get; set; }
    public decimal ServiceRevenue { get; set; }
    public decimal OtherRevenue { get; set; }
    
    public decimal Total => TripRevenue + ServiceRevenue + OtherRevenue;
}

/// <summary>
/// قسم التكاليف المباشرة
/// </summary>
public class DirectCostSection
{
    public decimal AccommodationCosts { get; set; }
    public decimal TransportationCosts { get; set; }
    public decimal GuideCosts { get; set; }
    public decimal OptionalTourCosts { get; set; }
    public decimal OtherDirectCosts { get; set; }
    
    public decimal Total => AccommodationCosts + TransportationCosts + 
                           GuideCosts + OptionalTourCosts + OtherDirectCosts;
}

/// <summary>
/// قسم المصروفات التشغيلية
/// </summary>
public class OperatingExpenseSection
{
    public decimal Salaries { get; set; }
    public decimal Rent { get; set; }
    public decimal Utilities { get; set; }
    public decimal Marketing { get; set; }
    public decimal Administrative { get; set; }
    public decimal Depreciation { get; set; }
    public decimal OtherExpenses { get; set; }
    
    public decimal Total => Salaries + Rent + Utilities + Marketing + 
                           Administrative + Depreciation + OtherExpenses;
}

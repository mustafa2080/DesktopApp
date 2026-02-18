using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Application.Services;

/// <summary>
/// خدمة حزم العمرة
/// </summary>
public interface IUmrahService
{
    // ══════════════════════════════════════
    // CRUD Operations
    // ══════════════════════════════════════
    
    /// <summary>
    /// الحصول على جميع حزم العمرة
    /// </summary>
    Task<List<UmrahPackage>> GetAllPackagesAsync();
    
    /// <summary>
    /// الحصول على جميع حزم العمرة مع فلاتر
    /// </summary>
    Task<List<UmrahPackage>> GetAllPackagesAsync(bool activeOnly = false, PackageStatus? status = null);
    
    /// <summary>
    /// الحصول على حزمة عمرة بالمعرف
    /// </summary>
    Task<UmrahPackage?> GetPackageByIdAsync(int packageId);
    
    /// <summary>
    /// إضافة حزمة عمرة جديدة
    /// </summary>
    Task<UmrahPackage> CreatePackageAsync(UmrahPackage package);
    
    /// <summary>
    /// تحديث حزمة عمرة
    /// </summary>
    Task<bool> UpdatePackageAsync(UmrahPackage package);
    
    /// <summary>
    /// حذف حزمة عمرة
    /// </summary>
    Task<bool> DeletePackageAsync(int packageId);
    
    // ══════════════════════════════════════
    // Search & Filter
    // ══════════════════════════════════════
    
    /// <summary>
    /// البحث عن حزم العمرة
    /// </summary>
    Task<List<UmrahPackage>> SearchPackagesAsync(string searchTerm);
    
    /// <summary>
    /// الحصول على الحزم حسب الحالة
    /// </summary>
    Task<List<UmrahPackage>> GetPackagesByStatusAsync(PackageStatus status);
    
    /// <summary>
    /// الحصول على الحزم حسب نوع الغرفة
    /// </summary>
    Task<List<UmrahPackage>> GetPackagesByRoomTypeAsync(RoomType roomType);
    
    /// <summary>
    /// الحصول على الحزم بين تاريخين
    /// </summary>
    Task<List<UmrahPackage>> GetPackagesByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// الحصول على الحزم النشطة فقط
    /// </summary>
    Task<List<UmrahPackage>> GetActivePackagesAsync();
    
    // ══════════════════════════════════════
    // Business Logic
    // ══════════════════════════════════════
    
    /// <summary>
    /// توليد رقم حزمة جديد
    /// </summary>
    Task<string> GeneratePackageNumberAsync();
    
    /// <summary>
    /// تغيير حالة الحزمة
    /// </summary>
    Task<bool> UpdatePackageStatusAsync(int packageId, PackageStatus newStatus, int userId);
    
    /// <summary>
    /// الحصول على إحصائيات الحزم
    /// </summary>
    Task<UmrahStatistics> GetPackageStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    
    /// <summary>
    /// التحقق من إمكانية حذف الحزمة
    /// </summary>
    Task<(bool CanDelete, string Reason)> CanDeletePackageAsync(int packageId);
    
    /// <summary>
    /// الحصول على تقرير الربحية
    /// </summary>
    Task<List<UmrahProfitabilityReport>> GetProfitabilityReportAsync(DateTime? startDate = null, DateTime? endDate = null);
}

/// <summary>
/// إحصائيات حزم العمرة
/// </summary>
public class UmrahStatistics
{
    public int TotalPackages { get; set; }
    public int ActivePackages { get; set; }
    public int CompletedPackages { get; set; }
    public int CancelledPackages { get; set; }
    public int TotalPilgrims { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCosts { get; set; }
    public decimal TotalProfit { get; set; }
    public decimal AverageProfitMargin { get; set; }
    
    // توزيع حسب نوع الغرفة
    public Dictionary<RoomType, int> PackagesByRoomType { get; set; } = new();
    
    // توزيع حسب الحالة
    public Dictionary<PackageStatus, int> PackagesByStatus { get; set; } = new();
}

/// <summary>
/// تقرير ربحية العمرة
/// </summary>
public class UmrahProfitabilityReport
{
    public int PackageId { get; set; }
    public string PackageNumber { get; set; } = string.Empty;
    public string TripName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int NumberOfPersons { get; set; }
    
    // الإيرادات
    public decimal TotalRevenue { get; set; }
    
    // التكاليف التفصيلية
    public decimal VisaCost { get; set; }
    public decimal AccommodationCost { get; set; }
    public decimal BarcodeCost { get; set; }
    public decimal SupervisorBarcodeCost { get; set; }
    public decimal FlightCost { get; set; }
    public decimal FastTrainCost { get; set; }
    public decimal BusCost { get; set; }
    public decimal GiftsCost { get; set; }
    public decimal OtherExpensesCost { get; set; }
    public decimal BrokerCommission { get; set; }
    public decimal SupervisorExpenses { get; set; }
    
    // الحسابات
    public decimal TotalCosts { get; set; }
    public decimal NetProfit { get; set; }
    public decimal ProfitMargin { get; set; }
    public string Status { get; set; } = string.Empty;
    
    // مؤشرات الأداء
    public decimal RevenuePerPerson => NumberOfPersons > 0 ? TotalRevenue / NumberOfPersons : 0;
    public decimal CostPerPerson => NumberOfPersons > 0 ? TotalCosts / NumberOfPersons : 0;
    public decimal ProfitPerPerson => NumberOfPersons > 0 ? NetProfit / NumberOfPersons : 0;
}

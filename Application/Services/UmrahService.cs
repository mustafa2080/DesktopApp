using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GraceWay.AccountingSystem.Application.Services;

/// <summary>
/// تنفيذ خدمة حزم العمرة
/// </summary>
public class UmrahService : IUmrahService
{
    private readonly AppDbContext _context;
    private readonly IAuthService _authService;
    
    public UmrahService(AppDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }
    
    // ══════════════════════════════════════
    // CRUD Operations
    // ══════════════════════════════════════
    
    public async Task<List<UmrahPackage>> GetAllPackagesAsync()
    {
        Console.WriteLine("🚀 GetAllPackagesAsync (no params) - START");
        
        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
        {
            Console.WriteLine("❌ No current user!");
            return new List<UmrahPackage>();
        }

        Console.WriteLine($"👤 Current User: ID={currentUser.UserId}, Username={currentUser.Username}");

        // ✅ إظهار كل الرحلات من كل الحسابات (بدون فلترة)
        var query = _context.UmrahPackages
            .Include(p => p.Creator)
            .Include(p => p.Updater)
            .Include(p => p.Pilgrims)
            .AsQueryable();

        Console.WriteLine("📊 Query created - Showing ALL packages from all users");

        var result = await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
            
        Console.WriteLine($"✅ Query executed: Found {result.Count} packages");
        
        if (result.Any())
        {
            Console.WriteLine("📦 Sample packages:");
            foreach (var p in result.Take(3))
            {
                Console.WriteLine($"  - ID={p.UmrahPackageId}, Number={p.PackageNumber}, CreatedBy={p.CreatedBy}");
            }
        }
        
        return result;
    }
    
    public async Task<List<UmrahPackage>> GetAllPackagesAsync(bool activeOnly = false, PackageStatus? status = null)
    {
        Console.WriteLine($"🔍 GetAllPackagesAsync called with: activeOnly={activeOnly}, status={status}");
        
        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
        {
            Console.WriteLine("⚠️ CurrentUser is NULL!");
            return new List<UmrahPackage>();
        }

        Console.WriteLine($"📊 User={currentUser.Username}, UserId={currentUser.UserId}");

        // ✅ إظهار كل الرحلات من كل الحسابات (بدون فلترة)
        var query = _context.UmrahPackages
            .Include(p => p.Creator)
            .Include(p => p.Updater)
            .Include(p => p.Pilgrims)
            .AsQueryable();
        
        Console.WriteLine($"📊 Query created - Showing ALL packages from all users");
        
        if (activeOnly)
        {
            query = query.Where(p => p.IsActive);
            Console.WriteLine($"🔹 Filtered by IsActive=true");
        }
        
        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
            Console.WriteLine($"🔹 Filtered by Status={status.Value}");
        }
        
        var result = await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        
        Console.WriteLine($"✅ Query executed. Found {result.Count} packages");
        
        if (result.Any())
        {
            var first = result.First();
            Console.WriteLine($"📦 First package: ID={first.UmrahPackageId}, CreatedBy={first.CreatedBy}, Creator={first.Creator?.FullName}, IsActive={first.IsActive}, Status={first.Status}");
        }
        else
        {
            Console.WriteLine("⚠️ No packages found in result!");
        }
        
        return result;
    }
    
    public async Task<UmrahPackage?> GetPackageByIdAsync(int packageId)
    {
        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
            return null;

        // ✅ إظهار أي رحلة بدون التحقق من اليوزر
        var package = await _context.UmrahPackages
            .Include(p => p.Creator)
            .Include(p => p.Updater)
            .Include(p => p.Pilgrims)
            .FirstOrDefaultAsync(p => p.UmrahPackageId == packageId);

        return package;
    }
    
    public async Task<UmrahPackage> CreatePackageAsync(UmrahPackage package)
    {
        try
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null)
                throw new UnauthorizedAccessException("لا يوجد مستخدم مسجل");

            // توليد رقم الحزمة تلقائياً
            if (string.IsNullOrWhiteSpace(package.PackageNumber))
            {
                package.PackageNumber = await GeneratePackageNumberAsync();
            }
            
            package.CreatedBy = currentUser.UserId; // ✅ حفظ معرف اليوزر
            package.CreatedAt = DateTime.UtcNow;
            package.UpdatedAt = DateTime.UtcNow;
            
            // Add to context
            _context.UmrahPackages.Add(package);
            
            // Save changes
            var result = await _context.SaveChangesAsync();
            
            Console.WriteLine($"✅ UmrahPackage saved successfully! ID: {package.UmrahPackageId}, Rows affected: {result}");
            
            return package;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error saving UmrahPackage: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }
    
    public async Task<bool> UpdatePackageAsync(UmrahPackage package)
    {
        try
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null)
                throw new UnauthorizedAccessException("لا يوجد مستخدم مسجل");

            Console.WriteLine($"🔄 Starting UpdatePackageAsync for package ID: {package.UmrahPackageId}");
            
            var existing = await _context.UmrahPackages
                .Include(p => p.Pilgrims)
                .FirstOrDefaultAsync(p => p.UmrahPackageId == package.UmrahPackageId);
            
            if (existing == null)
            {
                Console.WriteLine($"❌ Package not found: {package.UmrahPackageId}");
                return false;
            }

            // ✅ السماح للجميع بالتعديل (بدون تحقق من اليوزر)
            Console.WriteLine($"✅ Found existing package. Current pilgrims count: {existing.Pilgrims.Count}");
            
            // تحديث البيانات
            existing.Date = package.Date;
            existing.TripName = package.TripName;
            existing.NumberOfPersons = package.NumberOfPersons;
            existing.RoomType = package.RoomType;
            existing.MakkahHotel = package.MakkahHotel;
            existing.MakkahNights = package.MakkahNights;
            existing.MadinahHotel = package.MadinahHotel;
            existing.MadinahNights = package.MadinahNights;
            existing.TransportMethod = package.TransportMethod;
            existing.SellingPrice = package.SellingPrice;
            existing.VisaPriceSAR = package.VisaPriceSAR;
            existing.SARExchangeRate = package.SARExchangeRate;
            existing.AccommodationTotal = package.AccommodationTotal;
            existing.BarcodePrice = package.BarcodePrice;
            existing.FlightPrice = package.FlightPrice;
            existing.FastTrainPriceSAR = package.FastTrainPriceSAR;
            existing.BrokerName = package.BrokerName;
            existing.SupervisorName = package.SupervisorName;
            existing.Commission = package.Commission;
            existing.SupervisorExpenses = package.SupervisorExpenses;
            existing.Status = package.Status;
            existing.IsActive = package.IsActive;
            existing.Notes = package.Notes;
            existing.UpdatedBy = package.UpdatedBy;
            existing.UpdatedAt = DateTime.UtcNow;
            
            Console.WriteLine($"✅ Package data updated");
            
            // Update pilgrims if provided
            if (package.Pilgrims != null && package.Pilgrims.Any())
            {
                Console.WriteLine($"🔄 Updating pilgrims. New count: {package.Pilgrims.Count}");
                
                // Remove old pilgrims
                if (existing.Pilgrims.Any())
                {
                    Console.WriteLine($"🗑️ Removing {existing.Pilgrims.Count} old pilgrims");
                    _context.UmrahPilgrims.RemoveRange(existing.Pilgrims);
                }
                
                // Clear the collection
                existing.Pilgrims.Clear();
                
                // Add new pilgrims - let EF set the foreign key automatically
                foreach (var pilgrim in package.Pilgrims)
                {
                    // Make sure the pilgrim doesn't have an ID (it's new)
                    pilgrim.UmrahPilgrimId = 0;
                    // Don't set UmrahPackageId - EF will set it via navigation property
                    pilgrim.UmrahPackageId = existing.UmrahPackageId;
                    
                    existing.Pilgrims.Add(pilgrim);
                    Console.WriteLine($"➕ Added pilgrim: {pilgrim.FullName}");
                }
                
                Console.WriteLine($"✅ All {package.Pilgrims.Count} pilgrims added to existing package");
            }
            
            Console.WriteLine($"💾 Saving changes to database...");
            var result = await _context.SaveChangesAsync();
            Console.WriteLine($"✅ Successfully saved! Rows affected: {result}");
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error updating UmrahPackage: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            return false;
        }
    }
    
    public async Task<bool> DeletePackageAsync(int packageId)
    {
        try
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null)
                throw new UnauthorizedAccessException("لا يوجد مستخدم مسجل");

            var package = await _context.UmrahPackages
                .FirstOrDefaultAsync(p => p.UmrahPackageId == packageId);
            
            if (package == null)
                return false;

            // ✅ السماح للجميع بالحذف (بدون تحقق من اليوزر)
            _context.UmrahPackages.Remove(package);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    // ══════════════════════════════════════
    // Search & Filter
    // ══════════════════════════════════════
    
    public async Task<List<UmrahPackage>> SearchPackagesAsync(string searchTerm)
    {
        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
            return new List<UmrahPackage>();

        searchTerm = searchTerm.ToLower().Trim();
        
        // ✅ البحث في كل الرحلات من كل الحسابات
        var query = _context.UmrahPackages
            .Include(p => p.Creator)
            .Include(p => p.Updater)
            .Where(p => 
                p.PackageNumber.ToLower().Contains(searchTerm) ||
                p.TripName.ToLower().Contains(searchTerm) ||
                p.MakkahHotel.ToLower().Contains(searchTerm) ||
                p.MadinahHotel.ToLower().Contains(searchTerm) ||
                (p.BrokerName != null && p.BrokerName.ToLower().Contains(searchTerm)) ||
                (p.SupervisorName != null && p.SupervisorName.ToLower().Contains(searchTerm)));

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<List<UmrahPackage>> GetPackagesByStatusAsync(PackageStatus status)
    {
        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
            return new List<UmrahPackage>();

        // ✅ عرض كل الرحلات من كل الحسابات
        return await _context.UmrahPackages
            .Include(p => p.Creator)
            .Include(p => p.Updater)
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<List<UmrahPackage>> GetPackagesByRoomTypeAsync(RoomType roomType)
    {
        return await _context.UmrahPackages
            .Include(p => p.Creator)
            .Include(p => p.Updater)
            .Where(p => p.RoomType == roomType)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<List<UmrahPackage>> GetPackagesByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.UmrahPackages
            .Include(p => p.Creator)
            .Include(p => p.Updater)
            .Where(p => p.Date >= startDate && p.Date <= endDate)
            .OrderBy(p => p.Date)
            .ToListAsync();
    }
    
    public async Task<List<UmrahPackage>> GetActivePackagesAsync()
    {
        return await _context.UmrahPackages
            .Include(p => p.Creator)
            .Include(p => p.Updater)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }
    
    // ══════════════════════════════════════
    // Business Logic
    // ══════════════════════════════════════
    
    public async Task<string> GeneratePackageNumberAsync()
    {
        var year = DateTime.Now.Year;
        var prefix = $"UMR-{year}-";
        
        var lastPackage = await _context.UmrahPackages
            .Where(p => p.PackageNumber.StartsWith(prefix))
            .OrderByDescending(p => p.PackageNumber)
            .FirstOrDefaultAsync();
        
        int nextNumber = 1;
        if (lastPackage != null)
        {
            var lastNumberPart = lastPackage.PackageNumber.Substring(prefix.Length);
            if (int.TryParse(lastNumberPart, out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }
        
        return $"{prefix}{nextNumber:D4}";
    }
    
    public async Task<bool> UpdatePackageStatusAsync(int packageId, PackageStatus newStatus, int userId)
    {
        try
        {
            var package = await _context.UmrahPackages
                .FirstOrDefaultAsync(p => p.UmrahPackageId == packageId);
            
            if (package == null)
                return false;
            
            package.Status = newStatus;
            package.UpdatedBy = userId;
            package.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public async Task<UmrahStatistics> GetPackageStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.UmrahPackages.AsQueryable();
        
        if (startDate.HasValue && endDate.HasValue)
        {
            query = query.Where(p => p.Date >= startDate.Value && p.Date <= endDate.Value);
        }
        
        var packages = await query.ToListAsync();
        
        var statistics = new UmrahStatistics
        {
            TotalPackages = packages.Count,
            ActivePackages = packages.Count(p => p.IsActive),
            CompletedPackages = packages.Count(p => p.Status == PackageStatus.Completed),
            CancelledPackages = packages.Count(p => p.Status == PackageStatus.Cancelled),
            TotalPilgrims = packages.Sum(p => p.NumberOfPersons),
            TotalRevenue = packages.Sum(p => p.TotalRevenue),
            TotalCosts = packages.Sum(p => p.TotalCosts * p.NumberOfPersons),
            TotalProfit = packages.Sum(p => p.NetProfit),
            AverageProfitMargin = packages.Any() ? packages.Average(p => p.ProfitMargin) : 0
        };
        
        // توزيع حسب نوع الغرفة
        statistics.PackagesByRoomType = packages
            .GroupBy(p => p.RoomType)
            .ToDictionary(g => g.Key, g => g.Count());
        
        // توزيع حسب الحالة
        statistics.PackagesByStatus = packages
            .GroupBy(p => p.Status)
            .ToDictionary(g => g.Key, g => g.Count());
        
        return statistics;
    }
    
    public async Task<(bool CanDelete, string Reason)> CanDeletePackageAsync(int packageId)
    {
        var package = await _context.UmrahPackages
            .FirstOrDefaultAsync(p => p.UmrahPackageId == packageId);
        
        if (package == null)
            return (false, "الحزمة غير موجودة");
        
        // يمكن حذف الحزم في حالة المسودة أو الملغاة فقط
        if (package.Status == PackageStatus.InProgress || package.Status == PackageStatus.Completed)
        {
            return (false, "لا يمكن حذف حزمة قيد التنفيذ أو مكتملة");
        }
        
        return (true, string.Empty);
    }
    
    public async Task<List<UmrahProfitabilityReport>> GetProfitabilityReportAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.UmrahPackages.AsQueryable();
        
        if (startDate.HasValue && endDate.HasValue)
        {
            query = query.Where(p => p.Date >= startDate.Value && p.Date <= endDate.Value);
        }
        
        var packages = await query
            .OrderByDescending(p => p.NetProfit)
            .ToListAsync();
        
        return packages.Select(p => new UmrahProfitabilityReport
        {
            PackageId = p.UmrahPackageId,
            PackageNumber = p.PackageNumber,
            TripName = p.TripName,
            Date = p.Date,
            NumberOfPersons = p.NumberOfPersons,
            TotalRevenue = p.TotalRevenue,
            TotalCosts = p.TotalCosts * p.NumberOfPersons,
            NetProfit = p.NetProfit,
            ProfitMargin = p.ProfitMargin,
            Status = p.GetStatusDisplay()
        }).ToList();
    }

    private async Task<bool> IsAdminAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);
        
        Console.WriteLine($"🔍 IsAdminAsync: UserId={userId}, User={user?.Username}, Role={user?.Role?.RoleName}");
        
        bool isAdmin = user?.Role?.RoleName?.ToLower() == "admin" || 
                       user?.Role?.RoleName?.ToLower() == "مدير";
        
        Console.WriteLine($"✅ IsAdmin result: {isAdmin}");
        
        return isAdmin;
    }
}

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
            
            // ✅ استخدام AsTracking لإجبار EF على تتبع التغييرات (لأن الـ context بيستخدم NoTracking)
            var existing = await _context.UmrahPackages
                .AsTracking() // ✅ مهم جداً! 
                .FirstOrDefaultAsync(p => p.UmrahPackageId == package.UmrahPackageId);
            
            if (existing == null)
            {
                Console.WriteLine($"❌ Package not found: {package.UmrahPackageId}");
                return false;
            }

            Console.WriteLine($"✅ Found existing package");
            Console.WriteLine($"📊 Package before update:");
            Console.WriteLine($"   - TripName: {existing.TripName}");
            Console.WriteLine($"   - NumberOfPersons: {existing.NumberOfPersons}");
            Console.WriteLine($"   - MakkahHotel: {existing.MakkahHotel}");
            Console.WriteLine($"   - SellingPrice: {existing.SellingPrice}");
            
            // تحديث البيانات الأساسية
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
            existing.SupervisorBarcodePrice = package.SupervisorBarcodePrice;
            existing.FlightPrice = package.FlightPrice;
            existing.FastTrainPriceSAR = package.FastTrainPriceSAR;
            existing.BusesCount = package.BusesCount;
            existing.BusPriceSAR = package.BusPriceSAR;
            existing.GiftsPrice = package.GiftsPrice;
            existing.OtherExpenses = package.OtherExpenses;
            existing.OtherExpensesNotes = package.OtherExpensesNotes;
            existing.ProfitMarginEGP = package.ProfitMarginEGP;
            existing.BrokerName = package.BrokerName;
            existing.SupervisorName = package.SupervisorName;
            existing.Commission = package.Commission;
            existing.SupervisorExpensesSAR = package.SupervisorExpensesSAR;
            existing.Status = package.Status;
            existing.IsActive = package.IsActive;
            existing.Notes = package.Notes;
            existing.UpdatedBy = currentUser.UserId;
            existing.UpdatedAt = DateTime.UtcNow;
            
            Console.WriteLine($"✅ Package data updated - UpdatedBy set to: {currentUser.UserId}");
            Console.WriteLine($"📊 Package after update:");
            Console.WriteLine($"   - TripName: {existing.TripName}");
            Console.WriteLine($"   - NumberOfPersons: {existing.NumberOfPersons}");
            Console.WriteLine($"   - MakkahHotel: {existing.MakkahHotel}");
            Console.WriteLine($"   - SellingPrice: {existing.SellingPrice}");
            
            // ✅ التحقق من حالة الـ Entity
            var entityState = _context.Entry(existing).State;
            Console.WriteLine($"📊 Entity State before SaveChanges: {entityState}");
            
            // ✅ حفظ التغييرات الأساسية أولاً
            var rowsAffected = await _context.SaveChangesAsync();
            Console.WriteLine($"✅ Basic package data saved - Rows affected: {rowsAffected}");
            
            // ✅ تحديث الـ Pilgrims في عملية منفصلة لتجنب مشاكل الـ tracking
            if (package.Pilgrims != null && package.Pilgrims.Any())
            {
                Console.WriteLine($"🔄 Updating pilgrims. New count: {package.Pilgrims.Count}");
                
                // حذف كل الـ pilgrims القديمة مرة واحدة
                var oldPilgrims = await _context.UmrahPilgrims
                    .Where(p => p.UmrahPackageId == existing.UmrahPackageId)
                    .ToListAsync();
                
                if (oldPilgrims.Any())
                {
                    Console.WriteLine($"🗑️ Removing {oldPilgrims.Count} old pilgrims");
                    _context.UmrahPilgrims.RemoveRange(oldPilgrims);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"✅ Old pilgrims removed");
                }
                
                // إضافة الـ pilgrims الجديدة
                var newPilgrims = new List<UmrahPilgrim>();
                foreach (var pilgrim in package.Pilgrims)
                {
                    var newPilgrim = new UmrahPilgrim
                    {
                        UmrahPackageId = existing.UmrahPackageId,
                        PilgrimNumber = pilgrim.PilgrimNumber,
                        FullName = pilgrim.FullName,
                        RoomType = pilgrim.RoomType,
                        SharedRoomNumber = pilgrim.SharedRoomNumber,
                        TotalAmount = pilgrim.TotalAmount,
                        PaidAmount = pilgrim.PaidAmount,
                        CreatedBy = pilgrim.CreatedBy,
                        RegisteredAt = pilgrim.RegisteredAt,
                        UpdatedAt = DateTime.UtcNow,
                        Status = pilgrim.Status
                    };
                    newPilgrims.Add(newPilgrim);
                    Console.WriteLine($"➕ Prepared pilgrim: {newPilgrim.FullName}, RoomType: {newPilgrim.RoomType}");
                }
                
                if (newPilgrims.Any())
                {
                    await _context.UmrahPilgrims.AddRangeAsync(newPilgrims);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"✅ All {newPilgrims.Count} new pilgrims saved");
                }
            }
            
            Console.WriteLine($"✅ UpdatePackageAsync completed successfully!");
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
                .AsTracking() // ✅ مهم للحذف
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
                .AsTracking() // ✅ مهم للتحديث
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
            AverageProfitMargin = packages.Any() ? packages.Average(p => p.ProfitMarginEGP) : 0
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
            .OrderBy(p => p.Date)
            .ToListAsync();
        
        return packages
            .OrderByDescending(p => p.NetProfit)
            .Select(p => new UmrahProfitabilityReport
            {
                PackageId = p.UmrahPackageId,
                PackageNumber = p.PackageNumber,
                TripName = p.TripName,
                Date = p.Date,
                NumberOfPersons = p.NumberOfPersons,
                
                // الإيرادات
                TotalRevenue = p.TotalRevenue,
                
                // التكاليف التفصيلية
                VisaCost = p.VisaPriceEGP * p.NumberOfPersons,
                AccommodationCost = p.AccommodationTotal * p.NumberOfPersons,
                BarcodeCost = p.BarcodePrice * p.NumberOfPersons,
                SupervisorBarcodeCost = p.SupervisorBarcodePrice,
                FlightCost = p.FlightPrice * p.NumberOfPersons,
                FastTrainCost = p.FastTrainPriceEGP * p.NumberOfPersons,
                BusCost = p.BusPriceEGP,
                GiftsCost = p.GiftsPrice,
                OtherExpensesCost = p.OtherExpenses * p.NumberOfPersons,
                BrokerCommission = p.Commission,
                SupervisorExpenses = p.SupervisorExpensesEGP,
                
                // الحسابات - مجموع كل التكاليف الفعلية
                TotalCosts = (p.VisaPriceEGP * p.NumberOfPersons) + 
                            (p.AccommodationTotal * p.NumberOfPersons) + 
                            (p.BarcodePrice * p.NumberOfPersons) + 
                            p.SupervisorBarcodePrice +
                            (p.FlightPrice * p.NumberOfPersons) + 
                            (p.FastTrainPriceEGP * p.NumberOfPersons) + 
                            p.BusPriceEGP + 
                            p.GiftsPrice + 
                            (p.OtherExpenses * p.NumberOfPersons) + 
                            p.Commission + 
                            p.SupervisorExpensesEGP,
                NetProfit = p.NetProfit,
                ProfitMargin = p.ProfitMarginPercent,
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

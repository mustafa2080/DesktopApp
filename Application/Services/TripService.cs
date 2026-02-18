using Microsoft.EntityFrameworkCore;
using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;

namespace GraceWay.AccountingSystem.Application.Services;

/// <summary>
/// خدمة إدارة الرحلات السياحية
/// </summary>
public class TripService : ITripService
{
    private readonly AppDbContext _context;
    private readonly IAuditService? _auditService;
    
    public TripService(AppDbContext context, IAuditService? auditService = null)
    {
        _context = context;
        _auditService = auditService;
    }
    
    // ══════════════════════════════════════
    // CRUD Operations
    // ══════════════════════════════════════
    
    public async Task<Trip> CreateTripAsync(Trip trip)
    {
        // توليد رقم رحلة تلقائياً
        if (string.IsNullOrEmpty(trip.TripNumber))
        {
            trip.TripNumber = await GenerateTripNumberAsync();
        }
        
        // توليد كود الرحلة
        if (string.IsNullOrEmpty(trip.TripCode))
        {
            trip.TripCode = await GenerateTripCodeAsync();
        }
        
        trip.CreatedAt = DateTime.UtcNow;
        trip.UpdatedAt = DateTime.UtcNow;
        
        // تعيين العملة الافتراضية (EGP) إذا لم تكن محددة
        if (trip.CurrencyId == 0)
        {
            trip.CurrencyId = 1; // EGP
        }
        
        // ═══════════════════════════════════════════════════════
        // الحجز والتنفيذ التلقائي للرحلة عند الإنشاء
        // ═══════════════════════════════════════════════════════
        // تسجيل الرحلة كمحجوزة بالكامل ومنفذة
        trip.BookedSeats = trip.TotalCapacity;
        trip.Status = TripStatus.Completed; // الرحلة منفذة
        
        _context.Trips.Add(trip);
        await _context.SaveChangesAsync();
        
        // ✅ Audit Log: Trip Created
        if (_auditService != null)
        {
            await _auditService.LogAsync(
                AuditAction.Create,
                "Trip",
                trip.TripId,
                $"{trip.Destination} - {trip.StartDate:dd/MM/yyyy}",
                $"تم إضافة رحلة جديدة: {trip.Destination}",
                newValues: new { 
                    trip.Destination, 
                    trip.StartDate, 
                    trip.EndDate, 
                    trip.TotalCapacity,
                    trip.SellingPricePerPersonInEGP 
                }
            );
        }
        
        // ═══════════════════════════════════════════════════════
        // تسجيل إيرادات الرحلة في الخزنة - معطل مؤقتاً
        // ═══════════════════════════════════════════════════════
        // تم تعطيل التسجيل التلقائي في الخزنة
        // سيتم التسجيل يدوياً من خلال شاشة الخزنة أو عند استلام المدفوعات
        // await RegisterTripRevenueAsync(trip);
        
        return trip;
    }
    
    /// <summary>
    /// تسجيل إيرادات الرحلة في الخزنة الافتراضية
    /// </summary>
    private async Task RegisterTripRevenueAsync(Trip trip)
    {
        try
        {
            // الحصول على الخزنة الرئيسية (أول خزنة نشطة)
            var cashBox = await _context.CashBoxes
                .Where(c => c.IsActive && !c.IsDeleted)
                .OrderBy(c => c.Id)
                .FirstOrDefaultAsync();
            
            if (cashBox == null)
            {
                // لو مفيش خزنة، نسجل في الـ log بس مش نوقف العملية
                return;
            }
            
            // حساب إجمالي إيرادات الرحلة بالجنيه المصري
            var totalRevenue = trip.TotalCapacity * trip.SellingPricePerPersonInEGP;
            
            // الحصول على الرصيد الحالي
            var currentBalance = cashBox.CurrentBalance;
            
            // إنشاء حركة إيراد
            var transaction = new CashTransaction
            {
                VoucherNumber = await GenerateVoucherNumberAsync(cashBox.Id),
                Type = TransactionType.Income,
                CashBoxId = cashBox.Id,
                Amount = totalRevenue,
                TransactionDate = DateTime.UtcNow,
                Month = DateTime.UtcNow.Month,
                Year = DateTime.UtcNow.Year,
                Category = "إيرادات رحلات",
                Description = $"إيرادات رحلة: {trip.TripName} ({trip.TripNumber})",
                PartyName = trip.TripName,
                PaymentMethod = PaymentMethod.Cash,
                BalanceBefore = currentBalance,
                BalanceAfter = currentBalance + totalRevenue,
                CreatedBy = trip.CreatedBy,
                CreatedAt = DateTime.UtcNow,
                Notes = $"تم التسجيل تلقائياً عند إنشاء الرحلة - الطاقة: {trip.TotalCapacity} فرد - السعر: {trip.SellingPricePerPerson:N2} {GetCurrencyName(trip.CurrencyId)}"
            };
            
            _context.CashTransactions.Add(transaction);
            
            // تحديث رصيد الخزنة
            cashBox.CurrentBalance += totalRevenue;
            _context.CashBoxes.Update(cashBox);
            
            await _context.SaveChangesAsync();
        }
        catch (Exception)
        {
            // في حالة حدوث خطأ، نتجاهله لعدم إيقاف عملية حفظ الرحلة
            // يمكن تسجيل الخطأ في log للمراجعة لاحقاً
        }
    }
    
    /// <summary>
    /// توليد رقم سند تلقائي للخزنة
    /// </summary>
    private async Task<string> GenerateVoucherNumberAsync(int cashBoxId)
    {
        var lastTransaction = await _context.CashTransactions
            .Where(t => t.CashBoxId == cashBoxId && !t.IsDeleted)
            .OrderByDescending(t => t.Id)
            .FirstOrDefaultAsync();
        
        int nextNumber = 1;
        if (lastTransaction != null && !string.IsNullOrEmpty(lastTransaction.VoucherNumber))
        {
            var parts = lastTransaction.VoucherNumber.Split('-');
            if (parts.Length >= 2 && int.TryParse(parts[^1], out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }
        
        return $"VCH-{DateTime.UtcNow:yyyyMMdd}-{nextNumber:D4}";
    }
    
    /// <summary>
    /// الحصول على اسم العملة بالعربية
    /// </summary>
    private string GetCurrencyName(int currencyId)
    {
        return currencyId switch
        {
            1 => "جنيه مصري",
            2 => "دولار أمريكي",
            3 => "ريال سعودي",
            4 => "يورو",
            _ => "جنيه مصري"
        };
    }
    
    public async Task<Trip> UpdateTripAsync(Trip trip)
    {
        Console.WriteLine("════════════════════════════════════════");
        Console.WriteLine($"🔄 UpdateTripAsync called for Trip ID: {trip.TripId}");
        Console.WriteLine($"📝 Description: '{trip.Description}'");
        Console.WriteLine($"📍 Destination: '{trip.Destination}'");
        Console.WriteLine($"📅 StartDate: {trip.StartDate:yyyy-MM-dd}");
        Console.WriteLine($"📅 EndDate: {trip.EndDate:yyyy-MM-dd}");
        Console.WriteLine($"👥 AdultCount: {trip.AdultCount}");
        Console.WriteLine($"👶 ChildCount: {trip.ChildCount}");
        Console.WriteLine($"🎫 TotalCapacity: {trip.TotalCapacity}");
        Console.WriteLine("════════════════════════════════════════");
        
        // ✅ Clear tracker to avoid conflicts with passed-in entity
        _context.ChangeTracker.Clear();
        
        // ════════════════════════════════════════════════════════════
        // STEP 1: Update basic trip fields using raw SQL (bypass tracking)
        // ════════════════════════════════════════════════════════════
        Console.WriteLine("📝 Step 1: Updating basic trip fields...");
        
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE trips SET
                tripname              = {trip.TripName},
                destination           = {trip.Destination},
                triptype              = {(int)trip.TripType},
                description           = {trip.Description},
                startdate             = {trip.StartDate.ToUniversalTime()},
                enddate               = {trip.EndDate.ToUniversalTime()},
                totalcapacity         = {trip.TotalCapacity},
                adultcount            = {trip.AdultCount},
                childcount            = {trip.ChildCount},
                sellingpriceperperson = {trip.SellingPricePerPerson},
                currencyid            = {trip.CurrencyId},
                exchangerate          = {trip.ExchangeRate},
                totalcost             = {trip.TotalCost},
                status                = {(int)trip.Status},
                ispublished           = {trip.IsPublished},
                isactive              = {trip.IsActive},
                updatedby             = {trip.UpdatedBy},
                updatedat             = {DateTime.UtcNow}
            WHERE tripid = {trip.TripId}");
        
        Console.WriteLine("✅ Basic fields updated via SQL");
        
        var tripId = trip.TripId;
        
        // ════════════════════════════════════════════════════════════
        // STEP 2: Delete all child collections via raw SQL
        // ════════════════════════════════════════════════════════════
        Console.WriteLine("🗑️ Step 2: Deleting old child records...");
        
        await _context.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tripprograms WHERE tripid = {tripId}");
        await _context.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM triptransportations WHERE tripid = {tripId}");
        await _context.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tripaccommodations WHERE tripid = {tripId}");
        await _context.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tripguides WHERE tripid = {tripId}");
        await _context.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tripoptionaltours WHERE tripid = {tripId}");
        await _context.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM tripexpenses WHERE tripid = {tripId}");
        
        Console.WriteLine("✅ Old records deleted");
        
        // ════════════════════════════════════════════════════════════
        // STEP 3: Re-insert child collections via EF (clean tracker)
        // ════════════════════════════════════════════════════════════
        Console.WriteLine("➕ Step 3: Adding new child records...");
        _context.ChangeTracker.Clear();
        
        // البرنامج اليومي
        foreach (var program in trip.Programs)
        {
            _context.TripPrograms.Add(new TripProgram
            {
                TripId = tripId,
                DayNumber = program.DayNumber,
                DayTitle = program.DayTitle,
                DayDate = program.DayDate,
                Activities = program.Activities,
                Visits = program.Visits,
                MealsIncluded = program.MealsIncluded,
                VisitsCost = program.VisitsCost,
                GuideCost = program.GuideCost,
                ParticipantsCount = program.ParticipantsCount,
                DriverTip = program.DriverTip,
                BookingType = program.BookingType,
                Notes = program.Notes
            });
        }
        
        // النقل
        foreach (var transport in trip.Transportation)
        {
            _context.TripTransportations.Add(new TripTransportation
            {
                TripId = tripId,
                Type = transport.Type,
                TransportDate = transport.TransportDate,
                Route = transport.Route,
                NumberOfVehicles = transport.NumberOfVehicles,
                SeatsPerVehicle = transport.SeatsPerVehicle,
                ParticipantsCount = transport.ParticipantsCount,
                CostPerVehicle = transport.CostPerVehicle,
                TourLeaderTip = transport.TourLeaderTip,
                DriverTip = transport.DriverTip
            });
        }
        
        // الإقامة
        foreach (var accommodation in trip.Accommodations)
        {
            _context.TripAccommodations.Add(new TripAccommodation
            {
                TripId = tripId,
                Type = accommodation.Type,
                HotelName = accommodation.HotelName,
                Rating = accommodation.Rating,
                CruiseLevel = accommodation.CruiseLevel,
                RoomType = accommodation.RoomType,
                NumberOfRooms = accommodation.NumberOfRooms,
                NumberOfNights = accommodation.NumberOfNights,
                ParticipantsCount = accommodation.ParticipantsCount,
                CostPerRoomPerNight = accommodation.CostPerRoomPerNight,
                GuideCost = accommodation.GuideCost,
                MealPlan = accommodation.MealPlan,
                CheckInDate = accommodation.CheckInDate,
                CheckOutDate = accommodation.CheckOutDate
            });
        }
        
        // المرشدين
        foreach (var guide in trip.Guides)
        {
            _context.TripGuides.Add(new TripGuide
            {
                TripId = tripId,
                GuideName = guide.GuideName,
                Phone = guide.Phone,
                Email = guide.Email,
                Languages = guide.Languages,
                BaseFee = guide.BaseFee,
                CommissionPercentage = guide.CommissionPercentage,
                CommissionAmount = guide.CommissionAmount,
                Notes = guide.Notes
            });
        }
        
        // الرحلات الاختيارية
        foreach (var tour in trip.OptionalTours)
        {
            _context.TripOptionalTours.Add(new TripOptionalTour
            {
                TripId = tripId,
                TourName = tour.TourName,
                ParticipantsCount = tour.ParticipantsCount,
                SellingPrice = tour.SellingPrice,
                PurchasePrice = tour.PurchasePrice,
                GuideCommission = tour.GuideCommission,
                SalesRepCommission = tour.SalesRepCommission
            });
        }
        
        // المصاريف
        foreach (var expense in trip.Expenses)
        {
            _context.TripExpenses.Add(new TripExpense
            {
                TripId = tripId,
                ExpenseType = expense.ExpenseType,
                Description = expense.Description,
                Amount = expense.Amount,
                ExpenseDate = expense.ExpenseDate
            });
        }
        
        // ✅ حفظ جميع الأبناء الجديدة
        Console.WriteLine("💾 Saving all new child records...");
        
        try
        {
            int changesCount = await _context.SaveChangesAsync();
            Console.WriteLine($"✅ SaveChanges completed! Changes saved: {changesCount}");
            
            // ✅ Audit Log: Trip Updated
            if (_auditService != null)
            {
                await _auditService.LogAsync(
                    AuditAction.Update,
                    "Trip",
                    tripId,
                    $"{trip.Destination} - {trip.StartDate:dd/MM/yyyy}",
                    $"تم تعديل الرحلة: {trip.Destination}"
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ SaveChanges FAILED!");
            Console.WriteLine($"   Error: {ex.Message}");
            Console.WriteLine($"   InnerException: {ex.InnerException?.Message}");
            Console.WriteLine($"   StackTrace: {ex.StackTrace}");
            throw;
        }
        
        Console.WriteLine("════════════════════════════════════════");
        
        return trip;
    }
    
    public async Task<bool> DeleteTripAsync(int tripId)
    {
        var trip = await _context.Trips
            .Include(t => t.Bookings)
            .Include(t => t.Programs)
            .Include(t => t.Transportation)
            .Include(t => t.Accommodations)
            .Include(t => t.Guides)
            .Include(t => t.OptionalTours)
            .Include(t => t.Expenses)
            .FirstOrDefaultAsync(t => t.TripId == tripId);
        
        if (trip == null)
            return false;
        
        if (trip.Bookings.Any(b => b.Status == BookingStatus.Confirmed))
        {
            throw new Exception("لا يمكن حذف رحلة بها حجوزات مؤكدة");
        }
        
        // حذف جميع البيانات المرتبطة
        if (trip.Programs.Any())
            _context.TripPrograms.RemoveRange(trip.Programs);
            
        if (trip.Transportation.Any())
            _context.TripTransportations.RemoveRange(trip.Transportation);
            
        if (trip.Accommodations.Any())
            _context.TripAccommodations.RemoveRange(trip.Accommodations);
            
        if (trip.Guides.Any())
            _context.TripGuides.RemoveRange(trip.Guides);
            
        if (trip.OptionalTours.Any())
            _context.TripOptionalTours.RemoveRange(trip.OptionalTours);
            
        if (trip.Expenses.Any())
            _context.TripExpenses.RemoveRange(trip.Expenses);
        
        // حذف الرحلة نفسها
        _context.Trips.Remove(trip);
        await _context.SaveChangesAsync();
        
        // ✅ Audit Log: Trip Deleted
        if (_auditService != null)
        {
            await _auditService.LogAsync(
                AuditAction.Delete,
                "Trip",
                tripId,
                $"{trip.Destination} - {trip.StartDate:dd/MM/yyyy}",
                $"تم حذف الرحلة: {trip.Destination}"
            );
        }
        
        return true;
    }
    
    public async Task<Trip?> GetTripByIdAsync(int tripId, bool includeDetails = false)
    {
        var query = _context.Trips.AsQueryable();
        
        if (includeDetails)
        {
            query = query
                .Include(t => t.Currency)
                .Include(t => t.Creator)
                .Include(t => t.Programs)
                .Include(t => t.Transportation)
                .Include(t => t.Accommodations)
                .Include(t => t.Guides)
                .Include(t => t.OptionalTours)
                .Include(t => t.Expenses)
                .Include(t => t.Bookings).ThenInclude(b => b.Customer);
        }
        
        return await query.FirstOrDefaultAsync(t => t.TripId == tripId);
    }
    
    public async Task<List<Trip>> GetAllTripsAsync(bool includeDetails = false)
    {
        var query = _context.Trips.AsQueryable();
        
        if (includeDetails)
        {
            query = query
                .Include(t => t.Currency)
                .Include(t => t.Creator)
                .Include(t => t.Programs)
                .Include(t => t.Transportation)
                .Include(t => t.Accommodations)
                .Include(t => t.Guides)
                .Include(t => t.OptionalTours)
                .Include(t => t.Expenses)
                .Include(t => t.Bookings)
                    .ThenInclude(b => b.Payments);
        }
        else
        {
            // ✅ حتى لو مش includeDetails، لازم نجيب Creator عشان نعرض اسم اليوزر
            query = query.Include(t => t.Creator);
        }
        
        var trips = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        
        // حساب التكاليف لكل رحلة وحفظها
        if (includeDetails)
        {
            bool hasChanges = false;
            foreach (var trip in trips)
            {
                var oldCost = trip.TotalCost;
                trip.CalculateTotalCost();
                
                // ✅ حفظ التكلفة الجديدة إذا تغيرت
                if (Math.Abs(trip.TotalCost - oldCost) > 0.01m)
                {
                    hasChanges = true;
                }
            }
            
            // ✅ حفظ التغييرات في قاعدة البيانات
            if (hasChanges)
            {
                await _context.SaveChangesAsync();
            }
        }
        
        return trips;
    }
    
    // ══════════════════════════════════════
    // Business Logic
    // ══════════════════════════════════════
    
    public async Task<decimal> CalculateTotalCostAsync(int tripId)
    {
        var trip = await GetTripByIdAsync(tripId, true);
        if (trip == null) return 0;
        
        trip.CalculateTotalCost();
        return trip.TotalCost;
    }
    
    public async Task<decimal> CalculateProfitAsync(int tripId)
    {
        var trip = await GetTripByIdAsync(tripId, true);
        if (trip == null) return 0;
        
        return trip.NetProfit;
    }
    
    public async Task<bool> CheckAvailabilityAsync(int tripId, int numberOfPersons)
    {
        var trip = await _context.Trips.FindAsync(tripId);
        if (trip == null) return false;
        
        return trip.AvailableSeats >= numberOfPersons;
    }
    
    public async Task<bool> UpdateTripStatusAsync(int tripId, TripStatus newStatus, int userId)
    {
        // ✅ استخدام ExecuteUpdate لتجنب مشاكل الـ tracking
        var rowsAffected = await _context.Trips
            .Where(t => t.TripId == tripId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(t => t.Status, newStatus)
                .SetProperty(t => t.UpdatedBy, userId)
                .SetProperty(t => t.UpdatedAt, DateTime.UtcNow));
        
        return rowsAffected > 0;
    }
    
    public async Task<string> GenerateTripNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var lastTrip = await _context.Trips
            .Where(t => t.TripNumber.StartsWith($"TR-{year}-"))
            .OrderByDescending(t => t.TripNumber)
            .FirstOrDefaultAsync();
        
        int nextNumber = 1;
        if (lastTrip != null)
        {
            var parts = lastTrip.TripNumber.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }
        
        return $"TR-{year}-{nextNumber:D3}";
    }
    
    public async Task<string> GenerateTripCodeAsync()
    {
        // توليد كود فريد بناءً على التاريخ والوقت
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        var code = $"TC{timestamp}{random}";
        
        // التحقق من عدم وجود تكرار
        while (await _context.Trips.AnyAsync(t => t.TripCode == code))
        {
            random = new Random().Next(1000, 9999);
            code = $"TC{timestamp}{random}";
        }
        
        return code;
    }
    
    public async Task<bool> PublishTripAsync(int tripId, int userId)
    {
        var trip = await _context.Trips.FindAsync(tripId);
        if (trip == null) return false;
        
        trip.IsPublished = true;
        trip.Status = TripStatus.Confirmed;
        trip.UpdatedBy = userId;
        trip.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> UnpublishTripAsync(int tripId, int userId)
    {
        var trip = await _context.Trips.FindAsync(tripId);
        if (trip == null) return false;
        
        trip.IsPublished = false;
        trip.Status = TripStatus.Draft;
        trip.UpdatedBy = userId;
        trip.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
    
    // ══════════════════════════════════════
    // Filtering & Search
    // ══════════════════════════════════════
    
    public async Task<List<Trip>> GetTripsByTypeAsync(TripType type)
    {
        var trips = await _context.Trips
            .Where(t => t.IsActive && t.TripType == type)
            .Include(t => t.Currency)
            .Include(t => t.Programs)
            .Include(t => t.Transportation)
            .Include(t => t.Accommodations)
            .Include(t => t.Guides)
            .Include(t => t.OptionalTours)
            .Include(t => t.Expenses)
            .Include(t => t.Bookings)
                .ThenInclude(b => b.Payments)
            .OrderByDescending(t => t.StartDate)
            .ToListAsync();
        
        // حساب التكاليف وحفظها
        bool hasChanges = false;
        foreach (var trip in trips)
        {
            var oldCost = trip.TotalCost;
            trip.CalculateTotalCost();
            
            if (Math.Abs(trip.TotalCost - oldCost) > 0.01m)
            {
                hasChanges = true;
            }
        }
        
        if (hasChanges)
        {
            await _context.SaveChangesAsync();
        }
        
        return trips;
    }
    
    public async Task<List<Trip>> GetTripsByStatusAsync(TripStatus status)
    {
        var trips = await _context.Trips
            .Where(t => t.IsActive && t.Status == status)
            .Include(t => t.Currency)
            .Include(t => t.Programs)
            .Include(t => t.Transportation)
            .Include(t => t.Accommodations)
            .Include(t => t.Guides)
            .Include(t => t.OptionalTours)
            .Include(t => t.Expenses)
            .Include(t => t.Bookings)
                .ThenInclude(b => b.Payments)
            .OrderByDescending(t => t.StartDate)
            .ToListAsync();
        
        // حساب التكاليف وحفظها
        bool hasChanges = false;
        foreach (var trip in trips)
        {
            var oldCost = trip.TotalCost;
            trip.CalculateTotalCost();
            
            if (Math.Abs(trip.TotalCost - oldCost) > 0.01m)
            {
                hasChanges = true;
            }
        }
        
        if (hasChanges)
        {
            await _context.SaveChangesAsync();
        }
        
        return trips;
    }
    
    public async Task<List<Trip>> GetTripsByDateRangeAsync(DateTime from, DateTime to)
    {
        return await _context.Trips
            .Where(t => t.IsActive && t.StartDate >= from && t.EndDate <= to)
            .Include(t => t.Currency)
            .OrderBy(t => t.StartDate)
            .ToListAsync();
    }
    
    public async Task<List<Trip>> SearchTripsAsync(string searchTerm)
    {
        searchTerm = searchTerm.ToLower();
        var trips = await _context.Trips
            .Where(t => t.IsActive && 
                (t.TripName.ToLower().Contains(searchTerm) ||
                 t.TripNumber.ToLower().Contains(searchTerm) ||
                 (t.Description != null && t.Description.ToLower().Contains(searchTerm))))
            .Include(t => t.Currency)
            .Include(t => t.Programs)
            .Include(t => t.Transportation)
            .Include(t => t.Accommodations)
            .Include(t => t.Guides)
            .Include(t => t.OptionalTours)
            .Include(t => t.Expenses)
            .Include(t => t.Bookings)
                .ThenInclude(b => b.Payments)
            .OrderByDescending(t => t.StartDate)
            .ToListAsync();
        
        // حساب التكاليف وحفظها
        bool hasChanges = false;
        foreach (var trip in trips)
        {
            var oldCost = trip.TotalCost;
            trip.CalculateTotalCost();
            
            if (Math.Abs(trip.TotalCost - oldCost) > 0.01m)
            {
                hasChanges = true;
            }
        }
        
        if (hasChanges)
        {
            await _context.SaveChangesAsync();
        }
        
        return trips;
    }
    
    public async Task<List<Trip>> GetActiveTripsAsync()
    {
        return await _context.Trips
            .Where(t => t.IsActive && t.IsPublished && t.Status == TripStatus.Confirmed)
            .Include(t => t.Currency)
            .OrderBy(t => t.StartDate)
            .ToListAsync();
    }
    
    public async Task<List<Trip>> GetUpcomingTripsAsync()
    {
        var today = DateTime.Today;
        return await _context.Trips
            .Where(t => t.IsActive && t.StartDate > today)
            .Include(t => t.Currency)
            .OrderBy(t => t.StartDate)
            .ToListAsync();
    }
    
    /// <summary>
    /// قفل الرحلة من التعديل في قسم الرحلات
    /// </summary>
    public async Task<bool> LockTripForTripsAsync(int tripId)
    {
        // ✅ استخدام ExecuteUpdate لتجنب مشاكل الـ tracking
        var rowsAffected = await _context.Trips
            .Where(t => t.TripId == tripId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(t => t.IsLockedForTrips, true)
                .SetProperty(t => t.UpdatedAt, DateTime.UtcNow));
        
        return rowsAffected > 0;
    }
    
    /// <summary>
    /// فتح الرحلة للتعديل في قسم الرحلات
    /// </summary>
    public async Task<bool> UnlockTripForTripsAsync(int tripId)
    {
        // ✅ استخدام ExecuteUpdate لتجنب مشاكل الـ tracking
        var rowsAffected = await _context.Trips
            .Where(t => t.TripId == tripId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(t => t.IsLockedForTrips, false)
                .SetProperty(t => t.UpdatedAt, DateTime.UtcNow));
        
        return rowsAffected > 0;
    }
    
    // ══════════════════════════════════════
    // Trip Components - مكونات الرحلة
    // ══════════════════════════════════════
    
    // ── Programs ──
    public async Task<TripProgram> AddProgramAsync(TripProgram program)
    {
        _context.TripPrograms.Add(program);
        await _context.SaveChangesAsync();
        return program;
    }
    
    public async Task<TripProgram> UpdateProgramAsync(TripProgram program)
    {
        _context.TripPrograms.Update(program);
        await _context.SaveChangesAsync();
        return program;
    }
    
    public async Task<bool> DeleteProgramAsync(int programId)
    {
        var program = await _context.TripPrograms.FindAsync(programId);
        if (program == null) return false;
        
        _context.TripPrograms.Remove(program);
        await _context.SaveChangesAsync();
        return true;
    }
    
    // ── Transportation ──
    public async Task<TripTransportation> AddTransportationAsync(TripTransportation transportation)
    {
        _context.TripTransportations.Add(transportation);
        await _context.SaveChangesAsync();
        
        await RecalculateTripCostAsync(transportation.TripId);
        return transportation;
    }
    
    public async Task<TripTransportation> UpdateTransportationAsync(TripTransportation transportation)
    {
        _context.TripTransportations.Update(transportation);
        await _context.SaveChangesAsync();
        
        await RecalculateTripCostAsync(transportation.TripId);
        return transportation;
    }
    
    public async Task<bool> DeleteTransportationAsync(int transportationId)
    {
        var transportation = await _context.TripTransportations.FindAsync(transportationId);
        if (transportation == null) return false;
        
        var tripId = transportation.TripId;
        _context.TripTransportations.Remove(transportation);
        await _context.SaveChangesAsync();
        
        await RecalculateTripCostAsync(tripId);
        return true;
    }
    
    // ── Accommodation ──
    public async Task<TripAccommodation> AddAccommodationAsync(TripAccommodation accommodation)
    {
        _context.TripAccommodations.Add(accommodation);
        await _context.SaveChangesAsync();
        
        await RecalculateTripCostAsync(accommodation.TripId);
        return accommodation;
    }
    
    public async Task<TripAccommodation> UpdateAccommodationAsync(TripAccommodation accommodation)
    {
        _context.TripAccommodations.Update(accommodation);
        await _context.SaveChangesAsync();
        
        await RecalculateTripCostAsync(accommodation.TripId);
        return accommodation;
    }
    
    public async Task<bool> DeleteAccommodationAsync(int accommodationId)
    {
        var accommodation = await _context.TripAccommodations.FindAsync(accommodationId);
        if (accommodation == null) return false;
        
        var tripId = accommodation.TripId;
        _context.TripAccommodations.Remove(accommodation);
        await _context.SaveChangesAsync();
        
        await RecalculateTripCostAsync(tripId);
        return true;
    }
    
    // ── Guide ──
    public async Task<TripGuide> AddGuideAsync(TripGuide guide)
    {
        _context.TripGuides.Add(guide);
        await _context.SaveChangesAsync();
        
        await RecalculateTripCostAsync(guide.TripId);
        return guide;
    }
    
    public async Task<TripGuide> UpdateGuideAsync(TripGuide guide)
    {
        _context.TripGuides.Update(guide);
        await _context.SaveChangesAsync();
        
        await RecalculateTripCostAsync(guide.TripId);
        return guide;
    }
    
    public async Task<bool> DeleteGuideAsync(int guideId)
    {
        var guide = await _context.TripGuides.FindAsync(guideId);
        if (guide == null) return false;
        
        var tripId = guide.TripId;
        _context.TripGuides.Remove(guide);
        await _context.SaveChangesAsync();
        
        await RecalculateTripCostAsync(tripId);
        return true;
    }
    
    // ── Optional Tours ──
    public async Task<TripOptionalTour> AddOptionalTourAsync(TripOptionalTour tour)
    {
        _context.TripOptionalTours.Add(tour);
        await _context.SaveChangesAsync();
        return tour;
    }
    
    public async Task<TripOptionalTour> UpdateOptionalTourAsync(TripOptionalTour tour)
    {
        _context.TripOptionalTours.Update(tour);
        await _context.SaveChangesAsync();
        return tour;
    }
    
    public async Task<bool> DeleteOptionalTourAsync(int tourId)
    {
        var tour = await _context.TripOptionalTours.FindAsync(tourId);
        if (tour == null) return false;
        
        _context.TripOptionalTours.Remove(tour);
        await _context.SaveChangesAsync();
        return true;
    }
    
    // ── Expenses ──
    public async Task<TripExpense> AddExpenseAsync(TripExpense expense)
    {
        _context.TripExpenses.Add(expense);
        await _context.SaveChangesAsync();
        
        await RecalculateTripCostAsync(expense.TripId);
        return expense;
    }
    
    public async Task<TripExpense> UpdateExpenseAsync(TripExpense expense)
    {
        _context.TripExpenses.Update(expense);
        await _context.SaveChangesAsync();
        
        await RecalculateTripCostAsync(expense.TripId);
        return expense;
    }
    
    public async Task<bool> DeleteExpenseAsync(int expenseId)
    {
        var expense = await _context.TripExpenses.FindAsync(expenseId);
        if (expense == null) return false;
        
        var tripId = expense.TripId;
        _context.TripExpenses.Remove(expense);
        await _context.SaveChangesAsync();
        
        await RecalculateTripCostAsync(tripId);
        return true;
    }
    
    // ══════════════════════════════════════
    // Validation & Helper Methods
    // ══════════════════════════════════════
    
    public Task<(bool IsValid, List<string> Errors)> ValidateTripAsync(Trip trip)
    {
        var errors = new List<string>();
        
        if (string.IsNullOrWhiteSpace(trip.TripName))
            errors.Add("اسم الرحلة مطلوب");
        
        if (trip.StartDate >= trip.EndDate)
            errors.Add("تاريخ البدء يجب أن يكون قبل تاريخ الانتهاء");
        
        if (trip.TotalCapacity <= 0)
            errors.Add("الطاقة الاستيعابية يجب أن تكون أكبر من صفر");
        
        if (trip.SellingPricePerPerson <= 0)
            errors.Add("سعر البيع للفرد يجب أن يكون أكبر من صفر");
        
        if (trip.CurrencyId <= 0)
            errors.Add("العملة مطلوبة");
        
        return Task.FromResult((errors.Count == 0, errors));
    }
    
    public async Task<(bool CanDelete, string Reason)> CanDeleteTripAsync(int tripId)
    {
        var trip = await _context.Trips
            .Include(t => t.Bookings)
            .FirstOrDefaultAsync(t => t.TripId == tripId);
        
        if (trip == null)
            return (false, "الرحلة غير موجودة");
        
        var confirmedBookings = trip.Bookings.Count(b => b.Status == BookingStatus.Confirmed);
        if (confirmedBookings > 0)
            return (false, $"لا يمكن حذف الرحلة لوجود {confirmedBookings} حجز مؤكد");
        
        return (true, string.Empty);
    }
    
    // ── Helper: إعادة حساب التكلفة الإجمالية ──
    private async Task RecalculateTripCostAsync(int tripId)
    {
        var trip = await GetTripByIdAsync(tripId, true);
        if (trip != null)
        {
            trip.CalculateTotalCost();
            await _context.SaveChangesAsync();
        }
    }
    
    /// <summary>
    /// إعادة حساب تكاليف جميع الرحلات (One-time fix للداتا الموجودة)
    /// </summary>
    public async Task<int> RecalculateAllTripCostsAsync()
    {
        var trips = await _context.Trips
            .Include(t => t.Programs)
            .Include(t => t.Transportation)
            .Include(t => t.Accommodations)
            .Include(t => t.Guides)
            .Include(t => t.OptionalTours)
            .Include(t => t.Expenses)
            .ToListAsync();
        
        int updatedCount = 0;
        foreach (var trip in trips)
        {
            var oldCost = trip.TotalCost;
            trip.CalculateTotalCost();
            
            if (Math.Abs(trip.TotalCost - oldCost) > 0.01m)
            {
                updatedCount++;
            }
        }
        
        if (updatedCount > 0)
        {
            await _context.SaveChangesAsync();
        }
        
        return updatedCount;
    }
}

using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GraceWay.AccountingSystem.Application.Services;

public class FlightBookingService : IFlightBookingService
{
    private readonly AppDbContext _context;
    private readonly IAuthService _authService;

    public FlightBookingService(AppDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<List<FlightBooking>> GetAllFlightBookingsAsync(string? searchTerm = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null)
                return new List<FlightBooking>();

            // ✅ التحقق من الأدمن أولاً قبل بناء الـ query
            bool isAdmin = await IsAdminAsync(currentUser.UserId);
            
            Console.WriteLine($"🔍 GetAllFlightBookingsAsync: User={currentUser.Username}, IsAdmin={isAdmin}");

            var query = _context.FlightBookings
                .AsNoTracking()
                .Include(f => f.CreatedByUser)
                .AsQueryable();

            // ✅ فلترة حسب اليوزر (إذا لم يكن أدمن)
            if (!isAdmin)
            {
                query = query.Where(f => f.CreatedByUserId == currentUser.UserId);
                Console.WriteLine($"🔹 Filtering by CreatedByUserId={currentUser.UserId}");
            }
            else
            {
                Console.WriteLine($"✅ Admin mode: Showing all flight bookings");
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(f => 
                    f.BookingNumber.Contains(searchTerm) ||
                    f.ClientName.Contains(searchTerm) ||
                    f.Supplier.Contains(searchTerm) ||
                    f.TicketNumbers.Contains(searchTerm) ||
                    f.MobileNumber.Contains(searchTerm)
                );
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(f => f.TicketStatus == status);
            }

            // Apply date range filter
            if (fromDate.HasValue)
            {
                query = query.Where(f => f.TravelDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(f => f.TravelDate <= toDate.Value);
            }

            var result = await query.OrderByDescending(f => f.IssuanceDate).ToListAsync();
            Console.WriteLine($"✅ Found {result.Count} flight bookings");
            
            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in GetAllFlightBookingsAsync: {ex.Message}");
            throw new Exception($"خطأ في جلب حجوزات الطيران: {ex.Message}", ex);
        }
    }

    public async Task<FlightBooking?> GetFlightBookingByIdAsync(int flightBookingId)
    {
        try
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null)
                return null;

            // ✅ التحقق من الأدمن أولاً
            bool isAdmin = await IsAdminAsync(currentUser.UserId);

            var query = _context.FlightBookings
                .AsNoTracking()
                .Include(f => f.CreatedByUser)
                .AsQueryable();

            // ✅ التحقق من الصلاحية
            if (!isAdmin)
            {
                query = query.Where(f => f.CreatedByUserId == currentUser.UserId);
            }

            return await query.FirstOrDefaultAsync(f => f.FlightBookingId == flightBookingId);
        }
        catch (Exception ex)
        {
            throw new Exception($"خطأ في جلب حجز الطيران: {ex.Message}", ex);
        }
    }

    public async Task<FlightBooking> CreateFlightBookingAsync(FlightBooking flightBooking)
    {
        try
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null)
                throw new UnauthorizedAccessException("لا يوجد مستخدم مسجل");

            // Generate booking number if not provided
            if (string.IsNullOrWhiteSpace(flightBooking.BookingNumber))
            {
                flightBooking.BookingNumber = await GenerateBookingNumberAsync();
            }

            flightBooking.CreatedByUserId = currentUser.UserId; // ✅ حفظ معرف اليوزر
            flightBooking.CreatedAt = DateTime.UtcNow;
            flightBooking.UpdatedAt = DateTime.UtcNow;

            _context.FlightBookings.Add(flightBooking);
            await _context.SaveChangesAsync();

            return flightBooking;
        }
        catch (Exception ex)
        {
            throw new Exception($"خطأ في إنشاء حجز الطيران: {ex.Message}", ex);
        }
    }

    public async Task<bool> UpdateFlightBookingAsync(FlightBooking flightBooking)
    {
        try
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null)
                throw new UnauthorizedAccessException("لا يوجد مستخدم مسجل");

            var existing = await _context.FlightBookings.FindAsync(flightBooking.FlightBookingId);
            if (existing == null)
                throw new Exception("حجز الطيران غير موجود");

            // ✅ التحقق من الصلاحية
            bool isAdmin = await IsAdminAsync(currentUser.UserId);
            if (!isAdmin && existing.CreatedByUserId != currentUser.UserId)
                throw new UnauthorizedAccessException("غير مصرح لك بتعديل هذا الحجز");

            // Update properties
            existing.BookingNumber = flightBooking.BookingNumber;
            existing.IssuanceDate = flightBooking.IssuanceDate;
            existing.TravelDate = flightBooking.TravelDate;
            existing.ClientName = flightBooking.ClientName;
            existing.ClientRoute = flightBooking.ClientRoute ?? string.Empty;
            existing.Supplier = flightBooking.Supplier;
            existing.System = flightBooking.System;
            existing.TicketStatus = flightBooking.TicketStatus;
            existing.PaymentMethod = flightBooking.PaymentMethod;
            existing.SellingPrice = flightBooking.SellingPrice;
            existing.NetPrice = flightBooking.NetPrice;
            existing.TicketCount = flightBooking.TicketCount;
            existing.TicketNumbers = flightBooking.TicketNumbers ?? string.Empty;
            existing.MobileNumber = flightBooking.MobileNumber;
            existing.Notes = flightBooking.Notes ?? string.Empty;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.Entry(existing).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"خطأ في تحديث حجز الطيران: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteFlightBookingAsync(int flightBookingId)
    {
        try
        {
            var currentUser = _authService.CurrentUser;
            if (currentUser == null)
                throw new UnauthorizedAccessException("لا يوجد مستخدم مسجل");

            var flightBooking = await _context.FlightBookings.FindAsync(flightBookingId);
            if (flightBooking == null)
                throw new Exception("حجز الطيران غير موجود");

            // ✅ التحقق من الصلاحية
            bool isAdmin = await IsAdminAsync(currentUser.UserId);
            if (!isAdmin && flightBooking.CreatedByUserId != currentUser.UserId)
                throw new UnauthorizedAccessException("غير مصرح لك بحذف هذا الحجز");

            _context.FlightBookings.Remove(flightBooking);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"خطأ في حذف حجز الطيران: {ex.Message}", ex);
        }
    }

    private Task<bool> IsAdminAsync(int userId)
    {
        return ServiceHelpers.IsAdminAsync(_context, userId);
    }

    public async Task<string> GenerateBookingNumberAsync()
    {
        try
        {
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month;
            var lastBooking = await _context.FlightBookings
                .Where(f => f.BookingNumber.StartsWith($"FL{year}{month:D2}"))
                .OrderByDescending(f => f.BookingNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastBooking != null)
            {
                var lastNumber = lastBooking.BookingNumber.Substring(8);
                if (int.TryParse(lastNumber, out int num))
                {
                    nextNumber = num + 1;
                }
            }

            return $"FL{year}{month:D2}{nextNumber:D4}";
        }
        catch
        {
            return $"FL{DateTime.Now:yyyyMMdd}{new Random().Next(1000, 9999)}";
        }
    }

    public async Task<List<FlightBooking>> GetFlightBookingsBySupplierAsync(string supplier, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.FlightBookings.AsNoTracking().Where(f => f.Supplier == supplier);

        if (fromDate.HasValue)
        {
            query = query.Where(f => f.TravelDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(f => f.TravelDate <= toDate.Value);
        }

        return await query.OrderByDescending(f => f.TravelDate).ToListAsync();
    }

    public async Task<List<FlightBooking>> GetFlightBookingsByClientAsync(string clientName, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.FlightBookings.AsNoTracking().Where(f => f.ClientName.Contains(clientName));

        if (fromDate.HasValue)
        {
            query = query.Where(f => f.TravelDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(f => f.TravelDate <= toDate.Value);
        }

        return await query.OrderByDescending(f => f.TravelDate).ToListAsync();
    }

    public async Task<decimal> GetTotalProfitAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.FlightBookings.AsNoTracking();

        if (fromDate.HasValue)
            query = query.Where(f => f.TravelDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(f => f.TravelDate <= toDate.Value);

        return await query.SumAsync(f => (f.SellingPrice - f.NetPrice) * f.TicketCount);
    }

    public async Task<Dictionary<string, decimal>> GetProfitBySupplierAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.FlightBookings.AsNoTracking();

        if (fromDate.HasValue)
            query = query.Where(f => f.TravelDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(f => f.TravelDate <= toDate.Value);

        return await query
            .GroupBy(f => f.Supplier)
            .Select(g => new { Supplier = g.Key, Profit = g.Sum(f => (f.SellingPrice - f.NetPrice) * f.TicketCount) })
            .ToDictionaryAsync(x => x.Supplier, x => x.Profit);
    }
}

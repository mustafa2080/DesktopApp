using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GraceWay.AccountingSystem.Application.Services;

public class UmrahTripService : IUmrahTripService
{
    private readonly AppDbContext _context;
    private readonly IAuthService _authService;
    
    public UmrahTripService(AppDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }
    
    // ══════════════════════════════════════
    // Umrah Trips
    // ══════════════════════════════════════
    
    public async Task<List<UmrahTrip>> GetAllTripsAsync()
    {
        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
            return new List<UmrahTrip>();

        // إذا كان أدمن، يشوف كل الرحلات
        // غير كذلك، يشوف رحلاته فقط
        IQueryable<UmrahTrip> query = _context.Set<UmrahTrip>()
            .Include(t => t.Pilgrims)
            .Include(t => t.CreatedByUser);

        if (!await IsAdminAsync(currentUser.UserId))
        {
            query = query.Where(t => t.CreatedByUserId == currentUser.UserId);
        }

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<UmrahTrip?> GetTripByIdAsync(int tripId)
    {
        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
            return null;

        IQueryable<UmrahTrip> query = _context.Set<UmrahTrip>()
            .Include(t => t.Pilgrims)
                .ThenInclude(p => p.Payments)
            .Include(t => t.CreatedByUser);

        // التحقق من الصلاحية: الأدمن يشوف كل حاجة، واليوزر العادي يشوف ملفاته فقط
        if (!await IsAdminAsync(currentUser.UserId))
        {
            query = query.Where(t => t.CreatedByUserId == currentUser.UserId);
        }

        return await query.FirstOrDefaultAsync(t => t.UmrahTripId == tripId);
    }
    
    public async Task<UmrahTrip> CreateTripAsync(UmrahTrip trip)
    {
        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
            throw new UnauthorizedAccessException("لا يوجد مستخدم مسجل");

        trip.TripNumber = await GenerateTripNumberAsync();
        trip.CreatedByUserId = currentUser.UserId; // ✅ حفظ معرف اليوزر
        trip.CreatedAt = DateTime.UtcNow;
        trip.UpdatedAt = DateTime.UtcNow;
        
        _context.Set<UmrahTrip>().Add(trip);
        await _context.SaveChangesAsync();
        return trip;
    }
    
    public async Task<UmrahTrip> UpdateTripAsync(UmrahTrip trip)
    {
        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
            throw new UnauthorizedAccessException("لا يوجد مستخدم مسجل");

        // التحقق من الصلاحية
        var existingTrip = await GetTripByIdAsync(trip.UmrahTripId);
        if (existingTrip == null)
            throw new UnauthorizedAccessException("غير مصرح لك بتعديل هذه الرحلة");

        trip.UpdatedAt = DateTime.UtcNow;
        trip.UpdatedBy = currentUser.UserId;
        _context.Set<UmrahTrip>().Update(trip);
        await _context.SaveChangesAsync();
        return trip;
    }
    
    public async Task<bool> DeleteTripAsync(int tripId)
    {
        var currentUser = _authService.CurrentUser;
        if (currentUser == null)
            return false;

        // التحقق من الصلاحية
        var trip = await GetTripByIdAsync(tripId);
        if (trip == null)
            return false;
        
        _context.Set<UmrahTrip>().Remove(trip);
        await _context.SaveChangesAsync();
        return true;
    }
    
    private async Task<bool> IsAdminAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);
        
        return user?.Role?.RoleName?.ToLower() == "admin" || 
               user?.Role?.RoleName?.ToLower() == "مدير";
    }
    
    public async Task<string> GenerateTripNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var lastTrip = await _context.Set<UmrahTrip>()
            .Where(t => t.TripNumber.StartsWith($"UMT-{year}-"))
            .OrderByDescending(t => t.TripNumber)
            .FirstOrDefaultAsync();
        
        int nextNumber = 1;
        if (lastTrip != null)
        {
            var parts = lastTrip.TripNumber.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int num))
            {
                nextNumber = num + 1;
            }
        }
        
        return $"UMT-{year}-{nextNumber:D3}";
    }
    
    // ══════════════════════════════════════
    // Pilgrims
    // ══════════════════════════════════════
    
    // DEPRECATED: UmrahPilgrim now belongs to UmrahPackage, not UmrahTrip
    // This method is kept for backward compatibility but should not be used
    public async Task<List<UmrahPilgrim>> GetTripPilgrimsAsync(int tripId)
    {
        // Return empty list as UmrahPilgrim is now linked to UmrahPackage
        return await Task.FromResult(new List<UmrahPilgrim>());
    }
    
    // DEPRECATED: UmrahPilgrim now belongs to UmrahPackage, not UmrahTrip
    // This method is kept for backward compatibility but should not be used
    public async Task<UmrahPilgrim?> GetPilgrimByIdAsync(int pilgrimId)
    {
        return await _context.Set<UmrahPilgrim>()
            .Include(p => p.Payments)
            .Include(p => p.Package)
            .FirstOrDefaultAsync(p => p.UmrahPilgrimId == pilgrimId);
    }
    
    public async Task<UmrahPilgrim> AddPilgrimToTripAsync(UmrahPilgrim pilgrim)
    {
        pilgrim.PilgrimNumber = await GeneratePilgrimNumberAsync();
        pilgrim.RegisteredAt = DateTime.UtcNow;
        pilgrim.UpdatedAt = DateTime.UtcNow;
        
        _context.Set<UmrahPilgrim>().Add(pilgrim);
        await _context.SaveChangesAsync();
        return pilgrim;
    }
    
    public async Task<UmrahPilgrim> UpdatePilgrimAsync(UmrahPilgrim pilgrim)
    {
        pilgrim.UpdatedAt = DateTime.UtcNow;
        _context.Set<UmrahPilgrim>().Update(pilgrim);
        await _context.SaveChangesAsync();
        return pilgrim;
    }
    
    public async Task<bool> DeletePilgrimAsync(int pilgrimId)
    {
        var pilgrim = await _context.Set<UmrahPilgrim>().FindAsync(pilgrimId);
        if (pilgrim == null) return false;
        
        _context.Set<UmrahPilgrim>().Remove(pilgrim);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<string> GeneratePilgrimNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var lastPilgrim = await _context.Set<UmrahPilgrim>()
            .Where(p => p.PilgrimNumber.StartsWith($"UMP-{year}-"))
            .OrderByDescending(p => p.PilgrimNumber)
            .FirstOrDefaultAsync();
        
        int nextNumber = 1;
        if (lastPilgrim != null)
        {
            var parts = lastPilgrim.PilgrimNumber.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int num))
            {
                nextNumber = num + 1;
            }
        }
        
        return $"UMP-{year}-{nextNumber:D4}";
    }
    
    // ══════════════════════════════════════
    // Payments
    // ══════════════════════════════════════
    
    public async Task<List<UmrahPayment>> GetPilgrimPaymentsAsync(int pilgrimId)
    {
        return await _context.Set<UmrahPayment>()
            .Where(p => p.UmrahPilgrimId == pilgrimId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }
    
    public async Task<UmrahPayment> AddPaymentAsync(UmrahPayment payment)
    {
        payment.PaymentNumber = await GeneratePaymentNumberAsync();
        payment.CreatedAt = DateTime.UtcNow;
        
        _context.Set<UmrahPayment>().Add(payment);
        
        // تحديث المبلغ المدفوع للمعتمر
        var pilgrim = await _context.Set<UmrahPilgrim>().FindAsync(payment.UmrahPilgrimId);
        if (pilgrim != null)
        {
            pilgrim.PaidAmount += payment.Amount;
            pilgrim.UpdatedAt = DateTime.UtcNow;
        }
        
        await _context.SaveChangesAsync();
        return payment;
    }
    
    public async Task<bool> DeletePaymentAsync(int paymentId)
    {
        var payment = await _context.Set<UmrahPayment>().FindAsync(paymentId);
        if (payment == null) return false;
        
        // تحديث المبلغ المدفوع للمعتمر
        var pilgrim = await _context.Set<UmrahPilgrim>().FindAsync(payment.UmrahPilgrimId);
        if (pilgrim != null)
        {
            pilgrim.PaidAmount -= payment.Amount;
            pilgrim.UpdatedAt = DateTime.UtcNow;
        }
        
        _context.Set<UmrahPayment>().Remove(payment);
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<string> GeneratePaymentNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month;
        var lastPayment = await _context.Set<UmrahPayment>()
            .Where(p => p.PaymentNumber.StartsWith($"UMPAY-{year}{month:D2}-"))
            .OrderByDescending(p => p.PaymentNumber)
            .FirstOrDefaultAsync();
        
        int nextNumber = 1;
        if (lastPayment != null)
        {
            var parts = lastPayment.PaymentNumber.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[1].Substring(6), out int num))
            {
                nextNumber = num + 1;
            }
        }
        
        return $"UMPAY-{year}{month:D2}-{nextNumber:D4}";
    }
    
    // ══════════════════════════════════════
    // Statistics
    // ══════════════════════════════════════
    
    public async Task<decimal> GetTripTotalRevenueAsync(int tripId)
    {
        var pilgrims = await GetTripPilgrimsAsync(tripId);
        return pilgrims.Sum(p => p.TotalAmount);
    }
    
    public async Task<decimal> GetTripTotalPaidAsync(int tripId)
    {
        var pilgrims = await GetTripPilgrimsAsync(tripId);
        return pilgrims.Sum(p => p.PaidAmount);
    }
    
    public async Task<decimal> GetTripTotalRemainingAsync(int tripId)
    {
        var pilgrims = await GetTripPilgrimsAsync(tripId);
        return pilgrims.Sum(p => p.RemainingAmount);
    }
    
    public async Task<int> GetTripPaidPilgrimsCountAsync(int tripId)
    {
        var pilgrims = await GetTripPilgrimsAsync(tripId);
        return pilgrims.Count(p => p.PaymentStatus == UmrahPaymentStatus.FullyPaid);
    }
    
    public async Task<int> GetTripPartialPaidPilgrimsCountAsync(int tripId)
    {
        var pilgrims = await GetTripPilgrimsAsync(tripId);
        return pilgrims.Count(p => p.PaymentStatus == UmrahPaymentStatus.PartiallyPaid);
    }
    
    public async Task<int> GetTripNotPaidPilgrimsCountAsync(int tripId)
    {
        var pilgrims = await GetTripPilgrimsAsync(tripId);
        return pilgrims.Count(p => p.PaymentStatus == UmrahPaymentStatus.NotPaid);
    }
}

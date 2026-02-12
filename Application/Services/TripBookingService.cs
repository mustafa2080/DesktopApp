using Microsoft.EntityFrameworkCore;
using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;

namespace GraceWay.AccountingSystem.Application.Services;

public class TripBookingService : ITripBookingService
{
    private readonly AppDbContext _context;
    private readonly ICashBoxService _cashBoxService;
    private readonly IJournalService _journalService;
    
    public TripBookingService(AppDbContext context, ICashBoxService cashBoxService, IJournalService journalService)
    {
        _context = context;
        _cashBoxService = cashBoxService;
        _journalService = journalService;
    }
    
    // ══════════════════════════════════════
    // CRUD Operations
    // ══════════════════════════════════════
    
    public async Task<TripBooking> CreateBookingAsync(TripBooking booking)
    {
        // توليد رقم حجز
        if (string.IsNullOrEmpty(booking.BookingNumber))
        {
            booking.BookingNumber = await GenerateBookingNumberAsync();
        }
        
        booking.BookingDate = DateTime.Now;
        booking.Status = BookingStatus.Pending;
        
        // التحقق من الصحة
        var validation = await ValidateBookingAsync(booking);
        if (!validation.IsValid)
        {
            throw new Exception(string.Join(", ", validation.Errors));
        }
        
        _context.TripBookings.Add(booking);
        
        // تحديث عدد المقاعد المحجوزة
        var trip = await _context.Trips.FindAsync(booking.TripId);
        if (trip != null)
        {
            trip.BookedSeats += booking.NumberOfPersons;
        }
        
        await _context.SaveChangesAsync();
        
        // إنشاء قيد يومي تلقائي إذا تم الدفع
        if (booking.TotalAmount > 0)
        {
            try
            {
                await _journalService.CreateTripBookingJournalEntryAsync(booking);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"فشل إنشاء القيد اليومي: {ex.Message}");
            }
        }
        
        return booking;
    }
    
    public async Task<TripBooking> UpdateBookingAsync(TripBooking booking)
    {
        var existing = await _context.TripBookings.FindAsync(booking.TripBookingId);
        if (existing == null)
            throw new Exception("الحجز غير موجود");
        
        // تحديث المقاعد إذا تغير العدد
        if (existing.NumberOfPersons != booking.NumberOfPersons)
        {
            var trip = await _context.Trips.FindAsync(booking.TripId);
            if (trip != null)
            {
                trip.BookedSeats = trip.BookedSeats - existing.NumberOfPersons + booking.NumberOfPersons;
            }
        }
        
        existing.NumberOfPersons = booking.NumberOfPersons;
        existing.PricePerPerson = booking.PricePerPerson;
        existing.SpecialRequests = booking.SpecialRequests;
        existing.Notes = booking.Notes;
        
        await _context.SaveChangesAsync();
        return existing;
    }
    
    public async Task<bool> CancelBookingAsync(int bookingId, string reason, int userId)
    {
        var booking = await _context.TripBookings
            .Include(b => b.Payments)
            .FirstOrDefaultAsync(b => b.TripBookingId == bookingId);
        
        if (booking == null) return false;
        
        if (!await CanCancelBookingAsync(bookingId))
        {
            throw new Exception("لا يمكن إلغاء الحجز بعد موعد الرحلة أو بعد الدفع الكامل");
        }
        
        booking.Status = BookingStatus.Cancelled;
        booking.Notes = $"تم الإلغاء: {reason}";
        
        // إعادة المقاعد للرحلة
        var trip = await _context.Trips.FindAsync(booking.TripId);
        if (trip != null)
        {
            trip.BookedSeats -= booking.NumberOfPersons;
        }
        
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<TripBooking?> GetBookingByIdAsync(int bookingId, bool includeDetails = false)
    {
        var query = _context.TripBookings.AsQueryable();
        
        if (includeDetails)
        {
            query = query
                .Include(b => b.Trip).ThenInclude(t => t.Currency)
                .Include(b => b.Customer)
                .Include(b => b.Creator)
                .Include(b => b.Payments).ThenInclude(p => p.CashBox);
        }
        
        return await query.FirstOrDefaultAsync(b => b.TripBookingId == bookingId);
    }
    
    public async Task<List<TripBooking>> GetAllBookingsAsync()
    {
        return await _context.TripBookings
            .Include(b => b.Trip)
            .Include(b => b.Customer)
            .Include(b => b.Creator)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();
    }
    
    public async Task<List<TripBooking>> GetBookingsByTripAsync(int tripId)
    {
        return await _context.TripBookings
            .Where(b => b.TripId == tripId)
            .Include(b => b.Customer)
            .Include(b => b.Creator)
            .Include(b => b.Payments)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();
    }
    
    public async Task<List<TripBooking>> GetBookingsByCustomerAsync(int customerId)
    {
        return await _context.TripBookings
            .Where(b => b.CustomerId == customerId)
            .Include(b => b.Trip)
            .Include(b => b.Creator)
            .Include(b => b.Payments)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();
    }
    
    public async Task<List<TripBooking>> GetBookingsByStatusAsync(BookingStatus status)
    {
        return await _context.TripBookings
            .Where(b => b.Status == status)
            .Include(b => b.Trip)
            .Include(b => b.Customer)
            .Include(b => b.Creator)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();
    }
    
    // ══════════════════════════════════════
    // Payment Operations
    // ══════════════════════════════════════
    
    public async Task<TripBookingPayment> RecordPaymentAsync(TripBookingPayment payment)
    {
        payment.PaymentDate = DateTime.Now;
        
        // حساب عمولة إنستا باي
        if (payment.PaymentMethod == PaymentMethod.InstaPay)
        {
            payment.InstaPayCommission = payment.Amount * 0.025m; // 2.5%
        }
        
        _context.TripBookingPayments.Add(payment);
        
        // تسجيل في الخزنة
        var cashTransaction = new CashTransaction
        {
            CashBoxId = payment.CashBoxId,
            Type = TransactionType.Income,
            Amount = payment.NetAmount,
            Description = $"دفعة حجز رحلة - {payment.Booking?.BookingNumber ?? ""}",
            TransactionDate = DateTime.Now,
            PaymentMethod = payment.PaymentMethod,
            ReferenceNumber = payment.ReferenceNumber,
            Notes = payment.Notes,
            CreatedBy = payment.CreatedBy
        };
        
        await _cashBoxService.AddTransactionAsync(cashTransaction);
        payment.CashTransactionId = cashTransaction.Id;
        
        // تحديث حالة الدفع
        var booking = await _context.TripBookings.FindAsync(payment.TripBookingId);
        if (booking != null)
        {
            booking.PaidAmount += payment.NetAmount;
            await UpdatePaymentStatusAsync(payment.TripBookingId);
        }
        
        await _context.SaveChangesAsync();
        return payment;
    }
    
    public async Task<decimal> GetTotalPaidAsync(int bookingId)
    {
        return await _context.TripBookingPayments
            .Where(p => p.TripBookingId == bookingId)
            .SumAsync(p => p.NetAmount);
    }
    
    public async Task<decimal> GetRemainingAmountAsync(int bookingId)
    {
        var booking = await _context.TripBookings.FindAsync(bookingId);
        if (booking == null) return 0;
        
        return booking.RemainingAmount;
    }
    
    public async Task<List<TripBookingPayment>> GetBookingPaymentsAsync(int bookingId)
    {
        return await _context.TripBookingPayments
            .Where(p => p.TripBookingId == bookingId)
            .Include(p => p.CashBox)
            .Include(p => p.Creator)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }
    
    // ══════════════════════════════════════
    // Business Logic
    // ══════════════════════════════════════
    
    public async Task<string> GenerateBookingNumberAsync()
    {
        var year = DateTime.Now.Year;
        var lastBooking = await _context.TripBookings
            .Where(b => b.BookingNumber.StartsWith($"TB-{year}-"))
            .OrderByDescending(b => b.BookingNumber)
            .FirstOrDefaultAsync();
        
        int nextNumber = 1;
        if (lastBooking != null)
        {
            var parts = lastBooking.BookingNumber.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
            {
                nextNumber = lastNumber + 1;
            }
        }
        
        return $"TB-{year}-{nextNumber:D4}";
    }
    
    public async Task<(bool IsValid, List<string> Errors)> ValidateBookingAsync(TripBooking booking)
    {
        var errors = new List<string>();
        
        // التحقق من الرحلة
        var trip = await _context.Trips.FindAsync(booking.TripId);
        if (trip == null)
        {
            errors.Add("الرحلة غير موجودة");
            return (false, errors);
        }
        
        if (!trip.IsPublished || trip.Status != TripStatus.Confirmed)
            errors.Add("الرحلة غير متاحة للحجز");
        
        if (trip.AvailableSeats < booking.NumberOfPersons)
            errors.Add($"لا توجد أماكن كافية. المتاح: {trip.AvailableSeats}");
        
        if (trip.StartDate < DateTime.Today)
            errors.Add("لا يمكن الحجز في رحلة انتهى موعدها");
        
        // التحقق من العميل
        var customer = await _context.Customers.FindAsync(booking.CustomerId);
        if (customer == null || !customer.IsActive)
            errors.Add("العميل غير موجود أو غير نشط");
        
        if (booking.NumberOfPersons <= 0)
            errors.Add("عدد الأفراد يجب أن يكون أكبر من صفر");
        
        if (booking.PricePerPerson <= 0)
            errors.Add("السعر للفرد يجب أن يكون أكبر من صفر");
        
        return (errors.Count == 0, errors);
    }
    
    public async Task<bool> ConfirmBookingAsync(int bookingId, int userId)
    {
        var booking = await _context.TripBookings.FindAsync(bookingId);
        if (booking == null) return false;
        
        if (booking.PaymentStatus != PaymentStatus.FullyPaid)
        {
            throw new Exception("يجب إكمال الدفع أولاً");
        }
        
        booking.Status = BookingStatus.Confirmed;
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> UpdatePaymentStatusAsync(int bookingId)
    {
        var booking = await _context.TripBookings.FindAsync(bookingId);
        if (booking == null) return false;
        
        var totalPaid = await GetTotalPaidAsync(bookingId);
        var totalAmount = booking.TotalAmount;
        
        if (totalPaid == 0)
            booking.PaymentStatus = PaymentStatus.NotPaid;
        else if (totalPaid >= totalAmount)
            booking.PaymentStatus = PaymentStatus.FullyPaid;
        else
            booking.PaymentStatus = PaymentStatus.PartiallyPaid;
        
        booking.PaidAmount = totalPaid;
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> CanCancelBookingAsync(int bookingId)
    {
        var booking = await _context.TripBookings
            .Include(b => b.Trip)
            .FirstOrDefaultAsync(b => b.TripBookingId == bookingId);
        
        if (booking == null) return false;
        if (booking.Status == BookingStatus.Cancelled) return false;
        if (booking.Trip.StartDate < DateTime.Today) return false;
        
        return true;
    }
    
    // ══════════════════════════════════════
    // Statistics
    // ══════════════════════════════════════
    
    public async Task<int> GetTotalBookingsCountAsync()
    {
        return await _context.TripBookings
            .Where(b => b.Status != BookingStatus.Cancelled)
            .CountAsync();
    }
    
    public async Task<decimal> GetTotalRevenueAsync()
    {
        return await _context.TripBookingPayments
            .SumAsync(p => p.NetAmount);
    }
    
    public async Task<decimal> GetTotalPendingPaymentsAsync()
    {
        return await _context.TripBookings
            .Where(b => b.Status != BookingStatus.Cancelled)
            .SumAsync(b => b.RemainingAmount);
    }
}

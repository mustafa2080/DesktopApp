using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Application.Services;

/// <summary>
/// واجهة خدمة إدارة حجوزات الرحلات
/// </summary>
public interface ITripBookingService
{
    // ══════════════════════════════════════
    // CRUD Operations - الحجوزات
    // ══════════════════════════════════════
    
    Task<TripBooking> CreateBookingAsync(TripBooking booking);
    Task<TripBooking> UpdateBookingAsync(TripBooking booking);
    Task<bool> CancelBookingAsync(int bookingId, string reason, int userId);
    Task<TripBooking?> GetBookingByIdAsync(int bookingId, bool includeDetails = false);
    Task<List<TripBooking>> GetAllBookingsAsync();
    Task<List<TripBooking>> GetBookingsByTripAsync(int tripId);
    Task<List<TripBooking>> GetBookingsByCustomerAsync(int customerId);
    Task<List<TripBooking>> GetBookingsByStatusAsync(BookingStatus status);
    
    // ══════════════════════════════════════
    // Payment Operations - المدفوعات
    // ══════════════════════════════════════
    
    Task<TripBookingPayment> RecordPaymentAsync(TripBookingPayment payment);
    Task<decimal> GetTotalPaidAsync(int bookingId);
    Task<decimal> GetRemainingAmountAsync(int bookingId);
    Task<List<TripBookingPayment>> GetBookingPaymentsAsync(int bookingId);
    
    // ══════════════════════════════════════
    // Business Logic - منطق العمل
    // ══════════════════════════════════════
    
    Task<string> GenerateBookingNumberAsync();
    Task<(bool IsValid, List<string> Errors)> ValidateBookingAsync(TripBooking booking);
    Task<bool> ConfirmBookingAsync(int bookingId, int userId);
    Task<bool> UpdatePaymentStatusAsync(int bookingId);
    Task<bool> CanCancelBookingAsync(int bookingId);
    
    // ══════════════════════════════════════
    // Statistics - الإحصائيات
    // ══════════════════════════════════════
    
    Task<int> GetTotalBookingsCountAsync();
    Task<decimal> GetTotalRevenueAsync();
    Task<decimal> GetTotalPendingPaymentsAsync();
}

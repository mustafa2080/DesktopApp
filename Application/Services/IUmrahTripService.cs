using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Application.Services;

/// <summary>
/// واجهة خدمة رحلات العمرة الجديدة
/// </summary>
public interface IUmrahTripService
{
    // ══════════════════════════════════════
    // Umrah Trips (الرحلات)
    // ══════════════════════════════════════
    
    Task<List<UmrahTrip>> GetAllTripsAsync();
    Task<UmrahTrip?> GetTripByIdAsync(int tripId);
    Task<UmrahTrip> CreateTripAsync(UmrahTrip trip);
    Task<UmrahTrip> UpdateTripAsync(UmrahTrip trip);
    Task<bool> DeleteTripAsync(int tripId);
    Task<string> GenerateTripNumberAsync();
    
    // ══════════════════════════════════════
    // Pilgrims (المعتمرين)
    // ══════════════════════════════════════
    
    Task<List<UmrahPilgrim>> GetTripPilgrimsAsync(int tripId);
    Task<UmrahPilgrim?> GetPilgrimByIdAsync(int pilgrimId);
    Task<UmrahPilgrim> AddPilgrimToTripAsync(UmrahPilgrim pilgrim);
    Task<UmrahPilgrim> UpdatePilgrimAsync(UmrahPilgrim pilgrim);
    Task<bool> DeletePilgrimAsync(int pilgrimId);
    Task<string> GeneratePilgrimNumberAsync();
    
    // ══════════════════════════════════════
    // Payments (الدفعات)
    // ══════════════════════════════════════
    
    Task<List<UmrahPayment>> GetPilgrimPaymentsAsync(int pilgrimId);
    Task<UmrahPayment> AddPaymentAsync(UmrahPayment payment);
    Task<bool> DeletePaymentAsync(int paymentId);
    Task<string> GeneratePaymentNumberAsync();
    
    // ══════════════════════════════════════
    // Statistics (الإحصائيات)
    // ══════════════════════════════════════
    
    Task<decimal> GetTripTotalRevenueAsync(int tripId);
    Task<decimal> GetTripTotalPaidAsync(int tripId);
    Task<decimal> GetTripTotalRemainingAsync(int tripId);
    Task<int> GetTripPaidPilgrimsCountAsync(int tripId);
    Task<int> GetTripPartialPaidPilgrimsCountAsync(int tripId);
    Task<int> GetTripNotPaidPilgrimsCountAsync(int tripId);
}

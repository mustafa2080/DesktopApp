using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Application.Services;

public interface IReservationService
{
    // Reservation CRUD
    Task<List<Reservation>> GetAllReservationsAsync(string? searchTerm = null, string? status = null);
    Task<Reservation?> GetReservationByIdAsync(int reservationId);
    Task<Reservation> CreateReservationAsync(Reservation reservation);
    Task<bool> UpdateReservationAsync(Reservation reservation);
    Task<bool> DeleteReservationAsync(int reservationId);
    Task<bool> ChangeReservationStatusAsync(int reservationId, string newStatus);
    
    // ServiceType CRUD
    Task<List<ServiceType>> GetAllServiceTypesAsync();
    Task<ServiceType?> GetServiceTypeByIdAsync(int serviceTypeId);
    Task<ServiceType> CreateServiceTypeAsync(ServiceType serviceType);
    Task<bool> UpdateServiceTypeAsync(ServiceType serviceType);
    Task<bool> DeleteServiceTypeAsync(int serviceTypeId);
    
    // Business Logic
    Task<string> GenerateReservationNumberAsync();
    Task<decimal> CalculateProfitAsync(int reservationId);
    Task<List<Reservation>> GetReservationsByCustomerAsync(int customerId);
    Task<List<Reservation>> GetReservationsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Dictionary<string, decimal>> GetReservationStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
}

using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Application.Services;

public interface IFlightBookingService
{
    // CRUD Operations
    Task<List<FlightBooking>> GetAllFlightBookingsAsync(string? searchTerm = null, string? status = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<FlightBooking?> GetFlightBookingByIdAsync(int flightBookingId);
    Task<FlightBooking> CreateFlightBookingAsync(FlightBooking flightBooking);
    Task<bool> UpdateFlightBookingAsync(FlightBooking flightBooking);
    Task<bool> DeleteFlightBookingAsync(int flightBookingId);

    // Business Operations
    Task<string> GenerateBookingNumberAsync();
    
    // Reports
    Task<List<FlightBooking>> GetFlightBookingsBySupplierAsync(string supplier, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<FlightBooking>> GetFlightBookingsByClientAsync(string clientName, DateTime? fromDate = null, DateTime? toDate = null);
    Task<decimal> GetTotalProfitAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<Dictionary<string, decimal>> GetProfitBySupplierAsync(DateTime? fromDate = null, DateTime? toDate = null);
}

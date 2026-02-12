using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Application.Services;

public interface ISupplierService
{
    // CRUD Operations
    Task<Supplier> CreateSupplierAsync(Supplier supplier);
    Task<Supplier?> GetSupplierByIdAsync(int supplierId);
    Task<List<Supplier>> GetAllSuppliersAsync();
    Task<List<Supplier>> GetActiveSuppliersAsync();
    Task UpdateSupplierAsync(Supplier supplier);
    Task DeleteSupplierAsync(int supplierId);
    
    // Balance Operations
    Task<decimal> GetSupplierBalanceAsync(int supplierId);
    Task UpdateSupplierBalanceAsync(int supplierId, decimal amount);
    
    // Reports
    Task<List<Supplier>> GetSuppliersWithBalanceAsync();
    Task<List<Supplier>> GetSuppliersExceedingCreditLimitAsync();
    Task<SupplierStatement> GetSupplierStatementAsync(int supplierId, DateTime? startDate = null, DateTime? endDate = null);
    
    // Code Generation
    Task<string> GenerateSupplierCodeAsync();
}

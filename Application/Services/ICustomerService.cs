using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Application.Services;

public interface ICustomerService
{
    // CRUD Operations
    Task<List<Customer>> GetAllCustomersAsync();
    Task<Customer?> GetCustomerByIdAsync(int id);
    Task<Customer?> GetCustomerByCodeAsync(string code);
    Task<Customer> CreateCustomerAsync(Customer customer);
    Task<Customer> UpdateCustomerAsync(Customer customer);
    Task DeleteCustomerAsync(int id);
    
    // Active Customers
    Task<List<Customer>> GetActiveCustomersAsync();
    
    // Search
    Task<List<Customer>> SearchCustomersAsync(string searchTerm);
    
    // Balance Operations
    Task<decimal> GetCustomerBalanceAsync(int customerId);
    Task UpdateCustomerBalanceAsync(int customerId, decimal amount);
    
    // Reports
    Task<List<Customer>> GetCustomersWithBalanceAsync();
    Task<List<Customer>> GetCustomersExceedingCreditLimitAsync();
    
    // Customer Statement
    Task<CustomerStatementDto> GetCustomerStatementAsync(int customerId, DateTime? startDate = null, DateTime? endDate = null);
    
    // Code Generation
    Task<string> GenerateCustomerCodeAsync();
}

// DTO for Customer Statement
public class CustomerStatementDto
{
    public Customer Customer { get; set; } = null!;
    public decimal OpeningBalance { get; set; }
    public List<StatementTransaction> Transactions { get; set; } = new();
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal ClosingBalance { get; set; }
}

public class StatementTransaction
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal Balance { get; set; }
}

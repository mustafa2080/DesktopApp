using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GraceWay.AccountingSystem.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _context;

    public CustomerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        return await _context.Customers
            .AsNoTracking()
            .OrderBy(c => c.CustomerName)
            .ToListAsync();
    }

    public async Task<Customer?> GetCustomerByIdAsync(int id)
    {
        return await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CustomerId == id);
    }

    public async Task<Customer?> GetCustomerByCodeAsync(string code)
    {
        return await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CustomerCode == code);
    }

    public async Task<Customer> CreateCustomerAsync(Customer customer)
    {
        // Generate customer code if not provided
        if (string.IsNullOrEmpty(customer.CustomerCode))
        {
            customer.CustomerCode = await GenerateCustomerCodeAsync();
        }
        
        customer.CreatedAt = DateTime.UtcNow;
        customer.CurrentBalance = customer.OpeningBalance;
        
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        
        return customer;
    }

    public async Task<Customer> UpdateCustomerAsync(Customer customer)
    {
        var existing = await _context.Customers.FindAsync(customer.CustomerId);
        if (existing == null)
            throw new Exception("العميل غير موجود");
        
        existing.CustomerName = customer.CustomerName;
        existing.CustomerNameEn = customer.CustomerNameEn;
        existing.Phone = customer.Phone;
        existing.Mobile = customer.Mobile;
        existing.Email = customer.Email;
        existing.Address = customer.Address;
        existing.City = customer.City;
        existing.Country = customer.Country;
        existing.TaxNumber = customer.TaxNumber;
        existing.CreditLimit = customer.CreditLimit;
        existing.PaymentTermDays = customer.PaymentTermDays;
        existing.IsActive = customer.IsActive;
        existing.Notes = customer.Notes;
        
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteCustomerAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            throw new Exception("العميل غير موجود");
        
        // Check if customer has invoices
        var hasInvoices = await _context.SalesInvoices.AnyAsync(i => i.CustomerId == id);
        if (hasInvoices)
            throw new Exception("لا يمكن حذف العميل لوجود فواتير مرتبطة به");
        
        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Customer>> GetActiveCustomersAsync()
    {
        return await _context.Customers
            .Where(c => c.IsActive)
            .OrderBy(c => c.CustomerName)
            .ToListAsync();
    }

    public async Task<List<Customer>> SearchCustomersAsync(string searchTerm)
    {
        searchTerm = searchTerm.ToLower();
        
        return await _context.Customers
            .Where(c => c.CustomerName.ToLower().Contains(searchTerm) ||
                       c.CustomerCode.ToLower().Contains(searchTerm) ||
                       (c.Phone != null && c.Phone.Contains(searchTerm)) ||
                       (c.Mobile != null && c.Mobile.Contains(searchTerm)))
            .OrderBy(c => c.CustomerName)
            .ToListAsync();
    }

    public async Task<decimal> GetCustomerBalanceAsync(int customerId)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        return customer?.CurrentBalance ?? 0;
    }

    public async Task UpdateCustomerBalanceAsync(int customerId, decimal amount)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer != null)
        {
            customer.CurrentBalance += amount;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Customer>> GetCustomersWithBalanceAsync()
    {
        return await _context.Customers
            .Where(c => c.CurrentBalance != 0)
            .OrderByDescending(c => Math.Abs(c.CurrentBalance))
            .ToListAsync();
    }

    public async Task<List<Customer>> GetCustomersExceedingCreditLimitAsync()
    {
        return await _context.Customers
            .Where(c => c.CurrentBalance > c.CreditLimit && c.CreditLimit > 0)
            .OrderByDescending(c => c.CurrentBalance - c.CreditLimit)
            .ToListAsync();
    }

    public async Task<CustomerStatementDto> GetCustomerStatementAsync(int customerId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var customer = await GetCustomerByIdAsync(customerId);
        if (customer == null)
            throw new Exception("العميل غير موجود");
        
        var statement = new CustomerStatementDto
        {
            Customer = customer,
            OpeningBalance = customer.OpeningBalance
        };
        
        // Get invoices
        var invoices = await _context.SalesInvoices
            .Where(i => i.CustomerId == customerId)
            .Where(i => !startDate.HasValue || i.InvoiceDate >= startDate.Value)
            .Where(i => !endDate.HasValue || i.InvoiceDate <= endDate.Value)
            .OrderBy(i => i.InvoiceDate)
            .ToListAsync();
        
        // Get payments
        var payments = await _context.InvoicePayments
            .Where(p => p.SalesInvoiceId.HasValue)
            .Where(p => !startDate.HasValue || p.PaymentDate >= startDate.Value)
            .Where(p => !endDate.HasValue || p.PaymentDate <= endDate.Value)
            .OrderBy(p => p.PaymentDate)
            .ToListAsync();
        
        // Filter payments by customer (need to check each invoice)
        var customerPayments = new List<InvoicePayment>();
        foreach (var payment in payments)
        {
            var invoice = await _context.SalesInvoices.FindAsync(payment.SalesInvoiceId);
            if (invoice?.CustomerId == customerId)
            {
                customerPayments.Add(payment);
            }
        }
        
        decimal runningBalance = statement.OpeningBalance;
        
        // Add invoice transactions
        foreach (var invoice in invoices)
        {
            runningBalance += invoice.TotalAmount;
            statement.Transactions.Add(new StatementTransaction
            {
                Date = invoice.InvoiceDate,
                Description = $"فاتورة مبيعات",
                ReferenceNumber = invoice.InvoiceNumber,
                Debit = invoice.TotalAmount,
                Credit = 0,
                Balance = runningBalance
            });
            statement.TotalDebit += invoice.TotalAmount;
        }
        
        // Add payment transactions
        foreach (var payment in customerPayments)
        {
            runningBalance -= payment.Amount;
            statement.Transactions.Add(new StatementTransaction
            {
                Date = payment.PaymentDate,
                Description = $"دفعة - {payment.PaymentMethod}",
                ReferenceNumber = payment.ReferenceNumber ?? "",
                Debit = 0,
                Credit = payment.Amount,
                Balance = runningBalance
            });
            statement.TotalCredit += payment.Amount;
        }
        
        // Sort all transactions by date
        statement.Transactions = statement.Transactions.OrderBy(t => t.Date).ToList();
        
        statement.ClosingBalance = runningBalance;
        
        return statement;
    }

    public async Task<string> GenerateCustomerCodeAsync()
    {
        var lastCustomer = await _context.Customers
            .OrderByDescending(c => c.CustomerId)
            .FirstOrDefaultAsync();
        
        int nextNumber = 1;
        if (lastCustomer != null && !string.IsNullOrEmpty(lastCustomer.CustomerCode))
        {
            // Try to extract number from code
            var parts = lastCustomer.CustomerCode.Split('-');
            if (parts.Length > 1 && int.TryParse(parts[1], out int num))
            {
                nextNumber = num + 1;
            }
            else
            {
                nextNumber = lastCustomer.CustomerId + 1;
            }
        }
        
        return $"CUS-{nextNumber:D5}";
    }
}

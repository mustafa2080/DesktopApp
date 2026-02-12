using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GraceWay.AccountingSystem.Application.Services;

public class SupplierService : ISupplierService
{
    private readonly AppDbContext _context;
    
    public SupplierService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<Supplier> CreateSupplierAsync(Supplier supplier)
    {
        // Generate code if not provided
        if (string.IsNullOrEmpty(supplier.SupplierCode))
        {
            supplier.SupplierCode = await GenerateSupplierCodeAsync();
        }
        
        supplier.CreatedAt = DateTime.UtcNow;
        supplier.CurrentBalance = supplier.OpeningBalance;
        
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();
        
        return supplier;
    }
    
    public async Task<Supplier?> GetSupplierByIdAsync(int supplierId)
    {
        return await _context.Suppliers
            .FirstOrDefaultAsync(s => s.SupplierId == supplierId);
    }
    
    public async Task<List<Supplier>> GetAllSuppliersAsync()
    {
        // Use LINQ instead of SqlQuery to avoid navigation property issues
        var suppliers = await _context.Suppliers
            .OrderBy(s => s.SupplierName)
            .ToListAsync();
        
        return suppliers;
    }
    
    public async Task<List<Supplier>> GetActiveSuppliersAsync()
    {
        var suppliers = await _context.Suppliers
            .Where(s => s.IsActive)
            .OrderBy(s => s.SupplierName)
            .ToListAsync();
        
        return suppliers;
    }
    
    public async Task UpdateSupplierAsync(Supplier supplier)
    {
        var existing = await _context.Suppliers
            .FindAsync(supplier.SupplierId);
        
        if (existing == null)
        {
            throw new Exception("Supplier not found");
        }
        
        existing.SupplierCode = supplier.SupplierCode;
        existing.SupplierName = supplier.SupplierName;
        existing.SupplierNameEn = supplier.SupplierNameEn;
        existing.Phone = supplier.Phone;
        existing.Mobile = supplier.Mobile;
        existing.Email = supplier.Email;
        existing.Address = supplier.Address;
        existing.City = supplier.City;
        existing.Country = supplier.Country;
        existing.TaxNumber = supplier.TaxNumber;
        existing.CreditLimit = supplier.CreditLimit;
        existing.PaymentTermDays = supplier.PaymentTermDays;
        existing.IsActive = supplier.IsActive;
        existing.Notes = supplier.Notes;
        
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteSupplierAsync(int supplierId)
    {
        var supplier = await _context.Suppliers.FindAsync(supplierId);
        
        if (supplier == null)
        {
            throw new Exception("Supplier not found");
        }
        
        // Soft delete
        supplier.IsActive = false;
        await _context.SaveChangesAsync();
    }
    
    public async Task<decimal> GetSupplierBalanceAsync(int supplierId)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.SupplierId == supplierId);
        
        return supplier?.CurrentBalance ?? 0;
    }
    
    public async Task UpdateSupplierBalanceAsync(int supplierId, decimal amount)
    {
        var supplier = await _context.Suppliers
            .FindAsync(supplierId);
        
        if (supplier == null)
        {
            throw new Exception("Supplier not found");
        }
        
        supplier.CurrentBalance += amount;
        await _context.SaveChangesAsync();
    }
    
    public async Task<List<Supplier>> GetSuppliersWithBalanceAsync()
    {
        // Use LINQ instead of FromSqlRaw
        var suppliers = await _context.Suppliers
            .Where(s => s.CurrentBalance != 0)
            .OrderByDescending(s => Math.Abs(s.CurrentBalance))
            .ToListAsync();
        
        return suppliers;
    }
    
    public async Task<List<Supplier>> GetSuppliersExceedingCreditLimitAsync()
    {
        // Use LINQ instead of FromSqlRaw
        var suppliers = await _context.Suppliers
            .Where(s => s.CurrentBalance > s.CreditLimit && s.CreditLimit > 0)
            .OrderByDescending(s => s.CurrentBalance - s.CreditLimit)
            .ToListAsync();
        
        return suppliers;
    }
    
    public async Task<string> GenerateSupplierCodeAsync()
    {
        var lastSupplier = await _context.Suppliers
            .OrderByDescending(s => s.SupplierId)
            .FirstOrDefaultAsync();
        
        int nextNumber = (lastSupplier?.SupplierId ?? 0) + 1;
        return $"SUP-{nextNumber:D5}";
    }
    
    /// <summary>
    /// كشف حساب المورد مع جميع الحركات (فواتير شراء + مدفوعات)
    /// </summary>
    public async Task<SupplierStatement> GetSupplierStatementAsync(
        int supplierId, 
        DateTime? startDate = null, 
        DateTime? endDate = null)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.SupplierId == supplierId);
            
        if (supplier == null)
        {
            throw new Exception("المورد غير موجود");
        }
        
        // تحديد تاريخ البداية والنهاية - تحويل لـ UTC
        var start = (startDate ?? new DateTime(DateTime.Now.Year, 1, 1)).ToUniversalTime();
        var end = (endDate ?? DateTime.Now.Date.AddDays(1).AddSeconds(-1)).ToUniversalTime();
        
        var statement = new SupplierStatement
        {
            SupplierId = supplier.SupplierId,
            SupplierName = supplier.SupplierName,
            SupplierCode = supplier.SupplierCode,
            OpeningBalance = supplier.OpeningBalance,
            Transactions = new List<SupplierStatementLine>()
        };
        
        // 1. إضافة الرصيد الافتتاحي كأول سطر
        decimal runningBalance = supplier.OpeningBalance;
        statement.Transactions.Add(new SupplierStatementLine
        {
            Date = DateTime.SpecifyKind(start, DateTimeKind.Utc),
            Type = "رصيد افتتاحي",
            Description = "الرصيد الافتتاحي للمورد",
            Balance = runningBalance,
            ReferenceNumber = "-"
        });
        
        // 2. جلب فواتير الشراء
        var purchaseInvoices = await _context.Set<PurchaseInvoice>()
            .Where(pi => pi.SupplierId == supplierId 
                && pi.InvoiceDate >= start 
                && pi.InvoiceDate <= end)
            .OrderBy(pi => pi.InvoiceDate)
            .ToListAsync();
        
        // 3. جلب المدفوعات
        var payments = await _context.Set<InvoicePayment>()
            .Where(p => p.PurchaseInvoiceId != null 
                && p.PaymentDate >= start 
                && p.PaymentDate <= end)
            .ToListAsync();
        
        // تصفية المدفوعات للفواتير الخاصة بهذا المورد فقط
        var supplierInvoiceIds = purchaseInvoices.Select(pi => pi.PurchaseInvoiceId).ToList();
        var supplierPayments = payments
            .Where(p => supplierInvoiceIds.Contains(p.PurchaseInvoiceId ?? 0))
            .ToList();
        
        // 4. دمج الحركات وترتيبها حسب التاريخ
        var allTransactions = new List<SupplierStatementLine>();
        
        // إضافة فواتير الشراء (دائن - له علينا)
        foreach (var invoice in purchaseInvoices)
        {
            allTransactions.Add(new SupplierStatementLine
            {
                Date = invoice.InvoiceDate,
                Type = "فاتورة شراء",
                ReferenceNumber = invoice.InvoiceNumber,
                Description = $"فاتورة شراء رقم {invoice.InvoiceNumber}",
                Credit = invoice.TotalAmount,
                Debit = 0,
                PurchaseInvoiceId = invoice.PurchaseInvoiceId,
                Notes = invoice.Notes ?? ""
            });
        }
        
        // إضافة المدفوعات (مدين - سددنا له)
        foreach (var payment in supplierPayments)
        {
            allTransactions.Add(new SupplierStatementLine
            {
                Date = payment.PaymentDate,
                Type = "سداد",
                ReferenceNumber = payment.ReferenceNumber ?? $"PAY-{payment.PaymentId}",
                Description = $"سداد بمبلغ {payment.Amount:N2}",
                Debit = payment.Amount,
                Credit = 0,
                PaymentId = payment.PaymentId,
                Notes = payment.Notes ?? ""
            });
        }
        
        // ترتيب جميع الحركات حسب التاريخ
        var sortedTransactions = allTransactions
            .OrderBy(t => t.Date)
            .ThenBy(t => t.Type == "فاتورة شراء" ? 0 : 1) // الفواتير أولاً في نفس اليوم
            .ToList();
        
        // 5. حساب الرصيد الجاري لكل سطر
        foreach (var transaction in sortedTransactions)
        {
            runningBalance += transaction.Credit - transaction.Debit;
            transaction.Balance = runningBalance;
            statement.Transactions.Add(transaction);
        }
        
        statement.ClosingBalance = runningBalance;
        
        return statement;
    }
}

using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GraceWay.AccountingSystem.Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly AppDbContext _context;
    private readonly ICashBoxService _cashBoxService;
    private readonly IJournalService _journalService;

    public InvoiceService(AppDbContext context, ICashBoxService cashBoxService, IJournalService journalService)
    {
        _context = context;
        _cashBoxService = cashBoxService;
        _journalService = journalService;
    }

    #region Sales Invoices

    public async Task<List<SalesInvoice>> GetAllSalesInvoicesAsync()
    {
        return await _context.SalesInvoices
            .Include(i => i.Customer)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<SalesInvoice?> GetSalesInvoiceByIdAsync(int id)
    {
        return await _context.SalesInvoices
            .Include(i => i.Customer)
            .FirstOrDefaultAsync(i => i.SalesInvoiceId == id);
    }

    public async Task<SalesInvoice> CreateSalesInvoiceAsync(SalesInvoice invoice)
    {
        // Generate invoice number
        invoice.InvoiceNumber = await GenerateSalesInvoiceNumberAsync();
        invoice.CreatedAt = DateTime.UtcNow;
        
        _context.SalesInvoices.Add(invoice);
        await _context.SaveChangesAsync();
        
        // إنشاء قيد يومي تلقائي للفاتورة
        try
        {
            await _journalService.CreateSalesInvoiceJournalEntryAsync(invoice);
        }
        catch (Exception ex)
        {
            // في حالة فشل إنشاء القيد، نسجل الخطأ ولكن لا نلغي الفاتورة
            System.Diagnostics.Debug.WriteLine($"فشل إنشاء القيد اليومي للفاتورة: {ex.Message}");
        }
        
        return invoice;
    }

    public async Task<SalesInvoice> UpdateSalesInvoiceAsync(SalesInvoice invoice)
    {
        _context.SalesInvoices.Update(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }

    public async Task<bool> DeleteSalesInvoiceAsync(int id)
    {
        try
        {
            var invoice = await _context.SalesInvoices
                .FirstOrDefaultAsync(i => i.SalesInvoiceId == id);
                
            if (invoice == null)
                return false;
            
            // حذف الدفعات المرتبطة
            var payments = await _context.InvoicePayments
                .Where(p => p.SalesInvoiceId == id)
                .ToListAsync();
            if (payments.Any())
            {
                _context.InvoicePayments.RemoveRange(payments);
            }
            
            // حذف الأصناف
            var items = await _context.SalesInvoiceItems
                .Where(i => i.SalesInvoiceId == id)
                .ToListAsync();
            if (items.Any())
            {
                _context.SalesInvoiceItems.RemoveRange(items);
            }
            
            // حذف الفاتورة
            _context.SalesInvoices.Remove(invoice);
            await _context.SaveChangesAsync();
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<string> GenerateSalesInvoiceNumberAsync()
    {
        var lastInvoice = await _context.SalesInvoices
            .OrderByDescending(i => i.SalesInvoiceId)
            .FirstOrDefaultAsync();
            
        int nextNumber = 1;
        if (lastInvoice != null)
        {
            var lastNumber = lastInvoice.InvoiceNumber.Replace("SI-", "");
            if (int.TryParse(lastNumber, out int num))
            {
                nextNumber = num + 1;
            }
        }
        
        return $"SI-{nextNumber:D6}";
    }

    #endregion

    #region Purchase Invoices

    public async Task<List<PurchaseInvoice>> GetAllPurchaseInvoicesAsync()
    {
        var invoices = await _context.PurchaseInvoices
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
        
        // Load suppliers explicitly to avoid column name issue
        foreach (var invoice in invoices)
        {
            if (invoice.SupplierId > 0)
            {
                invoice.Supplier = await _context.Suppliers
                    .FirstOrDefaultAsync(s => s.SupplierId == invoice.SupplierId);
            }
        }
        
        return invoices;
    }

    public async Task<PurchaseInvoice?> GetPurchaseInvoiceByIdAsync(int id)
    {
        var invoice = await _context.PurchaseInvoices
            .FirstOrDefaultAsync(i => i.PurchaseInvoiceId == id);
        
        // Load supplier explicitly
        if (invoice != null && invoice.SupplierId > 0)
        {
            invoice.Supplier = await _context.Suppliers
                .FirstOrDefaultAsync(s => s.SupplierId == invoice.SupplierId);
        }
        
        return invoice;
    }

    public async Task<PurchaseInvoice> CreatePurchaseInvoiceAsync(PurchaseInvoice invoice)
    {
        // Generate invoice number
        invoice.InvoiceNumber = await GeneratePurchaseInvoiceNumberAsync();
        invoice.CreatedAt = DateTime.UtcNow;
        
        _context.PurchaseInvoices.Add(invoice);
        await _context.SaveChangesAsync();
        
        // إنشاء قيد يومي تلقائي للفاتورة
        try
        {
            await _journalService.CreatePurchaseInvoiceJournalEntryAsync(invoice);
        }
        catch (Exception ex)
        {
            // في حالة فشل إنشاء القيد، نسجل الخطأ ولكن لا نلغي الفاتورة
            System.Diagnostics.Debug.WriteLine($"فشل إنشاء القيد اليومي للفاتورة: {ex.Message}");
        }
        
        return invoice;
    }

    public async Task<PurchaseInvoice> UpdatePurchaseInvoiceAsync(PurchaseInvoice invoice)
    {
        _context.PurchaseInvoices.Update(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }

    public async Task<bool> DeletePurchaseInvoiceAsync(int id)
    {
        try
        {
            var invoice = await _context.PurchaseInvoices
                .FirstOrDefaultAsync(i => i.PurchaseInvoiceId == id);
                
            if (invoice == null)
                return false;
            
            // حذف الدفعات المرتبطة
            var payments = await _context.InvoicePayments
                .Where(p => p.PurchaseInvoiceId == id)
                .ToListAsync();
            if (payments.Any())
            {
                _context.InvoicePayments.RemoveRange(payments);
            }
            
            // حذف الأصناف
            var items = await _context.PurchaseInvoiceItems
                .Where(i => i.PurchaseInvoiceId == id)
                .ToListAsync();
            if (items.Any())
            {
                _context.PurchaseInvoiceItems.RemoveRange(items);
            }
            
            // حذف الفاتورة
            _context.PurchaseInvoices.Remove(invoice);
            await _context.SaveChangesAsync();
            
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<string> GeneratePurchaseInvoiceNumberAsync()
    {
        var lastInvoice = await _context.PurchaseInvoices
            .OrderByDescending(i => i.PurchaseInvoiceId)
            .FirstOrDefaultAsync();
            
        int nextNumber = 1;
        if (lastInvoice != null)
        {
            var lastNumber = lastInvoice.InvoiceNumber.Replace("PI-", "");
            if (int.TryParse(lastNumber, out int num))
            {
                nextNumber = num + 1;
            }
        }
        
        return $"PI-{nextNumber:D6}";
    }

    #endregion

    #region Invoice Payments

    public async Task<InvoicePayment> AddPaymentAsync(InvoicePayment payment)
    {
        payment.CreatedAt = DateTime.UtcNow;
        payment.PaymentDate = DateTime.UtcNow;
        
        // إنشاء حركة في الخزنة
        var transaction = new CashTransaction
        {
            CashBoxId = payment.CashBoxId,
            Type = payment.SalesInvoiceId.HasValue ? TransactionType.Income : TransactionType.Expense,
            Amount = payment.Amount,
            TransactionDate = payment.PaymentDate,
            Category = payment.SalesInvoiceId.HasValue ? "مبيعات" : "مشتريات",
            Description = $"دفعة فاتورة {(payment.SalesInvoiceId.HasValue ? $"مبيعات SI-{payment.SalesInvoiceId}" : $"مشتريات PI-{payment.PurchaseInvoiceId}")}",
            PaymentMethod = Enum.TryParse<PaymentMethod>(payment.PaymentMethod, true, out var method) ? method : PaymentMethod.Cash,
            ReferenceNumber = payment.ReferenceNumber,
            Notes = payment.Notes,
            CreatedBy = payment.CreatedBy ?? 0
        };
        
        var cashTransaction = await _cashBoxService.AddTransactionAsync(transaction);
        payment.CashTransactionId = cashTransaction.Id;
        
        // إضافة الدفعة
        _context.InvoicePayments.Add(payment);
        
        // تحديث حالة الفاتورة
        if (payment.SalesInvoiceId.HasValue)
        {
            var invoice = await _context.SalesInvoices.FindAsync(payment.SalesInvoiceId.Value);
            if (invoice != null)
            {
                invoice.PaidAmount += payment.Amount;
                if (invoice.PaidAmount >= invoice.TotalAmount)
                {
                    invoice.Status = "Paid";
                }
                else if (invoice.PaidAmount > 0)
                {
                    invoice.Status = "Partial";
                }
            }
        }
        else if (payment.PurchaseInvoiceId.HasValue)
        {
            var invoice = await _context.PurchaseInvoices.FindAsync(payment.PurchaseInvoiceId.Value);
            if (invoice != null)
            {
                invoice.PaidAmount += payment.Amount;
                if (invoice.PaidAmount >= invoice.TotalAmount)
                {
                    invoice.Status = "Paid";
                }
                else if (invoice.PaidAmount > 0)
                {
                    invoice.Status = "Partial";
                }
            }
        }
        
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<List<InvoicePayment>> GetInvoicePaymentsAsync(int? salesInvoiceId, int? purchaseInvoiceId)
    {
        var query = _context.InvoicePayments.AsQueryable();
            
        if (salesInvoiceId.HasValue)
        {
            query = query.Where(p => p.SalesInvoiceId == salesInvoiceId.Value);
        }
        
        if (purchaseInvoiceId.HasValue)
        {
            query = query.Where(p => p.PurchaseInvoiceId == purchaseInvoiceId.Value);
        }
        
        return await query.OrderByDescending(p => p.PaymentDate).ToListAsync();
    }

    #endregion

    #region Reports

    public async Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.SalesInvoices.AsQueryable();
        
        if (startDate.HasValue)
        {
            query = query.Where(i => i.InvoiceDate >= startDate.Value);
        }
        
        if (endDate.HasValue)
        {
            query = query.Where(i => i.InvoiceDate <= endDate.Value);
        }
        
        return await query.SumAsync(i => i.TotalAmount);
    }

    public async Task<decimal> GetTotalPurchasesAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.PurchaseInvoices.AsQueryable();
        
        if (startDate.HasValue)
        {
            query = query.Where(i => i.InvoiceDate >= startDate.Value);
        }
        
        if (endDate.HasValue)
        {
            query = query.Where(i => i.InvoiceDate <= endDate.Value);
        }
        
        return await query.SumAsync(i => i.TotalAmount);
    }

    public async Task<List<SalesInvoice>> GetUnpaidSalesInvoicesAsync()
    {
        return await _context.SalesInvoices
            .Include(i => i.Customer)
            .Where(i => i.Status != "Paid")
            .OrderBy(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<List<PurchaseInvoice>> GetUnpaidPurchaseInvoicesAsync()
    {
        var invoices = await _context.PurchaseInvoices
            .Where(i => i.Status != "Paid")
            .OrderBy(i => i.InvoiceDate)
            .ToListAsync();
        
        // Load suppliers explicitly
        foreach (var invoice in invoices)
        {
            if (invoice.SupplierId > 0)
            {
                invoice.Supplier = await _context.Suppliers
                    .FirstOrDefaultAsync(s => s.SupplierId == invoice.SupplierId);
            }
        }
        
        return invoices;
    }

    #endregion
}

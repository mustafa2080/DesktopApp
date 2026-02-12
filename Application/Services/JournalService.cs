using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GraceWay.AccountingSystem.Application.Services;

public class JournalService : IJournalService
{
    private readonly AppDbContext _context;

    public JournalService(AppDbContext context)
    {
        _context = context;
    }

    #region Manual Journal Entry

    public async Task<JournalEntry> CreateManualJournalEntryAsync(JournalEntry entry)
    {
        entry.EntryNumber = await GenerateEntryNumberAsync();
        entry.EntryType = "Manual";
        entry.CreatedAt = DateTime.UtcNow;
        
        _context.Set<JournalEntry>().Add(entry);
        await _context.SaveChangesAsync();
        
        return entry;
    }

    #endregion

    #region Sales Invoice Journal Entry

    public async Task<JournalEntry> CreateSalesInvoiceJournalEntryAsync(SalesInvoice invoice)
    {
        // البحث عن الحسابات المطلوبة
        var salesAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountCode == "4-001" && a.IsActive); // حساب المبيعات
        
        var customerAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountCode == "1-002" && a.IsActive); // حساب العملاء
        
        var taxAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountCode == "2-003" && a.IsActive); // حساب الضرائب
        
        if (salesAccount == null || customerAccount == null)
        {
            throw new InvalidOperationException("الحسابات المطلوبة غير موجودة. يجب إنشاء حسابات: المبيعات، العملاء");
        }

        // جلب بيانات العميل
        var customer = await _context.Customers.FindAsync(invoice.CustomerId);
        var customerName = customer?.CustomerName ?? "عميل";

        // إنشاء القيد
        var entry = new JournalEntry
        {
            EntryNumber = await GenerateEntryNumberAsync(),
            EntryDate = invoice.InvoiceDate.ToUniversalTime(),
            EntryType = "Auto",
            ReferenceType = "SalesInvoice",
            ReferenceId = invoice.SalesInvoiceId,
            Description = $"قيد تلقائي - فاتورة مبيعات رقم {invoice.InvoiceNumber}",
            TotalDebit = invoice.TotalAmount,
            TotalCredit = invoice.TotalAmount,
            IsPosted = true,
            PostedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<JournalEntry>().Add(entry);
        await _context.SaveChangesAsync();

        // إضافة بنود القيد
        var lines = new List<JournalEntryLine>();
        
        // البند الأول: من ح/ العملاء (مدين)
        lines.Add(new JournalEntryLine
        {
            JournalEntryId = entry.JournalEntryId,
            AccountId = customerAccount.AccountId,
            Description = $"فاتورة مبيعات {invoice.InvoiceNumber} - العميل: {customerName}",
            DebitAmount = invoice.TotalAmount,
            CreditAmount = 0,
            LineOrder = 1
        });
        
        // البند الثاني: إلى ح/ المبيعات (دائن)
        lines.Add(new JournalEntryLine
        {
            JournalEntryId = entry.JournalEntryId,
            AccountId = salesAccount.AccountId,
            Description = $"مبيعات بفاتورة رقم {invoice.InvoiceNumber}",
            DebitAmount = 0,
            CreditAmount = invoice.SubTotal,
            LineOrder = 2
        });
        
        // إذا كان فيه ضريبة
        if (invoice.TaxAmount > 0 && taxAccount != null)
        {
            lines.Add(new JournalEntryLine
            {
                JournalEntryId = entry.JournalEntryId,
                AccountId = taxAccount.AccountId,
                Description = $"ضريبة على فاتورة مبيعات {invoice.InvoiceNumber}",
                DebitAmount = 0,
                CreditAmount = invoice.TaxAmount,
                LineOrder = 3
            });
        }
        
        _context.Set<JournalEntryLine>().AddRange(lines);
        await _context.SaveChangesAsync();
        
        return entry;
    }

    #endregion

    #region Purchase Invoice Journal Entry

    public async Task<JournalEntry> CreatePurchaseInvoiceJournalEntryAsync(PurchaseInvoice invoice)
    {
        // البحث عن الحسابات المطلوبة
        var purchasesAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountCode == "5-001" && a.IsActive); // حساب المشتريات
        
        var supplierAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountCode == "2-001" && a.IsActive); // حساب الموردين
        
        var taxAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountCode == "2-003" && a.IsActive); // حساب الضرائب
        
        if (purchasesAccount == null || supplierAccount == null)
        {
            throw new InvalidOperationException("الحسابات المطلوبة غير موجودة. يجب إنشاء حسابات: المشتريات، الموردين");
        }

        // جلب بيانات المورد
        var supplier = await _context.Suppliers.FindAsync(invoice.SupplierId);
        var supplierName = supplier?.SupplierName ?? "مورد";

        // إنشاء القيد
        var entry = new JournalEntry
        {
            EntryNumber = await GenerateEntryNumberAsync(),
            EntryDate = invoice.InvoiceDate.ToUniversalTime(),
            EntryType = "Auto",
            ReferenceType = "PurchaseInvoice",
            ReferenceId = invoice.PurchaseInvoiceId,
            Description = $"قيد تلقائي - فاتورة مشتريات رقم {invoice.InvoiceNumber}",
            TotalDebit = invoice.TotalAmount,
            TotalCredit = invoice.TotalAmount,
            IsPosted = true,
            PostedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<JournalEntry>().Add(entry);
        await _context.SaveChangesAsync();

        // إضافة بنود القيد
        var lines = new List<JournalEntryLine>();
        
        // البند الأول: من ح/ المشتريات (مدين)
        lines.Add(new JournalEntryLine
        {
            JournalEntryId = entry.JournalEntryId,
            AccountId = purchasesAccount.AccountId,
            Description = $"مشتريات بفاتورة رقم {invoice.InvoiceNumber}",
            DebitAmount = invoice.SubTotal,
            CreditAmount = 0,
            LineOrder = 1
        });
        
        // إذا كان فيه ضريبة
        if (invoice.TaxAmount > 0 && taxAccount != null)
        {
            lines.Add(new JournalEntryLine
            {
                JournalEntryId = entry.JournalEntryId,
                AccountId = taxAccount.AccountId,
                Description = $"ضريبة على فاتورة مشتريات {invoice.InvoiceNumber}",
                DebitAmount = invoice.TaxAmount,
                CreditAmount = 0,
                LineOrder = 2
            });
        }
        
        // البند الأخير: إلى ح/ الموردين (دائن)
        lines.Add(new JournalEntryLine
        {
            JournalEntryId = entry.JournalEntryId,
            AccountId = supplierAccount.AccountId,
            Description = $"فاتورة مشتريات {invoice.InvoiceNumber} - المورد: {supplierName}",
            DebitAmount = 0,
            CreditAmount = invoice.TotalAmount,
            LineOrder = invoice.TaxAmount > 0 ? 3 : 2
        });
        
        _context.Set<JournalEntryLine>().AddRange(lines);
        await _context.SaveChangesAsync();
        
        return entry;
    }

    #endregion

    #region Cash Transaction Journal Entry

    public async Task<JournalEntry> CreateCashTransactionJournalEntryAsync(CashTransaction transaction)
    {
        // البحث عن الحسابات المطلوبة
        var cashAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountCode == "1-001" && a.IsActive); // حساب النقدية/الخزينة
        
        if (cashAccount == null)
        {
            throw new InvalidOperationException("حساب النقدية غير موجود. يجب إنشاء حساب النقدية برمز 1-001");
        }

        // تحديد الحساب المقابل بناءً على نوع الحركة والفئة
        Account? oppositeAccount = null;
        
        if (transaction.Type == TransactionType.Income)
        {
            // إيراد - البحث عن حساب الإيراد المناسب
            oppositeAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountType == "Revenue" && a.IsActive);
        }
        else // Expense
        {
            // مصروف - البحث عن حساب المصروف المناسب
            oppositeAccount = await _context.Accounts
                .FirstOrDefaultAsync(a => a.AccountType == "Expense" && a.IsActive);
        }
        
        if (oppositeAccount == null)
        {
            // استخدام حساب عام للإيرادات أو المصروفات
            oppositeAccount = transaction.Type == TransactionType.Income
                ? await _context.Accounts.FirstOrDefaultAsync(a => a.AccountCode == "4-999" && a.IsActive)
                : await _context.Accounts.FirstOrDefaultAsync(a => a.AccountCode == "5-999" && a.IsActive);
                
            if (oppositeAccount == null)
            {
                throw new InvalidOperationException("حساب الإيراد أو المصروف العام غير موجود");
            }
        }

        // إنشاء القيد
        var entry = new JournalEntry
        {
            EntryNumber = await GenerateEntryNumberAsync(),
            EntryDate = transaction.TransactionDate.ToUniversalTime(),
            EntryType = "Auto",
            ReferenceType = "CashTransaction",
            ReferenceId = transaction.Id,
            Description = $"قيد تلقائي - حركة خزينة: {transaction.Description}",
            TotalDebit = transaction.Amount,
            TotalCredit = transaction.Amount,
            IsPosted = true,
            PostedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<JournalEntry>().Add(entry);
        await _context.SaveChangesAsync();

        // إضافة بنود القيد
        var lines = new List<JournalEntryLine>();
        
        if (transaction.Type == TransactionType.Income)
        {
            // إيراد: من ح/ النقدية (مدين) - إلى ح/ الإيراد (دائن)
            lines.Add(new JournalEntryLine
            {
                JournalEntryId = entry.JournalEntryId,
                AccountId = cashAccount.AccountId,
                Description = $"إيراد نقدي - {transaction.Category}",
                DebitAmount = transaction.Amount,
                CreditAmount = 0,
                LineOrder = 1
            });
            
            lines.Add(new JournalEntryLine
            {
                JournalEntryId = entry.JournalEntryId,
                AccountId = oppositeAccount.AccountId,
                Description = transaction.Description ?? $"إيراد - {transaction.Category}",
                DebitAmount = 0,
                CreditAmount = transaction.Amount,
                LineOrder = 2
            });
        }
        else // Expense
        {
            // مصروف: من ح/ المصروف (مدين) - إلى ح/ النقدية (دائن)
            lines.Add(new JournalEntryLine
            {
                JournalEntryId = entry.JournalEntryId,
                AccountId = oppositeAccount.AccountId,
                Description = transaction.Description ?? $"مصروف - {transaction.Category}",
                DebitAmount = transaction.Amount,
                CreditAmount = 0,
                LineOrder = 1
            });
            
            lines.Add(new JournalEntryLine
            {
                JournalEntryId = entry.JournalEntryId,
                AccountId = cashAccount.AccountId,
                Description = $"صرف نقدي - {transaction.Category}",
                DebitAmount = 0,
                CreditAmount = transaction.Amount,
                LineOrder = 2
            });
        }
        
        _context.Set<JournalEntryLine>().AddRange(lines);
        await _context.SaveChangesAsync();
        
        return entry;
    }

    #endregion

    #region Reservation Journal Entry

    public async Task<JournalEntry> CreateReservationJournalEntryAsync(Reservation reservation)
    {
        // البحث عن الحسابات المطلوبة
        var cashAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountCode == "1-001" && a.IsActive); // حساب النقدية
        
        var reservationRevenueAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountCode == "4-002" && a.IsActive); // حساب إيرادات الحجوزات
        
        if (cashAccount == null || reservationRevenueAccount == null)
        {
            throw new InvalidOperationException("الحسابات المطلوبة غير موجودة");
        }

        // إنشاء القيد
        var entry = new JournalEntry
        {
            EntryNumber = await GenerateEntryNumberAsync(),
            EntryDate = reservation.ReservationDate.ToUniversalTime(),
            EntryType = "Auto",
            ReferenceType = "Reservation",
            ReferenceId = reservation.ReservationId,
            Description = $"قيد تلقائي - حجز رقم {reservation.ReservationNumber}",
            TotalDebit = reservation.SellingPrice,
            TotalCredit = reservation.SellingPrice,
            IsPosted = true,
            PostedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<JournalEntry>().Add(entry);
        await _context.SaveChangesAsync();

        // إضافة بنود القيد: من ح/ النقدية - إلى ح/ إيرادات الحجوزات
        var lines = new List<JournalEntryLine>
        {
            new JournalEntryLine
            {
                JournalEntryId = entry.JournalEntryId,
                AccountId = cashAccount.AccountId,
                Description = $"حجز خدمة - {reservation.ServiceType?.ServiceTypeName}",
                DebitAmount = reservation.SellingPrice,
                CreditAmount = 0,
                LineOrder = 1
            },
            new JournalEntryLine
            {
                JournalEntryId = entry.JournalEntryId,
                AccountId = reservationRevenueAccount.AccountId,
                Description = $"إيراد حجز رقم {reservation.ReservationNumber}",
                DebitAmount = 0,
                CreditAmount = reservation.SellingPrice,
                LineOrder = 2
            }
        };
        
        _context.Set<JournalEntryLine>().AddRange(lines);
        await _context.SaveChangesAsync();
        
        return entry;
    }

    #endregion

    #region Trip Booking Journal Entry

    public async Task<JournalEntry> CreateTripBookingJournalEntryAsync(TripBooking booking)
    {
        // البحث عن الحسابات المطلوبة
        var cashAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountCode == "1-001" && a.IsActive); // حساب النقدية
        
        var tripRevenueAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountCode == "4-003" && a.IsActive); // حساب إيرادات الرحلات
        
        if (cashAccount == null || tripRevenueAccount == null)
        {
            throw new InvalidOperationException("الحسابات المطلوبة غير موجودة");
        }

        // إنشاء القيد
        var entry = new JournalEntry
        {
            EntryNumber = await GenerateEntryNumberAsync(),
            EntryDate = booking.BookingDate.ToUniversalTime(),
            EntryType = "Auto",
            ReferenceType = "TripBooking",
            ReferenceId = booking.TripBookingId,
            Description = $"قيد تلقائي - حجز رحلة رقم {booking.BookingNumber}",
            TotalDebit = booking.TotalAmount,
            TotalCredit = booking.TotalAmount,
            IsPosted = true,
            PostedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<JournalEntry>().Add(entry);
        await _context.SaveChangesAsync();

        // إضافة بنود القيد: من ح/ النقدية - إلى ح/ إيرادات الرحلات
        var lines = new List<JournalEntryLine>
        {
            new JournalEntryLine
            {
                JournalEntryId = entry.JournalEntryId,
                AccountId = cashAccount.AccountId,
                Description = $"حجز رحلة - {booking.Trip?.TripName ?? "رحلة"}",
                DebitAmount = booking.TotalAmount,
                CreditAmount = 0,
                LineOrder = 1
            },
            new JournalEntryLine
            {
                JournalEntryId = entry.JournalEntryId,
                AccountId = tripRevenueAccount.AccountId,
                Description = $"إيراد حجز رحلة رقم {booking.BookingNumber}",
                DebitAmount = 0,
                CreditAmount = booking.TotalAmount,
                LineOrder = 2
            }
        };
        
        _context.Set<JournalEntryLine>().AddRange(lines);
        await _context.SaveChangesAsync();
        
        return entry;
    }

    #endregion

    #region Helper Methods

    public async Task<bool> DeleteJournalEntryAsync(int journalEntryId)
    {
        var entry = await _context.Set<JournalEntry>()
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.JournalEntryId == journalEntryId);
        
        if (entry == null)
            return false;
        
        _context.Set<JournalEntry>().Remove(entry);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<JournalEntry?> GetJournalEntryByIdAsync(int journalEntryId)
    {
        return await _context.Set<JournalEntry>()
            .Include(j => j.Lines)
            .ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(j => j.JournalEntryId == journalEntryId);
    }

    public async Task<List<JournalEntry>> GetAllJournalEntriesAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Set<JournalEntry>()
            .Include(j => j.Lines)
            .ThenInclude(l => l.Account)
            .AsQueryable();
        
        if (startDate.HasValue)
            query = query.Where(j => j.EntryDate >= startDate.Value);
        
        if (endDate.HasValue)
            query = query.Where(j => j.EntryDate <= endDate.Value);
        
        return await query
            .OrderByDescending(j => j.EntryDate)
            .ThenByDescending(j => j.JournalEntryId)
            .ToListAsync();
    }

    private async Task<string> GenerateEntryNumberAsync()
    {
        var lastEntry = await _context.Set<JournalEntry>()
            .OrderByDescending(j => j.JournalEntryId)
            .FirstOrDefaultAsync();
        
        int nextNumber = (lastEntry?.JournalEntryId ?? 0) + 1;
        return $"JE-{nextNumber:D6}";
    }

    #endregion
}

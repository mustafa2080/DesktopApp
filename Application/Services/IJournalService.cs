using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Application.Services;

public interface IJournalService
{
    /// <summary>
    /// إنشاء قيد يومي يدوي
    /// </summary>
    Task<JournalEntry> CreateManualJournalEntryAsync(JournalEntry entry);
    
    /// <summary>
    /// إنشاء قيد تلقائي لفاتورة مبيعات
    /// </summary>
    Task<JournalEntry> CreateSalesInvoiceJournalEntryAsync(SalesInvoice invoice);
    
    /// <summary>
    /// إنشاء قيد تلقائي لفاتورة مشتريات
    /// </summary>
    Task<JournalEntry> CreatePurchaseInvoiceJournalEntryAsync(PurchaseInvoice invoice);
    
    /// <summary>
    /// إنشاء قيد تلقائي لحركة خزينة
    /// </summary>
    Task<JournalEntry> CreateCashTransactionJournalEntryAsync(CashTransaction transaction);
    
    /// <summary>
    /// إنشاء قيد تلقائي لحجز
    /// </summary>
    Task<JournalEntry> CreateReservationJournalEntryAsync(Reservation reservation);
    
    /// <summary>
    /// إنشاء قيد تلقائي لحجز رحلة
    /// </summary>
    Task<JournalEntry> CreateTripBookingJournalEntryAsync(TripBooking booking);
    
    /// <summary>
    /// حذف قيد يومي
    /// </summary>
    Task<bool> DeleteJournalEntryAsync(int journalEntryId);
    
    /// <summary>
    /// الحصول على قيد معين
    /// </summary>
    Task<JournalEntry?> GetJournalEntryByIdAsync(int journalEntryId);
    
    /// <summary>
    /// الحصول على جميع القيود
    /// </summary>
    Task<List<JournalEntry>> GetAllJournalEntriesAsync(DateTime? startDate = null, DateTime? endDate = null);
}

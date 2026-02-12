using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Application.Services;

public interface IInvoiceService
{
    // Sales Invoices
    Task<List<SalesInvoice>> GetAllSalesInvoicesAsync();
    Task<SalesInvoice?> GetSalesInvoiceByIdAsync(int id);
    Task<SalesInvoice> CreateSalesInvoiceAsync(SalesInvoice invoice);
    Task<SalesInvoice> UpdateSalesInvoiceAsync(SalesInvoice invoice);
    Task<bool> DeleteSalesInvoiceAsync(int id);
    
    // Purchase Invoices
    Task<List<PurchaseInvoice>> GetAllPurchaseInvoicesAsync();
    Task<PurchaseInvoice?> GetPurchaseInvoiceByIdAsync(int id);
    Task<PurchaseInvoice> CreatePurchaseInvoiceAsync(PurchaseInvoice invoice);
    Task<PurchaseInvoice> UpdatePurchaseInvoiceAsync(PurchaseInvoice invoice);
    Task<bool> DeletePurchaseInvoiceAsync(int id);
    
    // Invoice Payments
    Task<InvoicePayment> AddPaymentAsync(InvoicePayment payment);
    Task<List<InvoicePayment>> GetInvoicePaymentsAsync(int? salesInvoiceId, int? purchaseInvoiceId);
    
    // Reports
    Task<decimal> GetTotalSalesAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<decimal> GetTotalPurchasesAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<List<SalesInvoice>> GetUnpaidSalesInvoicesAsync();
    Task<List<PurchaseInvoice>> GetUnpaidPurchaseInvoicesAsync();
}

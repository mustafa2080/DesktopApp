using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Application.Services;

public interface ISettingService
{
    // Company Settings
    Task<CompanySetting?> GetCompanySettingsAsync();
    Task<bool> SaveCompanySettingsAsync(CompanySetting settings);
    Task<bool> UpdateCompanyLogoAsync(string logoPath);
    
    // Invoice Settings
    Task<InvoiceSetting?> GetInvoiceSettingsAsync();
    Task<bool> SaveInvoiceSettingsAsync(InvoiceSetting settings);
    Task<string> GenerateNextInvoiceNumberAsync(string invoiceType); // "sales" or "purchase"
}

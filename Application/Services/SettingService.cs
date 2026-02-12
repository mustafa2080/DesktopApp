using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GraceWay.AccountingSystem.Application.Services;

public class SettingService : ISettingService
{
    private readonly AppDbContext _context;

    public SettingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CompanySetting?> GetCompanySettingsAsync()
    {
        try
        {
            // Always return the first (and should be only) settings record
            return await _context.CompanySettings
                .Include(s => s.LastModifiedByUser)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting company settings: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> SaveCompanySettingsAsync(CompanySetting settings)
    {
        try
        {
            var existing = await _context.CompanySettings.FirstOrDefaultAsync();

            if (existing == null)
            {
                // Create new settings
                settings.CreatedDate = DateTime.UtcNow;
                _context.CompanySettings.Add(settings);
            }
            else
            {
                // Update existing settings
                existing.CompanyName = settings.CompanyName;
                existing.CompanyNameEnglish = settings.CompanyNameEnglish;
                existing.Address = settings.Address;
                existing.City = settings.City;
                existing.Country = settings.Country;
                existing.Phone = settings.Phone;
                existing.Mobile = settings.Mobile;
                existing.Email = settings.Email;
                existing.Website = settings.Website;
                existing.TaxRegistrationNumber = settings.TaxRegistrationNumber;
                existing.CommercialRegistrationNumber = settings.CommercialRegistrationNumber;
                existing.BankName = settings.BankName;
                existing.BankAccountNumber = settings.BankAccountNumber;
                existing.BankIBAN = settings.BankIBAN;
                existing.LastModifiedDate = DateTime.UtcNow;
                existing.LastModifiedByUserId = settings.LastModifiedByUserId;
                existing.LogoPath = settings.LogoPath;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving company settings: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateCompanyLogoAsync(string logoPath)
    {
        try
        {
            var settings = await _context.CompanySettings.FirstOrDefaultAsync();
            
            if (settings == null)
            {
                return false;
            }

            settings.LogoPath = logoPath;
            settings.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating company logo: {ex.Message}");
            return false;
        }
    }
    
    // ========== Invoice Settings ==========
    
    public async Task<InvoiceSetting?> GetInvoiceSettingsAsync()
    {
        try
        {
            return await _context.InvoiceSettings
                .Include(s => s.LastModifiedByUser)
                .FirstOrDefaultAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting invoice settings: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> SaveInvoiceSettingsAsync(InvoiceSetting settings)
    {
        try
        {
            var existing = await _context.InvoiceSettings.FirstOrDefaultAsync();

            if (existing == null)
            {
                settings.CreatedDate = DateTime.UtcNow;
                _context.InvoiceSettings.Add(settings);
            }
            else
            {
                existing.EnableTax = settings.EnableTax;
                existing.DefaultTaxRate = settings.DefaultTaxRate;
                existing.TaxLabel = settings.TaxLabel;
                existing.AutoNumbering = settings.AutoNumbering;
                existing.SalesInvoicePrefix = settings.SalesInvoicePrefix;
                existing.PurchaseInvoicePrefix = settings.PurchaseInvoicePrefix;
                existing.NextSalesInvoiceNumber = settings.NextSalesInvoiceNumber;
                existing.NextPurchaseInvoiceNumber = settings.NextPurchaseInvoiceNumber;
                existing.InvoiceNumberLength = settings.InvoiceNumberLength;
                existing.InvoiceFooterText = settings.InvoiceFooterText;
                existing.PaymentTerms = settings.PaymentTerms;
                existing.BankDetails = settings.BankDetails;
                existing.NotesTemplate = settings.NotesTemplate;
                existing.ShowCompanyLogo = settings.ShowCompanyLogo;
                existing.ShowTaxNumber = settings.ShowTaxNumber;
                existing.ShowBankDetails = settings.ShowBankDetails;
                existing.ShowPaymentTerms = settings.ShowPaymentTerms;
                existing.PaperSize = settings.PaperSize;
                existing.PrintInColor = settings.PrintInColor;
                existing.PrintDuplicateCopy = settings.PrintDuplicateCopy;
                existing.LastModifiedDate = DateTime.UtcNow;
                existing.LastModifiedByUserId = settings.LastModifiedByUserId;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving invoice settings: {ex.Message}");
            return false;
        }
    }

    public async Task<string> GenerateNextInvoiceNumberAsync(string invoiceType)
    {
        try
        {
            var settings = await _context.InvoiceSettings.FirstOrDefaultAsync();
            
            if (settings == null || !settings.AutoNumbering)
            {
                return string.Empty;
            }

            string prefix;
            int nextNumber;

            if (invoiceType.ToLower() == "sales")
            {
                prefix = settings.SalesInvoicePrefix ?? "SI";
                nextNumber = settings.NextSalesInvoiceNumber;
                settings.NextSalesInvoiceNumber++;
            }
            else // purchase
            {
                prefix = settings.PurchaseInvoicePrefix ?? "PI";
                nextNumber = settings.NextPurchaseInvoiceNumber;
                settings.NextPurchaseInvoiceNumber++;
            }

            await _context.SaveChangesAsync();

            string numberPart = nextNumber.ToString().PadLeft(settings.InvoiceNumberLength, '0');
            return $"{prefix}-{numberPart}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error generating invoice number: {ex.Message}");
            return string.Empty;
        }
    }
}

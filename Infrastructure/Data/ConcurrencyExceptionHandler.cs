using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace GraceWay.AccountingSystem.Infrastructure.Data;

/// <summary>
/// Handles concurrency conflicts when multiple users modify the same data
/// </summary>
public static class ConcurrencyExceptionHandler
{
    /// <summary>
    /// Maximum number of retry attempts for concurrency conflicts
    /// </summary>
    private const int MaxRetryAttempts = 3;

    /// <summary>
    /// Delay between retry attempts in milliseconds
    /// </summary>
    private const int RetryDelayMs = 100;

    /// <summary>
    /// Execute an operation with automatic retry on concurrency conflicts
    /// </summary>
    public static async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation)
    {
        int attempts = 0;
        
        while (true)
        {
            attempts++;
            
            try
            {
                return await operation();
            }
            catch (DbUpdateConcurrencyException ex) when (attempts < MaxRetryAttempts)
            {
                // Wait before retrying with exponential backoff
                await Task.Delay(RetryDelayMs * attempts);
                
                // Refresh the entities that caused the conflict
                foreach (var entry in ex.Entries)
                {
                    var databaseValues = await entry.GetDatabaseValuesAsync();
                    
                    if (databaseValues == null)
                    {
                        // Entity was deleted by another user
                        throw new InvalidOperationException(
                            "تم حذف هذا السجل بواسطة مستخدم آخر. الرجاء تحديث الصفحة.",
                            ex);
                    }
                    
                    // Reload the entity with latest values from database
                    entry.OriginalValues.SetValues(databaseValues);
                }
            }
            catch (DbUpdateConcurrencyException ex) when (attempts >= MaxRetryAttempts)
            {
                // Max retries reached - inform user
                throw new InvalidOperationException(
                    "تم تعديل هذا السجل بواسطة مستخدم آخر. الرجاء إعادة تحميل البيانات والمحاولة مرة أخرى.",
                    ex);
            }
        }
    }

    /// <summary>
    /// Execute a void operation with automatic retry on concurrency conflicts
    /// </summary>
    public static async Task ExecuteWithRetryAsync(Func<Task> operation)
    {
        await ExecuteWithRetryAsync(async () =>
        {
            await operation();
            return true;
        });
    }

    /// <summary>
    /// Handle concurrency exception and provide user-friendly error message
    /// </summary>
    public static string GetUserFriendlyMessage(DbUpdateConcurrencyException ex)
    {
        var affectedEntities = new System.Collections.Generic.List<string>();
        
        foreach (var entry in ex.Entries)
        {
            var entityName = GetEntityDisplayName(entry.Entity.GetType().Name);
            affectedEntities.Add(entityName);
        }

        var entitiesList = string.Join("، ", affectedEntities);
        
        return $"تم تعديل {entitiesList} بواسطة مستخدم آخر أثناء عملك.\n\n" +
               "الرجاء إعادة تحميل البيانات والمحاولة مرة أخرى.\n\n" +
               "نصيحة: حاول تجنب تعديل نفس السجلات في نفس الوقت مع مستخدمين آخرين.";
    }

    private static string GetEntityDisplayName(string entityName)
    {
        return entityName switch
        {
            "CashBox" => "الصندوق",
            "CashTransaction" => "الحركة المالية",
            "SalesInvoice" => "فاتورة المبيعات",
            "PurchaseInvoice" => "فاتورة المشتريات",
            "Trip" => "الرحلة",
            "TripBooking" => "حجز الرحلة",
            "UmrahPackage" => "باكدج العمرة",
            "UmrahTrip" => "رحلة العمرة",
            "FlightBooking" => "حجز الطيران",
            "Customer" => "العميل",
            "Supplier" => "المورد",
            "Account" => "الحساب",
            "JournalEntry" => "قيد اليومية",
            _ => "السجل"
        };
    }

    /// <summary>
    /// Check if an exception is a concurrency conflict
    /// </summary>
    public static bool IsConcurrencyException(Exception ex)
    {
        return ex is DbUpdateConcurrencyException ||
               ex.InnerException is DbUpdateConcurrencyException;
    }
}

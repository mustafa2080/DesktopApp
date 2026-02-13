using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Data;
using System.Threading.Tasks;

namespace GraceWay.AccountingSystem.Infrastructure.Data;

/// <summary>
/// Helper for managing database transactions with proper isolation levels
/// </summary>
public class TransactionScopeHelper
{
    /// <summary>
    /// Execute operation within a transaction with Read Committed isolation
    /// Best for most operations - prevents dirty reads
    /// </summary>
    public static async Task<T> ExecuteInTransactionAsync<T>(
        AppDbContext context,
        Func<Task<T>> operation)
    {
        return await ExecuteInTransactionAsync(
            context, 
            operation, 
            IsolationLevel.ReadCommitted);
    }

    /// <summary>
    /// Execute operation within a transaction with specified isolation level
    /// </summary>
    public static async Task<T> ExecuteInTransactionAsync<T>(
        AppDbContext context,
        Func<Task<T>> operation,
        IsolationLevel isolationLevel)
    {
        // Use retry logic for transaction
        return await ConcurrencyExceptionHandler.ExecuteWithRetryAsync(async () =>
        {
            var strategy = context.Database.CreateExecutionStrategy();
            
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await context.Database
                    .BeginTransactionAsync(isolationLevel);
                
                try
                {
                    var result = await operation();
                    await transaction.CommitAsync();
                    return result;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        });
    }

    /// <summary>
    /// Execute void operation within a transaction
    /// </summary>
    public static async Task ExecuteInTransactionAsync(
        AppDbContext context,
        Func<Task> operation,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        await ExecuteInTransactionAsync(context, async () =>
        {
            await operation();
            return true;
        }, isolationLevel);
    }

    /// <summary>
    /// Execute operation with Serializable isolation for critical operations
    /// Use for financial calculations and balance updates
    /// </summary>
    public static async Task<T> ExecuteInSerializableTransactionAsync<T>(
        AppDbContext context,
        Func<Task<T>> operation)
    {
        return await ExecuteInTransactionAsync(
            context,
            operation,
            IsolationLevel.Serializable);
    }

    /// <summary>
    /// Execute operation with Snapshot isolation for reporting
    /// Best for long-running read operations
    /// </summary>
    public static async Task<T> ExecuteInSnapshotTransactionAsync<T>(
        AppDbContext context,
        Func<Task<T>> operation)
    {
        return await ExecuteInTransactionAsync(
            context,
            operation,
            IsolationLevel.Snapshot);
    }
}

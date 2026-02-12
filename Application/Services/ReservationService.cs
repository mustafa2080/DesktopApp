using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GraceWay.AccountingSystem.Application.Services;

public class ReservationService : IReservationService
{
    private readonly AppDbContext _context;
    private readonly ICashBoxService _cashBoxService;
    private readonly IJournalService _journalService;

    public ReservationService(AppDbContext context, ICashBoxService cashBoxService, IJournalService journalService)
    {
        _context = context;
        _cashBoxService = cashBoxService;
        _journalService = journalService;
    }

    // ==================== Reservation CRUD ====================
    
    public async Task<List<Reservation>> GetAllReservationsAsync(string? searchTerm = null, string? status = null)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Starting GetAllReservationsAsync");
            
            // Use single query with Include to avoid multiple concurrent queries
            var query = _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.ServiceType)
                .AsNoTracking() // Important: Disable change tracking for read-only operations
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(r => 
                    r.ReservationNumber.Contains(searchTerm) ||
                    (r.ServiceDescription != null && r.ServiceDescription.Contains(searchTerm))
                );
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(r => r.Status == status);
            }

            // Execute single query with all data
            var reservations = await query
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();

            System.Diagnostics.Debug.WriteLine($"Found {reservations.Count} reservations");

            return reservations;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GetAllReservationsAsync: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            throw new Exception($"خطأ في جلب الحجوزات: {ex.Message}", ex);
        }
    }

    public async Task<Reservation?> GetReservationByIdAsync(int reservationId)
    {
        try
        {
            return await _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.ServiceType)
                .Include(r => r.Currency)
                .Include(r => r.Supplier)
                .Include(r => r.CashBox)
                .Include(r => r.CashTransaction)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);
        }
        catch (Exception ex)
        {
            throw new Exception($"خطأ في جلب الحجز: {ex.Message}", ex);
        }
    }

    public async Task<Reservation> CreateReservationAsync(Reservation reservation)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
            // Generate reservation number if not provided
            if (string.IsNullOrWhiteSpace(reservation.ReservationNumber))
            {
                reservation.ReservationNumber = await GenerateReservationNumberAsync();
            }

            // Set dates
            reservation.ReservationDate = DateTime.UtcNow;
            reservation.CreatedAt = DateTime.UtcNow;
            reservation.UpdatedAt = DateTime.UtcNow;

            // Validate
            if (reservation.CustomerId <= 0)
                throw new Exception("يجب اختيار العميل");

            if (reservation.ServiceTypeId <= 0)
                throw new Exception("يجب اختيار نوع الخدمة");

            if (reservation.SellingPrice < 0)
                throw new Exception("سعر البيع لا يمكن أن يكون سالباً");

            if (reservation.CostPrice < 0)
                throw new Exception("سعر التكلفة لا يمكن أن يكون سالباً");

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            
            // If status is confirmed and cashbox is selected, create cash transaction
            if (reservation.Status == "Confirmed" && reservation.CashBoxId.HasValue)
            {
                await CreateCashTransactionForReservation(reservation);
                await _context.SaveChangesAsync();
                
                // إنشاء قيد يومي تلقائي
                try
                {
                    await _journalService.CreateReservationJournalEntryAsync(reservation);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"فشل إنشاء القيد اليومي: {ex.Message}");
                }
            }
            
            await transaction.CommitAsync();

            return reservation;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception($"خطأ في إنشاء الحجز: {ex.Message}", ex);
        }
        });
    }

    public async Task<bool> UpdateReservationAsync(Reservation reservation)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingReservation = await _context.Reservations.FindAsync(reservation.ReservationId);
                if (existingReservation == null)
                    throw new Exception("الحجز غير موجود");

                // Don't allow editing if completed or cancelled
                if (existingReservation.Status == "Completed" || existingReservation.Status == "Cancelled")
                    throw new Exception("لا يمكن تعديل حجز مكتمل أو ملغي");

                var oldStatus = existingReservation.Status;
                var newStatus = reservation.Status;

                // Update properties
                existingReservation.CustomerId = reservation.CustomerId;
                existingReservation.ServiceTypeId = reservation.ServiceTypeId;
                existingReservation.ServiceDescription = reservation.ServiceDescription;
                existingReservation.TravelDate = reservation.TravelDate;
                existingReservation.ReturnDate = reservation.ReturnDate;
                existingReservation.NumberOfPeople = reservation.NumberOfPeople;
                existingReservation.SellingPrice = reservation.SellingPrice;
                existingReservation.CostPrice = reservation.CostPrice;
                existingReservation.CurrencyId = reservation.CurrencyId;
                existingReservation.ExchangeRate = reservation.ExchangeRate;
                existingReservation.SupplierId = reservation.SupplierId;
                existingReservation.SupplierCost = reservation.SupplierCost;
                existingReservation.Status = newStatus;
                existingReservation.CashBoxId = reservation.CashBoxId;  // تحديث الخزنة
                existingReservation.TripId = reservation.TripId;  // تحديث الرحلة
                existingReservation.Notes = reservation.Notes;
                existingReservation.UpdatedAt = DateTime.UtcNow;

                // Handle status changes
                if (oldStatus != newStatus)
                {
                    // Create cash transaction when confirming reservation
                    if (oldStatus == "Draft" && newStatus == "Confirmed" && existingReservation.CashBoxId.HasValue)
                    {
                        await CreateCashTransactionForReservation(existingReservation);
                        
                        // إنشاء قيد يومي تلقائي
                        try
                        {
                            await _journalService.CreateReservationJournalEntryAsync(existingReservation);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"فشل إنشاء القيد اليومي: {ex.Message}");
                        }
                    }

                    // Delete cash transaction when cancelling reservation
                    if ((oldStatus == "Confirmed" || oldStatus == "Completed") && newStatus == "Cancelled")
                    {
                        await DeleteCashTransactionForReservation(existingReservation);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"خطأ في تحديث الحجز: {ex.Message}", ex);
            }
        });
    }

    public async Task<bool> DeleteReservationAsync(int reservationId)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation == null)
                throw new Exception("الحجز غير موجود");

            // Allow deletion if Draft or Cancelled
            if (reservation.Status != "Draft" && reservation.Status != "Cancelled")
                throw new Exception("لا يمكن حذف حجز مؤكد أو مكتمل. يمكنك إلغاؤه أولاً ثم حذفه.");

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception($"خطأ في حذف الحجز: {ex.Message}", ex);
        }
        });
    }

    public async Task<bool> ChangeReservationStatusAsync(int reservationId, string newStatus)
    {
        try
        {
            // Validate status first
            var validStatuses = new[] { "Draft", "Confirmed", "Cancelled", "Completed" };
            if (!validStatuses.Contains(newStatus))
                throw new Exception("حالة غير صالحة");

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Load reservation with relationships inside transaction
                    var reservation = await _context.Reservations
                        .Include(r => r.Customer)
                        .Include(r => r.ServiceType)
                        .Include(r => r.CashBox)
                        .FirstOrDefaultAsync(r => r.ReservationId == reservationId);
                    
                    if (reservation == null)
                        throw new Exception("الحجز غير موجود");

                    // Business rules for status changes
                    if (reservation.Status == "Completed" && newStatus != "Completed")
                        throw new Exception("لا يمكن تغيير حالة حجز مكتمل");

                    if (reservation.Status == "Cancelled" && newStatus != "Cancelled")
                        throw new Exception("لا يمكن تغيير حالة حجز ملغي");

                    var oldStatus = reservation.Status;
                    
                    System.Diagnostics.Debug.WriteLine($"تغيير حالة الحجز {reservationId}: من '{oldStatus}' إلى '{newStatus}'");
                    
                    reservation.Status = newStatus;
                    reservation.UpdatedAt = DateTime.UtcNow;
                    
                    // Mark as modified explicitly
                    _context.Entry(reservation).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    
                    System.Diagnostics.Debug.WriteLine($"Reservation.Status بعد التغيير: '{reservation.Status}'");

                    // Create cash transaction when confirming reservation
                    if (oldStatus == "Draft" && newStatus == "Confirmed" && reservation.CashBoxId.HasValue)
                    {
                        await CreateCashTransactionForReservation(reservation);
                        
                        // إنشاء قيد يومي تلقائي
                        try
                        {
                            await _journalService.CreateReservationJournalEntryAsync(reservation);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"فشل إنشاء القيد اليومي: {ex.Message}");
                        }
                    }

                    // Delete cash transaction when cancelling reservation
                    if ((oldStatus == "Confirmed" || oldStatus == "Completed") && newStatus == "Cancelled")
                    {
                        await DeleteCashTransactionForReservation(reservation);
                    }

                    await _context.SaveChangesAsync();
                    
                    System.Diagnostics.Debug.WriteLine($"تم حفظ التغييرات في قاعدة البيانات");
                    
                    // Verify the change was saved
                    var verifyReservation = await _context.Reservations
                        .AsNoTracking()
                        .FirstOrDefaultAsync(r => r.ReservationId == reservationId);
                    
                    System.Diagnostics.Debug.WriteLine($"التحقق من الحالة بعد الحفظ: '{verifyReservation?.Status}'");
                    
                    await transaction.CommitAsync();
                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            throw new Exception($"خطأ في تغيير حالة الحجز: {ex.Message}", ex);
        }
    }
    
    private async Task CreateCashTransactionForReservation(Reservation reservation)
    {
        var cashTransaction = new CashTransaction
        {
            CashBoxId = reservation.CashBoxId!.Value,
            Type = TransactionType.Income,
            Amount = reservation.SellingPrice,
            TransactionDate = DateTime.UtcNow,
            Category = "حجوزات سياحية",
            Description = $"حجز رقم {reservation.ReservationNumber} - {reservation.ServiceType?.ServiceTypeName}",
            PartyName = reservation.Customer?.CustomerName,
            PaymentMethod = PaymentMethod.Cash,
            ReferenceNumber = reservation.ReservationNumber,
            Notes = reservation.Notes,
            CreatedBy = reservation.CreatedBy ?? 1,
            ReservationId = reservation.ReservationId
        };

        var createdTransaction = await _cashBoxService.AddIncomeAsync(cashTransaction);
        reservation.CashTransactionId = createdTransaction.Id;
    }

    private async Task DeleteCashTransactionForReservation(Reservation reservation)
    {
        // التحقق من وجود CashTransactionId قبل المحاولة
        if (!reservation.CashTransactionId.HasValue)
        {
            // لا توجد حركة مالية مرتبطة بهذا الحجز - لا حاجة للحذف
            return;
        }

        try
        {
            await _cashBoxService.DeleteTransactionAsync(reservation.CashTransactionId.Value);
            reservation.CashTransactionId = null;
        }
        catch (InvalidOperationException)
        {
            // الحركة غير موجودة أو محذوفة بالفعل
            reservation.CashTransactionId = null;
        }
    }

    // ==================== ServiceType CRUD ====================

    public async Task<List<ServiceType>> GetAllServiceTypesAsync()
    {
        try
        {
            return await _context.ServiceTypes
                .Where(st => st.IsActive)
                .OrderBy(st => st.ServiceTypeName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"خطأ في جلب أنواع الخدمات: {ex.Message}", ex);
        }
    }

    public async Task<ServiceType?> GetServiceTypeByIdAsync(int serviceTypeId)
    {
        try
        {
            return await _context.ServiceTypes.FindAsync(serviceTypeId);
        }
        catch (Exception ex)
        {
            throw new Exception($"خطأ في جلب نوع الخدمة: {ex.Message}", ex);
        }
    }

    public async Task<ServiceType> CreateServiceTypeAsync(ServiceType serviceType)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(serviceType.ServiceTypeName))
                throw new Exception("يجب إدخال اسم الخدمة");

            _context.ServiceTypes.Add(serviceType);
            await _context.SaveChangesAsync();
            return serviceType;
        }
        catch (Exception ex)
        {
            throw new Exception($"خطأ في إنشاء نوع الخدمة: {ex.Message}", ex);
        }
    }

    public async Task<bool> UpdateServiceTypeAsync(ServiceType serviceType)
    {
        try
        {
            var existing = await _context.ServiceTypes.FindAsync(serviceType.ServiceTypeId);
            if (existing == null)
                throw new Exception("نوع الخدمة غير موجود");

            existing.ServiceTypeName = serviceType.ServiceTypeName;
            existing.ServiceTypeNameEn = serviceType.ServiceTypeNameEn;
            existing.Description = serviceType.Description;
            existing.IsActive = serviceType.IsActive;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"خطأ في تحديث نوع الخدمة: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteServiceTypeAsync(int serviceTypeId)
    {
        try
        {
            var serviceType = await _context.ServiceTypes
                .Include(st => st.Reservations)
                .FirstOrDefaultAsync(st => st.ServiceTypeId == serviceTypeId);

            if (serviceType == null)
                throw new Exception("نوع الخدمة غير موجود");

            if (serviceType.Reservations.Any())
                throw new Exception("لا يمكن حذف نوع خدمة له حجوزات. يمكنك تعطيله بدلاً من ذلك.");

            _context.ServiceTypes.Remove(serviceType);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"خطأ في حذف نوع الخدمة: {ex.Message}", ex);
        }
    }

    // ==================== Business Logic ====================

    public async Task<string> GenerateReservationNumberAsync()
    {
        try
        {
            var year = DateTime.Now.Year;
            var lastReservation = await _context.Reservations
                .Where(r => r.ReservationNumber.StartsWith($"RES-{year}-"))
                .OrderByDescending(r => r.ReservationId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastReservation != null)
            {
                var parts = lastReservation.ReservationNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"RES-{year}-{nextNumber:D4}";
        }
        catch (Exception ex)
        {
            throw new Exception($"خطأ في توليد رقم الحجز: {ex.Message}", ex);
        }
    }

    public async Task<decimal> CalculateProfitAsync(int reservationId)
    {
        try
        {
            var reservation = await _context.Reservations.FindAsync(reservationId);
            if (reservation == null)
                throw new Exception("الحجز غير موجود");

            return reservation.ProfitAmount;
        }
        catch (Exception ex)
        {
            throw new Exception($"خطأ في حساب الربح: {ex.Message}", ex);
        }
    }

    public async Task<List<Reservation>> GetReservationsByCustomerAsync(int customerId)
    {
        try
        {
            return await _context.Reservations
                .Include(r => r.ServiceType)
                .Include(r => r.Currency)
                .Include(r => r.Supplier)
                .AsNoTracking()
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"خطأ في جلب حجوزات العميل: {ex.Message}", ex);
        }
    }

    public async Task<List<Reservation>> GetReservationsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            return await _context.Reservations
                .Include(r => r.Customer)
                .Include(r => r.ServiceType)
                .Include(r => r.Currency)
                .Include(r => r.Supplier)
                .Include(r => r.CashBox)
                .Include(r => r.CashTransaction)
                .AsNoTracking()
                .Where(r => r.ReservationDate >= startDate && r.ReservationDate <= endDate)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"خطأ في جلب الحجوزات حسب التاريخ: {ex.Message}", ex);
        }
    }

    public async Task<Dictionary<string, decimal>> GetReservationStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var query = _context.Reservations.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(r => r.ReservationDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(r => r.ReservationDate <= endDate.Value);

            var reservations = await query.ToListAsync();

            return new Dictionary<string, decimal>
            {
                { "TotalReservations", reservations.Count },
                { "TotalSales", reservations.Sum(r => r.SellingPrice) },
                { "TotalCosts", reservations.Sum(r => r.CostPrice) },
                { "TotalProfit", reservations.Sum(r => r.ProfitAmount) },
                { "ConfirmedCount", reservations.Count(r => r.Status == "Confirmed") },
                { "CompletedCount", reservations.Count(r => r.Status == "Completed") },
                { "CancelledCount", reservations.Count(r => r.Status == "Cancelled") },
                { "DraftCount", reservations.Count(r => r.Status == "Draft") }
            };
        }
        catch (Exception ex)
        {
            throw new Exception($"خطأ في حساب إحصائيات الحجوزات: {ex.Message}", ex);
        }
    }
}

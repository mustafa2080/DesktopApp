using GraceWay.AccountingSystem.Domain.Entities;

namespace GraceWay.AccountingSystem.Application.Services;

/// <summary>
/// واجهة خدمة إدارة الرحلات السياحية
/// </summary>
public interface ITripService
{
    // ══════════════════════════════════════
    // CRUD Operations - العمليات الأساسية
    // ══════════════════════════════════════
    
    /// <summary>
    /// إنشاء رحلة جديدة
    /// </summary>
    Task<Trip> CreateTripAsync(Trip trip);
    
    /// <summary>
    /// تحديث بيانات رحلة
    /// </summary>
    Task<Trip> UpdateTripAsync(Trip trip);
    
    /// <summary>
    /// حذف رحلة
    /// </summary>
    Task<bool> DeleteTripAsync(int tripId);
    
    /// <summary>
    /// الحصول على رحلة بالمعرف
    /// </summary>
    Task<Trip?> GetTripByIdAsync(int tripId, bool includeDetails = false);
    
    /// <summary>
    /// الحصول على جميع الرحلات
    /// </summary>
    Task<List<Trip>> GetAllTripsAsync(bool includeDetails = false);
    
    // ══════════════════════════════════════
    // Business Logic - المنطق الحسابي
    // ══════════════════════════════════════
    
    /// <summary>
    /// حساب التكلفة الإجمالية للرحلة
    /// </summary>
    Task<decimal> CalculateTotalCostAsync(int tripId);
    
    /// <summary>
    /// حساب صافي الربح
    /// </summary>
    Task<decimal> CalculateProfitAsync(int tripId);
    
    /// <summary>
    /// التحقق من توفر أماكن للحجز
    /// </summary>
    Task<bool> CheckAvailabilityAsync(int tripId, int numberOfPersons);
    
    /// <summary>
    /// تحديث حالة الرحلة
    /// </summary>
    Task<bool> UpdateTripStatusAsync(int tripId, TripStatus newStatus, int userId);
    
    /// <summary>
    /// توليد رقم رحلة جديد تلقائياً
    /// </summary>
    Task<string> GenerateTripNumberAsync();
    
    /// <summary>
    /// نشر الرحلة (جعلها متاحة للحجز)
    /// </summary>
    Task<bool> PublishTripAsync(int tripId, int userId);
    
    /// <summary>
    /// إلغاء نشر الرحلة
    /// </summary>
    Task<bool> UnpublishTripAsync(int tripId, int userId);
    
    // ══════════════════════════════════════
    // Filtering & Search - البحث والتصفية
    // ══════════════════════════════════════
    
    /// <summary>
    /// البحث في الرحلات حسب النوع
    /// </summary>
    Task<List<Trip>> GetTripsByTypeAsync(TripType type);
    
    /// <summary>
    /// البحث في الرحلات حسب الحالة
    /// </summary>
    Task<List<Trip>> GetTripsByStatusAsync(TripStatus status);
    
    /// <summary>
    /// البحث في الرحلات بفترة زمنية
    /// </summary>
    Task<List<Trip>> GetTripsByDateRangeAsync(DateTime from, DateTime to);
    
    /// <summary>
    /// البحث العام في الرحلات
    /// </summary>
    Task<List<Trip>> SearchTripsAsync(string searchTerm);
    
    /// <summary>
    /// قفل الرحلة من التعديل في قسم الرحلات
    /// </summary>
    Task<bool> LockTripForTripsAsync(int tripId);
    
    /// <summary>
    /// فتح الرحلة للتعديل في قسم الرحلات
    /// </summary>
    Task<bool> UnlockTripForTripsAsync(int tripId);
    
    /// <summary>
    /// الحصول على الرحلات النشطة والمتاحة للحجز
    /// </summary>
    Task<List<Trip>> GetActiveTripsAsync();
    
    /// <summary>
    /// الحصول على الرحلات القادمة
    /// </summary>
    Task<List<Trip>> GetUpcomingTripsAsync();
    
    // ══════════════════════════════════════
    // Trip Components - مكونات الرحلة
    // ══════════════════════════════════════
    
    /// <summary>
    /// إضافة برنامج يومي للرحلة
    /// </summary>
    Task<TripProgram> AddProgramAsync(TripProgram program);
    
    /// <summary>
    /// تحديث برنامج يومي
    /// </summary>
    Task<TripProgram> UpdateProgramAsync(TripProgram program);
    
    /// <summary>
    /// حذف برنامج يومي
    /// </summary>
    Task<bool> DeleteProgramAsync(int programId);
    
    /// <summary>
    /// إضافة وسيلة نقل
    /// </summary>
    Task<TripTransportation> AddTransportationAsync(TripTransportation transportation);
    
    /// <summary>
    /// تحديث وسيلة نقل
    /// </summary>
    Task<TripTransportation> UpdateTransportationAsync(TripTransportation transportation);
    
    /// <summary>
    /// حذف وسيلة نقل
    /// </summary>
    Task<bool> DeleteTransportationAsync(int transportationId);
    
    /// <summary>
    /// إضافة مكان إقامة
    /// </summary>
    Task<TripAccommodation> AddAccommodationAsync(TripAccommodation accommodation);
    
    /// <summary>
    /// تحديث مكان إقامة
    /// </summary>
    Task<TripAccommodation> UpdateAccommodationAsync(TripAccommodation accommodation);
    
    /// <summary>
    /// حذف مكان إقامة
    /// </summary>
    Task<bool> DeleteAccommodationAsync(int accommodationId);
    
    /// <summary>
    /// إضافة مرشد سياحي
    /// </summary>
    Task<TripGuide> AddGuideAsync(TripGuide guide);
    
    /// <summary>
    /// تحديث مرشد سياحي
    /// </summary>
    Task<TripGuide> UpdateGuideAsync(TripGuide guide);
    
    /// <summary>
    /// حذف مرشد سياحي
    /// </summary>
    Task<bool> DeleteGuideAsync(int guideId);
    
    /// <summary>
    /// إضافة رحلة اختيارية
    /// </summary>
    Task<TripOptionalTour> AddOptionalTourAsync(TripOptionalTour tour);
    
    /// <summary>
    /// تحديث رحلة اختيارية
    /// </summary>
    Task<TripOptionalTour> UpdateOptionalTourAsync(TripOptionalTour tour);
    
    /// <summary>
    /// حذف رحلة اختيارية
    /// </summary>
    Task<bool> DeleteOptionalTourAsync(int tourId);
    
    /// <summary>
    /// إضافة مصروف
    /// </summary>
    Task<TripExpense> AddExpenseAsync(TripExpense expense);
    
    /// <summary>
    /// تحديث مصروف
    /// </summary>
    Task<TripExpense> UpdateExpenseAsync(TripExpense expense);
    
    /// <summary>
    /// حذف مصروف
    /// </summary>
    Task<bool> DeleteExpenseAsync(int expenseId);
    
    // ══════════════════════════════════════
    // Validation - التحقق
    // ══════════════════════════════════════
    
    /// <summary>
    /// التحقق من صحة بيانات الرحلة
    /// </summary>
    Task<(bool IsValid, List<string> Errors)> ValidateTripAsync(Trip trip);
    
    /// <summary>
    /// التحقق من إمكانية حذف الرحلة
    /// </summary>
    Task<(bool CanDelete, string Reason)> CanDeleteTripAsync(int tripId);
    
    /// <summary>
    /// إعادة حساب تكاليف جميع الرحلات (One-time fix للداتا الموجودة)
    /// </summary>
    Task<int> RecalculateAllTripCostsAsync();
}

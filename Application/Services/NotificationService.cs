using GraceWay.AccountingSystem.Application.Services.Backup;
using GraceWay.AccountingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using GraceWay.AccountingSystem.Infrastructure.Data;

namespace GraceWay.AccountingSystem.Application.Services;

/// <summary>
/// خدمة الإشعارات — تفحص قاعدة البيانات وتولّد تنبيهات ذكية
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly ICashBoxService   _cashBoxService;
    private readonly IInvoiceService   _invoiceService;
    private readonly ITripService      _tripService;
    private readonly IUmrahService     _umrahService;

    private List<AppNotification> _cache = new();
    private int _nextId = 1;

    public event EventHandler<List<AppNotification>>? NotificationsRefreshed;
    public int UnreadCount => _cache.Count(n => !n.IsRead);

    public NotificationService(
        IDbContextFactory<AppDbContext> dbFactory,
        ICashBoxService   cashBoxService,
        IInvoiceService   invoiceService,
        ITripService      tripService,
        IUmrahService     umrahService)
    {
        _dbFactory      = dbFactory;
        _cashBoxService = cashBoxService;
        _invoiceService = invoiceService;
        _tripService    = tripService;
        _umrahService   = umrahService;
    }

    public Task<List<AppNotification>> GetAllAsync() =>
        Task.FromResult(_cache.OrderByDescending(n => n.CreatedAt).ToList());

    public void MarkAllRead()
    {
        foreach (var n in _cache) n.IsRead = true;
    }

    public async Task RefreshAsync()
    {
        var notifications = new List<AppNotification>();
        _nextId = 1;

        try { await CheckCashBoxAsync(notifications); }      catch { /* لا نوقف الباقي */ }
        try { await CheckInvoicesAsync(notifications); }     catch { }
        try { await CheckTripsAsync(notifications); }        catch { }
        try { await CheckUmrahAsync(notifications); }        catch { }
        try { await CheckReservationsAsync(notifications); } catch { }

        // الاحتفاظ بحالة "مقروء" للإشعارات الموجودة
        foreach (var n in notifications)
        {
            var old = _cache.FirstOrDefault(c => c.Title == n.Title && c.Message == n.Message);
            if (old != null) n.IsRead = old.IsRead;
        }

        _cache = notifications;
        NotificationsRefreshed?.Invoke(this, _cache);
    }

    // ── 1. الخزنة ──────────────────────────────────────────────
    private async Task CheckCashBoxAsync(List<AppNotification> list)
    {
        const decimal LowBalanceThreshold = 1000m;
        var cashBoxes = await _cashBoxService.GetAllCashBoxesAsync();
        foreach (var cb in cashBoxes)
        {
            var bal = await _cashBoxService.GetCurrentBalanceAsync(cb.Id);

            if (bal < 0)
                Add(list, "رصيد سالب!", $"خزنة «{cb.Name}» رصيدها سالب: {bal:N2} ج.م",
                    NotificationType.Danger, "cashbox");

            else if (bal < LowBalanceThreshold)
                Add(list, "رصيد منخفض", $"خزنة «{cb.Name}» وصلت لـ {bal:N2} ج.م — أقل من الحد الأدنى",
                    NotificationType.Warning, "cashbox");
        }
    }

    // ── 2. الفواتير ────────────────────────────────────────────
    private async Task CheckInvoicesAsync(List<AppNotification> list)
    {
        var unpaidSales     = await _invoiceService.GetUnpaidSalesInvoicesAsync();
        var unpaidPurchases = await _invoiceService.GetUnpaidPurchaseInvoicesAsync();
        var today           = DateTime.Today;

        // فواتير بيع غير مدفوعة أكثر من 30 يوم
        var overdue = unpaidSales.Where(inv => (today - inv.InvoiceDate.Date).Days > 30).ToList();
        if (overdue.Count > 0)
            Add(list, $"فواتير متأخرة ({overdue.Count})",
                $"يوجد {overdue.Count} فاتورة بيع غير مدفوعة تجاوزت 30 يوماً — إجمالي: {overdue.Sum(i => i.RemainingAmount):N2} ج.م",
                NotificationType.Danger, "invoices");

        // فواتير ستستحق خلال 7 أيام
        var dueSoon = unpaidSales.Where(inv =>
        {
            int age = (today - inv.InvoiceDate.Date).Days;
            return age is >= 21 and <= 30;
        }).ToList();
        if (dueSoon.Count > 0)
            Add(list, $"فواتير قاربت الاستحقاق ({dueSoon.Count})",
                $"{dueSoon.Count} فاتورة ستتجاوز 30 يوماً قريباً",
                NotificationType.Warning, "invoices");

        // فواتير شراء غير مدفوعة للمورد
        if (unpaidPurchases.Count > 5)
            Add(list, $"فواتير شراء معلقة ({unpaidPurchases.Count})",
                $"يوجد {unpaidPurchases.Count} فاتورة شراء لم تُسدَّد بعد",
                NotificationType.Warning, "invoices");
    }

    // ── 3. الرحلات ─────────────────────────────────────────────
    private async Task CheckTripsAsync(List<AppNotification> list)
    {
        var trips = await _tripService.GetAllTripsAsync();
        var today = DateTime.Today;

        foreach (var trip in trips.Where(t => t.IsActive))
        {
            int daysToStart = (trip.StartDate.Date - today).Days;

            // رحلة بعد 3 أيام أو أقل
            if (daysToStart is >= 0 and <= 3)
                Add(list, $"رحلة وشيكة ⚠",
                    $"«{trip.TripName}» تبدأ بعد {daysToStart} يوم — {trip.BookedSeats}/{trip.TotalCapacity} حجز",
                    daysToStart <= 1 ? NotificationType.Danger : NotificationType.Warning,
                    "trips");

            // رحلة ممتلئة 100%
            if (trip.IsFull())
                Add(list, "رحلة ممتلئة ✔",
                    $"«{trip.TripName}» امتلأت بالكامل ({trip.TotalCapacity} مقعد)",
                    NotificationType.Success, "trips");

            // رحلة نسبة إشغالها أقل من 30% وموعدها قريب
            if (daysToStart is >= 0 and <= 14 && trip.OccupancyRate < 30 && trip.TotalCapacity > 0)
                Add(list, "إشغال منخفض",
                    $"«{trip.TripName}» نسبة الإشغال {trip.OccupancyRate:N0}% فقط وتبدأ بعد {daysToStart} يوم",
                    NotificationType.Warning, "trips");
        }
    }

    // ── 4. العمرة ──────────────────────────────────────────────
    private async Task CheckUmrahAsync(List<AppNotification> list)
    {
        var stats = await _umrahService.GetPackageStatisticsAsync();
        var packages = await _umrahService.GetActivePackagesAsync();
        var today = DateTime.Today;

        foreach (var pkg in packages)
        {
            if (pkg.Date == default) continue;
            int days = (pkg.Date.Date - today).Days;

            if (days is >= 0 and <= 7)
                Add(list, $"حزمة عمرة وشيكة",
                    $"«{pkg.TripName}» تسافر بعد {days} يوم — {pkg.Pilgrims.Count}/{pkg.NumberOfPersons} حاج",
                    days <= 2 ? NotificationType.Danger : NotificationType.Warning,
                    "umrah");
        }

        if (stats.TotalPilgrims > 0 && stats.AverageProfitMargin < 10)
            Add(list, "هامش ربح العمرة منخفض",
                $"متوسط هامش الربح {stats.AverageProfitMargin:N1}% — راجع التسعير",
                NotificationType.Warning, "umrah");
    }

    // ── 5. الحجوزات ────────────────────────────────────────────
    private async Task CheckReservationsAsync(List<AppNotification> list)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var today = DateTime.Today;

        // حجوزات سفرها اليوم أو غداً
        var travelingSoon = await db.Reservations
            .Where(r => r.TravelDate.HasValue &&
                        r.TravelDate.Value.Date >= today &&
                        r.TravelDate.Value.Date <= today.AddDays(1) &&
                        r.Status != "Cancelled" && r.Status != "Completed")
            .CountAsync();

        if (travelingSoon > 0)
            Add(list, $"حجوزات سفر اليوم/غداً ({travelingSoon})",
                $"يوجد {travelingSoon} حجز موعد سفره اليوم أو غداً",
                NotificationType.Info, "reservations");
    }

    // ── Helper ──────────────────────────────────────────────────
    private void Add(List<AppNotification> list, string title, string msg,
        NotificationType type, string module)
    {
        list.Add(new AppNotification
        {
            Id        = _nextId++,
            Title     = title,
            Message   = msg,
            Type      = type,
            Module    = module,
            ActionKey = module,
            CreatedAt = DateTime.Now,
        });
    }
}

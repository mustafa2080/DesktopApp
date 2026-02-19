using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GraceWay.AccountingSystem.Domain.Reports;
using GraceWay.AccountingSystem.Domain.Entities;
using GraceWay.AccountingSystem.Infrastructure.Data;

namespace GraceWay.AccountingSystem.Application.Services.Reports;

public class FinancialReportService : IFinancialReportService
{
    private readonly AppDbContext _context;
    
    public FinancialReportService(AppDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// الحصول على قائمة الدخل
    /// </summary>
    public async Task<IncomeStatementReport> GetIncomeStatementAsync(
        DateTime startDate, DateTime endDate)
    {
        var report = new IncomeStatementReport
        {
            StartDate = startDate,
            EndDate = endDate,
            Period = $"{startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}"
        };
        
        // 1. حساب الإيرادات
        report.Revenue = await CalculateRevenueAsync(startDate, endDate);
        
        // 2. حساب التكاليف المباشرة
        report.DirectCosts = await CalculateDirectCostsAsync(startDate, endDate);
        
        // 3. حساب المصروفات التشغيلية
        report.OperatingExpenses = await CalculateOperatingExpensesAsync(startDate, endDate);
        
        return report;
    }
    
    /// <summary>
    /// حساب الإيرادات
    /// </summary>
    private async Task<RevenueSection> CalculateRevenueAsync(DateTime start, DateTime end)
    {
        var revenue = new RevenueSection();
        
        // إيرادات الرحلات من فواتير المبيعات
        var tripInvoices = await _context.SalesInvoices
            .Where(i => i.InvoiceDate >= start && i.InvoiceDate <= end)
            .Include(i => i.TripBookings)
            .Where(i => i.TripBookings.Any())
            .ToListAsync();
        
        revenue.TripRevenue = tripInvoices.Sum(i => i.TotalAmount);
        
        // إيرادات الخدمات (الحجوزات العادية)
        var serviceInvoices = await _context.SalesInvoices
            .Where(i => i.InvoiceDate >= start && i.InvoiceDate <= end)
            .Where(i => i.ReservationId != null && !i.TripBookings.Any())
            .ToListAsync();
        
        revenue.ServiceRevenue = serviceInvoices.Sum(i => i.TotalAmount);
        
        // إيرادات أخرى (معاملات نقدية - إيرادات)
        var otherIncome = await _context.CashTransactions
            .Where(t => t.TransactionDate >= start && t.TransactionDate <= end)
            .Where(t => t.Type == TransactionType.Income && t.Category == "Other Income")
            .SumAsync(t => t.Amount);
        
        revenue.OtherRevenue = otherIncome;
        
        return revenue;
    }
    
    /// <summary>
    /// حساب التكاليف المباشرة للرحلات
    /// </summary>
    private async Task<DirectCostSection> CalculateDirectCostsAsync(
        DateTime start, DateTime end)
    {
        var costs = new DirectCostSection();
        
        // الحصول على الرحلات في الفترة
        var trips = await _context.Trips
            .Where(t => t.StartDate >= start && t.StartDate <= end)
            .Include(t => t.Accommodations)
            .Include(t => t.Transportation)
            .Include(t => t.Guides)
            .Include(t => t.OptionalTours)
            .Include(t => t.Expenses)
            .ToListAsync();
        
        costs.AccommodationCosts = trips.SelectMany(t => t.Accommodations)
            .Sum(a => a.TotalCost);
        
        costs.TransportationCosts = trips.SelectMany(t => t.Transportation)
            .Sum(tr => tr.TotalCost);
        
        costs.GuideCosts = trips.SelectMany(t => t.Guides)
            .Sum(g => g.TotalCost);
        
        costs.OptionalTourCosts = trips.SelectMany(t => t.OptionalTours)
            .Sum(ot => ot.TotalCost);
        
        costs.OtherDirectCosts = trips.SelectMany(t => t.Expenses)
            .Sum(e => e.Amount);
        
        return costs;
    }
    
    /// <summary>
    /// حساب المصروفات التشغيلية - ✅ مُحسَّن بفلترة في SQL
    /// </summary>
    private async Task<OperatingExpenseSection> CalculateOperatingExpensesAsync(
        DateTime start, DateTime end)
    {
        var expenses = new OperatingExpenseSection();

        // ✅ فلترة مباشرة في SQL بدلاً من جلب كل البيانات وفلترتها في الميموري
        var baseQuery = _context.CashTransactions
            .Where(t => t.TransactionDate >= start && t.TransactionDate <= end
                     && t.Type == TransactionType.Expense
                     && !t.IsDeleted);

        expenses.Salaries = await baseQuery
            .Where(e => e.Category == "Salaries" || e.Category == "Payroll")
            .SumAsync(e => e.Amount);

        expenses.Rent = await baseQuery
            .Where(e => e.Category == "Rent")
            .SumAsync(e => e.Amount);

        expenses.Utilities = await baseQuery
            .Where(e => e.Category == "Utilities")
            .SumAsync(e => e.Amount);

        expenses.Marketing = await baseQuery
            .Where(e => e.Category == "Marketing" || e.Category == "Advertising")
            .SumAsync(e => e.Amount);

        expenses.Administrative = await baseQuery
            .Where(e => e.Category == "Administrative" || e.Category == "Office")
            .SumAsync(e => e.Amount);

        expenses.OtherExpenses = await baseQuery
            .Where(e => e.Category == "Other" || string.IsNullOrEmpty(e.Category))
            .SumAsync(e => e.Amount);

        return expenses;
    }
    
    /// <summary>
    /// قائمة دخل مقارنة بين فترتين
    /// </summary>
    public async Task<ComparativeIncomeStatement> GetComparativeIncomeStatementAsync(
        DateTime currentStart, DateTime currentEnd,
        DateTime previousStart, DateTime previousEnd)
    {
        var comparative = new ComparativeIncomeStatement
        {
            CurrentPeriod = await GetIncomeStatementAsync(currentStart, currentEnd),
            PreviousPeriod = await GetIncomeStatementAsync(previousStart, previousEnd)
        };
        
        return comparative;
    }
    
    /// <summary>
    /// الحصول على تقرير ربحية الرحلات
    /// </summary>
    public async Task<List<TripProfitabilityReport>> GetTripProfitabilityAsync(
        DateTime startDate, DateTime endDate)
    {
        var trips = await _context.Trips
            .Where(t => t.StartDate >= startDate && t.StartDate <= endDate)
            .Include(t => t.Bookings)
            .Include(t => t.Accommodations)
            .Include(t => t.Transportation)
            .Include(t => t.Guides)
            .Include(t => t.OptionalTours)
            .Include(t => t.Expenses)
            .Include(t => t.Reservations)  // إضافة الحجوزات العامة
            .ToListAsync();
        
        var reports = trips.Select(trip => 
        {
            // حساب الإيرادات من TripBooking أو من Reservations
            decimal revenue = 0;
            int bookingsCount = 0;
            int totalParticipants = 0;
            
            if (trip.Bookings.Any())
            {
                // استخدام TripBooking
                revenue = trip.Bookings.Sum(b => b.TotalAmount);
                bookingsCount = trip.Bookings.Count;
                totalParticipants = trip.Bookings.Sum(b => b.NumberOfPersons);
            }
            else if (trip.Reservations.Any())
            {
                // استخدام Reservations العامة
                revenue = trip.Reservations
                    .Where(r => r.Status == "Confirmed" || r.Status == "Completed")
                    .Sum(r => r.SellingPrice);
                bookingsCount = trip.Reservations.Count(r => r.Status == "Confirmed" || r.Status == "Completed");
                totalParticipants = trip.Reservations
                    .Where(r => r.Status == "Confirmed" || r.Status == "Completed")
                    .Sum(r => r.NumberOfPeople);
            }
            
            return new TripProfitabilityReport
            {
                TripId = trip.TripId,
                TripName = trip.TripName,
                TripCode = trip.TripCode ?? string.Empty,
                StartDate = trip.StartDate,
                EndDate = trip.EndDate,
                
                // الإيرادات
                Revenue = revenue,
                
                // التكاليف
                Costs = new TripCosts
                {
                    Accommodation = trip.Accommodations.Sum(a => a.TotalCost),
                    Transportation = trip.Transportation.Sum(t => t.TotalCost),
                    Guides = trip.Guides.Sum(g => g.TotalCost),
                    OptionalTours = trip.OptionalTours.Sum(ot => ot.TotalCost),
                    Other = trip.Expenses.Sum(e => e.Amount)
                },
                
                // الإحصائيات
                BookingsCount = bookingsCount,
                TotalParticipants = totalParticipants,
                AvailableSeats = trip.AvailableSeats
            };
        }).ToList();
        
        return reports;
    }
    
    /// <summary>
    /// الحصول على ربحية رحلة محددة
    /// </summary>
    public async Task<TripProfitabilityReport?> GetTripProfitabilityByIdAsync(int tripId)
    {
        var trip = await _context.Trips
            .Where(t => t.TripId == tripId)
            .Include(t => t.Bookings)
            .Include(t => t.Accommodations)
            .Include(t => t.Transportation)
            .Include(t => t.Guides)
            .Include(t => t.OptionalTours)
            .Include(t => t.Expenses)
            .FirstOrDefaultAsync();
        
        if (trip == null)
            return null;
        
        return new TripProfitabilityReport
        {
            TripId = trip.TripId,
            TripName = trip.TripName,
            TripCode = trip.TripCode ?? string.Empty,
            StartDate = trip.StartDate,
            EndDate = trip.EndDate,
            
            Revenue = trip.Bookings.Sum(b => b.TotalAmount),
            
            Costs = new TripCosts
            {
                Accommodation = trip.Accommodations.Sum(a => a.TotalCost),
                Transportation = trip.Transportation.Sum(t => t.TotalCost),
                Guides = trip.Guides.Sum(g => g.TotalCost),
                OptionalTours = trip.OptionalTours.Sum(ot => ot.TotalCost),
                Other = trip.Expenses.Sum(e => e.Amount)
            },
            
            BookingsCount = trip.Bookings.Count,
            TotalParticipants = trip.Bookings.Sum(b => b.NumberOfPersons),
            AvailableSeats = trip.AvailableSeats
        };
    }
}

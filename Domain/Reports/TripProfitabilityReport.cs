using System;
using System.Collections.Generic;

namespace GraceWay.AccountingSystem.Domain.Reports;

/// <summary>
/// تقرير ربحية الرحلة
/// </summary>
public class TripProfitabilityReport
{
    public int TripId { get; set; }
    public string TripName { get; set; } = string.Empty;
    public string TripCode { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    // ============================================
    // 💰 الإيرادات (Revenue)
    // ============================================
    public decimal Revenue { get; set; }
    public decimal OptionalToursRevenue { get; set; } // إيرادات الرحلات الاختيارية
    public decimal TotalRevenue => Revenue + OptionalToursRevenue;
    
    // ============================================
    // 💸 التكاليف (Costs)
    // ============================================
    public TripCosts Costs { get; set; } = new();
    
    // ============================================
    // 📈 الربح (Profit)
    // ============================================
    public decimal Profit => TotalRevenue - Costs.Total;
    public decimal ProfitMargin => TotalRevenue > 0 ? (Profit / TotalRevenue) * 100 : 0;
    
    // ============================================
    // 📊 الإحصائيات (Statistics)
    // ============================================
    public int BookingsCount { get; set; }
    public int TotalParticipants { get; set; }
    public int AvailableSeats { get; set; }
    public decimal OccupancyRate => AvailableSeats > 0 ? 
        ((decimal)TotalParticipants / AvailableSeats) * 100 : 0;
    
    // ============================================
    // 📊 مؤشرات الأداء (KPIs)
    // ============================================
    public decimal RevenuePerParticipant => TotalParticipants > 0 ? 
        TotalRevenue / TotalParticipants : 0;
    public decimal CostPerParticipant => TotalParticipants > 0 ? 
        Costs.Total / TotalParticipants : 0;
    public decimal ProfitPerParticipant => TotalParticipants > 0 ? 
        Profit / TotalParticipants : 0;
    
    // ============================================
    // 💳 تحليل التكاليف (Cost Analysis)
    // ============================================
    public decimal AccommodationPercentage => Costs.Total > 0 ? 
        (Costs.Accommodation / Costs.Total) * 100 : 0;
    public decimal TransportationPercentage => Costs.Total > 0 ? 
        (Costs.Transportation / Costs.Total) * 100 : 0;
    public decimal GuidesPercentage => Costs.Total > 0 ? 
        (Costs.Guides / Costs.Total) * 100 : 0;
    public decimal OptionalToursPercentage => Costs.Total > 0 ? 
        (Costs.OptionalTours / Costs.Total) * 100 : 0;
    public decimal OtherPercentage => Costs.Total > 0 ? 
        (Costs.Other / Costs.Total) * 100 : 0;
}

/// <summary>
/// تفاصيل تكاليف الرحلة
/// </summary>
public class TripCosts
{
    // 🏨 تكاليف الإقامة
    public decimal Accommodation { get; set; }
    
    // 🚌 تكاليف النقل
    public decimal Transportation { get; set; }
    
    // 👤 تكاليف المرشدين
    public decimal Guides { get; set; }
    
    // 🎫 تكاليف الرحلات الاختيارية
    public decimal OptionalTours { get; set; }
    
    // 📦 تكاليف أخرى
    public decimal Other { get; set; }
    
    // 💰 إجمالي التكاليف
    public decimal Total => Accommodation + Transportation + Guides + 
                           OptionalTours + Other;
    
    // 📊 هل يوجد تكاليف مسجلة؟
    public bool HasCosts => Total > 0;
}

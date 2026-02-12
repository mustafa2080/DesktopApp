using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities
{
    [Table("flightbookings")]
    public class FlightBooking
    {
        [Key]
        public int FlightBookingId { get; set; }

        [Required]
        [StringLength(50)]
        public string BookingNumber { get; set; } = string.Empty;

        [Required]
        public DateTime IssuanceDate { get; set; } // تاريخ الإصدار

        [Required]
        public DateTime TravelDate { get; set; } // تاريخ السفر

        [Required]
        [StringLength(200)]
        public string ClientName { get; set; } = string.Empty; // اسم العميل

        [StringLength(500)]
        public string ClientRoute { get; set; } = string.Empty; // مسار العميل

        [Required]
        [StringLength(200)]
        public string Supplier { get; set; } = string.Empty; // المورد

        [Required]
        [StringLength(100)]
        public string System { get; set; } = string.Empty; // النظام (Sabre, Amadeus, etc.)

        [Required]
        [StringLength(50)]
        public string TicketStatus { get; set; } = string.Empty; // حالة التذكرة

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = string.Empty; // طريقة الدفع

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SellingPrice { get; set; } // سعر البيع

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetPrice { get; set; } // السعر الصافي

        [Column(TypeName = "decimal(18,2)")]
        public decimal Profit => (SellingPrice - NetPrice) * TicketCount; // الربح الإجمالي

        [Required]
        public int TicketCount { get; set; } // عدد التذاكر

        [StringLength(500)]
        public string TicketNumbers { get; set; } = string.Empty; // أرقام التذاكر

        [Required]
        [StringLength(20)]
        public string MobileNumber { get; set; } = string.Empty; // رقم الموبايل

        [StringLength(1000)]
        public string? Notes { get; set; } // ملاحظات

        /// <summary>
        /// معرف اليوزر اللي أنشأ الحجز
        /// </summary>
        [Required]
        public int CreatedByUserId { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation Properties
        /// <summary>
        /// اليوزر اللي أنشأ الحجز
        /// </summary>
        [ForeignKey("CreatedByUserId")]
        public User CreatedByUser { get; set; } = null!;
    }
}

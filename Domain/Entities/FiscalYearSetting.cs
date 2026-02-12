using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GraceWay.AccountingSystem.Domain.Entities
{
    public class FiscalYearSetting
    {
        public int Id { get; set; }
        public DateTime FiscalYearStart { get; set; }
        public DateTime FiscalYearEnd { get; set; }
        public bool IsCurrentYear { get; set; }
        public bool IsClosed { get; set; }
        public string? Description { get; set; }
        [Column("createddate")]

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        [Column("createdby")]

        public int CreatedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public int? ModifiedBy { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace SmartCard.Models
{
    public class FuelQuota
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        // Navigation property
        public virtual Employee Employee { get; set; } = null!;

        public int Month { get; set; }

        public int Year { get; set; }

        public decimal QuotaInLiters { get; set; }

        public decimal UsedLiters { get; set; } = 0;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        // Validation to ensure used liters doesn't exceed quota
        public decimal AvailableLiters => QuotaInLiters - UsedLiters;
    }
}
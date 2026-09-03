using System.ComponentModel.DataAnnotations;

namespace SmartCard.DTOs
{
    public class FuelQuotaUpdateDto
    {
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public decimal QuotaInLiters { get; set; }

        public decimal UsedLiters { get; set; } = 0;
    }
}
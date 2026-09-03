using System.ComponentModel.DataAnnotations;

namespace SmartCard.DTOs
{
    public class FuelQuotaCreateDto
    {
        public int EmployeeId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal QuotaInLiters { get; set; }
        public decimal UsedLiters { get; set; } = 0;
    }
}
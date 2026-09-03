using System.ComponentModel.DataAnnotations;

namespace SmartCard.DTOs
{
    public class ConsumptionCreateDto
    {
        public int EmployeeId { get; set; }
        public int CardId { get; set; }
        public decimal AmountInLiters { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        [StringLength(200)]
        public string? Description { get; set; }
    }
}
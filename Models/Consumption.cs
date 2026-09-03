using System.ComponentModel.DataAnnotations;

namespace SmartCard.Models
{
    public class Consumption
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        // Navigation property
        public virtual Employee Employee { get; set; } = null!;

        public int CardId { get; set; }

        // Navigation property
        public virtual Card Card { get; set; } = null!;

        public decimal AmountInLiters { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [StringLength(200)]
        public string? Description { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
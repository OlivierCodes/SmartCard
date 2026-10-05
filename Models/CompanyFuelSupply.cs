using System.ComponentModel.DataAnnotations;

namespace SmartCard.Models
{
    public class CompanyFuelSupply
    {
        public int Id { get; set; }

        [Required]
        [Range(0.01, 10000000, ErrorMessage = "La quantité doit être supérieure à 0")]
        public decimal QuantityInLiters { get; set; }

        public DateTime SupplyDate { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? Supplier { get; set; }

        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}

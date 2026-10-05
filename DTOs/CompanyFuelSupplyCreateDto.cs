using System.ComponentModel.DataAnnotations;

namespace SmartCard.DTOs
{
    public class CompanyFuelSupplyCreateDto
    {
        [Required(ErrorMessage = "La quantité est requise")]
        [Range(0.01, 10000000, ErrorMessage = "La quantité doit être supérieure à 0")]
        public decimal QuantityInLiters { get; set; }

        public DateTime? SupplyDate { get; set; }

        [StringLength(100, ErrorMessage = "Le nom du fournisseur ne peut pas dépasser 100 caractères")]
        public string? Supplier { get; set; }

        [StringLength(100, ErrorMessage = "Le numéro de référence ne peut pas dépasser 100 caractères")]
        public string? ReferenceNumber { get; set; }

        [StringLength(500, ErrorMessage = "Les notes ne peuvent pas dépasser 500 caractères")]
        public string? Notes { get; set; }
    }
}

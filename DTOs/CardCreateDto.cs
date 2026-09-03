using System.ComponentModel.DataAnnotations;

namespace SmartCard.DTOs
{
    public class CardCreateDto
    {
        [Required]
        [StringLength(50)]
        public string CardNumber { get; set; } = string.Empty;

        [Required]
        public CardStatusDto Status { get; set; }

        public int? EmployeeId { get; set; }
    }
}
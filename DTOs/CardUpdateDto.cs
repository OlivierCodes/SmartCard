using System.ComponentModel.DataAnnotations;

namespace SmartCard.DTOs
{
    public class CardUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string CardNumber { get; set; } = string.Empty;

        [Required]
        public CardStatusDto Status { get; set; }

        public int? EmployeeId { get; set; }
    }
}
using SmartCard.Models;

namespace SmartCard.DTOs
{
    public class CardResponseDto
    {
        public int Id { get; set; }

        public string CardNumber { get; set; } = string.Empty;

        public CardStatusDto Status { get; set; }

        public EmployeeResponseDto? Employee { get; set; }

        public int? EmployeeId { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? LastUsedDate { get; set; }
    }
}
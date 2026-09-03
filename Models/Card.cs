using System.ComponentModel.DataAnnotations;

namespace SmartCard.Models
{
    public enum CardStatus
    {
        Active = 0,
        Inactive = 1,
        Suspended = 2,
        Lost = 3
    }

    public class Card
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string CardNumber { get; set; } = string.Empty;

        public CardStatus Status { get; set; } = CardStatus.Active;

        public int? EmployeeId { get; set; }

        // Navigation property
        public virtual Employee? Employee { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? LastUsedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        
        // Navigation property
        public virtual ICollection<Consumption> Consumptions { get; set; } = new List<Consumption>();
    }
}
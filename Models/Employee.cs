using System.ComponentModel.DataAnnotations;

namespace SmartCard.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string EmployeeNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Email { get; set; }

        public int? DepartmentId { get; set; }

        // Navigation property
        public virtual Department? Department { get; set; }

        public decimal MonthlyFuelQuota { get; set; }
        
        // Navigation properties
        public virtual ICollection<Card> Cards { get; set; } = new List<Card>();
        public virtual ICollection<FuelQuota> FuelQuotas { get; set; } = new List<FuelQuota>();
        public virtual ICollection<Consumption> Consumptions { get; set; } = new List<Consumption>();
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
using System.ComponentModel.DataAnnotations;

namespace SmartCard.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        // Navigation property for employees in this department
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
        
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
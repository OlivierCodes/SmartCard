using System.ComponentModel.DataAnnotations;

namespace SmartCard.DTOs
{
    public class EmployeeCreateDto
    {
        [StringLength(50)]
        public string? EmployeeNumber { get; set; }
        
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? Email { get; set; }
        
        public int? DepartmentId { get; set; }
        
        public decimal MonthlyFuelQuota { get; set; }
    }
}
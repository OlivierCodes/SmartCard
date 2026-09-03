using System.ComponentModel.DataAnnotations;

namespace SmartCard.DTOs
{
    public class EmployeeResponseDto
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

        [StringLength(100)]
        public string? Department { get; set; }

        public int? DepartmentId { get; set; }

        public decimal MonthlyFuelQuota { get; set; }

        public decimal AvailableFuelQuota { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public bool IsActive { get; set; }

        public List<CardResponseDto> Cards { get; set; } = new List<CardResponseDto>();
    }
}
using System.ComponentModel.DataAnnotations;

namespace SmartCard.DTOs
{
    public class DepartmentCreateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
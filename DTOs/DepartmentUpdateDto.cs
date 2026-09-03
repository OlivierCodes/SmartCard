using System.ComponentModel.DataAnnotations;

namespace SmartCard.DTOs
{
    public class DepartmentUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
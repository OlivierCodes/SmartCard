namespace SmartCard.DTOs
{
    public class CardEmployeeAssignmentDto
    {
        public int CardId { get; set; }
        public int? EmployeeId { get; set; } // null pour désaffecter
    }
}
using System.ComponentModel.DataAnnotations;

namespace SmartCard.DTOs
{
    public class MobileConsumptionHistoryDto
    {
        public int Id { get; set; }
        
        public int EmployeeId { get; set; }
        
        public string EmployeeName { get; set; } = string.Empty;
        
        public string EmployeeNumber { get; set; } = string.Empty;
        
        public int CardId { get; set; }
        
        public string CardNumber { get; set; } = string.Empty;
        
        public decimal AmountInLiters { get; set; }
        
        public DateTime TransactionDate { get; set; }
        
        public string? Description { get; set; }
        
        public DateTime CreatedDate { get; set; }
    }
}
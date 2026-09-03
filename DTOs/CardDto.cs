namespace SmartCard.DTOs
{
    public class CardValidationRequestDto
    {
        public string CardNumber { get; set; } = string.Empty;
    }
    
    public class CardValidationResponseDto
    {
        public bool IsValid { get; set; }
        public bool HasSufficientFuel { get; set; }
        public decimal AvailableFuel { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }
    
    public class FuelDeductionRequestDto
    {
        public string CardNumber { get; set; } = string.Empty;
        public decimal AmountInLiters { get; set; }
        public string? Description { get; set; }
    }
    
    public class FuelDeductionResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public decimal RemainingFuel { get; set; }
        public DateTime TransactionDate { get; set; }
    }
    
    public class TransactionSyncRequestDto
    {
        public string CardNumber { get; set; } = string.Empty;
        public decimal AmountInLiters { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Description { get; set; }
    }
    
    public class TransactionSyncResponseDto
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int TransactionId { get; set; }
    }
}
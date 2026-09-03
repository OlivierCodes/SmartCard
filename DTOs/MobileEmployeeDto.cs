namespace SmartCard.DTOs
{
    public class MobileEmployeeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public double AvailableQuota { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }
}
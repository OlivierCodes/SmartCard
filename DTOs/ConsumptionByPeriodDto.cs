namespace SmartCard.DTOs
{
    public class ConsumptionByPeriodDto
    {
        public string Period { get; set; } = string.Empty;
        public decimal TotalConsumption { get; set; }
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
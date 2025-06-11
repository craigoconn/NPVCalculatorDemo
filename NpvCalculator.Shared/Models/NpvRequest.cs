namespace NPVCalculator.Shared.Models
{
    public class NpvRequest
    {
        public List<decimal> CashFlows { get; set; } = new();
        public decimal LowerBoundRate { get; set; }
        public decimal UpperBoundRate { get; set; }
        public decimal RateIncrement { get; set; }
    }
}
using NPVCalculator.Shared.Models;

namespace NPVCalculator.Client.Models
{
    public class NpvInputModel
    {
        public string CashFlowsInput { get; set; } = "-1000,300,400,500";
        public decimal LowerBoundRate { get; set; } = 1.00m;
        public decimal UpperBoundRate { get; set; } = 15.00m;
        public decimal RateIncrement { get; set; } = 0.25m;
        public bool HasCashFlowError { get; set; }

        public NpvRequest ToNpvRequest()
        {
            var cashFlows = CashFlowsInput
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => decimal.Parse(s.Trim()))
                .ToList();

            return new NpvRequest
            {
                CashFlows = cashFlows,
                LowerBoundRate = LowerBoundRate,
                UpperBoundRate = UpperBoundRate,
                RateIncrement = RateIncrement
            };
        }
    }
}
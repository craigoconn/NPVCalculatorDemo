using NPVCalculator.Domain.Interfaces;

namespace NPVCalculator.Domain.Services
{
    public class NpvDomainService : INpvDomainService
    {
        public decimal CalculateNpv(IList<decimal> cashFlows, decimal discountRate)
        {
            if (cashFlows == null || !cashFlows.Any())
                throw new ArgumentException("Cash flows cannot be null or empty", nameof(cashFlows));

            decimal npv = 0;
            for (int period = 0; period < cashFlows.Count; period++)
            {
                var discountFactor = CalculateDiscountFactor(discountRate, period);
                npv += cashFlows[period] * discountFactor;
            }

            return Math.Round(npv, 2);
        }

        private static decimal CalculateDiscountFactor(decimal discountRate, int period)
        {
            if (period == 0)
                return 1m;

            var factor = 1.0 / Math.Pow((double)(1 + discountRate), period);
            return (decimal)factor;
        }
    }
}
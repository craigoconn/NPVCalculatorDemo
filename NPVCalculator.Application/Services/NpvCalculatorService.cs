using NPVCalculator.Domain.Interfaces;
using NPVCalculator.Shared.Models;
using Microsoft.Extensions.Logging;

namespace NPVCalculator.Application.Services
{
    public class NpvCalculatorService : INpvCalculator
    {
        private readonly ILogger<NpvCalculatorService> _logger;

        public NpvCalculatorService(ILogger<NpvCalculatorService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public decimal CalculateSingleNpv(IList<decimal> cashFlows, decimal discountRate)
        {
            if (cashFlows == null || !cashFlows.Any())
                throw new ArgumentException("Cash flows cannot be null or empty", nameof(cashFlows));

            decimal npv = 0;
            for (int t = 0; t < cashFlows.Count; t++)
            {
                var discountFactor = CalculateDiscountFactor(discountRate, t);
                npv += cashFlows[t] * discountFactor;
            }

            return Math.Round(npv, 2);
        }

        public async Task<IEnumerable<NpvResult>> CalculateAsync(NpvRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var results = new List<NpvResult>();
            var rates = GenerateDiscountRates(request).ToList();

            _logger.LogInformation("Starting NPV calculation for {Count} rates", rates.Count);

            await Task.Run(async () =>
            {
                for (int i = 0; i < rates.Count; i++)
                {
                    var rate = rates[i];
                    var npv = CalculateSingleNpv(request.CashFlows, rate / 100);

                    results.Add(new NpvResult
                    {
                        Rate = Math.Round(rate, 2),
                        Value = npv
                    });

                    if (i % 10 == 0)
                        await Task.Yield();
                }
            });

            _logger.LogInformation("NPV calculation completed with {Count} results", results.Count);
            return results;
        }

        public IEnumerable<NpvResult> Calculate(NpvRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var rates = GenerateDiscountRates(request);
            var results = new List<NpvResult>();

            foreach (var rate in rates)
            {
                var npv = CalculateSingleNpv(request.CashFlows, rate / 100);
                results.Add(new NpvResult
                {
                    Rate = Math.Round(rate, 2),
                    Value = npv
                });
            }

            return results;
        }

        private IEnumerable<decimal> GenerateDiscountRates(NpvRequest request)
        {
            var totalIterations = CalculateNumberOfIterations(request);

            for (int i = 0; i < totalIterations; i++)
            {
                var rate = request.LowerBoundRate + (request.RateIncrement * i);
                if (rate > request.UpperBoundRate + 0.001m)
                    yield break;

                yield return rate;
            }
        }

        private static decimal CalculateDiscountFactor(decimal discountRate, int period)
        {
            if (period == 0)
                return 1m;

            var factor = 1.0 / Math.Pow((double)(1 + discountRate), period);
            return (decimal)factor;
        }

        private static int CalculateNumberOfIterations(NpvRequest request)
        {
            return (int)Math.Ceiling((request.UpperBoundRate - request.LowerBoundRate) / request.RateIncrement) + 1;
        }
    }
}
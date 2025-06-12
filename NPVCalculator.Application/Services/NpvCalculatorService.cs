using NPVCalculator.Domain.Interfaces;
using NPVCalculator.Shared.Models;
using Microsoft.Extensions.Logging;

namespace NPVCalculator.Application.Services
{
    /// <summary>
    /// Contains logic for NPV calculation
    /// </summary>
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

            // Calculate NPV using the standard formula: NPV = Σ[CF_t / (1 + r)^t]
            for (int t = 0; t < cashFlows.Count; t++)
            {
                var discountFactor = CalculateDiscountFactor(discountRate, t);
                npv += cashFlows[t] * discountFactor;

                _logger.LogTrace("Period {Period}: CF={CashFlow}, Discount Factor={DiscountFactor}, PV={PresentValue}",
                    t, cashFlows[t], discountFactor, cashFlows[t] * discountFactor);
            }

            var roundedNpv = Math.Round(npv, 2);
            _logger.LogDebug("Calculated NPV: {NPV} for discount rate: {Rate}%", roundedNpv, discountRate * 100);

            return roundedNpv;
        }

        public async Task<IEnumerable<NpvResult>> CalculateAsync(NpvRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var results = new List<NpvResult>();
            var totalCalculations = CalculateNumberOfIterations(request);

            _logger.LogInformation("Starting async NPV calculation for {TotalCalculations} discount rates", totalCalculations);

            // True async implementation with periodic yielding
            await Task.Run(async () =>
            {
                var iterationCount = 0;

                for (decimal rate = request.LowerBoundRate; rate <= request.UpperBoundRate; rate += request.RateIncrement)
                {
                    iterationCount++;
                    var discountRateDecimal = rate / 100;
                    var npv = CalculateSingleNpv(request.CashFlows, discountRateDecimal);

                    results.Add(new NpvResult
                    {
                        Rate = Math.Round(rate, 2),
                        Value = npv
                    });

                    // Yield control every 10 calculations to prevent UI blocking
                    if (iterationCount % 10 == 0)
                    {
                        await Task.Yield();
                    }

                    if (totalCalculations > 100 && iterationCount % (totalCalculations / 10) == 0)
                    {
                        _logger.LogInformation("NPV calculation progress: {Completed}/{Total} ({Percentage:F1}%)",
                            iterationCount, totalCalculations, (double)iterationCount / totalCalculations * 100);
                    }
                }
            });

            _logger.LogInformation("Completed async NPV calculation with {ResultCount} results", results.Count);
            return results;
        }

        public IEnumerable<NpvResult> Calculate(NpvRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var results = new List<NpvResult>();
            var totalIterations = CalculateNumberOfIterations(request);

            _logger.LogInformation("Starting NPV calculation for range {LowerBound}% to {UpperBound}% with increment {Increment}%",
                request.LowerBoundRate, request.UpperBoundRate, request.RateIncrement);

            // Use integer-based iteration to avoid floating point precision issues
            for (int i = 0; i < totalIterations; i++)
            {
                var rate = request.LowerBoundRate + (request.RateIncrement * i);

                // Ensure we don't exceed upper bound due to precision
                if (rate > request.UpperBoundRate + 0.001m) break;

                var discountRateDecimal = rate / 100;
                var npv = CalculateSingleNpv(request.CashFlows, discountRateDecimal);

                results.Add(new NpvResult
                {
                    Rate = Math.Round(rate, 2),
                    Value = npv
                });

                if (totalIterations > 100 && (i + 1) % (totalIterations / 10) == 0)
                {
                    _logger.LogInformation("NPV calculation progress: {Completed}/{Total} ({Percentage:F1}%)",
                        i + 1, totalIterations, (double)(i + 1) / totalIterations * 100);
                }
            }

            _logger.LogInformation("NPV calculation completed. Generated {ResultCount} results", results.Count);
            return results;
        }

        /// <summary>
        /// CalculateDiscountFactor
        /// </summary>
        private static decimal CalculateDiscountFactor(decimal discountRate, int period)
        {
            if (period == 0)
                return 1m;

            // Use double for Math.Pow then convert back to decimal for precision
            var factor = 1.0 / Math.Pow((double)(1 + discountRate), period);
            return (decimal)factor;
        }

        /// <summary>
        /// CalculateNumberOfIterations
        /// </summary>
        private static int CalculateNumberOfIterations(NpvRequest request)
        {
            return (int)Math.Ceiling((request.UpperBoundRate - request.LowerBoundRate) / request.RateIncrement) + 1;
        }
    }
}
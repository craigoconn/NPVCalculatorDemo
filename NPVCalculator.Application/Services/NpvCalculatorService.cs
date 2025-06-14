using NPVCalculator.Domain.Interfaces;
using NPVCalculator.Shared.Models;
using Microsoft.Extensions.Logging;

namespace NPVCalculator.Application.Services
{
    public class NpvCalculatorService : INpvCalculator
    {
        private readonly INpvDomainService _npvDomainService;
        private readonly ILogger<NpvCalculatorService> _logger;

        public NpvCalculatorService(INpvDomainService npvDomainService, ILogger<NpvCalculatorService> logger)
        {
            _npvDomainService = npvDomainService ?? throw new ArgumentNullException(nameof(npvDomainService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public decimal CalculateSingleNpv(IList<decimal> cashFlows, decimal discountRate)
        {
            // Delegate to domain service
            return _npvDomainService.CalculateNpv(cashFlows, discountRate);
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

                    // Use domain service for calculation
                    var npv = _npvDomainService.CalculateNpv(request.CashFlows, rate / 100);

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
                // Use domain service for calculation
                var npv = _npvDomainService.CalculateNpv(request.CashFlows, rate / 100);

                results.Add(new NpvResult
                {
                    Rate = Math.Round(rate, 2),
                    Value = npv
                });
            }

            return results;
        }

        private static IEnumerable<decimal> GenerateDiscountRates(NpvRequest request)
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

        private static int CalculateNumberOfIterations(NpvRequest request)
        {
            return (int)Math.Ceiling((request.UpperBoundRate - request.LowerBoundRate) / request.RateIncrement) + 1;
        }
    }
}
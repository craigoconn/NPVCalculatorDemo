using Microsoft.Extensions.Logging;
using NPVCalculator.Domain.Interfaces;
using NPVCalculator.Shared.Models;

namespace NPVCalculator.Application.Services
{
    public class NpvCalculatorService : INpvCalculatorService
    {
        private readonly INpvDomainService _npvDomainService;
        private readonly ILogger<NpvCalculatorService> _logger;

        public NpvCalculatorService(INpvDomainService npvDomainService, ILogger<NpvCalculatorService> logger)
        {
            _npvDomainService = npvDomainService ?? throw new ArgumentNullException(nameof(npvDomainService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<IEnumerable<NpvResult>> CalculateAsync(NpvRequest request, CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var results = new List<NpvResult>();
            var rates = GenerateDiscountRates(request).ToList();

            _logger.LogInformation("Starting NPV calculation for {Count} rates", rates.Count);

            for (int i = 0; i < rates.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var rate = rates[i];
                var npv = _npvDomainService.CalculateNpv(request.CashFlows, rate / 100);
                results.Add(new NpvResult
                {
                    Rate = Math.Round(rate, 2),
                    Value = npv
                });

                // Yield control back to the scheduler every 5 calculations
                if (i % 5 == 0)
                    await Task.Yield();
            }

            _logger.LogInformation("NPV calculation completed with {Count} results", results.Count);
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
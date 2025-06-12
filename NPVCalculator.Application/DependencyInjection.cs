using Microsoft.Extensions.DependencyInjection;
using NPVCalculator.Domain.Interfaces;
using NPVCalculator.Application.Services;
using Microsoft.Extensions.Configuration;

namespace NPVCalculator.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services,
            IConfiguration configuration)  // ← This parameter needs to match
        {
            // Application services
            services.AddScoped<INpvCalculator, NpvCalculatorService>();
            services.AddScoped<IValidationService, ValidationService>();

            return services;
        }
    }
}
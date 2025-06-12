using Microsoft.Extensions.DependencyInjection;
using NPVCalculator.Domain.Interfaces;
using NPVCalculator.Application.Services;

namespace NPVCalculator.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<INpvCalculator, NpvCalculatorService>();
            services.AddScoped<IValidationService, ValidationService>();

            return services;
        }
    }
}
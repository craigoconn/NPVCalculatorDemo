using Microsoft.Extensions.DependencyInjection;
using NPVCalculator.Domain.Interfaces;
using NPVCalculator.Application.Services;
using NPVCalculator.Domain.Services;

namespace NPVCalculator.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<INpvDomainService, NpvDomainService>();
            services.AddScoped<INpvCalculator, NpvCalculatorService>();
            services.AddScoped<IValidationService, ValidationService>();

            return services;
        }
    }
}
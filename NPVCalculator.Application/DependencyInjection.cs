using Microsoft.Extensions.DependencyInjection;
using NPVCalculator.Application.Interfaces;
using NPVCalculator.Application.Services;
using NPVCalculator.Domain.Interfaces;
using NPVCalculator.Domain.Services;

namespace NPVCalculator.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Domain services
            services.AddScoped<INpvDomainService, NpvDomainService>();
            services.AddScoped<INpvCalculator, NpvCalculatorService>(); 
            services.AddScoped<IValidationService, ValidationService>();

            // Application services  
            services.AddScoped<INpvApplicationService, NpvApplicationService>();
            return services;
        }
    }
}
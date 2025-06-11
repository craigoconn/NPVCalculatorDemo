using Microsoft.Extensions.DependencyInjection;
using NPVCalculator.Domain.Interfaces;
using NPVCalculator.Application.Services;

namespace NPVCalculator.Infrastructure
{
    /// <summary>
    /// Dependency injection configuration
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Register all application services
        /// </summary>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Register application services (business logic)
            services.AddScoped<INpvCalculator, NpvCalculatorService>();
            services.AddScoped<IValidationService, ValidationService>();

            // Actual infrastructure services (E.g. repositories), go here:
            // services.AddScoped<IRepository, DatabaseRepository>();
            // services.AddScoped<IFileService, FileSystemService>();

            return services;
        }
    }
}
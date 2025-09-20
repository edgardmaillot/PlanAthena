// PlanAthena.Core.Infrastructure.DependencyInjection.cs
using Microsoft.Extensions.DependencyInjection;
using PlanAthena.Core.Application.Interfaces; // Pour ICalendrierService
using PlanAthena.Core.Application.Services;
using PlanAthena.Core.Infrastructure.Services;  // Pour CalendrierService

namespace PlanAthena.Core.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddTransient<ICalendrierService, CalendrierService>();
            services.AddScoped<ICapacityValidationService, CapacityValidationService>();
            services.AddScoped<ICrossReferenceValidationService, CrossReferenceValidationService>();
            return services;
        }
    }
}
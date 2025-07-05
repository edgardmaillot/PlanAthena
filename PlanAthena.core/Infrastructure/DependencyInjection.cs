// PlanAthena.Core.Infrastructure.DependencyInjection.cs
using Microsoft.Extensions.DependencyInjection;
using PlanAthena.Core.Application.Interfaces; // Pour ICalendrierService
using PlanAthena.Core.Infrastructure.Services;  // Pour CalendrierService

namespace PlanAthena.Core.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddTransient<ICalendrierService, CalendrierService>();
            // Enregistrer d'autres services d'infrastructure ici à l'avenir
            return services;
        }
    }
}
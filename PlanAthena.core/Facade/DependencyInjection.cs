using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using PlanAthena.Core.Application.Interfaces;
using PlanAthena.Core.Application.Services;
using PlanAthena.Core.Application.UseCases; // Assurez-vous que ce using est présent
using PlanAthena.Core.Application.Validation;

namespace PlanAthena.Core.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Enregistrement des validateurs
        services.AddValidatorsFromAssemblyContaining<ChantierSetupInputDtoValidator>();

        // Enregistrement des services de la couche Application
        services.AddScoped<IChantierSetupInputMapper, ChantierSetupInputMapper>();
        services.AddScoped<IChantierValidationService, ChantierValidationService>();
        services.AddScoped<IInitialFeasibilityAnalysisService, InitialFeasibilityAnalysisService>();
        services.AddScoped<IKeyResourceSuggestionService, KeyResourceSuggestionService>();

        // CORRECTION : Enregistrer le nouveau cas d'usage et supprimer les anciens
        services.AddScoped<IProcessChantierUseCase, ProcessChantierUseCase>();

        // SUPPRIMER les lignes suivantes si elles existent :
        // services.AddScoped<IInitializeAndAnalyzeChantierUseCase, InitializeAndAnalyzeChantierUseCase>();
        // services.AddScoped<IOptimizePlanningUseCase, OptimizePlanningUseCase>();

        return services;
    }
}
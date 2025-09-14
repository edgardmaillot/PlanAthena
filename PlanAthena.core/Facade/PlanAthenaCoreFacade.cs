using PlanAthena.Core.Application.Interfaces;
using PlanAthena.Core.Facade.Dto.Input;
using PlanAthena.Core.Facade.Dto.Output;

namespace PlanAthena.Core.Facade;

/// <summary>
/// Point d'entrée unique pour la bibliothèque PlanAthena.core.
/// Orchestre les cas d'usage de l'application.
/// </summary>
public class PlanAthenaCoreFacade
{
    private readonly IProcessChantierUseCase _processChantierUseCase;

    public PlanAthenaCoreFacade(IProcessChantierUseCase processChantierUseCase)
    {
        _processChantierUseCase = processChantierUseCase;
    }

    /// <summary>
    /// Traite une demande complète de chantier, de la validation à l'optimisation.
    /// </summary>
    /// <param name="inputDto">Les données complètes du chantier et la configuration de l'action souhaitée.</param>
    /// <returns>Un résultat contenant soit des erreurs, soit une analyse, soit un planning optimisé.</returns>
    public virtual async Task<ProcessChantierResultDto> ProcessChantierAsync(ChantierSetupInputDto inputDto)
    {
        return await _processChantierUseCase.ExecuteAsync(inputDto);
    }
}
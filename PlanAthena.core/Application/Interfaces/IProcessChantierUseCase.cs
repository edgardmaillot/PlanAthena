using PlanAthena.Core.Facade.Dto.Input;
using PlanAthena.Core.Facade.Dto.Output;
using System.Threading.Tasks;

namespace PlanAthena.Core.Application.Interfaces;

/// <summary>
/// Définit le cas d'usage principal qui traite une demande de chantier,
/// de la validation à l'optimisation.
/// </summary>
public interface IProcessChantierUseCase
{
    Task<ProcessChantierResultDto> ExecuteAsync(ChantierSetupInputDto inputDto);
}
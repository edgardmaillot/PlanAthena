using PlanAthena.Core.Facade.Dto.Enums;

namespace PlanAthena.Core.Facade.Dto.Output;

/// <summary>
/// Représente le résultat d'une tentative d'optimisation de planning.
/// </summary>
public record PlanningOptimizationResultDto
{
    public string ChantierId { get; init; }

    /// <summary>
    /// Le statut final retourné par le solveur.
    /// </summary>
    public OptimizationStatus Status { get; init; }

    /// <summary>
    /// Le coût total estimé du planning optimisé, si une solution a été trouvée.
    /// </summary>
    public long? CoutTotalEstime { get; init; }

    /// <summary>
    /// La durée totale du planning en jours ouvrés, si une solution a été trouvée.
    /// </summary>
    public long? DureeTotaleJoursOuvres { get; init; }
}
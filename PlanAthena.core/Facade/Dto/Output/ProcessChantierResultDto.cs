namespace PlanAthena.Core.Facade.Dto.Output;

/// <summary>
/// DTO de sortie générique pour le traitement d'un chantier.
/// Contient soit un résultat d'analyse, soit un résultat d'optimisation.
/// </summary>
public record ProcessChantierResultDto
{
    // On reprend les champs communs de ChantierSetupAnalysisResultDto
    public string ChantierId { get; init; }
    public PlanAthena.Core.Facade.Dto.Enums.EtatTraitementInput Etat { get; init; }
    public System.Collections.Generic.IReadOnlyList<MessageValidationDto> Messages { get; init; } = System.Array.Empty<MessageValidationDto>();

    // Contient le résultat si le but était une analyse
    public AnalyseRessourcesResultatDto? AnalyseResultat { get; init; }

    // Contient le résultat si le but était une optimisation
    public PlanningOptimizationResultDto? OptimisationResultat { get; init; }
}
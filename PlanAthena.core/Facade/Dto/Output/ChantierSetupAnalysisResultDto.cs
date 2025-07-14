// PlanAthena.Core.Facade.Dto.Output.ChantierSetupAnalysisResultDto.cs
using PlanAthena.Core.Facade.Dto.Enums; // Pour EtatTraitementInput

namespace PlanAthena.Core.Facade.Dto.Output
{
    public record ChantierSetupAnalysisResultDto
    {
        public required string ChantierId { get; init; } // Rappel de l'ID du chantier traité
        public required EtatTraitementInput Etat { get; init; }
        public DateTime DateAnalyse { get; init; } = DateTime.UtcNow;

        public IReadOnlyList<MessageValidationDto> Messages { get; init; } = Array.Empty<MessageValidationDto>();

        // Contient les résultats de l'analyse si l'état n'est pas EchecValidation.
        // Peut être null si aucune analyse pertinente n'a pu être faite ou si échec précoce.
        public AnalyseRessourcesResultatDto? AnalyseRessources { get; init; }
    }
}
// Fichier : Facade/Dto/Output/ProcessChantierResultDto.cs

using System;
using System.Collections.Generic;
using PlanAthena.Core.Facade.Dto.Enums;

namespace PlanAthena.Core.Facade.Dto.Output
{
    public record ProcessChantierResultDto
    {
        public string ChantierId { get; init; }
        public EtatTraitementInput Etat { get; init; }
        public IReadOnlyList<MessageValidationDto> Messages { get; init; } = Array.Empty<MessageValidationDto>();

        // --- MODIFICATIONS ---
        // Renommé pour clarifier qu'il s'agit de l'analyse statique initiale
        public AnalyseRessourcesResultatDto? AnalyseStatiqueResultat { get; init; }

        public PlanningOptimizationResultDto? OptimisationResultat { get; init; }

        // Ajout du nouveau rapport d'analyse post-optimisation
        public PlanningAnalysisReportDto? AnalysePostOptimisationResultat { get; init; }
    }
}
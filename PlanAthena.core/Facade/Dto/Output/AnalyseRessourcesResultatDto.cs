// PlanAthena.Core.Facade.Dto.Output.AnalyseRessourcesResultatDto.cs
using System; // Pour Array.Empty
using System.Collections.Generic;

namespace PlanAthena.Core.Facade.Dto.Output
{
    // DTO simplifié pour les résultats de l'analyse P5/P6 pour le MVP
    public record AnalyseRessourcesResultatDto
    {
        // Liste des OuvrierId suggérés comme ressources clés.
        public IReadOnlyList<string> OuvriersClesSuggereIds { get; init; } = Array.Empty<string>();

        // D'autres résultats d'analyse simples pourraient être ajoutés ici à l'avenir.
    }
}
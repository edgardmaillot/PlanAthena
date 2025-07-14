// PlanAthena.Core.Facade.Dto.Output.AnalyseRessourcesResultatDto.cs
namespace PlanAthena.Core.Facade.Dto.Output
{
    // DTO simplifié pour les résultats de l'analyse P5/P6 pour le MVP
    public record AnalyseRessourcesResultatDto
    {
        public IReadOnlyList<string> OuvriersClesSuggereIds { get; init; } = Array.Empty<string>();

        // --- NOUVELLES PROPRIÉTÉS POUR L'ESTIMATION ---

        /// <summary>
        /// Le coût total estimé (en centimes) basé sur une première solution faisable.
        /// Peut être null si aucune solution n'a pu être trouvée rapidement.
        /// </summary>
        public long? CoutTotalEstime { get; init; }

        /// <summary>
        /// La durée totale estimée en heures (slots d'1h) basée sur une première solution faisable.
        /// Peut être null si aucune solution n'a pu être trouvée rapidement.
        /// </summary>
        public long? DureeTotaleEstimeeEnSlots { get; init; }
    }
}
// Emplacement: /Services/Business/DTOs/PlanificationRunResult.cs

using PlanAthena.Core.Facade.Dto.Output;

namespace PlanAthena.Services.Business.DTOs
{
    /// <summary>
    /// DTO qui encapsule tous les résultats d'un cycle de planification exécuté par l'Orchestrateur.
    /// Il est destiné à être retourné à la couche de présentation (UI).
    /// </summary>
    public record PlanificationRunResult
    {
        /// <summary>
        /// Le résultat brut retourné par la DLL. Utile pour les logs et le débogage.
        /// </summary>
        public ProcessChantierResultDto RawResult { get; init; }

        /// <summary>
        /// Le rapport d'analyse complet, généré uniquement après un run d'optimisation (coût ou délai).
        /// Sera null après une analyse rapide.
        /// </summary>
        public AnalysisReport? AnalysisReport { get; init; }

        /// <summary>
        /// Le rapport sur la tension des métiers, généré uniquement après une analyse rapide.
        /// Sera null après un run d'optimisation.
        /// </summary>
        public MetierTensionReport? MetierTensionReport { get; init; }
    }
}
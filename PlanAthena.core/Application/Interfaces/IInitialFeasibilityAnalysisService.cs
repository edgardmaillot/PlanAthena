// PlanAthena.Core.Application.Interfaces.IInitialFeasibilityAnalysisService.cs 
// (ou PlanAthena.Core.Domain.Services.IInitialFeasibilityAnalysisService.cs)
using System.Collections.Generic;
using System.Threading.Tasks;
using PlanAthena.Core.Domain; // Pour l'entité Chantier
using PlanAthena.Core.Facade.Dto.Output; // Pour MessageValidationDto

namespace PlanAthena.Core.Application.Interfaces // Ou PlanAthena.Core.Domain.Services
{
    /// <summary>
    /// Effectue une analyse préliminaire de faisabilité sur un chantier validé.
    /// Identifie les alertes majeures (ex: couverture des métiers, déséquilibres charge/capacité initiaux).
    /// </summary>
    public interface IInitialFeasibilityAnalysisService
    {
        /// <summary>
        /// Analyse la faisabilité préliminaire du chantier.
        /// </summary>
        /// <param name="chantier">L'agrégat Chantier du domaine, supposé valide et complet.</param>
        /// <returns>Une liste de messages de validation (typiquement des Avertissements) 
        ///          concernant la faisabilité.</returns>
        Task<List<MessageValidationDto>> AnalyserFaisabiliteAsync(Chantier chantier);
    }
}
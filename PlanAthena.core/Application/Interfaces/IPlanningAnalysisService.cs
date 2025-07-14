// Fichier : Application/Interfaces/IPlanningAnalysisService.cs

using PlanAthena.Core.Domain;
using PlanAthena.Core.Facade.Dto.Output;

namespace PlanAthena.Core.Application.Interfaces
{
    public interface IPlanningAnalysisService
    {
        /// <summary>
        /// Analyse un planning d'affectations pour en extraire des KPIs métier.
        /// </summary>
        /// <param name="affectations">La liste des affectations du planning optimisé.</param>
        /// <param name="chantierDeReference">L'objet domaine du chantier pour les données de référence (coûts, etc.).</param>
        /// <returns>Un rapport d'analyse complet contenant les KPIs.</returns>
        Task<PlanningAnalysisReportDto> AnalyserLePlanningAsync(
            IReadOnlyList<AffectationDto> affectations,
            Chantier chantierDeReference);
    }
}
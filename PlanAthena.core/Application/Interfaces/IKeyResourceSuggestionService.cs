// PlanAthena.Core.Application.Interfaces.IKeyResourceSuggestionService.cs
// (ou PlanAthena.Core.Domain.Services.IKeyResourceSuggestionService.cs)
using System.Collections.Generic;
using System.Threading.Tasks;
using PlanAthena.Core.Domain; // Pour l'entité Chantier
using PlanAthena.Core.Domain.ValueObjects; // Pour OuvrierId

namespace PlanAthena.Core.Application.Interfaces // Ou PlanAthena.Core.Domain.Services
{
    /// <summary>
    /// Analyse le chantier pour suggérer des ressources (ouvriers) clés
    /// si non entièrement fournies ou pour valider celles fournies par le CdC.
    /// </summary>
    public interface IKeyResourceSuggestionService
    {
        /// <summary>
        /// Suggère des ouvriers clés pour le chantier.
        /// </summary>
        /// <param name="chantier">L'agrégat Chantier du domaine, supposé valide et complet.</param>
        /// <returns>Une liste d'OuvrierId des ouvriers suggérés comme clés.</returns>
        Task<List<OuvrierId>> SuggererOuvriersClesAsync(Chantier chantier);
    }
}
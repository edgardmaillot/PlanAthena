// Fichier : Application/Interfaces/ISolutionInterpreterService.cs

using Google.OrTools.Sat;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Facade.Dto.Output;
using System.Collections.Generic;

namespace PlanAthena.Core.Application.Interfaces
{
    public interface ISolutionInterpreterService
    {
        /// <summary>
        /// Traduit la solution brute du solveur CP-SAT en une liste d'affectations lisibles.
        /// </summary>
        /// <param name="solver">Le solveur après l'appel à Solve().</param>
        /// <param name="modeleCpSat">L'objet contenant les variables du modèle (intervalles, assignations).</param>
        /// <param name="probleme">Les données de référence du problème (chantier, échelle de temps).</param>
        /// <returns>Une liste d'objets AffectationDto représentant le planning.</returns>
        IReadOnlyList<AffectationDto> InterpreterLaSolution(
            CpSolver solver,
            ModeleCpSat modeleCpSat,
            ProblemeOptimisation probleme);
    }
}
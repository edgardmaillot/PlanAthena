using Google.OrTools.Sat;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Application.Interfaces;

namespace PlanAthena.Core.Infrastructure.Services.OrTools;

/// <summary>
/// Orchestrateur qui délègue la construction du modèle à des builders spécialisés.
/// </summary>
public class ConstructeurProblemeOrTools : IConstructeurProblemeOrTools
{
    public ModeleCpSat ConstruireModele(ProblemeOptimisation probleme)
    {
        var model = new CpModel();
        var tacheBuilder = new TacheModelBuilder();
        var coutBuilder = new CoutModelBuilder();

        var (tachesIntervals, tachesAssignables, makespan) = tacheBuilder.Construire(model, probleme);

        // On passe maintenant le makespan au CoutModelBuilder
        var coutTotal = coutBuilder.Construire(model, probleme, tachesIntervals, tachesAssignables, makespan);

        model.Minimize(coutTotal);

        return new ModeleCpSat
        {
            Model = model,
            TachesIntervals = tachesIntervals,
            TachesAssignables = tachesAssignables,
            Makespan = makespan,
            CoutTotal = coutTotal
        };
    }
}
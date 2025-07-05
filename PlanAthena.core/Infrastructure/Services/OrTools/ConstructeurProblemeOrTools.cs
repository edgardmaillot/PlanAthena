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

        // 1. Construire les variables et contraintes liées aux tâches
        // On récupère bien les 3 valeurs de retour, y compris le makespan
        var (tachesIntervals, tachesAssignables, makespan) = tacheBuilder.Construire(model, probleme);

        // 2. Construire les variables et contraintes liées aux coûts
        var coutTotal = coutBuilder.Construire(model, probleme, tachesIntervals, tachesAssignables);

        // 3. Définir l'objectif final du modèle
        //model.Minimize(coutTotal);
        model.Minimize(makespan); //Si on veut réduire le makespan à la place du coût total

        // 4. CORRECTION FINALE : On assigne bien TOUTES les propriétés au DTO de retour.
        return new ModeleCpSat
        {
            Model = model,
            TachesIntervals = tachesIntervals,
            TachesAssignables = tachesAssignables,
            Makespan = makespan,   // Cette ligne manquait
            CoutTotal = coutTotal    // Cette ligne manquait
        };
    }
}
// Fichier : Infrastructure/Services/OrTools/ConstructeurProblemeOrTools.cs

using Google.OrTools.Sat;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Application.Interfaces;

namespace PlanAthena.Core.Infrastructure.Services.OrTools
{
    public class ConstructeurProblemeOrTools : IConstructeurProblemeOrTools
    {
        // On modifie la signature pour correspondre à l'interface
        public ModeleCpSat ConstruireModele(ProblemeOptimisation probleme, string objectif)
        {
            var model = new CpModel();
            var tacheBuilder = new TacheModelBuilder();
            var coutBuilder = new CoutModelBuilder();

            var (tachesIntervals, tachesAssignables, makespan) = tacheBuilder.Construire(model, probleme);
            var coutTotal = coutBuilder.Construire(model, probleme, tachesIntervals, tachesAssignables, makespan);

            // --- AIGUILLAGE DE L'OBJECTIF ---
            switch (objectif)
            {
                case "DELAI":
                    model.Minimize(makespan);
                    break;

                case "COUT":
                default:
                    model.Minimize(coutTotal);
                    break;
            }

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
}
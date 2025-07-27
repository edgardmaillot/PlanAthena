// Fichier : Infrastructure/Services/OrTools/ConstructeurProblemeOrTools.cs

using Google.OrTools.Sat;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Application.Interfaces;

namespace PlanAthena.Core.Infrastructure.Services.OrTools
{
    public class ConstructeurProblemeOrTools : IConstructeurProblemeOrTools
    {
        public ModeleCpSat ConstruireModele(ProblemeOptimisation probleme, string objectif)
        {
            var model = new CpModel();
            var tacheBuilder = new TacheModelBuilder();
            var coutBuilder = new CoutModelBuilder();

            // *** MODIFICATION: Récupération des métadonnées en plus des éléments existants ***
            // Le TacheModelBuilder retourne maintenant 6 éléments au lieu de 3.
            // Les 3 nouveaux sont les métadonnées nécessaires pour corriger l'affichage des jalons
            var (tachesIntervals, tachesAssignables, makespan, dureesOriginales, typesActivites, nomsActivites) =
                tacheBuilder.Construire(model, probleme);

            // --- CORRECTION : Récupération correcte du tuple de coûts ---
            var (coutTotal, coutRh, coutIndirect) = coutBuilder.Construire(model, probleme, tachesIntervals, tachesAssignables, makespan);

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
                CoutTotal = coutTotal,
                CoutRh = coutRh,
                CoutIndirect = coutIndirect,

                // *** AJOUT: Transmission des métadonnées vers le modèle ***
                // Ces métadonnées permettront au SolutionInterpreterService 
                // de corriger les durées des jalons lors de l'export Gantt
                DureesOriginalesHeures = dureesOriginales,
                TypesActivites = typesActivites,
                NomsActivites = nomsActivites
            };
        }
    }
}
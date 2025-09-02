// Fichier : Infrastructure/Services/OrTools/ConstructeurProblemeOrTools.cs

using Google.OrTools.Sat;
using PlanAthena.core.Application.InternalDto;
using PlanAthena.Core.Application.Interfaces;

namespace PlanAthena.Core.Infrastructure.Services.OrTools
{
    public class ConstructeurProblemeOrTools : IConstructeurProblemeOrTools
    {
        // La méthode ConstruireModele est responsable de l'assemblage global du modèle CP-SAT.
        // Elle coordonne l'appel aux différents "builders" (TacheModelBuilder, CoutModelBuilder)
        // pour créer toutes les variables de décision et les contraintes, puis définit l'objectif d'optimisation.
        public ModeleCpSat ConstruireModele(ProblemeOptimisation probleme, string objectif)
        {
            var model = new CpModel();
            var tacheBuilder = new TacheModelBuilder();
            var coutBuilder = new CoutModelBuilder();

            // Appel au TacheModelBuilder pour construire les variables et contraintes liées aux tâches,
            // aux ressources et aux précédences (tâches et lots/groupes de priorité).
            // La signature de retour est élargie pour inclure les nouvelles variables de temps agrégées.
            var (tachesIntervals, tachesAssignables, makespan,
                 dureesOriginales, typesActivites, nomsActivites,
                 lotStarts, lotEnds, priorityGroupStarts, priorityGroupEnds) =
                tacheBuilder.Construire(model, probleme);

            // Appel au CoutModelBuilder pour modéliser les différents types de coûts (RH, indirects, total).
            var (coutTotal, coutRh, coutIndirect) = coutBuilder.Construire(model, probleme, tachesIntervals, tachesAssignables, makespan);

            // Définition de l'objectif d'optimisation pour le solveur.
            // Le solveur cherchera à minimiser soit le délai total (makespan), soit le coût total du chantier.
            switch (objectif)
            {
                case "DELAI":
                default:
                    model.Minimize(makespan);
                    break;
                case "COUT":
                    model.Minimize(coutTotal);
                    break;
            }

            // Retourne un objet ModeleCpSat encapsulant le modèle CP-SAT et toutes les variables clés.
            // Cela permet à l'interpréteur de solution d'accéder aux résultats nécessaires.
            return new ModeleCpSat
            {
                Model = model,
                TachesIntervals = tachesIntervals,
                TachesAssignables = tachesAssignables,
                Makespan = makespan,
                CoutTotal = coutTotal,
                CoutRh = coutRh,
                CoutIndirect = coutIndirect,

                DureesOriginalesHeures = dureesOriginales,
                TypesActivites = typesActivites,
                NomsActivites = nomsActivites,

                // Attribution des nouvelles collections de variables de temps pour les lots et groupes de priorité.
                LotStarts = lotStarts,
                LotEnds = lotEnds,
                PriorityGroupStarts = priorityGroupStarts,
                PriorityGroupEnds = priorityGroupEnds
            };
        }
    }
}
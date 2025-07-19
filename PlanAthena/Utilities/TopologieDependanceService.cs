// Fichier: PlanAthena.Utilities/TopologieDependanceService.cs (Version Sécurisée)

using PlanAthena.Data;
using PlanAthena.Services.Business;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthena.Utilities
{
    public class TopologieDependanceService
    {
        private readonly MetierService _metierService;

        public TopologieDependanceService(MetierService metierService)
        {
            _metierService = metierService;
        }

        public void AppliquerDependancesInterMetiers(List<Tache> toutesLesTaches)
        {
            if (toutesLesTaches == null || !toutesLesTaches.Any()) return;

            var tachesParBloc = toutesLesTaches.GroupBy(t => t.BlocId);

            foreach (var groupeBloc in tachesParBloc)
            {
                var tachesDuBloc = groupeBloc.ToList();
                foreach (var tacheCourante in tachesDuBloc)
                {
                    if (string.IsNullOrEmpty(tacheCourante.MetierId)) continue;

                    // On ne modifie que les têtes de chaîne. Votre logique originale est conservée.
                    if (!EstTeteDeChaine(tacheCourante, tachesDuBloc)) continue;

                    // On récupère les dépendances actuelles pour les préserver.
                    var dependancesActuelles = (tacheCourante.Dependencies ?? "")
                        .Split(',')
                        .Select(d => d.Trim())
                        .Where(d => !string.IsNullOrEmpty(d))
                        .ToHashSet();

                    // Règle 3 : Gestion de la transitivité
                    var prerequisMetiers = ObtenirTousLesPrerequisTransitives(tacheCourante.MetierId, toutesLesTaches, groupeBloc.Key);

                    foreach (var prerequisMetierId in prerequisMetiers)
                    {
                        var tachesDuMetierPrerequis = tachesDuBloc.Where(t => t.MetierId == prerequisMetierId).ToList();
                        var finsDeChaine = TrouverFinsDeChaine(tachesDuMetierPrerequis, tachesDuBloc);

                        // On ajoute les nouvelles dépendances SANS créer de doublons.
                        dependancesActuelles.UnionWith(finsDeChaine.Select(t => t.TacheId));
                    }
                    tacheCourante.Dependencies = string.Join(",", dependancesActuelles.OrderBy(d => d));
                }
            }
        }

        // NOUVELLE MÉTHODE pour gérer la Règle #3 (transitivité)
        private HashSet<string> ObtenirTousLesPrerequisTransitives(string metierIdInitial, List<Tache> toutesLesTaches, string blocId)
        {
            var aExplorer = new Queue<string>(_metierService.GetPrerequisForMetier(metierIdInitial));
            var prerequisFinaux = new HashSet<string>();
            var dejaExplores = new HashSet<string>();

            while (aExplorer.Count > 0)
            {
                var metierCourant = aExplorer.Dequeue();
                if (dejaExplores.Contains(metierCourant)) continue;
                dejaExplores.Add(metierCourant);

                // Si des tâches de ce métier existent dans le bloc, on s'arrête là pour cette branche.
                if (toutesLesTaches.Any(t => t.BlocId == blocId && t.MetierId == metierCourant))
                {
                    prerequisFinaux.Add(metierCourant);
                }
                else // Sinon, on continue à explorer les prérequis de ce métier
                {
                    var prerequisParents = _metierService.GetPrerequisForMetier(metierCourant);
                    foreach (var parent in prerequisParents)
                    {
                        aExplorer.Enqueue(parent);
                    }
                }
            }
            return prerequisFinaux;
        }

        // === VOS MÉTHODES ORIGINALES SONT CONSERVÉES INTÉGRALEMENT ===
        private bool EstTeteDeChaine(Tache tache, List<Tache> contexteDeTaches)
        {
            var dependancesIds = (tache.Dependencies ?? "").Split(',').Select(d => d.Trim());
            return !dependancesIds.Any(depId => contexteDeTaches.FirstOrDefault(t => t.TacheId == depId)?.MetierId == tache.MetierId);
        }

        private List<Tache> TrouverFinsDeChaine(List<Tache> tachesDuMetier, List<Tache> contexteTotal)
        {
            if (!tachesDuMetier.Any()) return new List<Tache>();
            var toutesLesDependancesIntraMetier = contexteTotal
                .Where(t => t.MetierId == tachesDuMetier.First().MetierId)
                .SelectMany(t => (t.Dependencies ?? "").Split(',').Select(d => d.Trim()))
                .ToHashSet();
            return tachesDuMetier.Where(t => !toutesLesDependancesIntraMetier.Contains(t.TacheId)).ToList();
        }
    }
}
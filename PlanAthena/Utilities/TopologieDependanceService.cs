// Fichier: Utilities/TopologieDependanceService.cs

using PlanAthena.Data;
using PlanAthena.Services.Business;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthena.Utilities
{
    /// <summary>
    /// Service responsable de l'application des dépendances logiques entre les métiers
    /// en analysant la topologie interne des chaînes de tâches.
    /// </summary>
    public class TopologieDependanceService
    {
        private readonly MetierService _metierService;

        public TopologieDependanceService(MetierService metierService)
        {
            _metierService = metierService;
        }

        /// <summary>
        /// Applique les dépendances inter-métiers sur une liste de tâches.
        /// Cette méthode modifie directement la propriété 'Dependencies' des tâches.
        /// </summary>
        public void AppliquerDependancesInterMetiers(List<Tache> toutesLesTaches)
        {
            if (toutesLesTaches == null || !toutesLesTaches.Any()) return;

            var tachesParBloc = toutesLesTaches.GroupBy(t => t.BlocId);

            foreach (var groupeBloc in tachesParBloc)
            {
                var tachesDuBloc = groupeBloc.ToList();
                var mapTachesDuBloc = tachesDuBloc.ToDictionary(t => t.TacheId);

                foreach (var tacheCourante in tachesDuBloc)
                {
                    // On ne traite que les tâches ayant un métier
                    if (string.IsNullOrEmpty(tacheCourante.MetierId)) continue;

                    var prerequisMetiers = _metierService.GetPrerequisForMetier(tacheCourante.MetierId);
                    if (!prerequisMetiers.Any()) continue;

                    // Si la tâche courante n'est PAS une "tête de chaîne" de son propre métier,
                    // alors elle ne doit pas recevoir de dépendances inter-métiers.
                    // Elle recevra ses dépendances de ses propres collègues de métier.
                    if (!EstTeteDeChaine(tacheCourante, tachesDuBloc)) continue;

                    var dependancesActuelles = (tacheCourante.Dependencies ?? "")
                        .Split(',')
                        .Select(d => d.Trim())
                        .Where(d => !string.IsNullOrEmpty(d))
                        .ToHashSet();

                    foreach (var prerequisMetierId in prerequisMetiers)
                    {
                        var tachesDuMetierPrerequis = tachesDuBloc
                            .Where(t => t.MetierId == prerequisMetierId)
                            .ToList();

                        // On ne veut dépendre que des "fins de chaîne" du métier prérequis.
                        var finsDeChaine = TrouverFinsDeChaine(tachesDuMetierPrerequis, tachesDuBloc);

                        dependancesActuelles.UnionWith(finsDeChaine.Select(t => t.TacheId));
                    }
                    tacheCourante.Dependencies = string.Join(",", dependancesActuelles.OrderBy(d => d));
                }
            }
        }

        /// <summary>
        /// Détermine si une tâche est une "tête de chaîne" dans son métier.
        /// Une tête de chaîne n'a aucune dépendance VERS une autre tâche du MÊME métier.
        /// </summary>
        private bool EstTeteDeChaine(Tache tache, List<Tache> contexteDeTaches)
        {
            var dependancesIds = (tache.Dependencies ?? "").Split(',').Select(d => d.Trim());

            // La tâche est une tête de chaîne s'il n'existe aucune dépendance
            // qui pointe vers une tâche du même métier qu'elle.
            return !dependancesIds.Any(depId =>
                contexteDeTaches.FirstOrDefault(t => t.TacheId == depId)?.MetierId == tache.MetierId
            );
        }

        /// <summary>
        /// Trouve les "fins de chaîne" pour un ensemble de tâches d'un métier donné.
        /// Une fin de chaîne n'est le prérequis d'AUCUNE autre tâche du MÊME métier.
        /// </summary>
        private List<Tache> TrouverFinsDeChaine(List<Tache> tachesDuMetier, List<Tache> contexteTotal)
        {
            if (!tachesDuMetier.Any()) return new List<Tache>();

            // Récupère les IDs de toutes les dépendances des tâches du même métier
            var toutesLesDependancesIntraMetier = contexteTotal
                .Where(t => t.MetierId == tachesDuMetier.First().MetierId)
                .SelectMany(t => (t.Dependencies ?? "").Split(',').Select(d => d.Trim()))
                .ToHashSet();

            // Une fin de chaîne est une tâche de notre groupe qui n'apparaît JAMAIS
            // dans la liste des dépendances de ses collègues.
            return tachesDuMetier
                .Where(t => !toutesLesDependancesIntraMetier.Contains(t.TacheId))
                .ToList();
        }
    }
}
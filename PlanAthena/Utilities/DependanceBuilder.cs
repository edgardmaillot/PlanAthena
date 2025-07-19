// Fichier : PlanAthena.Utilities/DependanceBuilder.cs

using PlanAthena.Data;
using PlanAthena.Services.Business;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthena.Utilities
{
    public class DependanceBuilder
    {
        private readonly TopologieDependanceService _topologieService;

        public DependanceBuilder(TopologieDependanceService topologieService)
        {
            _topologieService = topologieService;
        }

        /// <summary>
        /// Processus complet et sécurisé qui enrichit et nettoie les dépendances des tâches.
        /// </summary>
        public void ConstruireDependancesLogiques(List<Tache> taches, MetierService metierService)
        {
            if (taches == null || !taches.Any()) return;

            // Étape 1 : Applique la topologie pour AJOUTER les dépendances métier manquantes.
            _topologieService.AppliquerDependancesInterMetiers(taches);

            // Étape 2 : Crée les jalons de synchronisation pour simplifier les relations N:M.
            CreerEtLierJalonsDeSynchro(taches);

            // Étape 3 : Nettoie les dépendances transitives (redondantes) créées aux étapes précédentes.
            SimplifierDependancesTransitives(taches);
        }

        private void CreerEtLierJalonsDeSynchro(List<Tache> taches)
        {
            // Votre logique originale et fonctionnelle, sécurisée.
            var nouveauxJalons = new List<Tache>();
            var tachesParBloc = taches.GroupBy(t => t.BlocId);

            foreach (var groupeBloc in tachesParBloc)
            {
                var tachesDuBloc = groupeBloc.ToList();
                if (tachesDuBloc.Count < 2) continue;
                var mapTachesDuBloc = tachesDuBloc.ToDictionary(t => t.TacheId);
                var childCounts = new Dictionary<string, int>();
                foreach (var tache in tachesDuBloc)
                {
                    var dependancesIds = (tache.Dependencies ?? "").Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);
                    foreach (var parentId in dependancesIds)
                    {
                        if (mapTachesDuBloc.ContainsKey(parentId))
                        {
                            if (!childCounts.ContainsKey(parentId)) childCounts[parentId] = 0;
                            childCounts[parentId]++;
                        }
                    }
                }
                if (!childCounts.Any()) continue;
                var tousLesLiens = tachesDuBloc.SelectMany(enfant => (enfant.Dependencies ?? "").Split(',').Select(d => d.Trim()).Where(d => !string.IsNullOrEmpty(d)), (enfant, parentId) => new { Enfant = enfant, ParentId = parentId }).Where(lien => mapTachesDuBloc.ContainsKey(lien.ParentId)).Select(lien => new { Enfant = lien.Enfant, Parent = mapTachesDuBloc[lien.ParentId] }).ToList();
                var groupesParMetierParent = tousLesLiens.Where(l => !string.IsNullOrEmpty(l.Parent.MetierId)).GroupBy(l => l.Parent.MetierId);

                foreach (var groupe in groupesParMetierParent)
                {
                    var groupeParent = groupe.Select(l => l.Parent).Distinct().ToList();
                    var groupeEnfant = groupe.Select(l => l.Enfant).Distinct().ToList();
                    var parentIds = groupeParent.Select(p => p.TacheId).ToHashSet();

                    if (groupeEnfant.Any(e => parentIds.Contains(e.TacheId))) continue;
                    if (groupeParent.Count <= 1 || groupeEnfant.Count <= 1) continue;

                    bool aUnFanOut = groupeParent.Any(parent => childCounts.GetValueOrDefault(parent.TacheId, 0) > 1);
                    bool aUnFanIn = groupeEnfant.Any(enfant => (enfant.Dependencies ?? "").Split(',').Length > 1);

                    if (aUnFanOut && aUnFanIn)
                    {
                        string idJalon = $"J_Sync_{groupe.Key}_{groupeBloc.Key}";

                        if (taches.Any(t => t.TacheId == idJalon))
                        {
                            continue;
                        }

                        var refTache = groupeParent.First();
                        var jalon = new Tache
                        {
                            TacheId = idJalon,
                            TacheNom = $"Synchro Fin {groupe.Key}",
                            Type = TypeActivite.JalonUtilisateur,
                            Dependencies = string.Join(",", groupeParent.Select(t => t.TacheId)),
                            BlocId = refTache.BlocId,
                            BlocNom = refTache.BlocNom,
                            LotId = refTache.LotId,
                            LotNom = refTache.LotNom,
                            HeuresHommeEstimees = 0,
                            MetierId = ""
                        };
                        nouveauxJalons.Add(jalon);

                        foreach (var enfant in groupeEnfant)
                        {
                            var depsActuelles = (enfant.Dependencies ?? "").Split(',').Select(d => d.Trim()).ToHashSet();
                            depsActuelles.ExceptWith(parentIds);
                            depsActuelles.Add(jalon.TacheId);
                            enfant.Dependencies = string.Join(",", depsActuelles.OrderBy(d => d));
                        }
                    }
                }
            }
            if (nouveauxJalons.Any())
            {
                taches.AddRange(nouveauxJalons);
            }
        }

        /// <summary>
        /// NOUVELLE MÉTHODE : Implémente la Règle B.1 - Simplification Transitive.
        /// Pour chaque tâche, supprime les dépendances "grand-parent" redondantes.
        /// </summary>
        private void SimplifierDependancesTransitives(List<Tache> taches)
        {
            var mapTaches = taches.ToDictionary(t => t.TacheId);

            foreach (var tache in taches)
            {
                var parentsDirectsIds = (tache.Dependencies ?? "")
                    .Split(',')
                    .Select(d => d.Trim())
                    .Where(d => !string.IsNullOrEmpty(d))
                    .ToList();

                if (parentsDirectsIds.Count < 2) continue;

                var grandsParentsIds = new HashSet<string>();

                // On collecte tous les "grands-parents" en explorant les parents directs
                foreach (var parentId in parentsDirectsIds)
                {
                    if (mapTaches.TryGetValue(parentId, out var parentTache))
                    {
                        var parentDependencies = (parentTache.Dependencies ?? "").Split(',').Select(d => d.Trim());
                        grandsParentsIds.UnionWith(parentDependencies);
                    }
                }

                if (!grandsParentsIds.Any()) continue;

                // On reconstruit la liste des dépendances en ne gardant que les parents
                // qui ne sont PAS également des grands-parents.
                var dependancesFinales = parentsDirectsIds
                    .Where(parentId => !grandsParentsIds.Contains(parentId))
                    .ToList();

                // Il est possible qu'un parent direct ait été retiré, donc on s'assure
                // qu'il reste au moins une dépendance si la liste originale n'était pas vide.
                // S'il ne reste rien, c'est qu'il ne faut dépendre que des grands-parents, via les parents.
                // On garde donc la liste non filtrée dans ce cas pour ne pas rompre la chaîne.
                // Ce cas est très rare mais constitue une sécurité.
                if (parentsDirectsIds.Any() && !dependancesFinales.Any())
                {
                    // Ne rien faire, garder les dépendances originales pour ne pas isoler la tâche.
                    // La logique existante est suffisante.
                }
                else
                {
                    tache.Dependencies = string.Join(",", dependancesFinales.Distinct().OrderBy(d => d));
                }
            }
        }
    }
}
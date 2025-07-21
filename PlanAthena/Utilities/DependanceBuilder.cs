// Fichier : PlanAthena.Utilities/DependanceBuilder.cs

using PlanAthena.Data;
using PlanAthena.Services.Business;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthena.Utilities
{
    public class DependanceBuilder
    {
        private readonly TopologieDependanceService _topologieService;

        public DependanceBuilder(TopologieDependanceService topologieService)
        {
            _topologieService = topologieService ?? throw new ArgumentNullException(nameof(topologieService));
        }

        /// <summary>
        /// Processus complet et sécurisé qui enrichit et nettoie les dépendances des tâches.
        /// </summary>
        public void ConstruireDependancesLogiques(List<Tache> taches)
        {
            if (taches == null || taches.Count == 0) return;

            // Étape 1 : Applique la topologie pour AJOUTER les dépendances métier manquantes.
            _topologieService.AppliquerDependancesInterMetiers(taches);

            // Étape 2 : Crée les jalons de synchronisation pour simplifier les relations N:M.
            CreerEtLierJalonsDeSynchro(taches);

            // Étape 3 : Nettoie les dépendances transitives (redondantes) créées aux étapes précédentes.
            SimplifierDependancesTransitives(taches);
        }

        private static void CreerEtLierJalonsDeSynchro(List<Tache> taches)
        {
            var nouveauxJalons = new List<Tache>();
            var tachesParBloc = taches.GroupBy(t => t.BlocId);
            char[] splitChars = { ',' };

            foreach (var groupeBloc in tachesParBloc)
            {
                var tachesDuBloc = groupeBloc.ToList();
                if (tachesDuBloc.Count < 2) continue;

                var mapTachesDuBloc = tachesDuBloc.ToDictionary(t => t.TacheId);

                var childCounts = new Dictionary<string, int>();
                foreach (var tache in tachesDuBloc)
                {
                    var dependancesIds = tache.Dependencies.Split(splitChars, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    foreach (var parentId in dependancesIds)
                    {
                        if (mapTachesDuBloc.ContainsKey(parentId))
                        {
                            childCounts.TryGetValue(parentId, out int currentCount);
                            childCounts[parentId] = currentCount + 1;
                        }
                    }
                }

                if (childCounts.Count == 0) continue;

                var tousLesLiens = tachesDuBloc
                    .SelectMany(enfant => enfant.Dependencies.Split(splitChars, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                                (enfant, parentId) => new { Enfant = enfant, ParentId = parentId })
                    .Where(lien => mapTachesDuBloc.ContainsKey(lien.ParentId))
                    .Select(lien => new { lien.Enfant, Parent = mapTachesDuBloc[lien.ParentId] })
                    .ToList();

                var groupesParMetierParent = tousLesLiens
                    .Where(l => !string.IsNullOrEmpty(l.Parent.MetierId))
                    .GroupBy(l => l.Parent.MetierId);

                foreach (var groupe in groupesParMetierParent)
                {
                    var groupeParent = groupe.Select(l => l.Parent).Distinct().ToList();
                    var groupeEnfant = groupe.Select(l => l.Enfant).Distinct().ToList();
                    var parentIds = groupeParent.Select(p => p.TacheId).ToHashSet();

                    if (groupeEnfant.Any(e => parentIds.Contains(e.TacheId))) continue;
                    if (groupeParent.Count <= 1 || groupeEnfant.Count <= 1) continue;

                    bool aUnFanOut = groupeParent.Any(parent => childCounts.GetValueOrDefault(parent.TacheId, 0) > 1);
                    bool aUnFanIn = groupeEnfant.Any(enfant => enfant.Dependencies.Split(',').Length > 1);

                    if (aUnFanOut && aUnFanIn)
                    {
                        string idJalon = $"J_Sync_{groupe.Key}_{groupeBloc.Key}";
                        if (taches.Any(t => t.TacheId == idJalon)) continue;

                        var refTache = groupeParent[0];
                        var jalon = new Tache
                        {
                            TacheId = idJalon,
                            TacheNom = $"Synchro Fin {groupe.Key}",
                            Type = TypeActivite.JalonUtilisateur,
                            Dependencies = string.Join(",", groupeParent.Select(t => t.TacheId)),
                            BlocId = refTache.BlocId,
                            LotId = refTache.LotId,
                            HeuresHommeEstimees = 0,
                            MetierId = ""
                        };
                        nouveauxJalons.Add(jalon);

                        foreach (var enfant in groupeEnfant)
                        {
                            var depsActuelles = enfant.Dependencies.Split(',').Select(d => d.Trim()).ToHashSet();
                            depsActuelles.ExceptWith(parentIds);
                            depsActuelles.Add(jalon.TacheId);
                            enfant.Dependencies = string.Join(",", depsActuelles.OrderBy(d => d));
                        }
                    }
                }
            }

            if (nouveauxJalons.Count > 0)
            {
                taches.AddRange(nouveauxJalons);
            }
        }

        /// <summary>
        /// Implémente la Règle B.1 - Simplification Transitive.
        /// Pour chaque tâche, supprime les dépendances "grand-parent" redondantes.
        /// </summary>
        private static void SimplifierDependancesTransitives(List<Tache> taches)
        {
            var mapTaches = taches.ToDictionary(t => t.TacheId);

            foreach (var tache in taches)
            {
                var parentsDirectsIds = (tache.Dependencies ?? string.Empty)
                    .Split(',')
                    .Select(d => d.Trim())
                    .Where(d => !string.IsNullOrEmpty(d))
                    .ToList();

                if (parentsDirectsIds.Count < 2) continue;

                var grandsParentsIds = new HashSet<string>();

                foreach (var parentId in parentsDirectsIds)
                {
                    if (mapTaches.TryGetValue(parentId, out var parentTache))
                    {
                        var parentDependencies = (parentTache.Dependencies ?? string.Empty).Split(',').Select(d => d.Trim());
                        grandsParentsIds.UnionWith(parentDependencies);
                    }
                }

                if (grandsParentsIds.Count == 0) continue;

                var dependancesFinales = parentsDirectsIds
                    .Where(parentId => !grandsParentsIds.Contains(parentId))
                    .ToList();

                if (parentsDirectsIds.Count > 0 && dependancesFinales.Count == 0)
                {
                    // Ne rien faire, garder les dépendances originales pour ne pas isoler la tâche.
                }
                else
                {
                    tache.Dependencies = string.Join(",", dependancesFinales.Distinct().OrderBy(d => d));
                }
            }
        }
    }
}
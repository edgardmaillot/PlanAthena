// Fichier : DependanceBuilder.cs

using PlanAthena.Data;
using PlanAthena.Services.Business;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthena.Utilities
{
    /// <summary>
    /// Fournit des services pour construire et optimiser la structure des dépendances
    /// entre les tâches d'un projet.
    /// </summary>
    public class DependanceBuilder
    {
        private readonly TopologieDependanceService _topologieService;

        
        public DependanceBuilder(TopologieDependanceService topologieService)
        {
            _topologieService = topologieService;
        }

        public void ConstruireDependancesLogiques(List<Tache> taches, MetierService metierService)
        {
            if (taches == null || !taches.Any()) return;

            _topologieService.AppliquerDependancesInterMetiers(taches);
            CreerEtLierJalonsDeSynchro(taches);
        }

        private void CreerEtLierJalonsDeSynchro(List<Tache> taches)
        {
            var nouveauxJalons = new List<Tache>();
            var tachesParBloc = taches.GroupBy(t => t.BlocId);

            foreach (var groupeBloc in tachesParBloc)
            {
                var tachesDuBloc = groupeBloc.ToList();
                if (tachesDuBloc.Count < 2) continue;

                var mapTachesDuBloc = tachesDuBloc.ToDictionary(t => t.TacheId);

                // --- DEBUT DE LA LOGIQUE CORRIGÉE ---

                // Phase 1: Pré-calculer combien d'enfants a chaque tâche parente
                var childCounts = new Dictionary<string, int>();
                foreach (var tache in tachesDuBloc)
                {
                    var dependancesIds = (tache.Dependencies ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
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

                // Phase 2: Analyser les groupes de relations par métier parent
                var tousLesLiens = tachesDuBloc
                    .SelectMany(enfant => (enfant.Dependencies ?? "").Split(',').Select(d => d.Trim()).Where(d => !string.IsNullOrEmpty(d)),
                                (enfant, parentId) => new { Enfant = enfant, ParentId = parentId })
                    .Where(lien => mapTachesDuBloc.ContainsKey(lien.ParentId))
                    .Select(lien => new { Enfant = lien.Enfant, Parent = mapTachesDuBloc[lien.ParentId] })
                    .ToList();

                var groupesParMetierParent = tousLesLiens
                    .Where(l => !string.IsNullOrEmpty(l.Parent.MetierId))
                    .GroupBy(l => l.Parent.MetierId);

                foreach (var groupe in groupesParMetierParent)
                {
                    var groupeParent = groupe.Select(l => l.Parent).Distinct().ToList();
                    var groupeEnfant = groupe.Select(l => l.Enfant).Distinct().ToList();

                    // RÈGLE CRITIQUE DE NON-CHEVAUCHEMENT :
                    // Si une tâche est à la fois parente et enfant dans ce groupe, ce n'est pas
                    // une relation N:M à simplifier, mais une chaîne de tâches du même métier. On l'ignore.
                    var parentIds = groupeParent.Select(p => p.TacheId).ToHashSet();
                    if (groupeEnfant.Any(e => parentIds.Contains(e.TacheId)))
                    {
                        continue; // Ignorer ce groupe, il est invalide pour le jalonnement.
                    }

                    // Appliquer les conditions maintenant que le groupe est validé
                    if (groupeParent.Count <= 1 || groupeEnfant.Count <= 1) continue;

                    bool aUnFanOut = groupeParent.Any(parent => childCounts.GetValueOrDefault(parent.TacheId, 0) > 1);
                    bool aUnFanIn = groupeEnfant.Any(enfant => (enfant.Dependencies ?? "").Split(',').Length > 1);

                    if (aUnFanOut && aUnFanIn)
                    {
                        string idJalon = $"J_Sync_{groupe.Key}_{groupeBloc.Key}";
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

                        // Phase 3: Relier les dépendances immédiatement et proprement
                        // On ne modifie QUE les tâches du groupe enfant.
                        foreach (var enfant in groupeEnfant)
                        {
                            var depsActuelles = (enfant.Dependencies ?? "").Split(',').Select(d => d.Trim()).ToHashSet();

                            // Retirer toutes les dépendances vers les parents du groupe
                            depsActuelles.ExceptWith(parentIds);

                            // Ajouter la dépendance vers le nouveau jalon
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
    }
}
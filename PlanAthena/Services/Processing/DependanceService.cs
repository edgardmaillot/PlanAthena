using PlanAthena.Data;
using PlanAthena.Services.Business;

namespace PlanAthena.Services.Processing
{
    /// <summary>
    /// Service responsable du calcul et de l'optimisation des dépendances entre tâches.
    /// Gère la matérialisation des dépendances métier et la réduction transitive.
    /// </summary>
    public class DependanceService
    {
        private readonly MetierService _metierService;

        public DependanceService(MetierService metierService)
        {
            _metierService = metierService ?? throw new ArgumentNullException(nameof(metierService));
        }

        /// <summary>
        /// Calcule les dépendances métier complètes et applique la réduction transitive.
        /// </summary>
        /// <param name="tachesBrutes">Liste originale des tâches</param>
        /// <returns>Nouvelle liste avec dépendances optimisées pour l'affichage</returns>
        public List<TacheRecord> CalculerDependancesMetier(IReadOnlyList<TacheRecord> tachesBrutes)
        {
            if (tachesBrutes == null || !tachesBrutes.Any())
                return new List<TacheRecord>();

            // Étape 1: Copier les tâches pour ne pas modifier l'original
            var tachesResultat = tachesBrutes.Select(CopierTache).ToList();

            // Étape 2: Matérialiser les dépendances métier
            MaterialiserDependancesMetier(tachesResultat);

            // Étape 3: Appliquer la réduction transitive
            AppliquerReductionTransitive(tachesResultat);

            return tachesResultat;
        }

        /// <summary>
        /// Matérialise les dépendances basées sur les prérequis métier
        /// </summary>
        private void MaterialiserDependancesMetier(List<TacheRecord> taches)
        {
            var tachesParBloc = taches.GroupBy(t => t.BlocId);

            foreach (var groupeBloc in tachesParBloc)
            {
                var tachesDuBloc = groupeBloc.ToList();

                foreach (var tacheCourante in tachesDuBloc)
                {
                    if (string.IsNullOrEmpty(tacheCourante.MetierId))
                        continue;

                    var prerequisMetier = _metierService.GetPrerequisForMetier(tacheCourante.MetierId);
                    if (!prerequisMetier.Any())
                        continue;

                    // Trouver toutes les tâches du bloc qui correspondent aux métiers prérequis
                    var tachesPrecedentesIds = tachesDuBloc
                        .Where(t => !string.IsNullOrEmpty(t.MetierId) && prerequisMetier.Contains(t.MetierId))
                        .Where(t => t.TacheId != tacheCourante.TacheId) // Éviter l'auto-référence
                        .Select(t => t.TacheId)
                        .ToList();

                    if (tachesPrecedentesIds.Any())
                    {
                        // Fusionner avec les dépendances existantes
                        var dependancesExistantes = ObtenirDependancesExistantes(tacheCourante.Dependencies);
                        dependancesExistantes.AddRange(tachesPrecedentesIds);

                        tacheCourante.Dependencies = string.Join(",", dependancesExistantes.Distinct());
                    }
                }
            }
        }

        /// <summary>
        /// Applique l'algorithme de réduction transitive pour éliminer les dépendances redondantes
        /// </summary>
        private void AppliquerReductionTransitive(List<TacheRecord> taches)
        {
            // Construire un dictionnaire pour un accès rapide
            var tachesDict = taches.ToDictionary(t => t.TacheId, t => t);

            // Pour chaque tâche, calculer ses dépendances transitives
            var dependancesTransitives = new Dictionary<string, HashSet<string>>();

            foreach (var tache in taches)
            {
                dependancesTransitives[tache.TacheId] = CalculerDependancesTransitives(tache.TacheId, tachesDict, new HashSet<string>());
            }

            // Réduire les dépendances directes en supprimant celles qui sont transitives
            foreach (var tache in taches)
            {
                var dependancesDirectes = ObtenirDependancesExistantes(tache.Dependencies);
                if (!dependancesDirectes.Any())
                    continue;

                var dependancesReduites = new List<string>();

                foreach (var dependanceDirecte in dependancesDirectes)
                {
                    bool estRedondante = false;

                    // Vérifier si cette dépendance peut être atteinte par une autre dépendance directe
                    foreach (var autreDependance in dependancesDirectes)
                    {
                        if (autreDependance != dependanceDirecte &&
                            dependancesTransitives.ContainsKey(autreDependance) &&
                            dependancesTransitives[autreDependance].Contains(dependanceDirecte))
                        {
                            estRedondante = true;
                            break;
                        }
                    }

                    if (!estRedondante)
                    {
                        dependancesReduites.Add(dependanceDirecte);
                    }
                }

                tache.Dependencies = dependancesReduites.Any() ? string.Join(",", dependancesReduites) : "";
            }
        }

        /// <summary>
        /// Calcule récursivement toutes les dépendances transitives d'une tâche
        /// </summary>
        private HashSet<string> CalculerDependancesTransitives(string tacheId, Dictionary<string, TacheRecord> tachesDict, HashSet<string> visite)
        {
            var dependancesTransitives = new HashSet<string>();

            if (visite.Contains(tacheId) || !tachesDict.ContainsKey(tacheId))
                return dependancesTransitives;

            visite.Add(tacheId);

            var dependancesDirectes = ObtenirDependancesExistantes(tachesDict[tacheId].Dependencies);

            foreach (var dependance in dependancesDirectes)
            {
                if (!dependancesTransitives.Contains(dependance))
                {
                    dependancesTransitives.Add(dependance);

                    // Ajouter récursivement les dépendances transitives
                    var dependancesIndirectes = CalculerDependancesTransitives(dependance, tachesDict, new HashSet<string>(visite));
                    foreach (var indirecte in dependancesIndirectes)
                    {
                        dependancesTransitives.Add(indirecte);
                    }
                }
            }

            visite.Remove(tacheId);
            return dependancesTransitives;
        }

        /// <summary>
        /// Extrait la liste des dépendances depuis la chaîne Dependencies
        /// </summary>
        private List<string> ObtenirDependancesExistantes(string dependencies)
        {
            if (string.IsNullOrEmpty(dependencies))
                return new List<string>();

            return dependencies
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(d => d.Trim())
                .Where(d => !string.IsNullOrEmpty(d))
                .ToList();
        }

        /// <summary>
        /// Crée une copie profonde d'une tâche
        /// </summary>
        private TacheRecord CopierTache(TacheRecord source)
        {
            return new TacheRecord
            {
                TacheId = source.TacheId,
                TacheNom = source.TacheNom,
                HeuresHommeEstimees = source.HeuresHommeEstimees,
                MetierId = source.MetierId,
                Dependencies = source.Dependencies,
                LotId = source.LotId,
                LotNom = source.LotNom,
                LotPriorite = source.LotPriorite,
                BlocId = source.BlocId,
                BlocNom = source.BlocNom,
                BlocCapaciteMaxOuvriers = source.BlocCapaciteMaxOuvriers
            };
        }

        /// <summary>
        /// Valide qu'il n'y a pas de cycles dans les dépendances
        /// </summary>
        public List<string> DetecterCyclesDependances(IReadOnlyList<TacheRecord> taches)
        {
            var cycles = new List<string>();
            var tachesDict = taches.ToDictionary(t => t.TacheId, t => t);
            var visite = new HashSet<string>();
            var enCours = new HashSet<string>();

            foreach (var tache in taches)
            {
                if (!visite.Contains(tache.TacheId))
                {
                    var cycle = DetecterCycleRecursif(tache.TacheId, tachesDict, visite, enCours, new List<string>());
                    if (cycle.Any())
                    {
                        cycles.Add($"Cycle détecté: {string.Join(" -> ", cycle)}");
                    }
                }
            }

            return cycles;
        }

        /// <summary>
        /// Détection récursive de cycles dans le graphe de dépendances
        /// </summary>
        private List<string> DetecterCycleRecursif(string tacheId, Dictionary<string, TacheRecord> tachesDict,
            HashSet<string> visite, HashSet<string> enCours, List<string> chemin)
        {
            if (enCours.Contains(tacheId))
            {
                // Cycle détecté - retourner le chemin du cycle
                var indexCycle = chemin.IndexOf(tacheId);
                return chemin.Skip(indexCycle).Concat(new[] { tacheId }).ToList();
            }

            if (visite.Contains(tacheId) || !tachesDict.ContainsKey(tacheId))
                return new List<string>();

            visite.Add(tacheId);
            enCours.Add(tacheId);
            chemin.Add(tacheId);

            var dependances = ObtenirDependancesExistantes(tachesDict[tacheId].Dependencies);

            foreach (var dependance in dependances)
            {
                var cycle = DetecterCycleRecursif(dependance, tachesDict, visite, enCours, new List<string>(chemin));
                if (cycle.Any())
                    return cycle;
            }

            enCours.Remove(tacheId);
            return new List<string>();
        }

        /// <summary>
        /// Obtient des statistiques sur les dépendances
        /// </summary>
        public StatistiquesDependances ObtenirStatistiques(IReadOnlyList<TacheRecord> tachesBrutes, IReadOnlyList<TacheRecord> tachesOptimisees)
        {
            var dependancesBrutes = tachesBrutes.Sum(t => ObtenirDependancesExistantes(t.Dependencies).Count);
            var dependancesOptimisees = tachesOptimisees.Sum(t => ObtenirDependancesExistantes(t.Dependencies).Count);

            return new StatistiquesDependances
            {
                NombreDependancesOriginales = dependancesBrutes,
                NombreDependancesOptimisees = dependancesOptimisees,
                NombreDependancesSupprimees = dependancesBrutes - dependancesOptimisees,
                PourcentageReduction = dependancesBrutes > 0 ? ((double)(dependancesBrutes - dependancesOptimisees) / dependancesBrutes) * 100 : 0
            };
        }
    }

    /// <summary>
    /// Statistiques sur les optimisations de dépendances
    /// </summary>
    public class StatistiquesDependances
    {
        public int NombreDependancesOriginales { get; set; }
        public int NombreDependancesOptimisees { get; set; }
        public int NombreDependancesSupprimees { get; set; }
        public double PourcentageReduction { get; set; }
    }
}
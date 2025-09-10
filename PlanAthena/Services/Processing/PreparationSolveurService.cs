// Fichier: Services/Processing/PreparationSolveurService.cs V0.4.9.1 (Corrigé)
using PlanAthena.Data;
using PlanAthena.Services.Business.DTOs;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthena.Services.Processing
{
    /// <summary>
    /// Service responsable de la préparation technique des données pour le solveur.
    /// Gère le filtrage des tâches, le découpage, et la création de jalons techniques/proxys.
    /// Ce service est une pure fonction de transformation sans effet de bord.
    /// </summary>
    public class PreparationSolveurService
    {
        private const string JALON_TECHNIQUE_PREFIX = "JT_";
        private const string DECOUPAGE_SUFFIX = "_P";

        public PreparationSolveurService() { }

        /// <summary>
        /// Prépare les tâches pour le solveur en filtrant les tâches non pertinentes,
        /// en les découpant et en gérant les dépendances vers des tâches déjà terminées.
        /// </summary>
        /// <param name="tachesDuProjet">Liste de TOUTES les tâches du projet.</param>
        /// <returns>Résultat contenant les tâches prêtes pour le solveur et la table de mappage.</returns>
        public virtual PreparationResult PreparerPourSolveur(IReadOnlyList<Tache> tachesDuProjet, ConfigurationPlanification configuration)
        {
            if (tachesDuProjet == null || !tachesDuProjet.Any())
                return new PreparationResult();

            var tachesAPlanifier = tachesDuProjet
                .Where(t => t.Statut != Statut.Terminée && t.Statut != Statut.EnCours)
                .ToList();

            var tachesDejaTraiteesMap = tachesDuProjet
                .Where(t => t.Statut == Statut.Terminée || t.Statut == Statut.EnCours)
                .ToDictionary(t => t.TacheId);

            if (!tachesAPlanifier.Any())
                return new PreparationResult();

            int maxHeuresParSousTache = configuration.HeuresTravailEffectifParJour;
            int heureLimiteDecoupage = (maxHeuresParSousTache * configuration.SeuilJoursDecoupageTache) + 1;

            var tachesDeTravail = tachesAPlanifier.Select(CopierTache).ToList();
            var (tachesDecoupees, tableDecoupage) = DecouperTachesLongues(tachesDeTravail, heureLimiteDecoupage, maxHeuresParSousTache);

            // CORRECTION: Construire la table de mappage ICI, avant qu'elle ne soit modifiée.
            var parentIdParSousTacheId = ConstruireTableMappageInversee(tableDecoupage);

            var tachesAvecJalons = CreerJalonsTechniques(tachesDecoupees, tableDecoupage);
            var tachesFinales = MettreAJourDependances(tachesAvecJalons, tableDecoupage, tachesDejaTraiteesMap);

            return new PreparationResult
            {
                TachesPreparees = tachesFinales,
                ParentIdParSousTacheId = parentIdParSousTacheId // Utilisation de la table correcte
            };
        }

        private static Dictionary<string, string> ConstruireTableMappageInversee(Dictionary<string, List<string>> tableDecoupage)
        {
            var mappageInverse = new Dictionary<string, string>();
            foreach (var kvp in tableDecoupage)
            {
                var parentId = kvp.Key;
                foreach (var sousTacheId in kvp.Value)
                {
                    mappageInverse[sousTacheId] = parentId;
                }
            }
            return mappageInverse;
        }

        private static (List<Tache> TachesDecoupees, Dictionary<string, List<string>> TableDecoupage) DecouperTachesLongues(
            IReadOnlyList<Tache> taches, int heureLimiteDecoupage, int maxHeuresParSousTache)
        {
            var tachesResultat = new List<Tache>();
            var tableDecoupage = new Dictionary<string, List<string>>();
            foreach (var tache in taches)
            {
                if (tache.EstJalon || tache.HeuresHommeEstimees < heureLimiteDecoupage)
                {
                    tachesResultat.Add(tache);
                }
                else
                {
                    var sousTaches = DecouperTacheUnique(tache, maxHeuresParSousTache);
                    tachesResultat.AddRange(sousTaches);
                    tableDecoupage[tache.TacheId] = sousTaches.Select(st => st.TacheId).ToList();
                }
            }
            return (tachesResultat, tableDecoupage);
        }

        private static List<Tache> DecouperTacheUnique(Tache tacheOriginale, int maxHeuresParSousTache)
        {
            var sousTaches = new List<Tache>();
            int heuresRestantes = tacheOriginale.HeuresHommeEstimees;
            int compteur = 1;
            while (heuresRestantes > 0)
            {
                int heuresPourCeBloc = Math.Min(heuresRestantes, maxHeuresParSousTache);
                string nouvelId = $"{tacheOriginale.TacheId}{DECOUPAGE_SUFFIX}{compteur}";
                var sousTache = CopierTache(tacheOriginale);
                sousTache.TacheId = nouvelId;
                sousTache.TacheNom = $"{tacheOriginale.TacheNom} (Partie {compteur})";
                sousTache.HeuresHommeEstimees = heuresPourCeBloc;
                sousTache.Type = TypeActivite.Tache;
                sousTache.Dependencies = tacheOriginale.Dependencies;
                sousTaches.Add(sousTache);
                heuresRestantes -= heuresPourCeBloc;
                compteur++;
            }
            return sousTaches;
        }

        private static List<Tache> CreerJalonsTechniques(List<Tache> taches, Dictionary<string, List<string>> tableDecoupage)
        {
            var tachesAvecJalons = new List<Tache>(taches);
            foreach (var originalId in tableDecoupage.Keys)
            {
                string idJalon = $"{JALON_TECHNIQUE_PREFIX}{originalId}";
                var sousTachesIds = tableDecoupage[originalId];
                var tacheRef = taches.First(t => t.TacheId == sousTachesIds[0]);
                var jalon = new Tache
                {
                    TacheId = idJalon,
                    TacheNom = $"Convergence technique de {originalId}",
                    Type = TypeActivite.JalonTechnique,
                    HeuresHommeEstimees = 0,
                    Dependencies = string.Join(",", sousTachesIds),
                    MetierId = "",
                    BlocId = tacheRef.BlocId,
                    LotId = tacheRef.LotId
                };
                tachesAvecJalons.Add(jalon);
                tableDecoupage[originalId] = new List<string> { idJalon };
            }
            return tachesAvecJalons;
        }

        // --- MÉTHODE CORRIGÉE ---
        // Signature modifiée pour accepter un dictionnaire de Tache au lieu d'un Set d'IDs
        private static List<Tache> MettreAJourDependances(List<Tache> taches, Dictionary<string, List<string>> tableDecoupage, IReadOnlyDictionary<string, Tache> tachesDejaTraiteesMap)
        {
            var tachesResultat = new List<Tache>(taches);
            var nouveauxJalonsProxy = new Dictionary<string, Tache>();
            var idsTachesExistantes = taches.Select(t => t.TacheId).ToHashSet();

            foreach (var tache in tachesResultat)
            {
                if (string.IsNullOrEmpty(tache.Dependencies)) continue;

                var nouvellesDeps = new HashSet<string>();
                var anciennesDeps = tache.Dependencies.Split(',').Select(d => d.Trim()).Where(d => !string.IsNullOrEmpty(d));

                foreach (var depId in anciennesDeps)
                {
                    if (tableDecoupage.TryGetValue(depId, out var nouvellesRefs))
                    {
                        nouvellesDeps.Add(nouvellesRefs[^1]);
                    }
                    // CORRECTION: On utilise TryGetValue sur le dictionnaire pour récupérer la tâche originale
                    else if (tachesDejaTraiteesMap.TryGetValue(depId, out var tacheOriginaleProxifiee))
                    {
                        string idJalonProxy = $"{JALON_TECHNIQUE_PREFIX}{depId}";
                        if (!idsTachesExistantes.Contains(idJalonProxy) && !nouveauxJalonsProxy.ContainsKey(idJalonProxy))
                        {
                            // CORRECTION: On peuple LotId et BlocId depuis la tâche originale
                            nouveauxJalonsProxy[idJalonProxy] = new Tache
                            {
                                TacheId = idJalonProxy,
                                TacheNom = $"Proxy pour tâche terminée {depId}",
                                Type = TypeActivite.JalonTechnique,
                                HeuresHommeEstimees = 0,
                                LotId = tacheOriginaleProxifiee.LotId,
                                BlocId = tacheOriginaleProxifiee.BlocId
                            };
                        }
                        nouvellesDeps.Add(idJalonProxy);
                    }
                    else
                    {
                        nouvellesDeps.Add(depId);
                    }
                }
                tache.Dependencies = string.Join(",", nouvellesDeps);
            }

            tachesResultat.AddRange(nouveauxJalonsProxy.Values);
            return tachesResultat;
        }

        private static Tache CopierTache(Tache source)
        {
            return new Tache
            {
                TacheId = source.TacheId,
                TacheNom = source.TacheNom,
                HeuresHommeEstimees = source.HeuresHommeEstimees,
                MetierId = source.MetierId,
                Dependencies = source.Dependencies,
                ExclusionsDependances = source.ExclusionsDependances,
                LotId = source.LotId,
                BlocId = source.BlocId,
                Type = source.Type
            };
        }
    }
}
// Fichier: Services/Processing/PreparationSolveurService.cs
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Data;

namespace PlanAthena.Services.Processing
{
    /// <summary>
    /// Service responsable de la préparation technique des données pour le solveur.
    /// Gère le découpage des tâches longues pour parallélisation et la création des jalons techniques pour la convergence.
    /// Ce service ne produit aucun log, il se contente de transformer les données.
    /// </summary>
    public class PreparationSolveurService
    {
        
        private const string JALON_TECHNIQUE_PREFIX = "JT_";
        private const string DECOUPAGE_SUFFIX = "_P";

        public PreparationSolveurService() { }

        /// <summary>
        /// Prépare les tâches pour le solveur et retourne le résultat complet avec la table de mappage
        /// </summary>
        /// <param name="tachesDuProjet">Liste des tâches originales du projet</param>
        /// <returns>Résultat contenant les tâches préparées et la table de mappage parent/enfant</returns>
        public virtual PreparationResult PreparerPourSolveur(IReadOnlyList<Tache> tachesDuProjet, ConfigurationPlanification configuration)
        {
            if (tachesDuProjet == null || !tachesDuProjet.Any())
                return new PreparationResult();
            int maxHeuresParSousTache = configuration.HeuresTravailEffectifParJour;
            int heureLimiteDecoupage = (maxHeuresParSousTache * configuration.SeuilJoursDecoupageTache) + 1;

            var tachesDeTravail = tachesDuProjet.Select(CopierTache).ToList();
            var (tachesDecoupees, tableDecoupage) = DecouperTachesLongues(tachesDeTravail, heureLimiteDecoupage, maxHeuresParSousTache);
            var tachesAvecJalons = CreerJalonsTechniques(tachesDecoupees, tableDecoupage);
            var tachesFinales = MettreAJourDependances(tachesAvecJalons, tableDecoupage);

            // Création de la table de mappage inversée
            var parentIdParSousTacheId = ConstruireTableMappageInversee(tableDecoupage);

            return new PreparationResult
            {
                TachesPreparees = tachesFinales,
                ParentIdParSousTacheId = parentIdParSousTacheId
            };
        }

        /// <summary>
        /// Construit la table de mappage inversée : ID sous-tâche -> ID tâche mère
        /// </summary>
        private static Dictionary<string, string> ConstruireTableMappageInversee(Dictionary<string, List<string>> tableDecoupage)
        {
            var mappageInverse = new Dictionary<string, string>();

            foreach (var kvp in tableDecoupage)
            {
                var parentId = kvp.Key;
                var sousTaskIds = kvp.Value;

                foreach (var sousTacheId in sousTaskIds)
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


                // On s'assure que les dépendances de la tâche originale sont bien reportées sur chaque sous-tâche.
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

        private static List<Tache> MettreAJourDependances(List<Tache> taches, Dictionary<string, List<string>> tableDecoupage)
        {
            foreach (var tache in taches)
            {
                if (string.IsNullOrEmpty(tache.Dependencies))
                {
                    continue;
                }

                var nouvellesDeps = new HashSet<string>();
                var anciennesDeps = tache.Dependencies.Split(',').Select(d => d.Trim()).Where(d => !string.IsNullOrEmpty(d));

                foreach (var depId in anciennesDeps)
                {
                    if (tableDecoupage.TryGetValue(depId, out var nouvellesRefs))
                    {
                        nouvellesDeps.Add(nouvellesRefs[^1]);
                    }
                    else
                    {
                        nouvellesDeps.Add(depId);
                    }
                }
                tache.Dependencies = string.Join(",", nouvellesDeps);
            }
            return taches;
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
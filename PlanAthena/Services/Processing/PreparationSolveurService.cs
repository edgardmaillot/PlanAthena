// Fichier: Services/Processing/PreparationSolveurService.cs

using PlanAthena.Data;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System;
using PlanAthena.Services.Business;

namespace PlanAthena.Services.Processing
{
    /// <summary>
    /// Service responsable de la préparation technique des données pour le solveur.
    /// Gère le découpage des tâches longues pour parallélisation et la création des jalons techniques pour la convergence.
    /// </summary>
    public class PreparationSolveurService
    {
        private const int HEURE_LIMITE_DECOUPAGE = 8;
        private const int MAX_HEURES_PAR_SOUS_TACHE = 7;
        private const string JALON_TECHNIQUE_PREFIX = "JT_";
        private const string DECOUPAGE_SUFFIX = "_P";

        // Constructeur vide car les méthodes de traitement sont statiques ou n'ont pas de dépendances d'état.
        public PreparationSolveurService() { }

        public List<Tache> PreparerPourSolveur(IReadOnlyList<Tache> tachesDuProjet)
        {
            if (tachesDuProjet == null || tachesDuProjet.Count == 0)
                return new List<Tache>();

            LogTaches("État initial avant préparation :", tachesDuProjet);

            var tachesDeTravail = tachesDuProjet.Select(CopierTache).ToList();

            var (tachesDecoupees, tableDecoupage) = DecouperTachesLongues(tachesDeTravail);
            LogTaches("État après découpage des tâches longues :", tachesDecoupees, tableDecoupage);

            var tachesAvecJalons = CreerJalonsTechniques(tachesDecoupees, tableDecoupage);
            LogTaches("État après création des jalons techniques :", tachesAvecJalons, tableDecoupage);

            var tachesFinales = MettreAJourDependances(tachesAvecJalons, tableDecoupage);
            LogTaches("État final prêt pour le solveur :", tachesFinales);

            return tachesFinales;
        }

        private static (List<Tache> TachesDecoupees, Dictionary<string, List<string>> TableDecoupage) DecouperTachesLongues(IReadOnlyList<Tache> taches)
        {
            var tachesResultat = new List<Tache>();
            var tableDecoupage = new Dictionary<string, List<string>>();

            foreach (var tache in taches)
            {
                if (tache.Type == TypeActivite.JalonUtilisateur || tache.HeuresHommeEstimees <= HEURE_LIMITE_DECOUPAGE)
                {
                    tachesResultat.Add(tache);
                }
                else
                {
                    var sousTaches = DecouperTacheUnique(tache);
                    tachesResultat.AddRange(sousTaches);
                    tableDecoupage[tache.TacheId] = sousTaches.Select(st => st.TacheId).ToList();
                }
            }
            return (tachesResultat, tableDecoupage);
        }

        private static List<Tache> DecouperTacheUnique(Tache tacheOriginale)
        {
            var sousTaches = new List<Tache>();
            int heuresRestantes = tacheOriginale.HeuresHommeEstimees;
            int compteur = 1;

            while (heuresRestantes > 0)
            {
                int heuresPourCeBloc = Math.Min(heuresRestantes, MAX_HEURES_PAR_SOUS_TACHE);
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
                        nouvellesDeps.Add(nouvellesRefs[^1]); // ^1 est l'équivalent de .Last() mais plus performant
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

        [Conditional("DEBUG")]
        private static void LogTaches(string titre, IReadOnlyList<Tache> taches, Dictionary<string, List<string>>? tableDecoupage = null)
        {
            Debug.WriteLine($"\n--- {titre} ({taches.Count} éléments) ---");
            foreach (var tache in taches.OrderBy(t => t.TacheId))
            {
                Debug.WriteLine($"  - ID: {tache.TacheId,-40} | Nom: {tache.TacheNom,-50} | Type: {tache.Type,-15} | Durée: {tache.HeuresHommeEstimees,2}h | Dépend de: [{tache.Dependencies}]");
            }

            if (tableDecoupage != null && tableDecoupage.Count > 0)
            {
                Debug.WriteLine("\n  --- Table de Découpage Actuelle ---");
                foreach (var kvp in tableDecoupage.OrderBy(k => k.Key))
                {
                    Debug.WriteLine($"    - Origine: {kvp.Key,-40} -> Mappe vers: [{string.Join(", ", kvp.Value)}]");
                }
            }
            Debug.WriteLine("--- Fin de la section ---\n");
        }
    }
}
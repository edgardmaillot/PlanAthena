using PlanAthena.Data;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthena.Services.Processing
{
    /// <summary>
    /// Service responsable de la préparation technique des données pour le solveur.
    /// Gère le découpage des tâches longues et la création des tâches de regroupement.
    /// </summary>
    public class PreparationSolveurService
    {
        private const int HEURE_LIMITE_DECOUPAGE = 8;
        private const int MAX_HEURES_PAR_SOUS_TACHE = 7;
        private const string TACHE_REGROUPEMENT_PREFIX = "JT_"; // JT pour "Jonction Technique"
        private const string DECOUPAGE_SUFFIX = "_P";

        // Le service n'a plus besoin de MetierService
        public PreparationSolveurService() { }

        public List<Tache> PreparerPourSolveur(IReadOnlyList<Tache> tachesDuProjet)
        {
            if (tachesDuProjet == null || !tachesDuProjet.Any())
                return new List<Tache>();

            var (tachesDecoupees, tableDecoupage) = DecouperTachesLongues(tachesDuProjet);
            var tachesAvecRegroupement = CreerTachesDeRegroupement(tachesDecoupees, tableDecoupage);
            var tachesFinales = MettreAJourDependances(tachesAvecRegroupement, tableDecoupage);

            return tachesFinales;
        }

        // ... (Les méthodes privées DecouperTachesLongues, CreerTachesDeRegroupement, etc. suivent)
        // Note : J'ai renommé JalonTechnique en TacheDeRegroupement pour la clarté.

        private (List<Tache> TachesDecoupees, Dictionary<string, List<string>> TableDecoupage) DecouperTachesLongues(IReadOnlyList<Tache> taches)
        {
            var tachesDecoupees = new List<Tache>();
            var tableDecoupage = new Dictionary<string, List<string>>();

            foreach (var tache in taches)
            {
                if (tache.EstJalon || tache.HeuresHommeEstimees <= HEURE_LIMITE_DECOUPAGE)
                {
                    tachesDecoupees.Add(CopierTache(tache));
                }
                else
                {
                    var sousTaches = DecouperTacheUnique(tache);
                    tachesDecoupees.AddRange(sousTaches);
                    tableDecoupage[tache.TacheId] = sousTaches.Select(st => st.TacheId).ToList();
                }
            }
            return (tachesDecoupees, tableDecoupage);
        }

        private List<Tache> DecouperTacheUnique(Tache tacheOriginale)
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
                sousTache.Type = TypeActivite.Tache; // Une sous-tâche n'est jamais un jalon

                if (compteur == 1)
                {
                    sousTache.Dependencies = tacheOriginale.Dependencies;
                }
                else
                {
                    sousTache.Dependencies = $"{tacheOriginale.TacheId}{DECOUPAGE_SUFFIX}{compteur - 1}";
                }
                sousTaches.Add(sousTache);
                heuresRestantes -= heuresPourCeBloc;
                compteur++;
            }
            return sousTaches;
        }

        private List<Tache> CreerTachesDeRegroupement(List<Tache> taches, Dictionary<string, List<string>> tableDecoupage)
        {
            var tachesFinales = new List<Tache>(taches);
            var regroupementsCrees = new Dictionary<string, string>();

            var dependants = new Dictionary<string, List<string>>();
            foreach (var tache in taches)
            {
                if (string.IsNullOrEmpty(tache.Dependencies)) continue;
                var deps = tache.Dependencies.Split(',').Select(d => d.Trim()).Where(d => !string.IsNullOrEmpty(d));
                foreach (var dep in deps)
                {
                    if (tableDecoupage.ContainsKey(dep))
                    {
                        if (!dependants.ContainsKey(dep)) dependants[dep] = new List<string>();
                        dependants[dep].Add(tache.TacheId);
                    }
                }
            }

            foreach (var kvp in dependants.Where(d => d.Value.Count > 1))
            {
                string tacheOriginaleId = kvp.Key;
                if (!tableDecoupage.TryGetValue(tacheOriginaleId, out var sousTaches)) continue;

                string idRegroupement = $"{TACHE_REGROUPEMENT_PREFIX}{tacheOriginaleId}";
                var dernierePartie = sousTaches.Last();
                var tacheRef = taches.First(t => t.TacheId == dernierePartie);

                var tacheRegroupement = new Tache
                {
                    TacheId = idRegroupement,
                    TacheNom = $"Regroupement de {tacheOriginaleId}",
                    Type = TypeActivite.Tache,
                    HeuresHommeEstimees = 0,
                    Dependencies = dernierePartie,
                    BlocId = tacheRef.BlocId,
                    BlocNom = tacheRef.BlocNom,
                    LotId = tacheRef.LotId,
                    LotNom = tacheRef.LotNom,
                    LotPriorite = tacheRef.LotPriorite,
                    BlocCapaciteMaxOuvriers = tacheRef.BlocCapaciteMaxOuvriers
                };
                tachesFinales.Add(tacheRegroupement);
                regroupementsCrees[tacheOriginaleId] = idRegroupement;
            }

            foreach (var kvp in regroupementsCrees)
            {
                tableDecoupage[kvp.Key] = new List<string> { kvp.Value };
            }

            return tachesFinales;
        }

        private List<Tache> MettreAJourDependances(List<Tache> taches, Dictionary<string, List<string>> tableDecoupage)
        {
            var tachesFinales = new List<Tache>();
            foreach (var tache in taches)
            {
                if (string.IsNullOrEmpty(tache.Dependencies))
                {
                    tachesFinales.Add(tache);
                    continue;
                }
                var nouvellesDeps = new List<string>();
                var anciennesDeps = tache.Dependencies.Split(',').Select(d => d.Trim()).Where(d => !string.IsNullOrEmpty(d));

                foreach (var dep in anciennesDeps)
                {
                    if (tableDecoupage.TryGetValue(dep, out var nouvelleDep))
                    {
                        nouvellesDeps.Add(nouvelleDep.Last());
                    }
                    else
                    {
                        nouvellesDeps.Add(dep);
                    }
                }
                var tacheMiseAJour = CopierTache(tache);
                tacheMiseAJour.Dependencies = string.Join(",", nouvellesDeps.Distinct());
                tachesFinales.Add(tacheMiseAJour);
            }
            return tachesFinales;
        }

        private Tache CopierTache(Tache source)
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
                LotNom = source.LotNom,
                LotPriorite = source.LotPriorite,
                BlocId = source.BlocId,
                BlocNom = source.BlocNom,
                BlocCapaciteMaxOuvriers = source.BlocCapaciteMaxOuvriers,
                Type = source.Type
            };
        }
    }
}
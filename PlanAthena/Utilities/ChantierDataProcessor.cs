// Utilities/ChantierDataProcessor.cs (Version utilisant MetierService)
using PlanAthena.CsvModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanAthena.Utilities
{
    public class ChantierDataProcessor
    {
        private const int HEURE_LIMITE_DECOUPAGE = 8;
        private const int MAX_HEURES_PAR_SOUS_TACHE = 7;

        /// <summary>
        /// Orchestre toutes les étapes de pré-traitement des tâches en utilisant les services fournis.
        /// </summary>
        public List<TacheCsvRecord> ProcessTaches(IReadOnlyList<TacheCsvRecord> rawTaches, MetierService metierService)
        {
            // Étape 1 : Découper les tâches longues
            var (tachesDecoupees, tableReliaisonDecoupage) = DecouperTachesLongues(rawTaches);

            // Étape 2 : Matérialiser les dépendances métier en utilisant le MetierService
            var (tachesAvecDepsMetier, tachesFictives) = MaterialiserDependancesMetier(tachesDecoupees, metierService);

            var toutesLesTaches = tachesAvecDepsMetier.Concat(tachesFictives).ToList();

            // Étape 3 : Re-lier les dépendances qui pointaient vers des tâches découpées
            var tachesFinales = RelierDependances(toutesLesTaches, tableReliaisonDecoupage);

            return tachesFinales;
        }

        // --- PHASE 1 : Découpage des Tâches (inchangée) ---

        private (List<TacheCsvRecord> TachesDecoupees, Dictionary<string, List<string>> TableReliaison) DecouperTachesLongues(IReadOnlyList<TacheCsvRecord> rawTaches)
        {
            var tachesIntermediaires = new List<TacheCsvRecord>();
            var tableDeReliaison = new Dictionary<string, List<string>>();

            foreach (var tache in rawTaches)
            {
                if (tache.HeuresHommeEstimees > HEURE_LIMITE_DECOUPAGE)
                {
                    var sousTaches = SplitSingleTask(tache);
                    tachesIntermediaires.AddRange(sousTaches);
                    tableDeReliaison[tache.TacheId] = sousTaches.Select(st => st.TacheId).ToList();
                }
                else
                {
                    tachesIntermediaires.Add(tache);
                }
            }
            return (tachesIntermediaires, tableDeReliaison);
        }

        private List<TacheCsvRecord> SplitSingleTask(TacheCsvRecord originalTache)
        {
            var nouvellesTaches = new List<TacheCsvRecord>();
            int heuresRestantes = originalTache.HeuresHommeEstimees;
            int compteur = 1;

            while (heuresRestantes > 0)
            {
                int heuresPourCeBloc = Math.Min(heuresRestantes, MAX_HEURES_PAR_SOUS_TACHE);
                string nouvelId = $"{originalTache.TacheId}_split_{compteur}";

                var nouvelleSousTache = CopierTache(originalTache);
                nouvelleSousTache.TacheId = nouvelId;
                nouvelleSousTache.TacheNom = $"{originalTache.TacheNom} (Partie {compteur})";
                nouvelleSousTache.HeuresHommeEstimees = heuresPourCeBloc;
                nouvelleSousTache.Dependencies = originalTache.Dependencies;

                nouvellesTaches.Add(nouvelleSousTache);

                heuresRestantes -= heuresPourCeBloc;
                compteur++;
            }
            return nouvellesTaches;
        }

        // --- PHASE 2 : Matérialisation des Dépendances Métier (modifiée pour utiliser MetierService) ---

        private (List<TacheCsvRecord> TachesMisesAJour, List<TacheCsvRecord> TachesFictives) MaterialiserDependancesMetier(IReadOnlyList<TacheCsvRecord> taches, MetierService metierService)
        {
            var tachesParBloc = taches.GroupBy(t => t.BlocId).ToList();
            var tachesMisesAJour = new List<TacheCsvRecord>();
            var tachesFictivesGlobales = new List<TacheCsvRecord>();

            foreach (var groupeBloc in tachesParBloc)
            {
                var blocId = groupeBloc.Key;
                var tachesDuBloc = groupeBloc.ToList();
                var noeudsDeSynchro = new Dictionary<string, string>();
                var tachesFictivesDuBloc = new List<TacheCsvRecord>();

                var metiersPrerequisDansBloc = tachesDuBloc
                    .SelectMany(t => metierService.GetPrerequisForMetier(t.MetierId))
                    .Distinct()
                    .ToList();

                foreach (var metierPrerequisId in metiersPrerequisDansBloc)
                {
                    var tachesQuiLeConstituent = tachesDuBloc.Where(t => t.MetierId == metierPrerequisId).ToList();
                    if (tachesQuiLeConstituent.Any())
                    {
                        string idTacheFictive = $"Sync_{metierPrerequisId}_{blocId}";
                        var tacheDeReference = groupeBloc.First();

                        var tacheFictive = new TacheCsvRecord
                        {
                            TacheId = idTacheFictive,
                            TacheNom = $"Fin du métier {metierPrerequisId} dans le bloc {tacheDeReference.BlocNom}",
                            HeuresHommeEstimees = 0,
                            Dependencies = string.Join(",", tachesQuiLeConstituent.Select(t => t.TacheId)),
                            BlocId = blocId,
                            BlocNom = tacheDeReference.BlocNom,
                            // On demande au service de nous fournir l'ID du métier de synchro 0h
                            MetierId = metierService.GetOrCreateSyncMetierId(0),
                            LotId = tacheDeReference.LotId,
                            LotNom = tacheDeReference.LotNom,
                            LotPriorite = tacheDeReference.LotPriorite,
                            BlocCapaciteMaxOuvriers = tacheDeReference.BlocCapaciteMaxOuvriers
                        };
                        tachesFictivesDuBloc.Add(tacheFictive);
                        noeudsDeSynchro[metierPrerequisId] = idTacheFictive;
                    }
                }

                foreach (var tache in tachesDuBloc)
                {
                    var dependancesInitiales = tache.Dependencies?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() ?? new List<string>();
                    var prerequisMetierPourCetteTache = metierService.GetPrerequisForMetier(tache.MetierId);

                    foreach (var prerequisId in prerequisMetierPourCetteTache)
                    {
                        if (noeudsDeSynchro.TryGetValue(prerequisId, out var idTacheFictive))
                        {
                            dependancesInitiales.Add(idTacheFictive);
                        }
                    }

                    var tacheMiseAJour = CopierTache(tache);
                    tacheMiseAJour.Dependencies = string.Join(",", dependancesInitiales.Distinct());
                    tachesMisesAJour.Add(tacheMiseAJour);
                }

                tachesFictivesGlobales.AddRange(tachesFictivesDuBloc);
            }

            return (tachesMisesAJour, tachesFictivesGlobales);
        }

        // --- PHASE 3 : Re-liaison des Dépendances (inchangée) ---

        private List<TacheCsvRecord> RelierDependances(IReadOnlyList<TacheCsvRecord> taches, Dictionary<string, List<string>> tableReliaisonDecoupage)
        {
            if (!tableReliaisonDecoupage.Any()) return taches.ToList();

            var tachesFinales = new List<TacheCsvRecord>();
            foreach (var tache in taches)
            {
                if (string.IsNullOrEmpty(tache.Dependencies))
                {
                    tachesFinales.Add(tache);
                    continue;
                }

                var anciennesDependances = tache.Dependencies.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var nouvellesDependances = new List<string>();

                foreach (var depId in anciennesDependances)
                {
                    if (tableReliaisonDecoupage.TryGetValue(depId, out var idsSousTaches))
                    {
                        nouvellesDependances.AddRange(idsSousTaches);
                    }
                    else
                    {
                        nouvellesDependances.Add(depId);
                    }
                }

                var tacheMiseAJour = CopierTache(tache);
                tacheMiseAJour.Dependencies = string.Join(",", nouvellesDependances.Distinct());
                tachesFinales.Add(tacheMiseAJour);
            }
            return tachesFinales;
        }

        // --- UTILITAIRE (inchangé) ---
        private TacheCsvRecord CopierTache(TacheCsvRecord source)
        {
            return new TacheCsvRecord
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
    }
}

using PlanAthena.Data;
using PlanAthena.Services.Business; // Gardé pour MetierService dans le constructeur (si besoin futur)

namespace PlanAthena.Utilities
{
    public class ChantierDataProcessor
    {
        private readonly MetierService _metierService;
        private const int HEURE_LIMITE_DECOUPAGE = 8;
        private const int MAX_HEURES_PAR_SOUS_TACHE = 7;

        // Le constructeur peut prendre MetierService pour créer les tâches de synchro
        public ChantierDataProcessor(MetierService metierService)
        {
            _metierService = metierService ?? throw new ArgumentNullException(nameof(metierService));
        }

        /// <summary>
        /// Prépare les tâches pour le solveur : crée les nœuds de synchro et découpe les tâches longues.
        /// PRE-REQUIS: Les tâches en entrée doivent déjà avoir leurs dépendances métier matérialisées.
        /// </summary>
        public List<Tache> ProcessTachesPourSolveur(IReadOnlyList<Tache> tachesAvecDependances)
        {
            // Étape 1 : Créer les tâches de synchronisation à partir des dépendances existantes
            var (tachesAvecSynchro, tachesFictives) = CreerTachesDeSynchro(tachesAvecDependances);

            var toutesLesTaches = tachesAvecSynchro.Concat(tachesFictives).ToList();

            // Étape 2 : Découper les tâches longues (y compris les fictives si elles avaient une durée)
            var (tachesDecoupees, tableReliaisonDecoupage) = DecouperTachesLongues(toutesLesTaches);

            // Étape 3 : Re-lier les dépendances qui pointaient vers des tâches découpées
            var tachesFinales = RelierDependances(tachesDecoupees, tableReliaisonDecoupage);

            return tachesFinales;
        }

        private (List<Tache> TachesMisesAJour, List<Tache> TachesFictives) CreerTachesDeSynchro(IReadOnlyList<Tache> taches)
{
    var tachesParBloc = taches.GroupBy(t => t.BlocId).ToList();
    var tachesMisesAJourGlobal = new List<Tache>();
    var tachesFictivesGlobales = new List<Tache>();
    var mapTaches = taches.ToDictionary(t => t.TacheId);

    foreach (var groupeBloc in tachesParBloc)
    {
        var blocId = groupeBloc.Key;
        var tachesDuBloc = groupeBloc.ToList();
        var noeudsDeSynchro = new Dictionary<string, string>();
        var tachesFictivesDuBloc = new List<Tache>();

        // Identifier les métiers qui servent de prérequis à D'AUTRES métiers dans ce bloc
        var metiersPrerequisIds = new HashSet<string>();
        foreach (var tache in tachesDuBloc)
        {
            var dependancesIds = tache.Dependencies?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? Enumerable.Empty<string>();
            foreach (var depId in dependancesIds)
            {
                if (mapTaches.TryGetValue(depId, out var tachePrecedente) && tachePrecedente.MetierId != tache.MetierId)
                {
                    if (!string.IsNullOrEmpty(tachePrecedente.MetierId))
                    {
                        metiersPrerequisIds.Add(tachePrecedente.MetierId);
                    }
                }
            }
        }

        // Créer une tâche de synchro pour chaque métier prérequis identifié
        foreach (var metierId in metiersPrerequisIds)
        {
            var tachesDuMetier = tachesDuBloc.Where(t => t.MetierId == metierId).ToList();
            if (tachesDuMetier.Any())
            {
                string idTacheFictive = $"Sync_{metierId}_{blocId}";
                var tacheDeReference = tachesDuMetier.First();

                        var tacheFictive = new Tache
                        {
                            TacheId = idTacheFictive,
                            TacheNom = $"Fin du métier {metierId} dans le bloc {tacheDeReference.BlocNom}",
                            Type = TypeActivite.JalonDeSynchronisation,
                            HeuresHommeEstimees = 0,
                            Dependencies = string.Join(",", tachesDuMetier.Select(t => t.TacheId)),
                            BlocId = blocId,
                            BlocNom = tacheDeReference.BlocNom,
                            MetierId = "",
                            LotId = tacheDeReference.LotId,
                            LotNom = tacheDeReference.LotNom,
                            LotPriorite = tacheDeReference.LotPriorite,
                            BlocCapaciteMaxOuvriers = tacheDeReference.BlocCapaciteMaxOuvriers
                        };
                        tachesFictivesDuBloc.Add(tacheFictive);
                noeudsDeSynchro[metierId] = idTacheFictive;
            }
        }

        // Remplacer les multiples dépendances vers un métier par une seule dépendance vers la tâche de synchro
        var tachesMisesAJourDuBloc = new List<Tache>();
        foreach (var tache in tachesDuBloc)
        {
            var dependancesInitiales = tache.Dependencies?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() ?? new List<string>();
            var nouvellesDependances = new List<string>();
            var metiersDejaLies = new HashSet<string>();

            foreach (var depId in dependancesInitiales)
            {
                if (mapTaches.TryGetValue(depId, out var tachePrecedente) && 
                    tachePrecedente.MetierId != tache.MetierId && 
                    noeudsDeSynchro.TryGetValue(tachePrecedente.MetierId, out var idTacheFictive))
                {
                    if (metiersDejaLies.Add(tachePrecedente.MetierId))
                    {
                        nouvellesDependances.Add(idTacheFictive);
                    }
                }
                else
                {
                    nouvellesDependances.Add(depId);
                }
            }
            var tacheMiseAJour = CopierTache(tache);
            tacheMiseAJour.Dependencies = string.Join(",", nouvellesDependances.Distinct());
            tachesMisesAJourDuBloc.Add(tacheMiseAJour);
        }
        
        tachesMisesAJourGlobal.AddRange(tachesMisesAJourDuBloc);
        tachesFictivesGlobales.AddRange(tachesFictivesDuBloc);
    }

    return (tachesMisesAJourGlobal, tachesFictivesGlobales);
}

        private (List<Tache> TachesDecoupees, Dictionary<string, List<string>> TableReliaison) DecouperTachesLongues(IReadOnlyList<Tache> rawTaches)
        {
            var tachesIntermediaires = new List<Tache>();
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

        private List<Tache> SplitSingleTask(Tache originalTache)
        {
            var nouvellesTaches = new List<Tache>();
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

                if (compteur > 1)
                {
                    nouvelleSousTache.Dependencies = $"{originalTache.TacheId}_split_{compteur - 1}";
                }

                nouvellesTaches.Add(nouvelleSousTache);
                heuresRestantes -= heuresPourCeBloc;
                compteur++;
            }
            return nouvellesTaches;
        }

        private List<Tache> RelierDependances(IReadOnlyList<Tache> taches, Dictionary<string, List<string>> tableReliaisonDecoupage)
        {
            if (!tableReliaisonDecoupage.Any()) return taches.ToList();

            var tachesFinales = new List<Tache>();
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
                        nouvellesDependances.Add(idsSousTaches.Last());
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

        private Tache CopierTache(Tache source)
        {
            return new Tache
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
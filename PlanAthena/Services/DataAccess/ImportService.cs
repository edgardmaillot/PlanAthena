// PlanAthena/Services/DataAccess/ImportService.cs V0.4.8

using PlanAthena.Data;
using PlanAthena.Interfaces;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using System.Diagnostics;

namespace PlanAthena.Services.DataAccess
{
    public class ImportService
    {
        private readonly ProjetService _projetService;
        private readonly RessourceService _ressourceService;
        private readonly TaskManagerService _taskManagerService; // NOUVELLE DÉPENDANCE
        private readonly IIdGeneratorService _idGenerator;
        private readonly CsvDataService _csvDataService;

        public ImportService(
            ProjetService projetService,
            RessourceService ressourceService,
            TaskManagerService taskManagerService, // NOUVELLE DÉPENDANCE
            IIdGeneratorService idGenerator,
            CsvDataService csvDataService)
        {
            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService));
            _ressourceService = ressourceService ?? throw new ArgumentNullException(nameof(ressourceService));
            _taskManagerService = taskManagerService ?? throw new ArgumentNullException(nameof(taskManagerService)); // NOUVELLE DÉPENDANCE
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
            _csvDataService = csvDataService ?? throw new ArgumentNullException(nameof(csvDataService));
        }

        #region Import Tâches

        public ImportResult ImporterTachesCSV(string filePath, string lotIdCible, ImportMappingConfiguration mappingConfig, bool confirmerEcrasement = false)
        {
            var stopwatch = Stopwatch.StartNew();
            var allWarnings = new List<string>();

            try
            {
                var lot = _projetService.ObtenirLotParId(lotIdCible);
                if (lot == null)
                    return ImportResult.Echec($"Le lot cible avec l'ID '{lotIdCible}' n'a pas été trouvé.");

                if (!confirmerEcrasement && _taskManagerService.ObtenirToutesLesTaches(lotId: lotIdCible).Any())
                {
                    var message = $"⚠️ ATTENTION : L'import dans le lot '{lot.Nom}' écrasera les tâches existantes.\n\nCette action est irréversible.\n\nConfirmer l'import ?";
                    return ImportResult.DemandeConfirmation(message);
                }

                // Orchestration de la suppression : récupérer les tâches puis les supprimer une par une.
                // 1. Vider les tâches du lot (logique existante)
                var tachesASupprimer = _taskManagerService.ObtenirToutesLesTaches(lotId: lotIdCible);
                foreach (var tache in tachesASupprimer)
                {
                    _taskManagerService.SupprimerTache(tache.TacheId);
                }

                // 2. Vider les blocs du lot (NOUVELLE LOGIQUE D'ORCHESTRATION)
                var blocsASupprimer = _projetService.ObtenirBlocsParLot(lotIdCible);
                foreach (var bloc in blocsASupprimer)
                {
                    _projetService.SupprimerBloc(bloc.BlocId);
                }

                var (nbTaches, nbBlocs, importedTasks, importWarnings) = ImporterDonneesInitialTaches(filePath, lotIdCible, mappingConfig);
                allWarnings.AddRange(importWarnings);

                var remappingWarnings = RemapperDependancesDesTaches(importedTasks);
                allWarnings.AddRange(remappingWarnings);

                stopwatch.Stop();
                return ImportResult.Succes(nbTaches, 1, nbBlocs, allWarnings, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return ImportResult.Echec($"Erreur lors de l'import: {ex.Message}");
            }
        }

        #endregion

        #region Import/Export Ouvriers

        public int ImporterOuvriersCSV(string filePath, bool remplacerExistants)
        {
            var lignesCsv = _csvDataService.ImportCsv<OuvrierCsvRecord>(filePath);

            if (remplacerExistants)
            {
                _ressourceService.ViderOuvriers();
            }

            var idExterneVersIdInterneMap = new Dictionary<string, string>();
            var ouvriersUniquesExternes = lignesCsv.GroupBy(l => l.OuvrierId).Select(g => g.First());

            foreach (var ligneUnique in ouvriersUniquesExternes)
            {
                Ouvrier? ouvrierExistant = null;
                if (!remplacerExistants)
                {
                    ouvrierExistant = _ressourceService.GetAllOuvriers()
                        .FirstOrDefault(o => o.Nom == ligneUnique.Nom && o.Prenom == ligneUnique.Prenom);
                }

                if (ouvrierExistant != null)
                {
                    idExterneVersIdInterneMap[ligneUnique.OuvrierId] = ouvrierExistant.OuvrierId;
                }
                else
                {
                    var nouvelOuvrier = _ressourceService.CreerOuvrier(ligneUnique.Prenom, ligneUnique.Nom, ligneUnique.CoutJournalier);
                    idExterneVersIdInterneMap[ligneUnique.OuvrierId] = nouvelOuvrier.OuvrierId;
                }
            }

            var ouvriersGroupes = lignesCsv.GroupBy(ligne => ligne.OuvrierId);

            foreach (var groupe in ouvriersGroupes)
            {
                string idExterne = groupe.Key;
                if (!idExterneVersIdInterneMap.TryGetValue(idExterne, out string? idInterne))
                {
                    continue;
                }

                var ouvrierAModifier = _ressourceService.GetOuvrierById(idInterne);
                if (ouvrierAModifier == null) continue;

                var premiereLigne = groupe.First();
                ouvrierAModifier.Nom = premiereLigne.Nom;
                ouvrierAModifier.Prenom = premiereLigne.Prenom;
                ouvrierAModifier.CoutJournalier = premiereLigne.CoutJournalier;

                if (remplacerExistants && idExterneVersIdInterneMap.ContainsKey(idExterne))
                {
                    ouvrierAModifier.Competences.Clear();
                }

                foreach (var ligne in groupe)
                {
                    if (!string.IsNullOrWhiteSpace(ligne.MetierId) && !ouvrierAModifier.Competences.Any(c => c.MetierId == ligne.MetierId))
                    {
                        var estPrincipal = !ouvrierAModifier.Competences.Any();
                        ouvrierAModifier.Competences.Add(new CompetenceOuvrier { MetierId = ligne.MetierId, EstMetierPrincipal = estPrincipal });
                    }
                }

                _ressourceService.ModifierOuvrier(ouvrierAModifier);
            }

            return idExterneVersIdInterneMap.Count;
        }

        public void ExporterOuvriersCSV(string filePath)
        {
            var tousLesOuvriers = _ressourceService.GetAllOuvriers();
            var recordsPourCsv = new List<OuvrierCsvRecord>();

            foreach (var ouvrier in tousLesOuvriers)
            {
                if (ouvrier.Competences.Any())
                {
                    foreach (var competence in ouvrier.Competences)
                    {
                        recordsPourCsv.Add(new OuvrierCsvRecord
                        {
                            OuvrierId = ouvrier.OuvrierId,
                            Nom = ouvrier.Nom,
                            Prenom = ouvrier.Prenom,
                            CoutJournalier = ouvrier.CoutJournalier,
                            MetierId = competence.MetierId
                        });
                    }
                }
                else
                {
                    recordsPourCsv.Add(new OuvrierCsvRecord
                    {
                        OuvrierId = ouvrier.OuvrierId,
                        Nom = ouvrier.Nom,
                        Prenom = ouvrier.Prenom,
                        CoutJournalier = ouvrier.CoutJournalier,
                        MetierId = ""
                    });
                }
            }

            _csvDataService.ExportCsv(recordsPourCsv, filePath);
        }

        #endregion

        #region Méthodes Privées de l'Import de Tâches

        private (int nbTaches, int nbBlocs, List<Tache> importedTasks, List<string> warnings) ImporterDonneesInitialTaches(
            string filePath, string lotIdCible, ImportMappingConfiguration mappingConfig)
        {
            var warnings = new List<string>();
            var blocsCrees = new HashSet<string>();
            var blocCsvNameToBlocIdAthena = new Dictionary<string, string>();
            var importedTasks = new List<Tache>();
            var lot = _projetService.ObtenirLotParId(lotIdCible);

            var allCsvData = LireCsvBrut(filePath, mappingConfig, out var headers);
            if (allCsvData == null) throw new InvalidOperationException("La lecture du fichier CSV a échoué.");

            foreach (var ligne in allCsvData)
            {
                string blocNomCsv = GetValueOrDefault(ligne, mappingConfig.CsvColumn_BlocId, mappingConfig.NomBlocParDefaut);
                if (string.IsNullOrWhiteSpace(blocNomCsv)) continue;

                if (!blocCsvNameToBlocIdAthena.ContainsKey(blocNomCsv))
                {
                    var blocAthena = _projetService.ObtenirBlocsParLot(lotIdCible).FirstOrDefault(b => b.Nom.Equals(blocNomCsv, StringComparison.OrdinalIgnoreCase));
                    if (blocAthena == null)
                    {
                        blocAthena = _projetService.CreerBloc(lotIdCible, blocNomCsv, mappingConfig.CapaciteMaxOuvriersBlocParDefaut);
                        blocsCrees.Add(blocAthena.BlocId);
                        warnings.Add($"Bloc '{blocAthena.Nom}' (ID: {blocAthena.BlocId}) créé pour le lot '{lot.Nom}'.");
                    }
                    blocCsvNameToBlocIdAthena[blocNomCsv] = blocAthena.BlocId;
                }
            }

            foreach (var ligne in allCsvData)
            {
                var ligneIndexOriginalCsv = allCsvData.IndexOf(ligne) + (mappingConfig.HasHeaderRecord ? 2 : 1);
                string tacheNom = GetValueOrDefault(ligne, mappingConfig.CsvColumn_TacheNom);
                if (string.IsNullOrWhiteSpace(tacheNom)) { warnings.Add($"Ligne {ligneIndexOriginalCsv}: Nom de tâche manquant. Ignoré."); continue; }

                string metierId = GetValueOrDefault(ligne, mappingConfig.CsvColumn_MetierId);
                if (string.IsNullOrWhiteSpace(metierId) || _ressourceService.GetMetierById(metierId) == null)
                {
                    warnings.Add($"Ligne {ligneIndexOriginalCsv}: Métier '{metierId}' invalide ou manquant pour '{tacheNom}'. Ignoré.");
                    continue;
                }

                string blocNomCsvForTache = GetValueOrDefault(ligne, mappingConfig.CsvColumn_BlocId, mappingConfig.NomBlocParDefaut);
                string blocIdAthena = blocCsvNameToBlocIdAthena[blocNomCsvForTache];
                string heuresHommeEstimeesStr = GetValueOrDefault(ligne, mappingConfig.CsvColumn_HeuresHommeEstimees);
                if (!int.TryParse(heuresHommeEstimeesStr, out int heuresHommeEstimees)) { heuresHommeEstimees = mappingConfig.HeuresEstimeesParDefaut; }

                string estJalonStr = GetValueOrDefault(ligne, mappingConfig.CsvColumn_EstJalon);
                TypeActivite typeActivite = (bool.TryParse(estJalonStr, out bool j) && j) || (estJalonStr?.Contains("Jalon", StringComparison.OrdinalIgnoreCase) ?? false) ? TypeActivite.JalonUtilisateur : TypeActivite.Tache;

                var tache = _taskManagerService.CreerTache(lotIdCible, blocIdAthena, tacheNom, heuresHommeEstimees);
                tache.IdImporte = GetValueOrDefault(ligne, mappingConfig.CsvColumn_IdImporte);
                tache.MetierId = metierId;
                tache.Type = typeActivite;
                tache.Dependencies = GetValueOrDefault(ligne, mappingConfig.CsvColumn_Dependencies);
                tache.ExclusionsDependances = GetValueOrDefault(ligne, mappingConfig.CsvColumn_ExclusionsDependances);

                importedTasks.Add(tache);
            }

            return (importedTasks.Count, blocsCrees.Count, importedTasks, warnings);
        }

        private List<string> RemapperDependancesDesTaches(List<Tache> importedTasks)
        {
            var warnings = new List<string>();
            var nameOrIdToPlanAthenaIdMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var tache in _taskManagerService.ObtenirToutesLesTaches())
            {
                if (!string.IsNullOrWhiteSpace(tache.TacheNom) && !nameOrIdToPlanAthenaIdMap.ContainsKey(tache.TacheNom))
                    nameOrIdToPlanAthenaIdMap[tache.TacheNom] = tache.TacheId;
                if (!string.IsNullOrWhiteSpace(tache.IdImporte) && !nameOrIdToPlanAthenaIdMap.ContainsKey(tache.IdImporte))
                    nameOrIdToPlanAthenaIdMap[tache.IdImporte] = tache.TacheId;
            }

            foreach (var tache in importedTasks)
            {
                bool tacheModifiee = false;
                if (!string.IsNullOrWhiteSpace(tache.Dependencies))
                {
                    var oldDependencies = tache.Dependencies.Split(',').Select(d => d.Trim()).Where(d => !string.IsNullOrEmpty(d)).ToList();
                    var newDependencies = new List<string>();
                    foreach (var oldDep in oldDependencies)
                    {
                        if (nameOrIdToPlanAthenaIdMap.TryGetValue(oldDep, out string? newId)) newDependencies.Add(newId);
                        else { newDependencies.Add(oldDep); warnings.Add($"Dépendance '{oldDep}' pour la tâche '{tache.TacheNom}' non trouvée."); }
                    }
                    var newDependenciesString = string.Join(",", newDependencies.Distinct());
                    if (newDependenciesString != tache.Dependencies)
                    {
                        tache.Dependencies = newDependenciesString;
                        tacheModifiee = true;
                    }
                }
                if (!string.IsNullOrWhiteSpace(tache.ExclusionsDependances))
                {
                    var oldExclusions = tache.ExclusionsDependances.Split(',').Select(d => d.Trim()).Where(d => !string.IsNullOrEmpty(d)).ToList();
                    var newExclusions = new List<string>();
                    foreach (var oldExcl in oldExclusions)
                    {
                        if (nameOrIdToPlanAthenaIdMap.TryGetValue(oldExcl, out string? newId)) newExclusions.Add(newId);
                        else { newExclusions.Add(oldExcl); warnings.Add($"Exclusion '{oldExcl}' pour la tâche '{tache.TacheNom}' non trouvée."); }
                    }
                    var newExclusionsString = string.Join(",", newExclusions.Distinct());
                    if (newExclusionsString != tache.ExclusionsDependances)
                    {
                        tache.ExclusionsDependances = newExclusionsString;
                        tacheModifiee = true;
                    }
                }
                if (tacheModifiee)
                {
                    _taskManagerService.ModifierTache(tache);
                }
            }
            return warnings;
        }

        private List<Dictionary<string, string>> LireCsvBrut(string filePath, ImportMappingConfiguration mappingConfig, out List<string> headers)
        {
            headers = new List<string>();
            var allCsvData = new List<Dictionary<string, string>>();
            var lignes = File.ReadAllLines(filePath);
            if (lignes.Length < 1) return allCsvData;

            char separateur = lignes[0].Contains('\t') ? '\t' : ';';
            int debutDonneesIndex = 0;

            if (mappingConfig.HasHeaderRecord)
            {
                headers.AddRange(lignes[0].Split(separateur).Select(h => h.Trim()));
                debutDonneesIndex = 1;
            }
            else
            {
                int maxColumns = lignes[0].Split(separateur).Length;
                for (int i = 0; i < maxColumns; i++) headers.Add($"Colonne {i + 1}");
            }

            for (int i = debutDonneesIndex; i < lignes.Length; i++)
            {
                var valeurs = lignes[i].Split(separateur);
                var ligneDict = new Dictionary<string, string>();
                for (int j = 0; j < headers.Count; j++)
                    ligneDict[headers[j]] = j < valeurs.Length ? valeurs[j].Trim() : "";
                allCsvData.Add(ligneDict);
            }
            return allCsvData;
        }

        private static string GetValueOrDefault(Dictionary<string, string> dict, string mappedColumnName, string defaultValue = "")
        {
            if (string.IsNullOrWhiteSpace(mappedColumnName)) return defaultValue;
            return dict.TryGetValue(mappedColumnName, out string? value) ? value : defaultValue;
        }
        #endregion
    }

}
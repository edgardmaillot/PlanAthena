// PlanAthena/Services/DataAccess/ImportService.cs V0.4.8

using ChoETL; 
using PlanAthena.Data;
using PlanAthena.Interfaces;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Services.DTOs.ImportExport;
using PlanAthena.Services.Usecases;
using PlanAthena.View.Utils;
using System.Diagnostics;
using System.Text;

namespace PlanAthena.Services.DataAccess
{
    public class ImportService
    {
        private readonly ProjetService _projetService;
        private readonly RessourceService _ressourceService;
        private readonly TaskManagerService _taskManagerService;
        private readonly IIdGeneratorService _idGenerator;
        private readonly CsvDataService _csvDataService;

        public ImportService(
            ProjetService projetService,
            RessourceService ressourceService,
            TaskManagerService taskManagerService,
            IIdGeneratorService idGenerator,
            CsvDataService csvDataService)
        {
            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService));
            _ressourceService = ressourceService ?? throw new ArgumentNullException(nameof(ressourceService));
            _taskManagerService = taskManagerService ?? throw new ArgumentNullException(nameof(taskManagerService)); 
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

        #region Méthodes Privées de l'Import de Tâches


        public ImportResult ImporterTaches(TachesImportPlan plan, string lotIdCible, bool remplacerExistants)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var warnings = new List<string>();

            try
            {
                var lot = _projetService.ObtenirLotParId(lotIdCible);
                if (lot == null) return ImportResult.Echec($"Le lot cible '{lotIdCible}' n'a pas été trouvé.");

                if (remplacerExistants)
                {
                    var tachesASupprimer = _taskManagerService.ObtenirToutesLesTaches(lotId: lotIdCible);
                    foreach (var tache in tachesASupprimer) _taskManagerService.SupprimerTache(tache.TacheId);

                    var blocsASupprimer = _projetService.ObtenirBlocsParLot(lotIdCible);
                    foreach (var bloc in blocsASupprimer) _projetService.SupprimerBloc(bloc.BlocId);
                }

                var tempBlocIdToRealBlocIdMap = new Dictionary<string, string>();

                // Étape 1: Créer les blocs et mapper les IDs temporaires aux IDs réels
                foreach (var blocTemp in plan.NouveauxBlocs)
                {
                    var blocExistant = _projetService.ObtenirBlocsParLot(lotIdCible)
                                                .FirstOrDefault(b => b.Nom.Equals(blocTemp.Nom, StringComparison.OrdinalIgnoreCase));

                    Bloc blocReel = blocExistant ?? _projetService.CreerBloc(lotIdCible, blocTemp.Nom, blocTemp.CapaciteMaxOuvriers);

                    tempBlocIdToRealBlocIdMap[blocTemp.BlocId] = blocReel.BlocId;
                }

                // Étape 2: Créer les tâches en utilisant les vrais IDs de blocs
                var tachesCrees = new List<Tache>();
                foreach (var tacheTemp in plan.NouvellesTaches)
                {
                    if (!tempBlocIdToRealBlocIdMap.TryGetValue(tacheTemp.BlocId, out string realBlocId))
                    {
                        warnings.Add($"Le bloc '{tacheTemp.BlocId.Replace("TEMP_", "")}' pour la tâche '{tacheTemp.TacheNom}' n'a pas pu être créé/trouvé. Tâche ignorée.");
                        continue;
                    }

                    // Mettre à jour les propriétés avec les vrais IDs avant la création
                    tacheTemp.BlocId = realBlocId;
                    tacheTemp.LotId = lotIdCible;

                    var tacheCree = _taskManagerService.CreerTache(tacheTemp.LotId, tacheTemp.BlocId, tacheTemp.TacheNom, tacheTemp.HeuresHommeEstimees);

                    // Stocker les autres propriétés
                    tacheCree.IdImporte = tacheTemp.IdImporte;
                    tacheCree.Dependencies = tacheTemp.Dependencies;
                    tacheCree.ExclusionsDependances = tacheTemp.ExclusionsDependances;
                    tacheCree.Type = tacheTemp.Type;
                    tacheCree.MetierId = tacheTemp.MetierId;

                    tachesCrees.Add(tacheCree);
                }

                // Étape 3: Re-mapper les dépendances maintenant que toutes les tâches ont de vrais IDs
                var remappingWarnings = RemapperDependancesDesTaches(tachesCrees);
                warnings.AddRange(remappingWarnings);

                stopwatch.Stop();
                return ImportResult.Succes(tachesCrees.Count, 1, tempBlocIdToRealBlocIdMap.Count, warnings, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return ImportResult.Echec($"Erreur lors du chargement des tâches : {ex.Message}");
            }
        }
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

        #region Import Ouvriers

        /// <summary>
        /// Méthode finale d'importation des ouvriers qui utilise la configuration
        /// complète issue de l'assistant de mapping (wizard).
        /// </summary>
        /// <summary>
        /// Reçoit une liste d'objets Ouvrier propres et les charge dans le système.
        /// Ne contient aucune logique de mapping ou de transformation de données brutes.
        /// </summary>
        /// <param name="ouvriersAImporter">La liste des objets Ouvrier finaux.</param>
        /// <param name="remplacerExistants">True pour vider la base avant l'import.</param>
        /// <returns>Un objet ImportResult avec le résumé de l'opération.</returns>
        public ImportResult ImporterOuvriers(List<Ouvrier> ouvriersAImporter, bool remplacerExistants)
        {
            var stopwatch = Stopwatch.StartNew();
            var warnings = new List<string>();
            int ouvriersCreesOuMaj = 0;

            try
            {
                if (remplacerExistants)
                {
                    _ressourceService.ViderOuvriers();
                }

                foreach (var ouvrierImporte in ouvriersAImporter)
                {
                    // On cherche d'abord si un ouvrier correspondant existe
                    Ouvrier ouvrierExistant = null;
                    if (!remplacerExistants)
                    {
                        ouvrierExistant = _ressourceService.GetAllOuvriers()
                            .FirstOrDefault(o => o.Nom.Equals(ouvrierImporte.Nom, StringComparison.OrdinalIgnoreCase) &&
                                                 o.Prenom.Equals(ouvrierImporte.Prenom, StringComparison.OrdinalIgnoreCase));
                    }

                    Ouvrier ouvrierACreerOuModifier;

                    if (ouvrierExistant == null)
                    {
                        // L'ouvrier n'existe pas, on le crée en utilisant RessourceService
                        ouvrierACreerOuModifier = _ressourceService.CreerOuvrier(ouvrierImporte.Prenom, ouvrierImporte.Nom, ouvrierImporte.CoutJournalier);
                    }
                    else
                    {
                        // L'ouvrier existe, on le met à jour
                        ouvrierExistant.CoutJournalier = ouvrierImporte.CoutJournalier;
                        ouvrierACreerOuModifier = ouvrierExistant;
                    }

                    // Maintenant, on travaille avec ouvrierACreerOuModifier, qui n'est jamais null
                    // On ajoute les compétences une par une
                    foreach (var competence in ouvrierImporte.Competences)
                    {
                        try
                        {
                            // On vérifie que la compétence n'existe pas déjà avant de l'ajouter
                            if (!ouvrierACreerOuModifier.Competences.Any(c => c.MetierId == competence.MetierId))
                            {
                                _ressourceService.AjouterCompetence(ouvrierACreerOuModifier.OuvrierId, competence.MetierId);
                            }
                        }
                        catch (InvalidOperationException ex)
                        {
                            // Si l'ouvrier a déjà la compétence, on l'ignore silencieusement.
                            // On pourrait ajouter un warning si on voulait être plus verbeux.
                            warnings.Add($"Avertissement pour l'ouvrier {ouvrierACreerOuModifier.NomComplet}: {ex.Message}");
                        }
                    }

                    // Si c'est un ouvrier existant, on s'assure que les modifications sont sauvegardées
                    if (ouvrierExistant != null)
                    {
                        _ressourceService.ModifierOuvrier(ouvrierACreerOuModifier);
                    }

                    ouvriersCreesOuMaj++;
                }

                stopwatch.Stop();
                return ImportResult.SuccesOuvriers(ouvriersCreesOuMaj, warnings, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return ImportResult.Echec($"Erreur lors du chargement des données : {ex.Message}");
            }
        }
        public virtual string DetectCsvDelimiter(string filePath)
        {
            var delimiters = new[] { ';', ',', '\t' };
            string firstLine = File.ReadLines(filePath).FirstOrDefault();
            if (string.IsNullOrEmpty(firstLine)) return ";"; // Par défaut

            return delimiters.OrderByDescending(d => firstLine.Count(c => c == d))
                             .First()
                             .ToString();
        }
        #endregion


        #region Import Natif (Voie Rapide)

        /// <summary>
        /// Importe un fichier CSV d'ouvriers qui est déjà au format standard de PlanAthena.
        /// Contourne le wizard de mapping pour une expérience utilisateur rapide.
        /// </summary>
        /// <param name="filePath">Chemin du fichier CSV.</param>
        /// <param name="remplacerExistants">True pour vider la liste des ouvriers avant l'import.</param>
        /// <returns>Un objet ImportResult avec le résumé de l'opération.</returns>
        public ImportResult ImporterOuvriersFormatNatif(string filePath, bool remplacerExistants)
        {
            var stopwatch = Stopwatch.StartNew();
            var warnings = new List<string>();

            try
            {
                // --- Étape 1 : Lecture du fichier avec ChoETL et un DTO fortement typé ---
                List<OuvrierCsvRecord> lignesCsv;
                using (var reader = new ChoCSVReader<OuvrierCsvRecord>(filePath)
                    .WithFirstLineHeader()
                    .Configure(config =>
                    {
                        config.Delimiter = ";"; // Le format natif utilise le point-virgule
                        config.Encoding = Encoding.UTF8;
                        config.ThrowAndStopOnMissingField = false; // Plus flexible
                    })
                )
                {
                    lignesCsv = reader.ToList();
                }

                // --- Le reste de la logique est directement inspiré de votre ancienne méthode ---

                if (remplacerExistants)
                {
                    _ressourceService.ViderOuvriers();
                }

                var idExterneVersIdInterneMap = new Dictionary<string, string>();
                var ouvriersUniquesExternes = lignesCsv.GroupBy(l => l.OuvrierId).Select(g => g.First());

                foreach (var ligneUnique in ouvriersUniquesExternes)
                {
                    Ouvrier ouvrierExistant = null;
                    if (!remplacerExistants)
                    {
                        // La recherche d'un ouvrier existant se fait par Nom/Prénom
                        ouvrierExistant = _ressourceService.GetAllOuvriers()
                            .FirstOrDefault(o => o.Nom == ligneUnique.Nom && o.Prenom == ligneUnique.Prenom);
                    }

                    if (ouvrierExistant != null)
                    {
                        // On a trouvé un ouvrier existant, on met à jour son ID dans notre map
                        idExterneVersIdInterneMap[ligneUnique.OuvrierId] = ouvrierExistant.OuvrierId;
                    }
                    else
                    {
                        // L'ouvrier n'existe pas, on le crée
                        var nouvelOuvrier = _ressourceService.CreerOuvrier(ligneUnique.Prenom, ligneUnique.Nom, (int)ligneUnique.CoutJournalier);
                        idExterneVersIdInterneMap[ligneUnique.OuvrierId] = nouvelOuvrier.OuvrierId;
                    }
                }

                var ouvriersGroupes = lignesCsv.GroupBy(ligne => ligne.OuvrierId);

                foreach (var groupe in ouvriersGroupes)
                {
                    string idExterne = groupe.Key;
                    if (!idExterneVersIdInterneMap.TryGetValue(idExterne, out string idInterne))
                    {
                        continue;
                    }

                    var ouvrierAModifier = _ressourceService.GetOuvrierById(idInterne);
                    if (ouvrierAModifier == null) continue;

                    var premiereLigne = groupe.First();
                    ouvrierAModifier.Nom = premiereLigne.Nom;
                    ouvrierAModifier.Prenom = premiereLigne.Prenom;
                    ouvrierAModifier.CoutJournalier = (int)premiereLigne.CoutJournalier;

                    if (remplacerExistants)
                    {
                        ouvrierAModifier.Competences.Clear();
                    }

                    foreach (var ligne in groupe)
                    {
                        if (!string.IsNullOrWhiteSpace(ligne.MetierId) && _ressourceService.GetMetierById(ligne.MetierId) != null)
                        {
                            if (!ouvrierAModifier.Competences.Any(c => c.MetierId == ligne.MetierId))
                            {
                                var estPrincipal = !ouvrierAModifier.Competences.Any();
                                ouvrierAModifier.Competences.Add(new CompetenceOuvrier { MetierId = ligne.MetierId, EstMetierPrincipal = estPrincipal });
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(ligne.MetierId))
                        {
                            warnings.Add($"Le métier '{ligne.MetierId}' pour l'ouvrier '{premiereLigne.Nom}' n'existe pas et a été ignoré.");
                        }
                    }
                    _ressourceService.ModifierOuvrier(ouvrierAModifier);
                }

                stopwatch.Stop();
                return ImportResult.SuccesOuvriers(idExterneVersIdInterneMap.Count, warnings, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return ImportResult.Echec($"Erreur lors de l'import natif : {ex.Message}");
            }
        }

        #endregion

    }

}
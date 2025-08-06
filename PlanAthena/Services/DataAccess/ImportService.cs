using PlanAthena.Data;
using PlanAthena.Services.Business;
using PlanAthena.Services.Business.DTOs;
using PlanAthena.Utilities;
using System.Diagnostics;
using System.Globalization;

namespace PlanAthena.Services.DataAccess
{
    /// <summary>
    /// Service d'import de tâches depuis fichiers CSV - Version Simple POC
    /// </summary>
    public class ImportService
    {
        private readonly TacheService _tacheService;
        private readonly ProjetService _projetService;
        private readonly BlocService _blocService;
        private readonly IdGeneratorService _idGenerator;

        public ImportService(
            TacheService tacheService,
            ProjetService projetService,
            BlocService blocService,
            IdGeneratorService idGenerator)
        {
            _tacheService = tacheService ?? throw new ArgumentNullException(nameof(tacheService));
            _projetService = projetService ?? throw new ArgumentNullException(nameof(projetService));
            _blocService = blocService ?? throw new ArgumentNullException(nameof(blocService));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
        }

        /// <summary>
        /// Importe les tâches depuis un fichier CSV en utilisant une configuration de mappage.
        /// </summary>
        /// <param name="filePath">Chemin complet du fichier CSV.</param>
        /// <param name="lotIdCible">L'ID du lot dans lequel importer les tâches.</param>
        /// <param name="mappingConfig">La configuration de mappage des colonnes et paramètres d'import.</param>
        /// <param name="confirmerEcrasement">Indique si l'écrasement des tâches existantes a déjà été confirmé.</param>
        /// <returns>Un objet ImportResult détaillant le succès, l'échec ou la demande de confirmation.</returns>
        public ImportResult ImporterTachesCSV(string filePath, string lotIdCible, ImportMappingConfiguration mappingConfig, bool confirmerEcrasement = false)
        {
            var stopwatch = Stopwatch.StartNew();
            var allWarnings = new List<string>(); // Liste pour collecter TOUS les avertissements

            try
            {
                // 1. Validations de base
                if (!File.Exists(filePath))
                    return ImportResult.Echec($"Le fichier '{filePath}' n'existe pas.");

                if (string.IsNullOrWhiteSpace(lotIdCible))
                    return ImportResult.Echec("L'ID du lot cible ne peut pas être vide.");

                // 2. Détection automatique du séparateur et lecture CSV complète
                var lignes = File.ReadAllLines(filePath);
                if (lignes.Length < 1) // Au moins une ligne (en-tête ou données)
                    return ImportResult.Echec("Le fichier CSV est vide.");

                // Détecter le séparateur (TAB ou point-virgule)
                var premiereLigneBrute = lignes[0];
                char separateur = premiereLigneBrute.Contains('\t') ? '\t' : ';';

                List<string> headers = new List<string>();
                List<Dictionary<string, string>> allCsvData = new List<Dictionary<string, string>>(); // TOUTES les données CSV

                int debutDonneesIndex = 0;
                if (mappingConfig.HasHeaderRecord)
                {
                    headers = premiereLigneBrute.Split(separateur).Select(h => h.Trim()).ToList();
                    debutDonneesIndex = 1;
                }
                else
                {
                    int maxColumns = premiereLigneBrute.Split(separateur).Length;
                    for (int i = 0; i < maxColumns; i++)
                    {
                        headers.Add($"Colonne {i + 1}");
                    }
                }

                // Vérification que tous les en-têtes mappés existent dans le fichier CSV (ou sont génériques)
                var requiredMappings = new Dictionary<string, string>
                {
                    { nameof(ImportMappingConfiguration.CsvColumn_TacheNom), mappingConfig.CsvColumn_TacheNom },
                    { nameof(ImportMappingConfiguration.CsvColumn_HeuresHommeEstimees), mappingConfig.CsvColumn_HeuresHommeEstimees },
                    { nameof(ImportMappingConfiguration.CsvColumn_MetierId), mappingConfig.CsvColumn_MetierId },
                    { nameof(ImportMappingConfiguration.CsvColumn_BlocId), mappingConfig.CsvColumn_BlocId }
                };

                foreach (var entry in requiredMappings)
                {
                    var mappedColumnName = entry.Value;
                    if (string.IsNullOrWhiteSpace(mappedColumnName) || !headers.Contains(mappedColumnName))
                    {
                        return ImportResult.Echec($"Le mappage pour le champ obligatoire '{GetFriendlyFieldName(entry.Key)}' est manquant ou ne correspond pas à une colonne du fichier CSV. Assurez-vous que la colonne '{mappedColumnName}' existe.");
                    }
                }

                for (int i = debutDonneesIndex; i < lignes.Length; i++)
                {
                    var valeurs = lignes[i].Split(separateur);
                    var ligneDict = new Dictionary<string, string>();

                    for (int j = 0; j < headers.Count; j++)
                    {
                        var valeur = j < valeurs.Length ? valeurs[j].Trim() : "";
                        ligneDict[headers[j]] = valeur;
                    }
                    allCsvData.Add(ligneDict);
                }

                if (allCsvData.Count == 0)
                    return ImportResult.Echec("Le fichier CSV ne contient aucune ligne de données exploitable après l'en-tête.");


                // NOUVEAU : Pré-analyse des métiers sur TOUTES les lignes avant l'import réel
                // pour fournir des avertissements complets.
                if (!string.IsNullOrEmpty(mappingConfig.CsvColumn_MetierId))
                {
                    var allExistingMetiers = _projetService.GetAllMetiers().Select(m => m.MetierId).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    var missingMetiersInFullData = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var rowData in allCsvData)
                    {
                        string metierInCsv = GetValueOrDefault(rowData, mappingConfig.CsvColumn_MetierId);
                        if (!string.IsNullOrWhiteSpace(metierInCsv) && !allExistingMetiers.Contains(metierInCsv))
                        {
                            missingMetiersInFullData.Add(metierInCsv);
                        }
                    }

                    if (missingMetiersInFullData.Any())
                    {
                        foreach (var missing in missingMetiersInFullData.OrderBy(m => m))
                        {
                            allWarnings.Add($"Le métier '{missing}' (présent dans le CSV) n'existe pas dans PlanAthena. Veuillez le créer manuellement.");
                        }
                    }
                }


                // 3. Vérification confirmation si nécessaire
                if (!confirmerEcrasement)
                {
                    var lot = _projetService.ObtenirLotParId(lotIdCible);
                    if (lot != null)
                    {
                        var tachesExistantes = _tacheService.ObtenirTachesParLot(lotIdCible);
                        if (tachesExistantes.Count > 0)
                        {
                            var message = $"⚠️ ATTENTION : L'import dans le lot '{lot.Nom}' écrasera {tachesExistantes.Count} tâche(s) existante(s).\n\nCette action est irréversible.\n\nConfirmer l'import ?";
                            return ImportResult.DemandeConfirmation(message);
                        }
                    }
                }

                // 4. Vider le lot existant
                try
                {
                    ViderLot(lotIdCible);
                }
                catch (Exception ex)
                {
                    return ImportResult.Echec($"Impossible de vider le lot existant : {ex.Message}");
                }

                // 5. Import des données de manière atomique
                try
                {
                    // Importer les tâches une première fois pour générer les IDs PlanAthena et collecter les avertissements.
                    // Les dépendances sont stockées temporairement sous leur forme brute (nom/ancien ID).
                    var (nbTaches, nbBlocs, importedTasks, importWarnings) = ImporterDonneesInitial(allCsvData, lotIdCible, mappingConfig);
                    allWarnings.AddRange(importWarnings); // Ajouter les avertissements de l'import initial

                    // Phase de post-traitement : remapper les dépendances
                    var remappingWarnings = RemapperDependancesDesTaches(importedTasks);
                    allWarnings.AddRange(remappingWarnings);

                    stopwatch.Stop();
                    return ImportResult.Succes(nbTaches, 1, nbBlocs, allWarnings, stopwatch.Elapsed);
                }
                catch (Exception ex)
                {
                    // En cas d'erreur, le lot reste vide (pas de rollback partiel)
                    return ImportResult.Echec($"Erreur lors de l'import des données : {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return ImportResult.Echec($"Erreur lors de l'import: {ex.Message}");
            }
        }

        /// <summary>
        /// Vide complètement un lot en gérant les dépendances
        /// </summary>
        private void ViderLot(string lotId)
        {
            // 1. Récupérer toutes les tâches à supprimer
            var tachesASupprimer = _tacheService.ObtenirTachesParLot(lotId);
            var idsASupprimer = tachesASupprimer.Select(t => t.TacheId).ToHashSet();

            // 2. Nettoyer les dépendances dans TOUTES les tâches qui référencent celles du lot
            var toutesLesTaches = _tacheService.ObtenirToutesLesTaches();
            foreach (var tache in toutesLesTaches)
            {
                bool modifiee = false;

                // Nettoyer Dependencies
                if (!string.IsNullOrEmpty(tache.Dependencies))
                {
                    var deps = tache.Dependencies.Split(',')
                        .Select(d => d.Trim())
                        .Where(d => !string.IsNullOrEmpty(d) && !idsASupprimer.Contains(d))
                        .ToList();

                    var nouvelleDeps = string.Join(",", deps);
                    if (nouvelleDeps != tache.Dependencies)
                    {
                        tache.Dependencies = nouvelleDeps;
                        modifiee = true;
                    }
                }

                // Nettoyer ExclusionsDependances
                if (!string.IsNullOrEmpty(tache.ExclusionsDependances))
                {
                    var exclusions = tache.ExclusionsDependances.Split(',')
                        .Select(d => d.Trim())
                        .Where(d => !string.IsNullOrEmpty(d) && !idsASupprimer.Contains(d))
                        .ToList();

                    var nouvellesExclusions = string.Join(",", exclusions);
                    if (nouvellesExclusions != tache.ExclusionsDependances)
                    {
                        tache.ExclusionsDependances = nouvellesExclusions;
                        modifiee = true;
                    }
                }

                // Sauvegarder si modifiée (sauf si c'est une tâche à supprimer)
                if (modifiee && !idsASupprimer.Contains(tache.TacheId))
                {
                    _tacheService.ModifierTache(tache);
                }
            }

            // 3. Supprimer toutes les tâches du lot (maintenant sans dépendances)
            foreach (var tache in tachesASupprimer)
            {
                _tacheService.SupprimerTache(tache.TacheId);
            }

            // 4. Supprimer tous les blocs du lot
            var blocs = _blocService.ObtenirTousLesBlocs().Where(b => b.BlocId.StartsWith($"{lotId}_")).ToList();
            foreach (var bloc in blocs)
            {
                _blocService.SupprimerBloc(bloc.BlocId);
            }
        }

        /// <summary>
        /// Importe les données des tâches à partir des données CSV analysées, en utilisant la configuration de mappage.
        /// Cette méthode effectue l'import initial des tâches et des blocs.
        /// </summary>
        /// <param name="donnees">Liste des dictionnaires représentant chaque ligne CSV avec les en-têtes comme clés.</param>
        /// <param name="lotIdCible">ID du lot de destination.</param>
        /// <param name="mappingConfig">Configuration de mappage et paramètres d'import.</param>
        /// <returns>Un tuple contenant le nombre de tâches créées, le nombre de blocs créés,
        /// une liste des tâches importées (avec leurs IDs PlanAthena), et une liste d'avertissements.</returns>
        private (int nbTaches, int nbBlocs, List<Tache> importedTasks, List<string> warnings) ImporterDonneesInitial(
            List<Dictionary<string, string>> donnees,
            string lotIdCible,
            ImportMappingConfiguration mappingConfig)
        {
            var warnings = new List<string>();
            var blocsCrees = new HashSet<string>();
            var blocCsvNameToBlocIdAthena = new Dictionary<string, string>(); // Ancien nom/ID de bloc CSV -> Nouvel ID PlanAthena
            var importedTasks = new List<Tache>(); // Collecte les tâches nouvellement importées

            // 1. Créer/récupérer le lot
            var lot = _projetService.ObtenirLotParId(lotIdCible);
            if (lot == null)
            {
                lot = new Lot
                {
                    LotId = lotIdCible,
                    Nom = $"Lot Importé ({lotIdCible})", // Nom par défaut si non trouvé
                    Priorite = 1 // Priorité par défaut
                };
                try
                {
                    _projetService.AjouterLot(lot);
                    warnings.Add($"Le lot '{lot.Nom}' (ID: {lot.LotId}) a été créé car il n'existait pas.");
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Impossible de créer le lot '{lot.Nom}' (ID: {lot.LotId}): {ex.Message}");
                }
            }


            // 2. Traiter et créer les blocs
            foreach (var ligne in donnees)
            {
                string blocNomCsv = GetValueOrDefault(ligne, mappingConfig.CsvColumn_BlocId);

                // Si le nom du bloc est vide, utiliser le nom de bloc par défaut
                if (string.IsNullOrWhiteSpace(blocNomCsv))
                {
                    blocNomCsv = mappingConfig.NomBlocParDefaut;
                }

                if (!blocCsvNameToBlocIdAthena.ContainsKey(blocNomCsv))
                {
                    Bloc blocAthena = _blocService.ObtenirTousLesBlocs()
                        .FirstOrDefault(b => b.Nom == blocNomCsv && b.BlocId.StartsWith($"{lotIdCible}_"));

                    if (blocAthena == null)
                    {
                        // Créer un nouveau bloc si non trouvé
                        var nouveauBlocId = _idGenerator.GenererProchainBlocId(lotIdCible);
                        blocAthena = new Bloc
                        {
                            BlocId = nouveauBlocId,
                            Nom = blocNomCsv,
                            CapaciteMaxOuvriers = mappingConfig.CapaciteMaxOuvriersBlocParDefaut
                        };
                        _blocService.SaveBloc(blocAthena);
                        blocsCrees.Add(blocAthena.BlocId); // Ajout au set de blocs créés
                        warnings.Add($"Bloc '{blocAthena.Nom}' (ID: {blocAthena.BlocId}) créé pour le lot '{lot.Nom}'.");
                    }
                    else
                    {
                        // Réutiliser le bloc existant
                        warnings.Add($"Bloc '{blocAthena.Nom}' (ID: {blocAthena.BlocId}) réutilisé pour le lot '{lot.Nom}'.");
                    }
                    blocCsvNameToBlocIdAthena[blocNomCsv] = blocAthena.BlocId;
                }
            }

            // 3. Créer les tâches
            foreach (var ligne in donnees)
            {
                var ligneIndexOriginalCsv = donnees.IndexOf(ligne) + (mappingConfig.HasHeaderRecord ? 2 : 1); // Numéro de ligne dans le CSV d'origine

                string tacheNom = GetValueOrDefault(ligne, mappingConfig.CsvColumn_TacheNom);
                if (string.IsNullOrWhiteSpace(tacheNom))
                {
                    warnings.Add($"Ligne {ligneIndexOriginalCsv}: Nom de tâche manquant. La ligne sera ignorée.");
                    continue; // Ignorer la ligne si le nom de tâche est vide
                }

                string metierId = GetValueOrDefault(ligne, mappingConfig.CsvColumn_MetierId);
                if (string.IsNullOrWhiteSpace(metierId))
                {
                    warnings.Add($"Ligne {ligneIndexOriginalCsv}: ID Métier manquant pour la tâche '{tacheNom}'. La ligne sera ignorée.");
                    continue; // Ignorer la ligne si l'ID métier est vide
                }

                // Bloc ID mapping
                string blocNomCsvForTache = GetValueOrDefault(ligne, mappingConfig.CsvColumn_BlocId);
                if (string.IsNullOrWhiteSpace(blocNomCsvForTache))
                {
                    blocNomCsvForTache = mappingConfig.NomBlocParDefaut; // Utiliser le nom de bloc par défaut si non spécifié
                    // L'avertissement est déjà généré dans la phase de création des blocs pour ce cas, ou en amont par l'UI.
                    // warnings.Add($"Ligne {ligneIndexOriginalCsv}: Champ Bloc ID vide ou non mappé pour la tâche '{tacheNom}'. La tâche sera assignée au bloc par défaut '{mappingConfig.NomBlocParDefaut}'.");
                }

                string blocIdAthena = blocCsvNameToBlocIdAthena.ContainsKey(blocNomCsvForTache) ? blocCsvNameToBlocIdAthena[blocNomCsvForTache] : null;

                if (string.IsNullOrWhiteSpace(blocIdAthena))
                {
                    warnings.Add($"Ligne {ligneIndexOriginalCsv}: Impossible de trouver ou créer un bloc pour la tâche '{tacheNom}'. La ligne sera ignorée.");
                    continue; // Ignorer la ligne si le bloc ne peut pas être déterminé
                }

                // Heures Homme Estimées
                string heuresHommeEstimeesStr = GetValueOrDefault(ligne, mappingConfig.CsvColumn_HeuresHommeEstimees);
                int heuresHommeEstimees = mappingConfig.HeuresEstimeesParDefaut;
                if (!int.TryParse(heuresHommeEstimeesStr, out heuresHommeEstimees))
                {
                    heuresHommeEstimees = mappingConfig.HeuresEstimeesParDefaut;
                    if (!string.IsNullOrWhiteSpace(heuresHommeEstimeesStr)) // N'ajouter un warning que si une valeur était présente mais invalide
                    {
                        warnings.Add($"Ligne {ligneIndexOriginalCsv}: Heures Homme Estimées '{heuresHommeEstimeesStr}' invalides pour la tâche '{tacheNom}'. Valeur par défaut de {mappingConfig.HeuresEstimeesParDefaut} heures utilisée.");
                    }
                }

                // Type d'activité (Jalon ou Tache)
                string estJalonStr = GetValueOrDefault(ligne, mappingConfig.CsvColumn_EstJalon);
                TypeActivite typeActivite = TypeActivite.Tache; // Par défaut, c'est une tâche
                if (!string.IsNullOrWhiteSpace(estJalonStr))
                {
                    if (bool.TryParse(estJalonStr, out bool jalon) && jalon)
                    {
                        typeActivite = TypeActivite.JalonUtilisateur;
                    }
                    else if (estJalonStr.Contains("Jalon", StringComparison.OrdinalIgnoreCase)) // Permettre "Jalon" comme texte
                    {
                        typeActivite = TypeActivite.JalonUtilisateur;
                    }
                }

                // ID Importé
                string idImporte = GetValueOrDefault(ligne, mappingConfig.CsvColumn_IdImporte);

                // Dépendances brutes du CSV, elles seront remappées plus tard
                string rawDependencies = GetValueOrDefault(ligne, mappingConfig.CsvColumn_Dependencies);
                string rawExclusions = GetValueOrDefault(ligne, mappingConfig.CsvColumn_ExclusionsDependances);


                var tache = new Tache
                {
                    // TacheId sera généré automatiquement par TacheService.AjouterTache()
                    IdImporte = idImporte,
                    TacheNom = tacheNom,
                    LotId = lotIdCible,
                    BlocId = blocIdAthena,
                    HeuresHommeEstimees = heuresHommeEstimees,
                    MetierId = metierId,
                    Type = typeActivite,
                    Dependencies = rawDependencies, // Stocker la version brute pour le remappage
                    ExclusionsDependances = rawExclusions // Stocker la version brute pour le remappage
                };

                _tacheService.AjouterTache(tache); // TacheService génère l'ID automatiquement
                importedTasks.Add(tache); // Ajouter la tâche à la liste des tâches importées
            }

            return (importedTasks.Count, blocsCrees.Count, importedTasks, warnings);
        }

        /// <summary>
        /// Remappe les dépendances des tâches importées depuis leurs noms/anciens IDs vers les IDs PlanAthena générés.
        /// </summary>
        /// <param name="importedTasks">Liste des tâches qui viennent d'être importées (avec leurs IDs PlanAthena).</param>
        /// <returns>Liste des avertissements générés pendant le remappage.</returns>
        private List<string> RemapperDependancesDesTaches(List<Tache> importedTasks)
        {
            var warnings = new List<string>();

            // Construire un dictionnaire de mappage des noms de tâches/IDs importés vers les IDs PlanAthena.
            // On inclut les tâches déjà existantes car une tâche importée pourrait dépendre d'une tâche non écrasée.
            var allTachesInSystem = _tacheService.ObtenirToutesLesTaches();

            var nameOrIdToPlanAthenaIdMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var tache in allTachesInSystem)
            {
                // Mappage par TacheNom (le plus probable pour les dépendances CSV)
                if (!string.IsNullOrWhiteSpace(tache.TacheNom))
                {
                    // Priorité au premier mapping trouvé si noms dupliqués, ou ajouter un avertissement si noms non uniques sont utilisés comme dépendances
                    if (!nameOrIdToPlanAthenaIdMap.ContainsKey(tache.TacheNom))
                    {
                        nameOrIdToPlanAthenaIdMap[tache.TacheNom] = tache.TacheId;
                    }
                }
                // Mappage par IdImporte (si l'ID original était le préfixe de dépendance)
                if (!string.IsNullOrWhiteSpace(tache.IdImporte))
                {
                    if (!nameOrIdToPlanAthenaIdMap.ContainsKey(tache.IdImporte))
                    {
                        nameOrIdToPlanAthenaIdMap[tache.IdImporte] = tache.TacheId;
                    }
                }
            }

            // Itérer sur les tâches importées pour mettre à jour leurs dépendances
            foreach (var tache in importedTasks)
            {
                bool tacheModifiee = false;

                // Remapper Dependencies
                if (!string.IsNullOrWhiteSpace(tache.Dependencies))
                {
                    var oldDependencies = tache.Dependencies.Split(',')
                        .Select(d => d.Trim())
                        .Where(d => !string.IsNullOrEmpty(d))
                        .ToList();

                    var newDependencies = new List<string>();
                    foreach (var oldDep in oldDependencies)
                    {
                        if (nameOrIdToPlanAthenaIdMap.TryGetValue(oldDep, out string newId))
                        {
                            newDependencies.Add(newId);
                        }
                        else
                        {
                            // Si la dépendance n'est pas trouvée, elle est soit externe, soit invalide.
                            // Pour ce POC, nous laissons la dépendance brute, mais signalons.
                            newDependencies.Add(oldDep); // Garde la dépendance non résolue pour l'analyse par le solveur
                            warnings.Add($"Dépendance '{oldDep}' pour la tâche '{tache.TacheNom}' (ID: {tache.TacheId}) non trouvée parmi les tâches existantes ou importées. La dépendance pourrait être invalide.");
                        }
                    }
                    var newDependenciesString = string.Join(",", newDependencies.Distinct()); // Utiliser Distinct pour éviter les doublons si même dépendance plusieurs fois
                    if (newDependenciesString != tache.Dependencies)
                    {
                        tache.Dependencies = newDependenciesString;
                        tacheModifiee = true;
                    }
                }

                // Remapper ExclusionsDependances (même logique)
                if (!string.IsNullOrWhiteSpace(tache.ExclusionsDependances))
                {
                    var oldExclusions = tache.ExclusionsDependances.Split(',')
                        .Select(d => d.Trim())
                        .Where(d => !string.IsNullOrEmpty(d))
                        .ToList();

                    var newExclusions = new List<string>();
                    foreach (var oldExcl in oldExclusions)
                    {
                        if (nameOrIdToPlanAthenaIdMap.TryGetValue(oldExcl, out string newId))
                        {
                            newExclusions.Add(newId);
                        }
                        else
                        {
                            newExclusions.Add(oldExcl);
                            warnings.Add($"Exclusion de dépendance '{oldExcl}' pour la tâche '{tache.TacheNom}' (ID: {tache.TacheId}) non trouvée. L'exclusion pourrait être invalide.");
                        }
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
                    _tacheService.ModifierTache(tache); // Persister les dépendances remappées
                }
            }

            return warnings;
        }


        /// <summary>
        /// Récupère une valeur du dictionnaire de manière sécurisée en utilisant le nom de colonne mappé.
        /// </summary>
        /// <param name="dict">Le dictionnaire de la ligne CSV.</param>
        /// <param name="mappedColumnName">Le nom de la colonne CSV après mappage, tel que configuré.</param>
        /// <param name="defaultValue">Valeur par défaut si la colonne mappée n'existe pas ou est vide.</param>
        /// <returns>La valeur de la colonne, ou la valeur par défaut.</returns>
        private static string GetValueOrDefault(Dictionary<string, string> dict, string mappedColumnName, string defaultValue = "")
        {
            if (string.IsNullOrWhiteSpace(mappedColumnName))
                return defaultValue; // Colonne non mappée

            return dict.TryGetValue(mappedColumnName, out string value) ? value : defaultValue;
        }

        /// <summary>
        /// Retourne un nom de champ convivial basé sur le nom de la propriété du mapping.
        /// Utilisé pour les messages d'erreur à l'utilisateur.
        /// </summary>
        private string GetFriendlyFieldName(string propertyName)
        {
            return propertyName switch
            {
                nameof(ImportMappingConfiguration.CsvColumn_IdImporte) => "ID d'origine",
                nameof(ImportMappingConfiguration.CsvColumn_TacheNom) => "Nom de la Tâche",
                nameof(ImportMappingConfiguration.CsvColumn_HeuresHommeEstimees) => "Heures Homme Estimées",
                nameof(ImportMappingConfiguration.CsvColumn_MetierId) => "ID Métier",
                nameof(ImportMappingConfiguration.CsvColumn_BlocId) => "ID Bloc",
                nameof(ImportMappingConfiguration.CsvColumn_Dependencies) => "Dépendances",
                nameof(ImportMappingConfiguration.CsvColumn_ExclusionsDependances) => "Exclusions de Dépendances",
                nameof(ImportMappingConfiguration.CsvColumn_EstJalon) => "Est un Jalon",
                _ => propertyName.Replace("CsvColumn_", "").Replace("Tache", "Tâche ").Replace("HeuresHomme", "Heures ").Replace("Est", "Est "), // Fallback générique
            };
        }
    }
}
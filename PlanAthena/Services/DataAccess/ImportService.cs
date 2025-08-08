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
            var blocCsvNameToBlocIdAthena = new Dictionary<string, string>();
            var importedTasks = new List<Tache>();

            // --- PRÉPARATION ---
            // On récupère l'état initial des entités une seule fois AVANT les boucles.
            // On les met dans des listes mutables pour les mettre à jour au fur et à mesure.
            var tousLesBlocsActuels = _blocService.ObtenirTousLesBlocs().ToList();
            var toutesLesTachesActuelles = _tacheService.ObtenirToutesLesTaches().ToList();

            // 1. Gérer le lot cible
            var lot = _projetService.ObtenirLotParId(lotIdCible);
            if (lot == null)
            {
                // Si le lot n'existe pas, il faut le créer.
                // Puisqu'on ne sait pas si d'autres lots existent, on passe la liste complète au générateur.
                var tousLesLotsActuels = _projetService.ObtenirTousLesLots().ToList();
                lotIdCible = _idGenerator.GenererProchainLotId(tousLesLotsActuels); // Génère un ID sûr

                lot = new Lot
                {
                    LotId = lotIdCible,
                    Nom = $"Lot Importé ({lotIdCible})",
                    Priorite = 1
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

            // 2. Traiter et créer les blocs en une seule passe
            // Cette première boucle prépare le mapping entre les noms de bloc du CSV et les ID Athena.
            foreach (var ligne in donnees)
            {
                string blocNomCsv = GetValueOrDefault(ligne, mappingConfig.CsvColumn_BlocId) ?? mappingConfig.NomBlocParDefaut;

                if (!blocCsvNameToBlocIdAthena.ContainsKey(blocNomCsv))
                {
                    // Chercher si un bloc avec ce nom existe déjà pour ce lot DANS NOTRE LISTE LOCALE
                    Bloc blocAthena = tousLesBlocsActuels
                        .FirstOrDefault(b => b.Nom == blocNomCsv && b.BlocId.StartsWith($"{lotIdCible}_"));

                    if (blocAthena == null)
                    {
                        // Créer un nouveau bloc en utilisant notre liste locale mise à jour
                        var nouveauBlocId = _idGenerator.GenererProchainBlocId(lotIdCible, tousLesBlocsActuels);

                        blocAthena = new Bloc
                        {
                            BlocId = nouveauBlocId,
                            Nom = blocNomCsv,
                            CapaciteMaxOuvriers = mappingConfig.CapaciteMaxOuvriersBlocParDefaut
                        };

                        // Mettre à jour la liste locale ET le service
                        tousLesBlocsActuels.Add(blocAthena);
                        _blocService.SaveBloc(blocAthena);

                        blocsCrees.Add(blocAthena.BlocId);
                        warnings.Add($"Bloc '{blocAthena.Nom}' (ID: {blocAthena.BlocId}) créé pour le lot '{lot.Nom}'.");
                    }
                    // Si le bloc existe déjà, on ne fait rien de spécial, on va juste l'utiliser.

                    blocCsvNameToBlocIdAthena[blocNomCsv] = blocAthena.BlocId;
                }
            }

            // 3. Créer les tâches en une seconde passe
            foreach (var ligne in donnees)
            {
                // ... (votre code pour récupérer tacheNom, metierId, heuresHomme, etc. reste le même)
                #region Récupération des données de la ligne (votre code existant)
                var ligneIndexOriginalCsv = donnees.IndexOf(ligne) + (mappingConfig.HasHeaderRecord ? 2 : 1);
                string tacheNom = GetValueOrDefault(ligne, mappingConfig.CsvColumn_TacheNom);
                if (string.IsNullOrWhiteSpace(tacheNom)) { warnings.Add($"Ligne {ligneIndexOriginalCsv}: Nom de tâche manquant. Ignoré."); continue; }
                string metierId = GetValueOrDefault(ligne, mappingConfig.CsvColumn_MetierId);
                if (string.IsNullOrWhiteSpace(metierId)) { warnings.Add($"Ligne {ligneIndexOriginalCsv}: ID Métier manquant pour '{tacheNom}'. Ignoré."); continue; }
                string blocNomCsvForTache = GetValueOrDefault(ligne, mappingConfig.CsvColumn_BlocId) ?? mappingConfig.NomBlocParDefaut;
                string blocIdAthena = blocCsvNameToBlocIdAthena[blocNomCsvForTache];
                string heuresHommeEstimeesStr = GetValueOrDefault(ligne, mappingConfig.CsvColumn_HeuresHommeEstimees);
                if (!int.TryParse(heuresHommeEstimeesStr, out int heuresHommeEstimees)) { heuresHommeEstimees = mappingConfig.HeuresEstimeesParDefaut; /* ... warning ... */ }
                string estJalonStr = GetValueOrDefault(ligne, mappingConfig.CsvColumn_EstJalon);
                TypeActivite typeActivite = TypeActivite.Tache;
                if (!string.IsNullOrWhiteSpace(estJalonStr) && (bool.TryParse(estJalonStr, out bool j) && j || estJalonStr.Contains("Jalon", StringComparison.OrdinalIgnoreCase))) { typeActivite = TypeActivite.JalonUtilisateur; }
                string idImporte = GetValueOrDefault(ligne, mappingConfig.CsvColumn_IdImporte);
                string rawDependencies = GetValueOrDefault(ligne, mappingConfig.CsvColumn_Dependencies);
                string rawExclusions = GetValueOrDefault(ligne, mappingConfig.CsvColumn_ExclusionsDependances);
                #endregion

                // *** POINT DE MODIFICATION PRINCIPAL POUR LES TÂCHES ***

                // On génère l'ID ici, en utilisant la liste locale des tâches, avant de créer l'objet.
                string nouvelIdTache = _idGenerator.GenererProchainTacheId(blocIdAthena, toutesLesTachesActuelles, typeActivite);

                // Si un ID était importé, on tente de le normaliser.
                // S'il est valide, on l'utilise, sinon on garde celui qu'on vient de générer.
                string idFinal = _idGenerator.NormaliserIdDepuisCsv(idImporte, blocIdAthena, toutesLesTachesActuelles, typeActivite);
                if (idFinal != nouvelIdTache)
                {
                    // Logique pour gérer le cas où l'ID importé était valide (on ne le fait pas pour l'instant pour rester simple)
                    // Pour l'instant, on privilégie la génération d'un nouvel ID pour éviter les collisions.
                    // On peut améliorer ça plus tard si besoin.
                }


                var tache = new Tache
                {
                    TacheId = idFinal, // Utiliser l'ID généré et sécurisé.
                    IdImporte = idImporte,
                    TacheNom = tacheNom,
                    LotId = lotIdCible,
                    BlocId = blocIdAthena,
                    HeuresHommeEstimees = heuresHommeEstimees,
                    MetierId = metierId,
                    Type = typeActivite,
                    Dependencies = rawDependencies,
                    ExclusionsDependances = rawExclusions
                };

                // Mettre à jour la liste locale ET le service
                toutesLesTachesActuelles.Add(tache);
                _tacheService.AjouterTache(tache); // AjouterTache doit maintenant accepter un objet Tache avec son ID déjà défini.

                importedTasks.Add(tache);
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